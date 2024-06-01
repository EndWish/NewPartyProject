using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using System.IO;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;

public enum UnitType : int
{
    None, Garuda,
}

public enum StatType
{

    //체력, 속도, 공격력, 스택 공격력, 스킬 공격력, 방어 관통력, 치명타 확률, 치명타 배율, 방어력, 실드, 스택 실드, 명중, 회피, 치유력, 토큰 지분(3종류)
    Hpm, Speed, Str, StackStr, SkillStr, DefPen, CriCha, CriMul, Def, Shield, StackShield, Acc, Avoid, Healing,
    AtkTokenWeight, SkillTokenWeight, ShieldTokenWeight,

    Num
}

public enum StatForm
{
    Base, Final, EquipmentAdd, EquipmentMul, AbnormalAdd, AbnormalMul,
    
    Num
}

public class Unit : MonoBehaviourPun, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    // 서브 클래스 ////////////////////////////////////////////////////////////
    [Serializable]
    public class Data {
        public UnitType Type = UnitType.None;
        public int GrowthLevel { get; set; } = 0;

        public Data() { }
        public Data(UnitType type) {
            Type = type;
        }
        public Data(UnitType type, int growthLevel) : this(type) {
            GrowthLevel = growthLevel;
        }

        // Photon 직렬화
        public static byte[] Serialize(object customObject) {
            Data data = (Data)customObject;

            // 스트림에 필요한 메모리 사이즈(Byte)
            MemoryStream ms = new MemoryStream(sizeof(UnitType) + sizeof(int));
            // 각 변수들을 Byte 형식으로 변환, 마지막은 개별 사이즈
            ms.Write(BitConverter.GetBytes((int)data.Type), 0, sizeof(UnitType));
            ms.Write(BitConverter.GetBytes(data.GrowthLevel), 0, sizeof(int));

            // 만들어진 스트림을 배열 형식으로 반환
            return ms.ToArray();
        }

        // Photon 역직렬화
        public static object Deserialize(byte[] bytes) {
            Data data = new Data();
            // 바이트 배열을 필요한 만큼 자르고, 원하는 자료형으로 변환
            data.Type = (UnitType)BitConverter.ToInt32(bytes, 0);
            data.GrowthLevel = BitConverter.ToInt32(bytes, sizeof(UnitType));
            return data;
        }

    }

    // 공유 정보 //////////////////////////////////////////////////////////////
    static public float MaxActionGauge = 100f;

    // 연결 정보 //////////////////////////////////////////////////////////////
    [SerializeField] protected DamageText damageTextPrefab;

    [SerializeField] protected Token tokenPrefab;
    [SerializeField] protected Transform tokensParent;
    [SerializeField] protected TextMeshProUGUI growthLevelText;

    [SerializeField] protected Image flagBorderImage;
    public Image profileImage;

    [SerializeField] protected RectTransform actionGaugeFill;
    [SerializeField] protected RectTransform hpGaugeFill;
    [SerializeField] protected RectTransform hpGaugeBgFill;
    [SerializeField] protected RectTransform barrierGaugeFill;

    [SerializeField] protected Transform skillsParent;

    // 개인 정보 //////////////////////////////////////////////////////////////
    // 이름
    public string Name;

    // 저장에 필요한 변수
    public Data MyData;

    // 상태를 나타내는 변수
    private bool isDie = false;
    private Unit murderer = null;

    // 능력치 관련 변수
    [SerializeField, EnumNamedArrayAttribute(typeof(StatType))] protected float[] initStats = new float[(int)StatType.Num];
    protected float[,] stats = new float[(int)StatForm.Num, (int)StatType.Num];
    public UnityAction<Unit>[] OnUpdateFinalStat = new UnityAction<Unit>[(int)StatType.Num];

    protected float hp;
    public UnityAction<Unit, Ref<float>> OnRecoverHp;   // 유닛, 회복량
    public UnityAction<Unit, float, float> OnRecoveredHp;   // 유닛, 회복량, 초과량

    protected float actionGauge = 0;

    // 토큰 관련 변수
    protected int maxTokens = 5;
    public List<Token> Tokens = new List<Token>();
    public UnityAction<Unit, Token> OnCreateToken;
    public UnityAction<Unit, Token> OnRemoveToken;

    // 파티 관련 변수
    public TeamType TeamType { get; set; } = TeamType.None;
    public Party MyParty { get; set; }

    // 배리어 관련 변수
    public List<Barrier> Barriers { get; protected set; } = new List<Barrier>();

    // 스킬 관련 변수
    public List<Skill> Skills { get; protected set; } = new List<Skill>();

    // 상태이상 관련 변수

    // 장비 관련 변수



    // 배틀페이지와 관련한 이벤트 변수
    public Func<Unit, IEnumerator> CoOnBeginMyTurn, CoOnEndMyTurn;

    // 전투 관련 코루틴 변수
    public Func<Unit, IEnumerator> CoOnDie;

    // 유니티 함수 ////////////////////////////////////////////////////////////
    protected void Awake() {
        growthLevelText.text = GetGrowthLevelStr();
        UpdateAllStat();
        
        actionGauge = 0;

        foreach (Skill skill in skillsParent.GetComponentsInChildren<Skill>(true)) {
            skill.Owner = this;
            Skills.Add(skill);
        }
    }

    protected void Start() {
        hp = GetFinalStat(StatType.Hpm);    //  growthLevel가 Awake에서는 적용되지 않아 Start에서 초기화한다.
    }

    protected void Update() {
        // 위치
        if (TeamType != TeamType.None) {
            Vector3 pos = Vector3.zero;

            int sign = 0;
            if (TeamType == TeamType.Player) sign = -1;
            else if (TeamType == TeamType.Enemy) sign = 1;

            int index = GetIndex();
            pos += new Vector3(1.5f * sign * index, 0, -1); // 갈수록 작아지도록 하고 싶다

            transform.localPosition = Vector3.MoveTowards(transform.localPosition, pos, 10f * Time.deltaTime);
        }

        // 게이지
        hpGaugeFill.localScale = new Vector3(Hp / GetFinalStat(StatType.Hpm), 1, 1);
        hpGaugeBgFill.localScale = new Vector3(MathF.Max(hpGaugeBgFill.localScale.x - Time.deltaTime * 2, hpGaugeFill.localScale.x), 1, 1);
        actionGaugeFill.localScale = new Vector3(ActionGauge / MaxActionGauge, 1, 1);
        float sumBarrierAmount = 0;
        UsefulMethod.ActionAll(Barriers, (barrier) => { sumBarrierAmount += barrier.Amount; });
        barrierGaugeFill.localScale = new Vector3( Mathf.Min(1, sumBarrierAmount / GetFinalStat(StatType.Hpm)), 1, 1);

        // 크기
        if (HasTurn()) {
            transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.one * 1.3f, Time.deltaTime * 2f);
        } else {
            transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.one, Time.deltaTime * 2f);
        }

        // 깃발 테두기 색상
        if (IsClicked()) { flagBorderImage.color = new Color(1, 1, 0); } else if (IsOverOnMouse()) { flagBorderImage.color = new Color(1, 0.7f, 0); } else { flagBorderImage.color = new Color(1, 1, 1); }

    }

    // 함수 ///////////////////////////////////////////////////////////////////
    public int GrowthLevel {
        get { return MyData.GrowthLevel; }
        set {
            MyData.GrowthLevel = value;
            growthLevelText.text = GetGrowthLevelStr();
            UpdateBaseStat(true);
        }
    }

    public float ActionGauge {
        get { return actionGauge; }
        set {
            photonView.RPC("ActionGaugeRPC", RpcTarget.All, value);
        }
    }
    [PunRPC] protected void ActionGaugeRPC(float value) {
        actionGauge = value;
    }

    // 클릭 관련 함수
    public void OnPointerClick(PointerEventData eventData) {
        if (BattleSelectable.IsRunning)
            return;

        BattleManager battleManager = BattleManager.Instance;
        if (battleManager.UnitClicked == this)
            battleManager.UnitClicked = null;
        else
            battleManager.UnitClicked = this;
    }
    public void OnPointerEnter(PointerEventData eventData) {
        BattleManager battleManager = BattleManager.Instance;
        battleManager.UnitOnMouse = this;
    }
    public void OnPointerExit(PointerEventData eventData) {
        BattleManager battleManager = BattleManager.Instance;
        battleManager.UnitOnMouse = null;
    }

    // 능력치 관련 함수
    public float GetFinalStat(StatType type) {
        return stats[(int)StatForm.Final, (int)type];
    }

    public void UpdateFinalStat(StatType type) {
        int index = (int)type;
        stats[(int)StatForm.Final, index] =
            (stats[(int)StatForm.Base, index]
            + stats[(int)StatForm.EquipmentAdd, index]
            + stats[(int)StatForm.AbnormalAdd, index])
            * stats[(int)StatForm.EquipmentMul, index]
            * stats[(int)StatForm.AbnormalMul, index];

        OnUpdateFinalStat[index]?.Invoke(this);
    }
    public void UpdateFinalStat() {
        for (StatType type = 0; type < StatType.Num; ++type) {
            UpdateFinalStat(type);
        }
    }

    public void UpdateBaseStat(bool updateFinalStat) {
        // 기본 능력치 계산
        for (StatType type = 0; type < StatType.Num; ++type) {
            stats[(int)StatForm.Base, (int)type] = initStats[(int)type] * (1f + 0.01f * GrowthLevel);
        }

        // 최종 능력치에 적용
        if (updateFinalStat)
            UpdateFinalStat();
    }
    public void UpdateEquipmentStat(bool updateFinalStat) {
        // 능력치 초기화
        for (StatType type = 0; type < StatType.Num; ++type) {
            stats[(int)StatForm.EquipmentAdd, (int)type] = 0;
            stats[(int)StatForm.EquipmentMul, (int)type] = 1f;
        }

        // [추가] 장비의 능력치들 적용

        // 최종 능력치에 적용
        if (updateFinalStat)
            UpdateFinalStat();
    }
    public void UpdateAbnormalStat(bool updateFinalStat) {
        // 능력치 초기화
        for (StatType type = 0; type < StatType.Num; ++type) {
            stats[(int)StatForm.AbnormalAdd, (int)type] = 0;
            stats[(int)StatForm.AbnormalMul, (int)type] = 1f;
        }

        // [추가] 상태이상의 능력치들 적용

        // 최종 능력치에 적용
        if (updateFinalStat)
            UpdateFinalStat();
    }

    public void UpdateAllStat() {
        UpdateBaseStat(false);
        UpdateEquipmentStat(false);
        UpdateAbnormalStat(false);
        UpdateFinalStat();
    }

    // 토큰 관련 함수
    [PunRPC] protected void CreateTokenRPC(TokenType type) {
        Token newToken = Instantiate(tokenPrefab, tokensParent);
        Tokens.Add(newToken);
        newToken.Owner = this;
        newToken.Type = type;
    }
    public void CreateRandomToken() {
        if (Tokens.Count >= maxTokens)  // 최대개수를 넘어서 얻을 수 없다.
            return;

        float sum = GetFinalStat(StatType.AtkTokenWeight) + GetFinalStat(StatType.SkillTokenWeight) + GetFinalStat(StatType.ShieldTokenWeight);
        float random = UnityEngine.Random.Range(0f, sum);

        TokenType type = TokenType.None;
        if (random <= GetFinalStat(StatType.AtkTokenWeight))
            type = TokenType.Atk;
        else if (random <= GetFinalStat(StatType.AtkTokenWeight) + GetFinalStat(StatType.SkillTokenWeight))
            type = TokenType.Skill;
        else
            type = TokenType.Shield;

        photonView.RPC("CreateTokenRPC", RpcTarget.All, type);

        OnCreateToken?.Invoke(this, Tokens[Tokens.Count - 1]);
    }
    public void CreateRandomToken(int num) {
        for (int i = 0; i < num; ++i) {
            CreateRandomToken();
        }
    }
    public void CreateToken(TokenType type) {
        if (Tokens.Count >= maxTokens)  // 최대개수를 넘어서 얻을 수 없다.
            return;

        photonView.RPC("CreateTokenRPC", RpcTarget.All, type);

        OnCreateToken?.Invoke(this, Tokens[Tokens.Count - 1]);
    }
    public void CreateToken(TokenType type, int num) {
        for (int i = 0; i < num; ++i) {
            CreateToken(type);
        }
    }

    [PunRPC] protected void RemoveTokenRPC(int index) {
        Token token = Tokens[index];
        Tokens.RemoveAt(index);
        Destroy(token.gameObject);
    }
    public void RemoveToken(Token token) {
        OnRemoveToken?.Invoke(this, token);

        int index = Tokens.IndexOf(token);
        photonView.RPC("RemoveTokenRPC", RpcTarget.All, index);
    }
    public void RemoveSelectedToken() {
        List<Token> selectedTokens = Tokens.FindAll(token => token.IsSelected);
        foreach (Token token in selectedTokens)
            RemoveToken(token);
    }

    // 행동 관련 함수
    public IEnumerator CoPassAction() {
        yield return new WaitForSeconds(0.3f);
    }
    public IEnumerator CoDiscardAction() {
        RemoveSelectedToken();
        yield return new WaitForSeconds(0.3f);
    }
    public IEnumerator CoBasicAtk() {
        // 토큰을 개수를 세고 삭제한다
        int tokenStack = Tokens.FindAll(token => token.IsSelected).Count;
        RemoveSelectedToken();

        // 공격을 생성한다
        Unit target = BattleSelectable.Units[0];
        BasicAttack attack = PhotonNetwork.Instantiate(GameManager.GetAttackPrefabPath("BasicAttack"),
            target.transform.position, Quaternion.identity)
            .GetComponent<BasicAttack>();
        attack.Init(this, target, tokenStack);

        yield return StartCoroutine(attack.Animate());
    }
    public void UseBasicAtk() {
        BattleManager.Instance.ActionCoroutine = CoBasicAtk();
    }
    public bool BasicAtkSelectionPred(Unit unit) {
        return this.TeamType != unit.TeamType;
    }
    public IEnumerator CoBasicBarrier() {
        // 토큰을 개수를 세고 삭제한다
        int tokenStack = Tokens.FindAll(token => token.IsSelected).Count;
        RemoveSelectedToken();

        BasicBarrier barrier = PhotonNetwork.Instantiate(GameManager.GetBarrierPrefabPath("BasicBarrier"), 
            transform.position, Quaternion.identity)
            .GetComponent<BasicBarrier>();
        barrier.Amount = GetFinalStat(StatType.Shield) * (1f + GetFinalStat(StatType.StackShield) * (tokenStack - 1));
        barrier.Caster = this;

        AddBarrier(barrier);

        yield break;
    }

    public bool HasTurn() {
        return BattleManager.Instance.UnitOfTurn == this;
    }
    public bool IsClicked() {
        return BattleManager.Instance.UnitClicked == this;
    }
    public bool IsOverOnMouse() {
        return BattleManager.Instance.UnitOnMouse == this;
    }

    // 유닛의 소유상태를 반환하는 함수
    public bool IsMine() {
        return (MyParty.TeamType == TeamType.Player && photonView.IsMine) || (MyParty.TeamType == TeamType.Enemy && PhotonNetwork.IsMasterClient);
    }
    public bool IsAlly() {
        return MyParty.TeamType == TeamType.Player && !photonView.IsMine;
    }

    // Data 관련 함수
    [PunRPC] protected void ApplyDataRPC(Unit.Data data) {
        this.MyData = data;
        UpdateAllStat();
        growthLevelText.text = GetGrowthLevelStr();
        // 프로필 이미지도 적용
    }
    public void ApplyData(Unit.Data data) {
        ApplyDataRPC(data);
        if (photonView.IsMine) {    // 동기화 하지 않은 오브젝트일 경우 실행하지 않는다.
            photonView.RPC("ApplyDataRPC", RpcTarget.Others, data);
        }
    }

    // 전투관련 함수
    public bool IsDie {
        get { return isDie; }
        set { photonView.RPC("DieRPC", RpcTarget.All, value); }
    }
    [PunRPC] private void DieRPC(bool value) {
        isDie = value;
    }

    public Unit Murderer { 
        get { return murderer; }
        set {
            murderer = value;
            photonView.RPC("MurdererRPC", RpcTarget.Others, value.photonView.ViewID); 
        }
    }
    [PunRPC] private void MurdererRPC(int viewId) {
        murderer = viewId == -1 ? null : PhotonView.Find(viewId).GetComponent<Unit>();
    }

    public float Hp {  
        get { return hp; } 
        set { photonView.RPC("HpRPC", RpcTarget.All, value); } 
    }
    [PunRPC] private void HpRPC(float value) {
        hp = value;
    }

    public float RemainHp { get { return GetFinalStat(StatType.Hpm) - hp; } }

    public void RecoverHp(float baseAmount) {
        Ref<float> amount = new Ref<float>(baseAmount);

        // 회복할때 (회복수치를 조정가능)
        OnRecoverHp?.Invoke(this, amount);

        // 회복 초과량 확인하기
        float overAmount = 0;
        float lostHp = GetFinalStat(StatType.Hpm) - Hp;
        if(lostHp < amount.Value) {
            overAmount = amount.Value - lostHp;
            amount.Value = lostHp;
        }

        // Hp에 적용
        Hp += amount.Value;

        // 회복되었을 때 
        OnRecoveredHp?.Invoke(this, amount.Value, overAmount);
    }

    public IEnumerator TakeDmg(float dmg, DamageCalculator damageCalculator) {
        if (IsDie)
            yield break;

        // 배리어가 있으면 배리어부터 데미지를 입는다.
        while(0 < Barriers.Count && 0 < dmg) {
            Barrier barrier = Barriers[Barriers.Count - 1];

            float applyDmg = Mathf.Min(barrier.Amount, dmg);

            CreateDmgText((int)applyDmg, new Vector3(0.49f, 0.78f, 0.94f));
            yield return StartCoroutine(barrier.TakeDmg(applyDmg));
            dmg -= applyDmg;
        }

        // 배리어를 까고 남은 데미지를 hp에 적용한다
        if(0 < dmg) {
            Hp -= dmg;
            CreateDmgText((int)dmg, new Vector3(1, 0.92f, 0.016f));

            if (Hp <= 0) {
                IsDie = true;
                Murderer = damageCalculator.Attacker;

                if(CoOnDie != null)
                    foreach (Func<Unit, IEnumerator> func in CoOnDie.GetInvocationList())
                        yield return StartCoroutine(func(this));
            }
        }

    }
    [PunRPC] private void CreateDmgTextRPC(int dmg, Vector3 color) {
        DamageText damageText = Instantiate(damageTextPrefab, transform.position, Quaternion.identity);
        damageText.SetFormat(dmg, color);
    }
    public void CreateDmgText(int dmg, Vector3 color) {
        photonView.RPC("CreateDmgTextRPC", RpcTarget.All, dmg, color);
    }

    // 배리어 추가/삭제
    [PunRPC] protected void AddBarrierRPC(int viewId) {
        if (viewId == -1)
            return;

        Barrier barrier = PhotonView.Find(viewId).GetComponent<Barrier>();

        Barriers.Add(barrier);
        Barriers.Sort((a, b) => {
            float diff = b.GetPriority() - a.GetPriority();
            return diff < 0 ? -1 : 1;
            });
    }
    public void AddBarrier(Barrier barrier) {
        barrier.Target = this;
        photonView.RPC("AddBarrierRPC", RpcTarget.All, barrier.photonView.ViewID);
    }
    [PunRPC] protected void RemoveBarrierRPC(int viewId) {
        if (viewId == -1)
            return;

        Barrier barrier =PhotonView.Find(viewId).GetComponent<Barrier>();
        Barriers.Remove(barrier);
    }
    public void RemoveBarrier(Barrier barrier) {
        photonView.RPC("RemoveBarrierRPC", RpcTarget.All, barrier.photonView.ViewID);
    }

    // 파티 관련 함수
    public void OnSetTeam(TeamType type) {
        TeamType = type;
        if(TeamType == TeamType.Player) {
            profileImage.transform.localScale = new Vector3(1, 1, 1);
        } else if(TeamType == TeamType.Enemy) {
            profileImage.transform.localScale = new Vector3(-1, 1, 1);
        }
    }
    public int GetIndex() {
        return MyParty.Units.IndexOf(this);
    }

    // 기타
    public string GetGrowthLevelStr() {
        return 0 <= GrowthLevel ? ("+" + GrowthLevel.ToString()) : GrowthLevel.ToString();
    }

}
