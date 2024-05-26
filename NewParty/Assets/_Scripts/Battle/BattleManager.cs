using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static Photon.Pun.UtilityScripts.TabViewManager;

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

    [SerializeField] private RestSkillManager restSkillManager;

    public List<Party>[] Parties { get; private set; } = new List<Party>[(int)TeamType.Num];

    private DungeonNodeInfo dungeonNodeInfo;
    public int Wave { get; private set; } = 0;

    public Unit UnitOfTurn { get; private set; } = null;
    public Unit UnitClicked { get; set; } = null;
    public Unit UnitOnMouse { get; set; } = null;

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
        backgroundRenderer.sprite = dungeonNodeInfo?.BackgroundImg;

        restSkillManager.gameObject.SetActive(false);

        #region ForTest
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (testDungeonInfo != null) {
            dungeonNodeInfo = testDungeonInfo;
            backgroundRenderer.sprite = dungeonNodeInfo.BackgroundImg;
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

        

        #region �ε� ó��
        // Ȯ�� �޽��� ������
        myClient.HasLastRpc = true;

        // �ٸ� Ŭ���̾�Ʈ�� ��Ƽ�� ������ �����Ǿ����� Ȯ���ϰ� Ȯ�� �޽��� ������
        yield return new WaitUntil(() => { 
            return UsefulMethod.IsAll(clients, (client) => client.HasLastRpc); 
        });
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

        // �ε�, ��� ��� ���õ� �������� �ʱ�ȭ ���ش�.
        myClient.IsLoaded = false;
        myClient.IsReady = false;
        myClient.HasLastRpc = false;

        #endregion

        // ���� ����
        Debug.Log("���� ����");

        // (���̺� �غ� -> ���̺�) �ݺ�
        while (true) {
            #region ���̺� �غ�

            // ��Ƽ ��ų Ƚ�� �ʱ�ȭ
            foreach (var party in Parties[(int)TeamType.Player])
                party.RemainRestSkill = 1;

            // ��Ƽ ��ų UI Ȱ��ȭ, [�߰�]���� ���̺� ��ư Ȱ��ȭ
            restSkillManager.gameObject.SetActive(true);


            // ��� �÷��̾ ���� ���̺� ��ư�� ���� �� ���� ���
            yield return new WaitUntil( () => { 
                return UsefulMethod.IsAll(clients, (client) => client.IsReady); 
            });
            myClient.IsReady = false;

            // ���̺� �غ񿡼��� ���Ǵ� UI�� ��Ȱ��ȭ
            restSkillManager.gameObject.SetActive(false);

            #endregion

            #region ���� ���̺� ����

            ++Wave;
            #region (����)�� ��Ƽ �����ϱ�
            if (PhotonNetwork.IsMasterClient) {
                Party enemyParty = PhotonNetwork.Instantiate("Prefabs/Battles/Party", Vector3.zero, Quaternion.identity).GetComponent<Party>();


                List<DungeonNodeInfo.UnitInfo> unitInfoList = null;
                // Ư�� ���̺� ���������� ������ �״�� �����Ѵ�
                var waveInfo = dungeonNodeInfo.WaveInfoList?.Find((info) => info.Wave == this.Wave);
                if (waveInfo != null) {
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
            #region �ൿ ������ �������� ����
            ActionAllUnit((unit) => { unit.ActionGauge = UnityEngine.Random.Range(0, Unit.MaxActionGauge); });
            #endregion
            #region ��ū �����ϱ�
            for (int i = 0; i < 3; ++i) {
                ActionAllUnit((unit) => { unit.CreateRandomToken(); });
                yield return new WaitForSeconds(0.5f);
            }

            #endregion

            // �� �ݺ�
            while (true) {


                #region �� ���� �� ��ū ����
                // ������ ������ ã�� �ൿ �������� ������ �ش�.
                UnitOfTurn = GetUnitOfTurn();
                float gaugeFillingTime = (Unit.MaxActionGauge - UnitOfTurn.ActionGauge) / MathF.Min(UnitOfTurn.GetFinalStat(StatType.Speed), 0.0001f);
                ActionAllUnit((unit) => {
                    unit.ActionGauge += gaugeFillingTime * unit.GetFinalStat(StatType.Speed);
                });
                UnitOfTurn.ActionGauge = 0;

                // ��ū�� �����Ѵ�
                UnitOfTurn.CreateRandomToken(3);

                #endregion

                #region ��ū ��� �ϱ�
                while (true) {
                    // ��ū�� ����ϸ� ������ ����������
                    Debug.Log("��ū ����� ��ٸ��� ��..");
                    yield return null;
                }

                #endregion

                #region ���̺� Ŭ����/���� Ȯ��

                #endregion

                yield return null;
            }

            #endregion
        }

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

    public void OnClickNextWaveBtn() {
        GameManager.Instance.MyClientData.ToggleReady();
    }

    private void ActionAllUnit(UnityAction<Unit> action) {
        for (TeamType type = TeamType.None; type < TeamType.Num; ++type) {
            if (Parties[(int)type] == null)
                continue;

            foreach (Party party in Parties[(int)type]) {
                foreach (Unit unit in party.Units) {
                    action(unit);
                }
            }
        }
    }

    private Unit GetUnitOfTurn() {
        Unit result = null;
        float minTime = float.MaxValue;
        ActionAllUnit((unit) => {
            float unitSpeed = unit.GetFinalStat(StatType.Speed);
            float time = (Unit.MaxActionGauge - unit.ActionGauge) / MathF.Min(unitSpeed, 0.0001f); // �������� �� ä��µ� �ɸ��� �ð�, 0���� ������ ���� ����
            if(time < minTime || (time == minTime && result.GetFinalStat(StatType.Speed) > unit.GetFinalStat(StatType.Speed))) {
                result = unit;
                minTime = time;
            }
        });

        return result;
    }


}
