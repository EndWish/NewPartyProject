using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using static Photon.Pun.UtilityScripts.TabViewManager;

public enum TeamType : int
{
    None, Player, Enemy, Num,
}

public class BattleManager : MonoBehaviourPunCallbacksSingleton<BattleManager>
{
    // 연결 정보 //////////////////////////////////////////////////////////////
    #region ForTest
#if DEVELOPMENT_BUILD || UNITY_EDITOR
    public DungeonNodeInfo TestDungeonInfo { get; set; }
#endif
    #endregion

    public string TestBattlePage { get; set; }

    [SerializeField] private Transform playerPartyPos;
    [SerializeField] private Transform enemyPartyPos;
    [SerializeField] private SpriteRenderer backgroundRenderer;

    [SerializeField] private RestSkillManager restSkillManager;
    [SerializeField] private GameObject nextWaveBtn;

    public List<Party>[] Parties { get; private set; } = new List<Party>[(int)TeamType.Num];

    private DungeonNodeInfo dungeonNodeInfo;
    public int Wave { get; private set; } = 0;

    private Unit unitOfTurn = null;
    public Unit UnitClicked { get; set; } = null;
    public Unit UnitOnMouse { get; set; } = null;

    // 개인 정보 //////////////////////////////////////////////////////////////
    private int syncCount = 0;
    private bool isAllLoaded = false;
    public IEnumerator ActionCoroutine { get; set; } = null;

    // 유니티 함수 ////////////////////////////////////////////////////////////
    protected override void Awake() {
        base.Awake();

        for(TeamType type = TeamType.None + 1; type < TeamType.Num; ++type) {
            Parties[(int)type] = new List<Party>();
        }
    }

    protected void Start() {
        dungeonNodeInfo = GameManager.Instance.DungeonInfo;
        backgroundRenderer.sprite = dungeonNodeInfo?.BackgroundImg;

        #region ForTest
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (TestDungeonInfo != null) {
            dungeonNodeInfo = TestDungeonInfo;
            backgroundRenderer.sprite = dungeonNodeInfo.BackgroundImg;
        }
#endif
        #endregion

        restSkillManager.gameObject.SetActive(false);



        StartCoroutine(CoRun());
    }

    protected void Update() {
        // 키보드로 토큰 선택하기
        if(unitOfTurn != null && unitOfTurn.IsMine() && !BattleSelectable.IsRunning) {
            for(KeyCode keyCode = KeyCode.Alpha1; keyCode <= KeyCode.Alpha9; ++keyCode) {
                if (Input.GetKeyUp(keyCode)) {
                    int index = keyCode - KeyCode.Alpha1;
                    if(index < unitOfTurn.Tokens.Count) {
                        Token token = unitOfTurn.Tokens[index];
                        token.IsSelected = !token.IsSelected;
                    }
                }
            }
        }


    }

    // 함수 ///////////////////////////////////////////////////////////////////
    
    IEnumerator CoRun() {

        #region ForTest
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        TestBattlePage = "방에 접속할대 까지 대기중...";
        yield return new WaitUntil(() => PhotonNetwork.InRoom);
#endif
    #endregion

        List<ClientData> clients = GameManager.Instance.ClientDataList;
        ClientData myClient = GameManager.Instance.MyClientData;

        #region 나의 파티 생성하기
        TestBattlePage = "나의 파티 생성중...";
        Party myParty = PhotonNetwork.Instantiate("Prefabs/Battles/Party", Vector3.zero, Quaternion.identity).GetComponent<Party>();
        foreach (Unit.Data unitData in UserData.Instance.PartySequence) {
            if (unitData == null)
                continue;

            Unit syncUnit = PhotonNetwork.Instantiate(GameManager.GetUnitPrefabPath() + unitData.Type.ToString(), Vector3.zero, Quaternion.identity).GetComponent<Unit>();
            syncUnit.ApplyData(unitData);
            myParty.AddUnit(syncUnit);
        }
        AddParty(myParty, TeamType.Player);

        #endregion

        #region 로딩 처리
        TestBattlePage = "로딩중...";
        // 확인 메시지 보내기
        myClient.HasLastRpc = true;

        // 다른 클라이언트의 파티가 나에게 생성되었는지 확인하고 확인 메시지 보내기
        yield return new WaitUntil(() => { 
            return UsefulMethod.IsAll(clients, (client) => client.HasLastRpc); 
        });
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

        // 로딩, 대기 등과 관련된 변수들을 초기화 해준다.
        myClient.IsLoaded = false;
        myClient.IsReady = false;
        myClient.HasLastRpc = false;

        #endregion

        // (방장) 파티 순서 동기화하기
        if (PhotonNetwork.IsMasterClient) {
            for (int i = 0; i < Parties[(int)TeamType.Player].Count; ++i) {
                Party party = Parties[(int)TeamType.Player][i];
                party.SetIndex(i);
                party.IsSyncIndex(true);
            }
            RaiseSyncCount();
        }
        // (전부) 턴 계산 및 토큰 지급이 끝날때 까지 대기
        TestBattlePage = "파티 순서 동기화중...";
        yield return new WaitUntil(() => { return 0 < SyncCount; });
        DownSyncCount();

        // (웨이브 준비 -> 웨이브) 반복
        while (true) {
            // 웨이브 준비 ////////////////////////////////////////////////////
            #region 웨이브 준비
            TestBattlePage = "웨이브 준비 시작...";
            // 파티 스킬 횟수 초기화
            foreach (var party in Parties[(int)TeamType.Player])
                party.RemainRestSkill = 1;

            // 파티 스킬 UI 활성화, 다음 웨이브 버튼 활성화
            restSkillManager.gameObject.SetActive(true);
            nextWaveBtn.SetActive(true);
            nextWaveBtn.GetComponentInChildren<TextMeshProUGUI>().text = "다음 웨이브( " + (Wave + 1) + ") ▶";

            // 모든 플레이어가 다음 웨이브 버튼을 누를 때 까지 대기
            TestBattlePage = "모든 플레이어가 다음 웨이브 버튼을 누를 때 까지 대기중...";
            yield return new WaitUntil( () => { 
                return UsefulMethod.IsAll(clients, (client) => client.IsReady); 
            });
            myClient.IsReady = false;

            // 웨이브 준비에서만 사용되는 UI들 비활성화
            restSkillManager.gameObject.SetActive(false);
            nextWaveBtn.SetActive(false);

            #endregion

            // 다음 웨이브 시작 ///////////////////////////////////////////////
            ++Wave;
            #region (방장)적 파티 생성하기
            if (PhotonNetwork.IsMasterClient) {
                TestBattlePage = "적 파티 생성중";
                for(int i = 0; i < clients.Count; ++i) {
                    Party enemyParty = PhotonNetwork.Instantiate("Prefabs/Battles/Party", Vector3.zero, Quaternion.identity).GetComponent<Party>();


                    List<DungeonNodeInfo.UnitInfo> unitInfoList = null;
                    // 특정 웨이브 출현정보가 있으면 그대로 생성한다
                    var waveInfo = dungeonNodeInfo.WaveInfoList?.Find((info) => info.Wave == this.Wave);
                    if (waveInfo != null) {
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
                    enemyParty.IsSyncIndex(true);
                }
            }
            #endregion

            // (방장) 행동 게이지 랜덤으로 세팅
            if (PhotonNetwork.IsMasterClient) {
                TestBattlePage = "행동 게이지 세팅 및 토큰 생성중...";
                ActionAllUnit((unit) => { unit.ActionGauge = UnityEngine.Random.Range(0, Unit.MaxActionGauge); });
            }

            // (방장) OnBeginWave 함수 발동
            if (PhotonNetwork.IsMasterClient) {
                List<Unit> units = new List<Unit>();
                ActionAllUnit(unit => units.Add(unit));
                units.Sort((a, b) => { return 0 < (b.ActionGauge - a.ActionGauge) ? 1 : -1; });
                foreach (var unit in units) {
                    yield return StartCoroutine(GameManager.CoInvoke(unit.CoOnBeginWave));
                }
            }

            // 턴 반복 ////////////////////////////////////////////////////////
            while (true) {
                // (방장) 틱 수행
                if (PhotonNetwork.IsMasterClient) {
                    List<Unit> units = new List<Unit>();
                    ActionAllUnit(unit => units.Add(unit));
                    units.Sort((a, b) => { return 0 < (b.ActionGauge - a.ActionGauge) ? 1 : -1; });
                    foreach(var unit in units) {
                        yield return StartCoroutine(GameManager.CoInvoke(unit.CoOnBeginTick));
                    }
                }

                // (방장) 턴 계산 및 토큰 지급
                if (PhotonNetwork.IsMasterClient) {
                    TestBattlePage = "턴 계산 및 토큰 지급중...";
                    // 누구의 턴인지 찾고 행동 게이지를 수정해 준다.
                    UnitOfTurn = CalculateUnitOfTurn();
                    float gaugeFillingTime = (Unit.MaxActionGauge - UnitOfTurn.ActionGauge) / UnitOfTurn.GetFinalStat(StatType.Speed);
                    ActionAllUnit((unit) => {
                        unit.ActionGauge += gaugeFillingTime * unit.GetFinalStat(StatType.Speed);
                    });
                    UnitOfTurn.ActionGauge = 0;

                    RotateParties(UnitOfTurn.TeamType, -UnitOfTurn.MyParty.GetIndex());
                    yield return new WaitForSeconds(0.3f);

                    // 토큰을 지급한다
                    yield return StartCoroutine(GameManager.CoInvoke(UnitOfTurn.CoOnBeginMyTurn));
                    for(int i = 0; i < 3; ++i) {
                        yield return StartCoroutine(UnitOfTurn.AddToken(UnitOfTurn.CreateRandomToken()));
                    }

                    RaiseSyncCount();
                }

                // (전부) 턴 계산 및 토큰 지급이 끝날때 까지 대기
                TestBattlePage = "턴 계산 및 토큰 지급이 끝날때 까지 대기중...";
                yield return new WaitUntil(() => { return 0 < SyncCount; });
                DownSyncCount();

                // 내 유닛의 턴일 경우 액션 선택하기
                if (UnitOfTurn.IsMine() || (UnitOfTurn.IsEnemy() && PhotonNetwork.IsMasterClient)) {
                    if (UnitOfTurn.IsMine()) {
                        TestBattlePage = "내 유닛의 턴...";
                        ActionCoroutine = null;
                        TestBattlePage = "액션 코루틴이 설정되기를 기다리는 중...";
                        while (ActionCoroutine == null) {
                            yield return null;
                        }
                    } 
                    else if(UnitOfTurn.IsEnemy()) {
                        yield return StartCoroutine(UnitOfTurn.GetComponent<TokenSelector>().AutoSelect());
                    }

                    TestBattlePage = "액션 코루틴 실행중...";
                    yield return StartCoroutine(ActionCoroutine);
                    yield return new WaitForSeconds(0.5f);
                    TestBattlePage = "액션 코루틴 실행끝";

                    yield return StartCoroutine(GameManager.CoInvoke(UnitOfTurn.CoOnEndMyTurn));
                    UnitOfTurn = null;
                    ActionCoroutine = null;

                    RaiseSyncCount();
                }

                // (전부) 대기하기
                TestBattlePage = "액션 코루틴 실행 종료를 동기화중...";
                yield return new WaitUntil(() => { return 0 < SyncCount; });
                DownSyncCount();
                TestBattlePage = "액션 코루틴 실행 종료를 동기화 완료";

                #region 유닛, 파티 삭제하기
                // (방장) 죽은 유닛을 삭제한다
                if (PhotonNetwork.IsMasterClient) {
                    ActionAllUnit((unit) => {
                        if (unit.IsDie) { unit.Destroy(); }
                    });
                    RaiseSyncCount();
                }

                // (전부) 유닛 삭제 동기화
                yield return new WaitUntil(() => { return 0 < SyncCount; });
                DownSyncCount();

                // (전부) 파티의 Units에서 null이 된 요소들을  제거한다
                ActionAllParty((party) => { party.Units.RemoveAll(unit => unit == null); });

                // (방장) 비어버린 Party를 전부 삭제한다
                if(PhotonNetwork.IsMasterClient) {
                    ActionAllParty((party) => {
                        if (party.Units.Count == 0) { party.Destroy(); }
                    });
                    RaiseSyncCount();
                }

                // (전부) 파티 삭제 동기화
                yield return new WaitUntil(() => { return 0 < SyncCount; });
                DownSyncCount();

                // (전부) null이 된 요소들을  제거한다
                for (TeamType type = TeamType.None; type < TeamType.Num; ++type) {
                    Parties[(int)type]?.RemoveAll((party) => party == null);
                }
                #endregion

                // (전부) 웨이브 결과 확인
                if (Parties[(int)TeamType.Player].Count == 0) {
                    // 패배
                    yield return new WaitForSeconds(1f);
                    if(PhotonNetwork.IsMasterClient) {
                        PhotonNetwork.LoadLevel("VillageScene");
                    }
                    yield break;
                }
                else if(Parties[(int)TeamType.Enemy].Count == 0) {
                    // (방장) 토큰과 상태이상 제거하기
                    if (PhotonNetwork.IsMasterClient) {
                        ActionAllUnit((unit) => {
                            unit.ClearAllToken();
                            unit.ClearAllBarrier();
                            unit.ClearAllStatusEffect();
                        });
                        RaiseSyncCount();
                    }

                    // (방장) OnEndWave 함수 발동
                    if (PhotonNetwork.IsMasterClient) {
                        List<Unit> units = new List<Unit>();
                        ActionAllUnit(unit => units.Add(unit));
                        units.Sort((a, b) => { return 0 < (b.ActionGauge - a.ActionGauge) ? 1 : -1; });
                        foreach (var unit in units) {
                            yield return StartCoroutine(GameManager.CoInvoke(unit.CoOnEndWave));
                        }
                    }

                    yield return new WaitUntil(() => { return 0 < SyncCount; });
                    DownSyncCount();

                    break;
                } 
                else {
                    yield return null;
                }
            }

        }

    }

    [PunRPC] private void RaiseSyncCountRPC() {
        ++syncCount;
    }
    public void RaiseSyncCount() {
        photonView.RPC("RaiseSyncCountRPC", RpcTarget.All);
    }
    public void DownSyncCount() {
        --syncCount;
    }
    public int SyncCount {
        get { return syncCount; }
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
        party.OnSetTeam(teamType);
    }
    public void AddParty(Party party, TeamType teamType) {
        photonView.RPC("AddPartyRPC", RpcTarget.All, party.photonView.ViewID, teamType);
    }
    [PunRPC] 
    private void RemovePartyRPC(int partyViewId, TeamType teamType) {
        Party party = PhotonView.Find(partyViewId).GetComponent<Party>();
        Parties[(int)teamType].Remove(party);
    }
    public void RemoveParty(Party party, TeamType teamType) {
        photonView.RPC("RemovePartyRPC", RpcTarget.All, party.photonView.ViewID, teamType);
    }

    public Vector3 GetPartyPos(TeamType teamType) {
        if (teamType == TeamType.Player)
            return playerPartyPos.position;
        else if (teamType == TeamType.Enemy)
            return enemyPartyPos.position;
        else
            return Vector3.zero;
    }
    [PunRPC]
    private void RotatePartiesRPC(TeamType teamType, int rightOffset) {
        Parties[(int)teamType].Rotate(rightOffset);
        for (int i = 0; i < Parties[(int)teamType].Count; ++i) {
            Party party = Parties[(int)teamType][i];
        }
    }
    public void RotateParties(TeamType teamType, int rightOffset) {
        photonView.RPC("RotatePartiesRPC", RpcTarget.AllViaServer, teamType, rightOffset);
    }

    private int GetRandomEnemyNum() {
        int num = 0;
        float[] percentageOfNum = new float[4] { 0f, 35f, 40f, 25f };
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

    public void ActionAllUnit(UnityAction<Unit> action) {
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
    public void ActionAllParty(UnityAction<Party> action) {
        for (TeamType type = TeamType.None; type < TeamType.Num; ++type) {
            if (Parties[(int)type] == null)
                continue;

            foreach (Party party in Parties[(int)type]) {
                action(party);
            }
        }
    }

    [PunRPC] private void UnitOfTurnRPC(int viewId) {
        
        if (viewId == -1) {
            unitOfTurn = null;
            Debug.Log(Time.frameCount + " UnitOfTurnRPC = null");
            return;
        }
        unitOfTurn = PhotonView.Find(viewId).GetComponent<Unit>();
        Debug.Log(Time.frameCount + " UnitOfTurnRPC = " + unitOfTurn.photonView.ViewID);
    }
    public Unit UnitOfTurn {
        get { return unitOfTurn; }
        private set { 
            unitOfTurn = value;
            photonView.RPC("UnitOfTurnRPC", RpcTarget.Others, value?.photonView.ViewID ?? -1); 
        }
    }

    private Unit CalculateUnitOfTurn() {
        Unit result = null;
        float minTime = float.MaxValue;
        ActionAllUnit((unit) => {
            float unitSpeed = unit.GetFinalStat(StatType.Speed);
            float time = (Unit.MaxActionGauge - unit.ActionGauge) / MathF.Max(unitSpeed, 0.0001f); // 게이지를 다 채우는데 걸리는 시간, 0으로 나누는 것을 방지
            if(time < minTime || (time == minTime && result.GetFinalStat(StatType.Speed) > unit.GetFinalStat(StatType.Speed))) {
                result = unit;
                minTime = time;
            }
        });

        return result;
    }

    public int GetUnitNum() {
        int result = 0;
        ActionAllParty((party) => {
            result += party.Units.Count;
        });
        return result;
    }
    public int GetUnitNum(TeamType teamType) {
        int result = 0;
        foreach (Party party in Parties[(int)teamType]) {
            if (party == null) continue;
            result += party.Units.Count;
        }
        return result;
    }

}
