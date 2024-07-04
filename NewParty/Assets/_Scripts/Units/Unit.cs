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
using JetBrains.Annotations;
using System.Linq;

public enum UnitType : int
{
    None, Garuda,GrayWolf, RedWolf, SilverManeWolf, BloodyWolf, HowlingWolf, AwlMosquito, DrillMosquito, TransfusionMosquito, InfectedMosquito,
}

public class Unit : MonoBehaviourPun, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    // ���� Ŭ���� ////////////////////////////////////////////////////////////
    [Serializable]
    public class Data {
        public UnitType Type = UnitType.None;
        public int GrowthLevel { get; set; } = 0;

        public Data() { }
        public Data(UnitType type) : this() {
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
    static public int GrowthLevelWhenSummoned = -10;
    static public int NumSoulFragmentRequiredForSummon = 100;

    // ���� ���� //////////////////////////////////////////////////////////////
    [SerializeField] protected DamageText damageTextPrefab;
    [SerializeField] protected GameObject HealingFxPrefab;
    [SerializeField] protected Token tokenPrefab;

    [SerializeField] protected UnitCanvas unitCanvas;

    protected Image profileImage;
    public Image ProfileImage { get { return profileImage; } }

    protected Transform tokensParent;

    protected Transform statusEffectIconParent;
    public Transform StatusEffectIconParent { get { return statusEffectIconParent; } }

    public BasicAttackSkill BasicAtkSkill;
    public BasicBarrierSkill BasicBarrierSkill;
    [SerializeField] protected Transform skillsParent;

    // ���� ���� //////////////////////////////////////////////////////////////
    // �̸�
    public string Name;

    // ��ȥ���� ��ġ
    public int SoulFragmentValueAsDust;

    // ���忡 �ʿ��� ����
    public Data MyData;

    // ���¸� ��Ÿ���� ����
    private bool isDie = false;
    private Unit murderer = null;

    // �ɷ�ġ ���� ����
    [SerializeField, EnumNamedArrayAttribute(typeof(StatType))] protected float[] initStats = new float[(int)StatType.Num];
    public float[,] Stats { get; set; } = new float[(int)StatForm.Num, (int)StatType.Num];
    public UnityAction<Unit>[] OnUpdateFinalStat = new UnityAction<Unit>[(int)StatType.Num];

    protected float hp;
    public UnityAction<Unit, Ref<float>> OnRecoverHp;   // ����, ȸ����
    public UnityAction<Unit, float, float> OnRecoveredHp;   // ����, ȸ����, �ʰ���

    protected float actionGauge = 0;

    // �±� ���� ����
    public List<Tag> InitTags;
    public Tags Tags { get; set; } = new Tags();

    // ��ū ���� ����
    public int MaxTokens { get; set; } = 5;
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
    public List<StatusEffect> StatusEffects { get; protected set; } = new List<StatusEffect>();

    // ��� ���� ����

    // ��Ʋ�������� ������ �̺�Ʈ ����
    public Func<IEnumerator> CoOnBeginMyTurn, CoOnEndMyTurn;
    public Func<IEnumerator> CoOnBeginTick;
    public Func<IEnumerator> CoOnBeginWave, CoOnEndWave;

    // ���� ���� �ڷ�ƾ ����
    public Action<HitCalculator> OnBeforeCalculateHit;
    public Action<DamageCalculator> OnBeforeCalculateDmg, OnAfterCalculateDmg;
    public Func<IEnumerator> CoOnAvoid, CoOnDie;
    public Func<Unit, IEnumerator> CoOnKill; // �Ű�����(���� ���)
    public Func<Unit, Attack, IEnumerator> CoOnHit; // �Ű�����(������ ���, ����)
    public Func<Unit, DamageCalculator, IEnumerator> CoOnHitDmg; // �Ű�����(���� ���, DamageCalculator)
    public Func<Unit, float, float, IEnumerator> CoOnHitHp; // �Ű�����(���� ���, hp�� �� ����, �ʰ� ����)


    // ����Ƽ �Լ� ////////////////////////////////////////////////////////////
    protected void Awake() {
        if (InitTags != null)
            Tags.AddTag(InitTags);
        UpdateAllStat();
        
        actionGauge = 0;

        BasicAtkSkill.Owner = this;
        BasicBarrierSkill.Owner = this;

        foreach (Skill skill in skillsParent.GetComponentsInChildren<Skill>(true)) {
            skill.Owner = this;
            Skills.Add(skill);
        }

        profileImage = unitCanvas.transform.Find("Profile").GetComponent<Image>();
        tokensParent = unitCanvas.transform.Find("TokensParent").GetComponent<Transform>();
        statusEffectIconParent = unitCanvas.transform.Find("StatusEffectIconParent").GetComponent<Transform>();
    }

    protected void Start() {
        hp = GetFinalStat(StatType.Hpm);    //  growthLevel�� Awake������ ������� �ʾ� Start���� �ʱ�ȭ�Ѵ�.
        CoOnAvoid += CoCreateAvoidText;
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

        // ũ��
        if (HasTurn()) {
            transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.one * 1.3f, Time.deltaTime * 2f);
        } else {
            transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.one, Time.deltaTime * 2f);
        }

    }

    // �Լ� ///////////////////////////////////////////////////////////////////
    public int GrowthLevel {
        get { return MyData.GrowthLevel; }
        set {
            MyData.GrowthLevel = value;
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
        return Mathf.Max(StatClamp.MinStats[(int)type], Stats[(int)StatForm.Final, (int)type]);
    }

    public void UpdateFinalStat(StatType type) {
        int index = (int)type;
        Stats[(int)StatForm.Final, index] =
            (Stats[(int)StatForm.Base, index]
            + Stats[(int)StatForm.EquipmentAdd, index]
            + Stats[(int)StatForm.AbnormalAdd, index])
            * Stats[(int)StatForm.EquipmentMul, index]
            * Stats[(int)StatForm.AbnormalMul, index];

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
            Stats[(int)StatForm.Base, (int)type] = initStats[(int)type] * (1f + 0.01f * GrowthLevel);
        }

        // ���� �ɷ�ġ�� ����
        if (updateFinalStat)
            UpdateFinalStat();
    }
    public void UpdateEquipmentStat(bool updateFinalStat) {
        // �ɷ�ġ �ʱ�ȭ
        for (StatType type = 0; type < StatType.Num; ++type) {
            Stats[(int)StatForm.EquipmentAdd, (int)type] = 0;
            Stats[(int)StatForm.EquipmentMul, (int)type] = 1f;
        }

        // [�߰�] ����� �ɷ�ġ�� ����

        // ���� �ɷ�ġ�� ����
        if (updateFinalStat)
            UpdateFinalStat();
    }
    public void UpdateAbnormalStat(bool updateFinalStat) {
        // �ɷ�ġ �ʱ�ȭ
        for (StatType type = 0; type < StatType.Num; ++type) {
            Stats[(int)StatForm.AbnormalAdd, (int)type] = 0;
            Stats[(int)StatForm.AbnormalMul, (int)type] = 1f;
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

    // �����̻� ���� �Լ�
    [PunRPC] protected void AddStatusEffectRPC(int viewId) {
        if (viewId == -1) return;

        StatusEffect statusEffect = PhotonView.Find(viewId).GetComponent<StatusEffect>();
        StatusEffects.Add(statusEffect);
    }
    public void AddStatusEffect(StatusEffect statusEffect) {
        statusEffect.Target = this;
        photonView.RPC("AddStatusEffectRPC", RpcTarget.All, statusEffect.photonView.ViewID);
    }
    [PunRPC] protected void RemoveStatusEffectRPC(int viewId) {
        if (viewId == -1) return;

        StatusEffect statusEffect = PhotonView.Find(viewId).GetComponent<StatusEffect>();
        StatusEffects.Remove(statusEffect);
    }
    public void RemoveStatusEffect(StatusEffect statusEffect) {
        photonView.RPC("RemoveStatusEffectRPC", RpcTarget.All, statusEffect.photonView.ViewID);
        statusEffect.Target = null;
    }
    public void RemoveRandomStatusEffect(Predicate<StatusEffect> pred) {
        List<StatusEffect> targets = StatusEffects.FindAll(pred);
        if(targets.Count == 0) return;

        RemoveStatusEffect(targets.PickRandom());
    }

    public void ClearAllStatusEffect() {
        while(0 < StatusEffects.Count) {
            StatusEffects.Last().Destroy();
        }
    }

    // ��ū ���� �Լ�
    [PunRPC] protected void CreateTokenRPC(TokenType type) {
        Token newToken = Instantiate(tokenPrefab, tokensParent);
        Tokens.Add(newToken);
        newToken.Owner = this;
        newToken.Type = type;
    }
    public void CreateRandomToken() {
        if (Tokens.Count >= MaxTokens)  // �ִ밳���� �Ѿ ���� �� ����.
            return;

        float sum = GetFinalStat(StatType.AtkTokenWeight) + GetFinalStat(StatType.SkillTokenWeight) + GetFinalStat(StatType.ShieldTokenWeight);
        float random = UnityEngine.Random.Range(0f, sum);

        TokenType type = TokenType.None;
        if (random <= GetFinalStat(StatType.AtkTokenWeight))
            type = TokenType.Atk;
        else if (random <= GetFinalStat(StatType.AtkTokenWeight) + GetFinalStat(StatType.SkillTokenWeight))
            type = TokenType.Skill;
        else
            type = TokenType.Barrier;

        photonView.RPC("CreateTokenRPC", RpcTarget.All, type);

        OnCreateToken?.Invoke(this, Tokens[Tokens.Count - 1]);
    }
    public void CreateRandomToken(int num) {
        for (int i = 0; i < num; ++i) {
            CreateRandomToken();
        }
    }
    public void CreateToken(TokenType type) {
        if (Tokens.Count >= MaxTokens)  // �ִ밳���� �Ѿ ���� �� ����.
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
    public void RemoveRandomToken() {
        if (Tokens.Count == 0) return;
        RemoveToken(Tokens.PickRandom());
    }

    [PunRPC] protected void ClearAllTokenRPC() {
        foreach(var token in Tokens)
            Destroy(token.gameObject);
        Tokens.Clear();
    }
    public void ClearAllToken() {
        photonView.RPC("ClearAllTokenRPC", RpcTarget.All);
    }

    // �ൿ ���� �Լ�
    public IEnumerator CoPassAction() {
        yield return new WaitForSeconds(0.3f);
    }
    public IEnumerator CoDiscardAction() {
        RemoveSelectedToken();
        yield return new WaitForSeconds(0.3f);
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
        return MyParty.TeamType == TeamType.Player && photonView.IsMine;
    }
    public bool IsAlly() {
        return MyParty.TeamType == TeamType.Player && !photonView.IsMine;
    }
    public bool IsEnemy() {
        return MyParty.TeamType == TeamType.Enemy;
    }

    // Data ���� �Լ�
    [PunRPC] protected void ApplyDataRPC(Unit.Data data) {
        this.MyData = data;
        UpdateAllStat();
        // ������ �̹����� ����
    }
    public void ApplyData(Unit.Data data) {
        ApplyDataRPC(data);
        if (photonView.IsMine) {    // ����ȭ ���� ���� ������Ʈ�� ��� �������� �ʴ´�.
            photonView.RPC("ApplyDataRPC", RpcTarget.Others, data);
        }
    }

    // ������/ȸ�� ���� �Լ�
    [PunRPC] protected void CreateHealingFxRPC() {
        Instantiate(HealingFxPrefab, transform.position, Quaternion.identity, transform);
    }
    protected void CreateHealingFx() {
        photonView.RPC("CreateHealingFxRPC", RpcTarget.All);
    }
    public void RecoverHp(float baseAmount) {
        Ref<float> amount = new Ref<float>(baseAmount * GetFinalStat(StatType.Healing));

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
        CreateHealingFx();
        CreateDmgText(amount.Value, new Vector3(0, 1, 0));

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

            CreateDmgText(applyDmg, new Vector3(0.49f, 0.78f, 0.94f), damageCalculator.CriStack);
            yield return StartCoroutine(barrier.TakeDmg(applyDmg));
            dmg -= applyDmg;
        }

        // �踮� ��� ���� �������� hp�� �����Ѵ�
        if(0 < dmg) {
            float applyDmg = Mathf.Min(dmg, Hp);
            float overDmg = Mathf.Max(0, dmg - applyDmg);

            Hp -= applyDmg;
            CreateDmgText(dmg, new Vector3(1, 0.92f, 0.016f), damageCalculator.CriStack);

            // �������� CoOnHitDamage �̺�Ʈ�� �����Ѵ�
            yield return StartCoroutine(GameManager.CoInvoke(damageCalculator.Attacker.CoOnHitHp, this, applyDmg, overDmg));

            if (Hp <= 0) {
                IsDie = true;
                Murderer = damageCalculator.Attacker;

                yield return StartCoroutine(GameManager.CoInvoke(Murderer.CoOnKill, this));
                yield return StartCoroutine(GameManager.CoInvoke(CoOnDie));
            }
        }

    }

    // ������ �ؽ��� ����
    [PunRPC] private void CreateDmgTextRPC(float dmg, Vector3 color, int criStack) {
        DamageText damageText = Instantiate(damageTextPrefab, transform.position, Quaternion.identity);
        damageText.SetFormat(dmg, color, criStack);
    }
    public void CreateDmgText(float dmg, Vector3 color, int criStack = 0) {
        photonView.RPC("CreateDmgTextRPC", RpcTarget.All, dmg, color, criStack);
    }

    [PunRPC] private void CreateDmgTextRPC(DamageText.Type type) {
        DamageText damageText = Instantiate(damageTextPrefab, transform.position, Quaternion.identity);
        damageText.SetFormat(type);
    }
    public void CreateDmgText(DamageText.Type type) {
        photonView.RPC("CreateDmgTextRPC", RpcTarget.All, type);
    }
    public IEnumerator CoCreateAvoidText() {
        CreateDmgText(DamageText.Type.Avoid);
        yield break;
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
        barrier.Target = null;
    }

    public void ClearAllBarrier() {
        while (0 < Barriers.Count) {
            Barriers.Last().Destroy();
        }
    }

    // ��Ƽ ���� �Լ�
    public void OnSetTeam(TeamType type) {
        TeamType = type;
        if(TeamType == TeamType.Player) {
            ProfileImage.transform.localScale = new Vector3(1, 1, 1);
        } else if(TeamType == TeamType.Enemy) {
            ProfileImage.transform.localScale = new Vector3(-1, 1, 1);
        }
    }
    public int GetIndex() {
        return MyParty.Units.IndexOf(this);
    }

    // ����
    [PunRPC] private void DestroyRPC() {
        Destroy(gameObject);
    }
    public void Destroy() {
        photonView.RPC("DestroyRPC", RpcTarget.AllBuffered);
    }

}
