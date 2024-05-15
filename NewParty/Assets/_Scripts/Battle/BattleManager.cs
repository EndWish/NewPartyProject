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

    // 함수 ///////////////////////////////////////////////////////////////////

    IEnumerator CoRun() {

    #region ForTest
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        yield return new WaitUntil(() => PhotonNetwork.InRoom);
#endif
    #endregion

        List<ClientData> clients = GameManager.Instance.ClientDataList;
        ClientData myClient = GameManager.Instance.MyClientData;

        #region 나의 파티 생성하기
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

        #region (방장)적 파티 생성하기
        if (PhotonNetwork.IsMasterClient) {
            Party enemyParty = PhotonNetwork.Instantiate("Prefabs/Battles/Party", Vector3.zero, Quaternion.identity).GetComponent<Party>();


            List<DungeonNodeInfo.UnitInfo> unitInfoList = null;
            // 특정 웨이브 출현정보가 있으면 그대로 생성한다
            var waveInfo = dungeonNodeInfo.WaveInfoList?.Find((info) => info.Wave == this.Wave);
            if(waveInfo != null) {
                unitInfoList = waveInfo.UnitInfoList;
            }
            // 특정 웨이브 출현정보가 없으면 랜덤으로 생성한다
            else {
                // 몇 명을 생성할지 정하고 유닛을 생성한다
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

        #region 로딩 처리
        // 확인 메시지 보내기
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
        #endregion

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
