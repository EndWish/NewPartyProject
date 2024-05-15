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

    // 연결 정보 //////////////////////////////////////////////////////////////
    [SerializeField] private Transform playerPartyPos;
    [SerializeField] private Transform enemyPartyPos;
    public List<Party>[] Parties { get; private set; } = new List<Party>[(int)TeamType.Num];

    // 개인 정보 //////////////////////////////////////////////////////////////
    private bool isAllLoaded = false;

    // 유니티 함수 ////////////////////////////////////////////////////////////
    protected override void Awake() {
        base.Awake();

        for(TeamType type = TeamType.None + 1; type < TeamType.Num; ++type) {
            Parties[(int)type] = new List<Party>();
        }
    }

    protected void Start() {
        StartCoroutine(CoRun());
    }

    // 함수 ///////////////////////////////////////////////////////////////////

    IEnumerator CoRun() {

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        yield return new WaitUntil(() => PhotonNetwork.InRoom);
#endif

        List<ClientData> clients = GameManager.Instance.ClientDataList;
        ClientData myClient = GameManager.Instance.MyClientData;

        // 파티 생성하기, (방장)적 파티 생성하기, 확인 메시지 보내기
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

        #region 테스트
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

        // 다른 클라이언트의 파티가 나에게 생성되었는지 확인하고 확인 메시지 보내기
        yield return new WaitUntil(
            () => { return UsefulMethod.IsAll(clients, (client) => client.HasLastRpc); });
        myClient.IsLoaded = true;

        // (방장)다른 플레이어에게 로딩 완료를 알림, (게스트)로딩 완료 메시지를 받을때 까지 대기
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

        // 게임 시작
        Debug.Log("게임 시작");
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
