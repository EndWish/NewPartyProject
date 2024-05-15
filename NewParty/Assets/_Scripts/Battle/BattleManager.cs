using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TeamType : int
{
    None, Player, Enemy, Num,
}

public class BattleManager : MonoBehaviourPunCallbacksSingleton<BattleManager>
{

    // ���� ���� //////////////////////////////////////////////////////////////
    #region ForTest
#if DEVELOPMENT_BUILD || UNITY_EDITOR
    [SerializeField] private DungeonNodeInfo testDungeonInfo;
#endif
    #endregion

    [SerializeField] private Transform playerPartyPos;
    [SerializeField] private Transform enemyPartyPos;
    [SerializeField] private SpriteRenderer backgroundRenderer;
    public List<Party>[] Parties { get; private set; } = new List<Party>[(int)TeamType.Num];

    private DungeonNodeInfo dungeonNodeInfo;
    public int Wave { get; private set; } = 0;

    // ���� ���� //////////////////////////////////////////////////////////////
    private bool isAllLoaded = false;

    // ����Ƽ �Լ� ////////////////////////////////////////////////////////////
    protected override void Awake() {
        base.Awake();

        for(TeamType type = TeamType.None + 1; type < TeamType.Num; ++type) {
            Parties[(int)type] = new List<Party>();
        }
    }

    protected void Start() {
        dungeonNodeInfo = GameManager.Instance.DungeonInfo;
        backgroundRenderer.sprite = dungeonNodeInfo.BackgroundImg;
        #region ForTest
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (testDungeonInfo != null) {
            dungeonNodeInfo = testDungeonInfo;
        }
#endif
        #endregion

        StartCoroutine(CoRun());
    }

    // �Լ� ///////////////////////////////////////////////////////////////////

    IEnumerator CoRun() {

    #region ForTest
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        yield return new WaitUntil(() => PhotonNetwork.InRoom);
#endif
    #endregion

        List<ClientData> clients = GameManager.Instance.ClientDataList;
        ClientData myClient = GameManager.Instance.MyClientData;

        #region ���� ��Ƽ �����ϱ�
        Party myParty = PhotonNetwork.Instantiate("Prefabs/Battles/Party", Vector3.zero, Quaternion.identity).GetComponent<Party>();
        foreach (Unit partyUnit in UserData.Instance.PartyUnitList) {
            if(partyUnit == null)
                continue;

            Unit.Data unitData = partyUnit.MyData;
            Unit syncUnit = PhotonNetwork.Instantiate(GameManager.GetUnitPrefabPath() + unitData.Type.ToString(), Vector3.zero, Quaternion.identity).GetComponent<Unit>();
            syncUnit.ApplyData(unitData);
            myParty.AddUnit(syncUnit);
        }
        AddParty(myParty, TeamType.Player);
        #endregion

        #region (����)�� ��Ƽ �����ϱ�
        if (PhotonNetwork.IsMasterClient) {
            Party enemyParty = PhotonNetwork.Instantiate("Prefabs/Battles/Party", Vector3.zero, Quaternion.identity).GetComponent<Party>();


            List<DungeonNodeInfo.UnitInfo> unitInfoList = null;
            // Ư�� ���̺� ���������� ������ �״�� �����Ѵ�
            var waveInfo = dungeonNodeInfo.WaveInfoList?.Find((info) => info.Wave == this.Wave);
            if(waveInfo != null) {
                unitInfoList = waveInfo.UnitInfoList;
            }
            // Ư�� ���̺� ���������� ������ �������� �����Ѵ�
            else {
                // �� ���� �������� ���ϰ� ������ �����Ѵ�
                int nEnemy = GetRandomEnemyNum();
                unitInfoList = dungeonNodeInfo.GetRandomUnitInfo(nEnemy);
            }

            foreach (var unitInfo in unitInfoList) {
                Unit.Data unitData = unitInfo.ToUnitData();
                Unit syncUnit = PhotonNetwork.Instantiate(GameManager.GetUnitPrefabPath() + unitData.Type.ToString(), Vector3.zero, Quaternion.identity).GetComponent<Unit>();
                syncUnit.ApplyData(unitData);
                enemyParty.AddUnit(syncUnit);
            }
            AddParty(enemyParty, TeamType.Enemy);
        }
        #endregion

        #region �ε� ó��
        // Ȯ�� �޽��� ������
        myClient.HasLastRpc = true;

        // �ٸ� Ŭ���̾�Ʈ�� ��Ƽ�� ������ �����Ǿ����� Ȯ���ϰ� Ȯ�� �޽��� ������
        yield return new WaitUntil(
            () => { return UsefulMethod.IsAll(clients, (client) => client.HasLastRpc); });
        myClient.IsLoaded = true;

        // (����)�ٸ� �÷��̾�� �ε� �ϷḦ �˸�, (�Խ�Ʈ)�ε� �Ϸ� �޽����� ������ ���� ���
        while (true) {
            if (PhotonNetwork.IsMasterClient) {
                if(UsefulMethod.IsAll(clients, (client) => client.IsLoaded == true)) {
                    IsAllLoaded = true;
                    break;
                }
            } else {
                if (IsAllLoaded) break;
            }
            yield return null;
        }
        #endregion

        // ���� ����
        Debug.Log("���� ����");
    }

    [PunRPC]
    private void IsAllLoadedRPC(bool result) {
        isAllLoaded = result;
    }
    public bool IsAllLoaded {
        get { return isAllLoaded; }
        set { photonView.RPC("IsAllLoadedRPC", RpcTarget.All, value); }
    }

    [PunRPC]
    private void AddPartyRPC(int partyViewId, TeamType teamType) {
        Party party = PhotonView.Find(partyViewId).GetComponent<Party>();
        Parties[(int)teamType].Add(party);
        party.MySortingGroup.sortingOrder = -(Parties[(int)teamType].Count - 1);
        party.OnSetTeam(teamType);
    }
    public void AddParty(Party party, TeamType teamType) {
        photonView.RPC("AddPartyRPC", RpcTarget.All, party.photonView.ViewID, teamType);
    }

    public Vector3 GetPartyPos(TeamType teamType) {
        if (teamType == TeamType.Player)
            return playerPartyPos.position;
        else if (teamType == TeamType.Enemy)
            return enemyPartyPos.position;
        else
            return Vector3.zero;
    }
    public void RotateParties(TeamType teamType, int rightOffset) {
        Parties[(int)teamType].Rotate(rightOffset);
        for(int i = 0; i < Parties[(int)teamType].Count; ++i) {
            Party party = Parties[(int)teamType][i];
            party.MySortingGroup.sortingOrder = -i;
        }
    }

    private int GetRandomEnemyNum() {
        int num = 0;
        float[] percentageOfNum = new float[4] { 15f, 20f, 40f, 25f };
        float random = UnityEngine.Random.Range(0f, 100f);

        float stack = 0f;
        for(int i = 0; i < percentageOfNum.Length; ++i) {
            if (stack <= random)
                ++num;
            stack += percentageOfNum[i];
        }

        return num;
    }

}
