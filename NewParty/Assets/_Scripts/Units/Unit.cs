using Photon.Pun;
using Photon.Pun.Demo.Procedural;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public enum UnitType : int
{
    None, Garuda,GrayWolf, RedWolf, SilverManeWolf, BloodyWolf, HowlingWolf, AwlMosquito, DrillMosquito, TransfusionMosquito, InfectedMosquito,
    StoneTurtle, IronTurtle, SteelTurtle, EmeraldTurtle, RainbowTurtle,
}

public partial class Unit : MonoBehaviourPun
{
    // ���� ���� //////////////////////////////////////////////////////////////
    static public float MaxActionGauge = 100f;
    static public int NumSoulFragmentRequiredForSummon = 100;

    // ���� ���� //////////////////////////////////////////////////////////////
    [SerializeField] protected DamageText damageTextPrefab;
    [SerializeField] protected GameObject HealingFxPrefab;
    [SerializeField] protected Token tokenPrefab;

    [SerializeField] protected UnitCanvas unitCanvas;
    public Image ProfileImage { get { return unitCanvas.ProfileImage; } }
    protected Transform tokensParent;
    protected Transform statusEffectIconParent;
    public Transform StatusEffectIconParent { get { return statusEffectIconParent; } }

    public BasicAttackSkill BasicAtkSkill;
    public BasicBarrierSkill BasicBarrierSkill;
    [SerializeField] protected Transform skillsParent;

    // ���� ���� //////////////////////////////////////////////////////////////

    // Unit.Data�� ������ ����
    public Data MyData;

    // ���¸� ��Ÿ���� ����
    private bool isDie = false;
    private Unit murderer = null;

    // �ɷ�ġ ���� ����
    public float[,] Stats { get; set; } = new float[(int)StatForm.Num, (int)StatType.Num];
    public UnityAction<Unit>[] OnUpdateFinalStat = new UnityAction<Unit>[(int)StatType.Num];

    protected float hp;
    public UnityAction<Unit, Ref<float>> OnRecoverHp;   // ����, ȸ����
    public UnityAction<Unit, float, float> OnRecoveredHp;   // ����, ȸ����, �ʰ���

    protected float actionGauge = 0;

    // �±� ���� ����
    public Tags Tags { get; set; } = new Tags();

    // ��ū ���� ����
    public int MaxTokens { get; set; } = 5;
    public List<Token> Tokens { get; set; } = new List<Token>();

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

    // �̺�Ʈ ����


    
    public Action<HitCalculator> OnBeforeCalculateHit;
    public Action<DamageCalculator> OnBeforeCalculateDmg, OnAfterCalculateDmg;
    public Action<Attack, AttackTargetsSetting> OnBecomeAttackTarget;
    public Action<StunTurnDebuff> OnStun;
    public Action<Token> OnCreateToken;
    public Action<ActiveSkill> OnUseActiveSkill;

    public Func<IEnumerator> CoOnBeginMyTurn, CoOnEndMyTurn;
    public Func<IEnumerator> CoOnBeginTick;
    public Func<IEnumerator> CoOnBeginWave, CoOnEndWave;
    public Func<IEnumerator> CoOnAvoid, CoOnDie;
    public Func<Unit, IEnumerator> CoOnKill; // �Ű�����(���� ���)
    public Func<Unit, Attack, IEnumerator> CoOnHit, CoOnHitMiss; // �Ű�����(���, ����)
    public Func<Unit, DamageCalculator, IEnumerator> CoOnHitDmg; // �Ű�����(���� ���, DamageCalculator)
    public Func<Unit, float, float, IEnumerator> CoOnHitHp; // �Ű�����(���� ���, hp�� �� ����, �ʰ� ����)
    public Func<Token, IEnumerator> CoOnGetToken, CoOnUseToken, CoOnDiscardToken, CoOnOverflowToken;
    

    // ����Ƽ �Լ� ////////////////////////////////////////////////////////////
    protected void Awake() {
        if (SharedData.InitTags != null)
            Tags.AddTag(SharedData.InitTags);
        UpdateAllStat();

        tokensParent = unitCanvas.transform.Find("TokensParent").GetComponent<Transform>();
        statusEffectIconParent = unitCanvas.transform.Find("StatusEffectIconParent").GetComponent<Transform>();
    }

    protected void Start() {
        BasicAtkSkill.Owner = this;
        BasicBarrierSkill.Owner = this;

        foreach (Skill skill in skillsParent.GetComponentsInChildren<Skill>(true)) {
            skill.Owner = this;
            Skills.Add(skill);
        }

        hp = GetFinalStat(StatType.Hpm);

        CoOnAvoid += CoCreateAvoidText;
        OnStun += GainStunResistance;
    }

    // �Լ� ///////////////////////////////////////////////////////////////////
    public string Name {
        get { return SharedData.Name; }
    }
    public int GrowthLevel {
        get { return MyData.GrowthLevel; }
        set {
            MyData.GrowthLevel = value;
            UpdateBaseStat(true);
        }
    }
    public UnitSharedData SharedData {
        get { return MyData.SharedData; }
    }

    public float ActionGauge {
        get { return actionGauge; }
        set { photonView.RPC("ActionGaugeRPC", RpcTarget.All, value); }
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

    // �ɷ�ġ ���� �Լ�
    public float GetFinalStat(StatType type) {
        return Mathf.Max(StatFeatures.GetMin(type), Stats[(int)StatForm.Final, (int)type]);
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
            Stats[(int)StatForm.Base, (int)type] = SharedData.InitStats[(int)type];

            if(StatFeatures.GetOperation(type) == StatOperation.Figure)
                Stats[(int)StatForm.Base, (int)type] *= SharedData.SpeciesMul * (1f + 0.01f * GrowthLevel);
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
    public Token CreateRandomToken() {
        // ��ū�� �����Ѵ�
        Token newToken = PhotonNetwork.Instantiate(Token.GetTokenPrefabPath(), Vector3.zero, Quaternion.identity).GetComponent<Token>();

        // ��ū�� Ÿ���� �����Ѵ�
        float sum = GetFinalStat(StatType.AtkTokenWeight) + GetFinalStat(StatType.SkillTokenWeight) + GetFinalStat(StatType.ShieldTokenWeight);
        float random = UnityEngine.Random.Range(0f, sum);

        TokenType tokenType = TokenType.None;
        if (random <= GetFinalStat(StatType.AtkTokenWeight))
            tokenType = TokenType.Atk;
        else if (random <= GetFinalStat(StatType.AtkTokenWeight) + GetFinalStat(StatType.SkillTokenWeight))
            tokenType = TokenType.Skill;
        else
            tokenType = TokenType.Barrier;
        newToken.Type = tokenType;

        // ��ū �̺�Ʈ ȣ��
        OnCreateToken?.Invoke(newToken);

        return newToken;
    }

    [PunRPC]
    private void AddTokenToListRPC(int viewId) {
        Token token = PhotonView.Find(viewId).GetComponent<Token>();
        Tokens.Add(token);
    }
    private void AddTokenToList(Token token) {
        photonView.RPC("AddTokenToListRPC", RpcTarget.All, token?.photonView.ViewID ?? -1);
    }

    [PunRPC]
    private void SetTokenParentRPC(int viewId) {
        Transform token = PhotonView.Find(viewId).transform;
        token.SetParent(tokensParent);
        token.transform.localScale = Vector3.one;
    }
    private void SetTokenParent(Token token) {
        photonView.RPC("SetTokenParentRPC", RpcTarget.All, token?.photonView.ViewID ?? -1);
    }

    public IEnumerator AddToken(Token token) {
        token.Owner = this; // �Ҹ������ �� ��ū�� ���� ������ �� �� �ֵ��� �̸� ������ �������ش�.

        // ���� ��ū�� ������ �Ǵ��Ѵ�
        if (Tokens.Count < MaxTokens) {
            // ��ū�� ��ū ����Ʈ�� �߰��ϰ� ��ū�� �����ָ� �ش� �������� �����Ѵ�
            AddTokenToList(token);
            SetTokenParent(token);
            yield return StartCoroutine(GameManager.CoInvoke(CoOnGetToken, token));
        }
        else {
            // �ִ� �������� �ʰ��Ͽ� �Ҹ��Ų��.
            yield return StartCoroutine(OverflowToken(token));
        }
    }

    [PunRPC]
    private void RemoveTokeFromListRPC(int viewId) {
        Token token = PhotonView.Find(viewId).GetComponent<Token>();
        Tokens.Remove(token);
    }
    private void RemoveTokeFromList(Token token) {
        photonView.RPC("RemoveTokeFromListRPC", RpcTarget.All, token?.photonView.ViewID ?? -1);
    }

    public IEnumerator UseToken(Token token) {
        yield return StartCoroutine(GameManager.CoInvoke(CoOnUseToken, token));
        RemoveTokeFromList(token);
        token.Destroy();
    }
    public IEnumerator DiscardToken(Token token) {
        yield return StartCoroutine(GameManager.CoInvoke(CoOnDiscardToken, token));
        RemoveTokeFromList(token);
        token.Destroy();
    }
    public IEnumerator OverflowToken(Token token) {
        yield return StartCoroutine(GameManager.CoInvoke(CoOnOverflowToken, token));
        RemoveTokeFromList(token);
        token.Destroy();
    }
    public IEnumerator UseSelectedTokens() {
        List<Token> selectedTokens = Tokens.FindAll(token => token.IsSelected);
        foreach (Token token in selectedTokens)
            yield return StartCoroutine(UseToken(token));
    }
    public IEnumerator DiscardRandomToken() {
        if (Tokens.Count == 0)
            yield break;
        yield return StartCoroutine(DiscardToken(Tokens.PickRandom()));
    }

    public void ClearAllToken() {
        foreach (var token in Tokens)
            token.Destroy();
        Tokens.Clear();
    }

    public int GetSelectedTokensNum() {
        return Tokens.FindAll(token => token.IsSelected).Count;
    }

    // �ൿ ���� �Լ�
    public IEnumerator CoPassAction() {
        yield return new WaitForSeconds(0.1f);
    }
    public IEnumerator CoDiscardAction() {
        List<Token> selectedTokens = Tokens.FindAll(token => token.IsSelected);
        foreach (Token token in selectedTokens)
            yield return StartCoroutine(DiscardToken(token));

        yield return new WaitForSeconds(0.3f);
    }
    public bool CanUseDiscardAction() {
        if (Tags.Contains(Tag.����))
            return false;

        foreach (Token token in Tokens) {
            if (token.IsSelected) {
                return true;
            }
        }
        return false;
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

    // �⺻ �ɷ��� �̺�Ʈ�� ���
    protected void GainStunResistance(StunTurnDebuff stun) {
        StatTurnStatusEffect.Create(this, this, StatForm.AbnormalMul, StatType.StunSen, StatusEffectForm.Buff, 0.5f, stun.Turn + 3);
    }

}
