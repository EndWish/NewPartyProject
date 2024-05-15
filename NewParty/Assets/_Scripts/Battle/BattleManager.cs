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
    [SerializeField] private Transform playerPartyPos;
    [SerializeField] private Transform enemyPartyPos;
    public List<Party>[] Parties { get; private set; } = new List<Party>[(int)TeamType.Num];

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
        StartCoroutine(CoRun());
    }

    // �Լ� ///////////////////////////////////////////////////////////////////

    IEnumerator CoRun() {

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        yield return new WaitUntil(() => PhotonNetwork.InRoom);
#endif

        List<ClientData> clients = GameManager.Instance.ClientDataList;
        ClientData myClient = GameManager.Instance.MyClientData;

        // ��Ƽ �����ϱ�, (����)�� ��Ƽ �����ϱ�, Ȯ�� �޽��� ������
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

        #region �׽�Ʈ
        for(int i = 0; i < 3; ++i) {
            myParty = PhotonNetwork.Instantiate("Prefabs/Battles/Party", Vector3.zero, Quaternion.identity).GetComponent<Party>();
            foreach (Unit partyUnit in UserData.Instance.PartyUnitList) {
                if (partyUnit == null)
                    continue;

                Unit.Data unitData = partyUnit.MyData;
                Unit syncUnit = PhotonNetwork.Instantiate(GameManager.GetUnitPrefabPath() + unitData.Type.ToString(), Vector3.zero, Quaternion.identity).GetComponent<Unit>();
                syncUnit.ApplyData(unitData);
                myParty.AddUnit(syncUnit);
            }
            AddParty(myParty, TeamType.Enemy);
        }

        #endregion

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

}
