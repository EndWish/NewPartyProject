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

    public string TestBattlePage = "";

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

    // ���� ���� //////////////////////////////////////////////////////////////
    private int syncCount = 0;
    private bool isAllLoaded = false;
    public IEnumerator ActionCoroutine { get; set; } = null;

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
        TestBattlePage = "�濡 �����Ҵ� ���� �����...";
        yield return new WaitUntil(() => PhotonNetwork.InRoom);
#endif
    #endregion

        List<ClientData> clients = GameManager.Instance.ClientDataList;
        ClientData myClient = GameManager.Instance.MyClientData;

        #region ���� ��Ƽ �����ϱ�
        TestBattlePage = "���� ��Ƽ ������...";
        Party myParty = PhotonNetwork.Instantiate("Prefabs/Battles/Party", Vector3.zero, Quaternion.identity).GetComponent<Party>();
        foreach (Unit partyUnit in UserData.Instance.PartyUnitList) {
            if (partyUnit == null)
                continue;

            Unit.Data unitData = partyUnit.MyData;
            Unit syncUnit = PhotonNetwork.Instantiate(GameManager.GetUnitPrefabPath() + unitData.Type.ToString(), Vector3.zero, Quaternion.identity).GetComponent<Unit>();
            syncUnit.ApplyData(unitData);
            myParty.AddUnit(syncUnit);
        }
        AddParty(myParty, TeamType.Player);
        #endregion



        #region �ε� ó��
        TestBattlePage = "�ε���...";
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
            // ���̺� �غ� ////////////////////////////////////////////////////
            #region ���̺� �غ�
            TestBattlePage = "���̺� �غ� ����...";
            // ��Ƽ ��ų Ƚ�� �ʱ�ȭ
            foreach (var party in Parties[(int)TeamType.Player])
                party.RemainRestSkill = 1;

            // ��Ƽ ��ų UI Ȱ��ȭ, ���� ���̺� ��ư Ȱ��ȭ
            restSkillManager.gameObject.SetActive(true);
            nextWaveBtn.SetActive(true);

            // ��� �÷��̾ ���� ���̺� ��ư�� ���� �� ���� ���
            TestBattlePage = "��� �÷��̾ ���� ���̺� ��ư�� ���� �� ���� �����...";
            yield return new WaitUntil( () => { 
                return UsefulMethod.IsAll(clients, (client) => client.IsReady); 
            });
            myClient.IsReady = false;

            // ���̺� �غ񿡼��� ���Ǵ� UI�� ��Ȱ��ȭ
            restSkillManager.gameObject.SetActive(false);
            nextWaveBtn.SetActive(false);

            #endregion

            // ���� ���̺� ���� ///////////////////////////////////////////////
            ++Wave;
            #region (����)�� ��Ƽ �����ϱ�
            if (PhotonNetwork.IsMasterClient) {
                TestBattlePage = "�� ��Ƽ ������";
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

            // (����) �ൿ ������ �������� ����
            // (����) ��ū �����ϱ�
            if (PhotonNetwork.IsMasterClient) {
                TestBattlePage = "�ൿ ������ ���� �� ��ū ������...";
                ActionAllUnit((unit) => { unit.ActionGauge = UnityEngine.Random.Range(0, Unit.MaxActionGauge); });
                
                for (int i = 0; i < 3; ++i) {
                    ActionAllUnit((unit) => { unit.CreateRandomToken(); });
                    yield return new WaitForSeconds(0.3f);
                }
            }

            // �� �ݺ� ////////////////////////////////////////////////////////
            while (true) {

                // (����) �� ��� �� ��ū ����
                if (PhotonNetwork.IsMasterClient) {
                    TestBattlePage = "�� ��� �� ��ū ������...";
                    // ������ ������ ã�� �ൿ �������� ������ �ش�.
                    UnitOfTurn = CalculateUnitOfTurn();
                    float gaugeFillingTime = (Unit.MaxActionGauge - UnitOfTurn.ActionGauge) / MathF.Max(UnitOfTurn.GetFinalStat(StatType.Speed), 0.0001f);
                    ActionAllUnit((unit) => {
                        unit.ActionGauge += gaugeFillingTime * unit.GetFinalStat(StatType.Speed);
                    });
                    UnitOfTurn.ActionGauge = 0;

                    // ��ū�� �����Ѵ�
                    if (UnitOfTurn.CoOnBeginMyTurn != null) {
                        foreach (Func<Unit, IEnumerator> func in UnitOfTurn.CoOnBeginMyTurn.GetInvocationList())
                            yield return StartCoroutine(func(UnitOfTurn));
                    }
                    UnitOfTurn.CreateRandomToken(3);

                    RaiseSyncCount();
                    //UsefulMethod.ActionAll(clients, (client) => client.HasLastRpc = true);
                }

                // (����) �� ��� �� ��ū ������ ������ ���� ���
                TestBattlePage = "�� ��� �� ��ū ������ ������ ���� �����...";
                yield return new WaitUntil(() => { return 0 < SyncCount; });
                DownSyncCount();

                // (����) ������ �Ͻ��� �̺�Ʈ�� �����Ѵ�


                // �� ������ ���� ��� �׼� �����ϱ�
                if (UnitOfTurn.IsMine()) {
                    TestBattlePage = "�� ������ ��...";
                    ActionCoroutine = null;
                    TestBattlePage = "�׼� �ڷ�ƾ�� �����Ǳ⸦ ��ٸ��� ��...";
                    while (ActionCoroutine == null) {
                        yield return null;
                    }

                    TestBattlePage = "�׼� �ڷ�ƾ ������...";
                    yield return StartCoroutine(ActionCoroutine);
                    TestBattlePage = "�׼� �ڷ�ƾ ���ೡ";
                    if (UnitOfTurn.CoOnEndMyTurn != null) {
                        foreach (Func<Unit, IEnumerator> func in UnitOfTurn.CoOnEndMyTurn.GetInvocationList())
                            yield return StartCoroutine(func(UnitOfTurn));
                    }
                    UnitOfTurn = null;
                    ActionCoroutine = null;

                    RaiseSyncCount();
                    //UsefulMethod.ActionAll(clients, (client) => client.HasLastRpc = true);
                }

                // (����) ����ϱ�
                TestBattlePage = "�׼� �ڷ�ƾ ���� ���Ḧ ����ȭ��...";
                yield return new WaitUntil(() => { return 0 < SyncCount; });
                DownSyncCount();
                TestBattlePage = "�׼� �ڷ�ƾ ���� ���Ḧ ����ȭ �Ϸ�";

                // (����) ���̺� Ŭ����/���� Ȯ��


                yield return null;
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

    [PunRPC] private void IsAllLoadedRPC(bool result) {
        isAllLoaded = result;
    }
    public bool IsAllLoaded {
        get { return isAllLoaded; }
        set { photonView.RPC("IsAllLoadedRPC", RpcTarget.All, value); }
    }

    [PunRPC] private void AddPartyRPC(int partyViewId, TeamType teamType) {
        Party party = PhotonView.Find(partyViewId).GetComponent<Party>();
        Parties[(int)teamType].Add(party);
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
            float time = (Unit.MaxActionGauge - unit.ActionGauge) / MathF.Max(unitSpeed, 0.0001f); // �������� �� ä��µ� �ɸ��� �ð�, 0���� ������ ���� ����
            if(time < minTime || (time == minTime && result.GetFinalStat(StatType.Speed) > unit.GetFinalStat(StatType.Speed))) {
                result = unit;
                minTime = time;
            }
        });

        return result;
    }


}
