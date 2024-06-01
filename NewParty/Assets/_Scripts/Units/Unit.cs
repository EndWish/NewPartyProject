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

    //ü��, �ӵ�, ���ݷ�, ���� ���ݷ�, ��ų ���ݷ�, ��� �����, ġ��Ÿ Ȯ��, ġ��Ÿ ����, ����, �ǵ�, ���� �ǵ�, ����, ȸ��, ġ����, ��ū ����(3����)
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
    // ���� Ŭ���� ////////////////////////////////////////////////////////////
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

        // Photon ����ȭ
        public static byte[] Serialize(object customObject) {
            Data data = (Data)customObject;

            // ��Ʈ���� �ʿ��� �޸� ������(Byte)
            MemoryStream ms = new MemoryStream(sizeof(UnitType) + sizeof(int));
            // �� �������� Byte �������� ��ȯ, �������� ���� ������
            ms.Write(BitConverter.GetBytes((int)data.Type), 0, sizeof(UnitType));
            ms.Write(BitConverter.GetBytes(data.GrowthLevel), 0, sizeof(int));

            // ������� ��Ʈ���� �迭 �������� ��ȯ
            return ms.ToArray();
        }

        // Photon ������ȭ
        public static object Deserialize(byte[] bytes) {
            Data data = new Data();
            // ����Ʈ �迭�� �ʿ��� ��ŭ �ڸ���, ���ϴ� �ڷ������� ��ȯ
            data.Type = (UnitType)BitConverter.ToInt32(bytes, 0);
            data.GrowthLevel = BitConverter.ToInt32(bytes, sizeof(UnitType));
            return data;
        }

    }

    // ���� ���� //////////////////////////////////////////////////////////////
    static public float MaxActionGauge = 100f;

    // ���� ���� //////////////////////////////////////////////////////////////
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

    // ���� ���� //////////////////////////////////////////////////////////////
    // �̸�
    public string Name;

    // ���忡 �ʿ��� ����
    public Data MyData;

    // ���¸� ��Ÿ���� ����
    private bool isDie = false;
    private Unit murderer = null;

    // �ɷ�ġ ���� ����
    [SerializeField, EnumNamedArrayAttribute(typeof(StatType))] protected float[] initStats = new float[(int)StatType.Num];
    protected float[,] stats = new float[(int)StatForm.Num, (int)StatType.Num];
    public UnityAction<Unit>[] OnUpdateFinalStat = new UnityAction<Unit>[(int)StatType.Num];

    protected float hp;
    public UnityAction<Unit, Ref<float>> OnRecoverHp;   // ����, ȸ����
    public UnityAction<Unit, float, float> OnRecoveredHp;   // ����, ȸ����, �ʰ���

    protected float actionGauge = 0;

    // ��ū ���� ����
    protected int maxTokens = 5;
    public List<Token> Tokens = new List<Token>();
    public UnityAction<Unit, Token> OnCreateToken;
    public UnityAction<Unit, Token> OnRemoveToken;

    // ��Ƽ ���� ����
    public TeamType TeamType { get; set; } = TeamType.None;
    public Party MyParty { get; set; }

    // �踮�� ���� ����
    public List<Barrier> Barriers { get; protected set; } = new List<Barrier>();

    // ��ų ���� ����
    public List<Skill> Skills { get; protected set; } = new List<Skill>();

    // �����̻� ���� ����

    // ��� ���� ����



    // ��Ʋ�������� ������ �̺�Ʈ ����
    public Func<Unit, IEnumerator> CoOnBeginMyTurn, CoOnEndMyTurn;

    // ���� ���� �ڷ�ƾ ����
    public Func<Unit, IEnumerator> CoOnDie;

    // ����Ƽ �Լ� ////////////////////////////////////////////////////////////
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
        hp = GetFinalStat(StatType.Hpm);    //  growthLevel�� Awake������ ������� �ʾ� Start���� �ʱ�ȭ�Ѵ�.
    }

    protected void Update() {
        // ��ġ
        if (TeamType != TeamType.None) {
            Vector3 pos = Vector3.zero;

            int sign = 0;
            if (TeamType == TeamType.Player) sign = -1;
            else if (TeamType == TeamType.Enemy) sign = 1;

            int index = GetIndex();
            pos += new Vector3(1.5f * sign * index, 0, -1); // ������ �۾������� �ϰ� �ʹ�

            transform.localPosition = Vector3.MoveTowards(transform.localPosition, pos, 10f * Time.deltaTime);
        }

        // ������
        hpGaugeFill.localScale = new Vector3(Hp / GetFinalStat(StatType.Hpm), 1, 1);
        hpGaugeBgFill.localScale = new Vector3(MathF.Max(hpGaugeBgFill.localScale.x - Time.deltaTime * 2, hpGaugeFill.localScale.x), 1, 1);
        actionGaugeFill.localScale = new Vector3(ActionGauge / MaxActionGauge, 1, 1);
        float sumBarrierAmount = 0;
        UsefulMethod.ActionAll(Barriers, (barrier) => { sumBarrierAmount += barrier.Amount; });
        barrierGaugeFill.localScale = new Vector3( Mathf.Min(1, sumBarrierAmount / GetFinalStat(StatType.Hpm)), 1, 1);

        // ũ��
        if (HasTurn()) {
            transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.one * 1.3f, Time.deltaTime * 2f);
        } else {
            transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.one, Time.deltaTime * 2f);
        }

        // ��� �׵α� ����
        if (IsClicked()) { flagBorderImage.color = new Color(1, 1, 0); } else if (IsOverOnMouse()) { flagBorderImage.color = new Color(1, 0.7f, 0); } else { flagBorderImage.color = new Color(1, 1, 1); }

    }

    // �Լ� ///////////////////////////////////////////////////////////////////
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

    // Ŭ�� ���� �Լ�
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

    // �ɷ�ġ ���� �Լ�
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
        // �⺻ �ɷ�ġ ���
        for (StatType type = 0; type < StatType.Num; ++type) {
            stats[(int)StatForm.Base, (int)type] = initStats[(int)type] * (1f + 0.01f * GrowthLevel);
        }

        // ���� �ɷ�ġ�� ����
        if (updateFinalStat)
            UpdateFinalStat();
    }
    public void UpdateEquipmentStat(bool updateFinalStat) {
        // �ɷ�ġ �ʱ�ȭ
        for (StatType type = 0; type < StatType.Num; ++type) {
            stats[(int)StatForm.EquipmentAdd, (int)type] = 0;
            stats[(int)StatForm.EquipmentMul, (int)type] = 1f;
        }

        // [�߰�] ����� �ɷ�ġ�� ����

        // ���� �ɷ�ġ�� ����
        if (updateFinalStat)
            UpdateFinalStat();
    }
    public void UpdateAbnormalStat(bool updateFinalStat) {
        // �ɷ�ġ �ʱ�ȭ
        for (StatType type = 0; type < StatType.Num; ++type) {
            stats[(int)StatForm.AbnormalAdd, (int)type] = 0;
            stats[(int)StatForm.AbnormalMul, (int)type] = 1f;
        }

        // [�߰�] �����̻��� �ɷ�ġ�� ����

        // ���� �ɷ�ġ�� ����
        if (updateFinalStat)
            UpdateFinalStat();
    }

    public void UpdateAllStat() {
        UpdateBaseStat(false);
        UpdateEquipmentStat(false);
        UpdateAbnormalStat(false);
        UpdateFinalStat();
    }

    // ��ū ���� �Լ�
    [PunRPC] protected void CreateTokenRPC(TokenType type) {
        Token newToken = Instantiate(tokenPrefab, tokensParent);
        Tokens.Add(newToken);
        newToken.Owner = this;
        newToken.Type = type;
    }
    public void CreateRandomToken() {
        if (Tokens.Count >= maxTokens)  // �ִ밳���� �Ѿ ���� �� ����.
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
        if (Tokens.Count >= maxTokens)  // �ִ밳���� �Ѿ ���� �� ����.
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

    // �ൿ ���� �Լ�
    public IEnumerator CoPassAction() {
        yield return new WaitForSeconds(0.3f);
    }
    public IEnumerator CoDiscardAction() {
        RemoveSelectedToken();
        yield return new WaitForSeconds(0.3f);
    }
    public IEnumerator CoBasicAtk() {
        // ��ū�� ������ ���� �����Ѵ�
        int tokenStack = Tokens.FindAll(token => token.IsSelected).Count;
        RemoveSelectedToken();

        // ������ �����Ѵ�
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
        // ��ū�� ������ ���� �����Ѵ�
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

    // ������ �������¸� ��ȯ�ϴ� �Լ�
    public bool IsMine() {
        return (MyParty.TeamType == TeamType.Player && photonView.IsMine) || (MyParty.TeamType == TeamType.Enemy && PhotonNetwork.IsMasterClient);
    }
    public bool IsAlly() {
        return MyParty.TeamType == TeamType.Player && !photonView.IsMine;
    }

    // Data ���� �Լ�
    [PunRPC] protected void ApplyDataRPC(Unit.Data data) {
        this.MyData = data;
        UpdateAllStat();
        growthLevelText.text = GetGrowthLevelStr();
        // ������ �̹����� ����
    }
    public void ApplyData(Unit.Data data) {
        ApplyDataRPC(data);
        if (photonView.IsMine) {    // ����ȭ ���� ���� ������Ʈ�� ��� �������� �ʴ´�.
            photonView.RPC("ApplyDataRPC", RpcTarget.Others, data);
        }
    }

    // �������� �Լ�
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

        // ȸ���Ҷ� (ȸ����ġ�� ��������)
        OnRecoverHp?.Invoke(this, amount);

        // ȸ�� �ʰ��� Ȯ���ϱ�
        float overAmount = 0;
        float lostHp = GetFinalStat(StatType.Hpm) - Hp;
        if(lostHp < amount.Value) {
            overAmount = amount.Value - lostHp;
            amount.Value = lostHp;
        }

        // Hp�� ����
        Hp += amount.Value;

        // ȸ���Ǿ��� �� 
        OnRecoveredHp?.Invoke(this, amount.Value, overAmount);
    }

    public IEnumerator TakeDmg(float dmg, DamageCalculator damageCalculator) {
        if (IsDie)
            yield break;

        // �踮� ������ �踮����� �������� �Դ´�.
        while(0 < Barriers.Count && 0 < dmg) {
            Barrier barrier = Barriers[Barriers.Count - 1];

            float applyDmg = Mathf.Min(barrier.Amount, dmg);

            CreateDmgText((int)applyDmg, new Vector3(0.49f, 0.78f, 0.94f));
            yield return StartCoroutine(barrier.TakeDmg(applyDmg));
            dmg -= applyDmg;
        }

        // �踮� ��� ���� �������� hp�� �����Ѵ�
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

    // �踮�� �߰�/����
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

    // ��Ƽ ���� �Լ�
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

    // ��Ÿ
    public string GetGrowthLevelStr() {
        return 0 <= GrowthLevel ? ("+" + GrowthLevel.ToString()) : GrowthLevel.ToString();
    }

}
