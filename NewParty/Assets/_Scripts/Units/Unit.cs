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
    // 공유 정보 //////////////////////////////////////////////////////////////
    static public float MaxActionGauge = 100f;
    static public int NumSoulFragmentRequiredForSummon = 100;

    // 연결 정보 //////////////////////////////////////////////////////////////
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

    // 개인 정보 //////////////////////////////////////////////////////////////

    // Unit.Data와 관련한 변수
    public Data MyData;

    // 상태를 나타내는 변수
    private bool isDie = false;
    private Unit murderer = null;

    // 능력치 관련 변수
    public float[,] Stats { get; set; } = new float[(int)StatForm.Num, (int)StatType.Num];
    public UnityAction<Unit>[] OnUpdateFinalStat = new UnityAction<Unit>[(int)StatType.Num];

    protected float hp;
    public UnityAction<Unit, Ref<float>> OnRecoverHp;   // 유닛, 회복량
    public UnityAction<Unit, float, float> OnRecoveredHp;   // 유닛, 회복량, 초과량

    protected float actionGauge = 0;

    // 태그 관련 변수
    public Tags Tags { get; set; } = new Tags();

    // 토큰 관련 변수
    public int MaxTokens { get; set; } = 5;
    public List<Token> Tokens { get; set; } = new List<Token>();

    // 파티 관련 변수
    public TeamType TeamType { get; set; } = TeamType.None;
    public Party MyParty { get; set; }

    // 배리어 관련 변수
    public List<Barrier> Barriers { get; protected set; } = new List<Barrier>();

    // 스킬 관련 변수
    public List<Skill> Skills { get; protected set; } = new List<Skill>();

    // 상태이상 관련 변수
    public List<StatusEffect> StatusEffects { get; protected set; } = new List<StatusEffect>();

    // 장비 관련 변수

    // 이벤트 변수


    
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
    public Func<Unit, IEnumerator> CoOnKill; // 매개변수(죽인 대상)
    public Func<Unit, Attack, IEnumerator> CoOnHit, CoOnHitMiss; // 매개변수(대상, 공격)
    public Func<Unit, DamageCalculator, IEnumerator> CoOnHitDmg; // 매개변수(때린 대상, DamageCalculator)
    public Func<Unit, float, float, IEnumerator> CoOnHitHp; // 매개변수(때린 대상, hp에 준 피해, 초과 피해)
    public Func<Token, IEnumerator> CoOnGetToken, CoOnUseToken, CoOnDiscardToken, CoOnOverflowToken;
    

    // 유니티 함수 ////////////////////////////////////////////////////////////
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

    // 함수 ///////////////////////////////////////////////////////////////////
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

    // 능력치 관련 함수
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
        // 기본 능력치 계산
        for (StatType type = 0; type < StatType.Num; ++type) {
            Stats[(int)StatForm.Base, (int)type] = SharedData.InitStats[(int)type];

            if(StatFeatures.GetOperation(type) == StatOperation.Figure)
                Stats[(int)StatForm.Base, (int)type] *= SharedData.SpeciesMul * (1f + 0.01f * GrowthLevel);
        }

        // 최종 능력치에 적용
        if (updateFinalStat)
            UpdateFinalStat();
    }
    public void UpdateEquipmentStat(bool updateFinalStat) {
        // 능력치 초기화
        for (StatType type = 0; type < StatType.Num; ++type) {
            Stats[(int)StatForm.EquipmentAdd, (int)type] = 0;
            Stats[(int)StatForm.EquipmentMul, (int)type] = 1f;
        }

        // [추가] 장비의 능력치들 적용

        // 최종 능력치에 적용
        if (updateFinalStat)
            UpdateFinalStat();
    }
    public void UpdateAbnormalStat(bool updateFinalStat) {
        // 능력치 초기화
        for (StatType type = 0; type < StatType.Num; ++type) {
            Stats[(int)StatForm.AbnormalAdd, (int)type] = 0;
            Stats[(int)StatForm.AbnormalMul, (int)type] = 1f;
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

    // 상태이상 관련 함수
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

    // 토큰 관련 함수
    public Token CreateRandomToken() {
        // 토큰을 생성한다
        Token newToken = PhotonNetwork.Instantiate(Token.GetTokenPrefabPath(), Vector3.zero, Quaternion.identity).GetComponent<Token>();

        // 토큰의 타입을 설정한다
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

        // 토큰 이벤트 호출
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
        token.Owner = this; // 소멸될지라도 이 토큰을 가질 주인을 알 수 있도록 미리 변수를 변경해준다.

        // 만든 토큰을 얻을지 판단한다
        if (Tokens.Count < MaxTokens) {
            // 토큰을 토큰 리스트에 추가하고 토큰의 소유주를 해당 유닛으로 변경한다
            AddTokenToList(token);
            SetTokenParent(token);
            yield return StartCoroutine(GameManager.CoInvoke(CoOnGetToken, token));
        }
        else {
            // 최대 소지량을 초과하여 소멸시킨다.
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

    // 행동 관련 함수
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
        if (Tags.Contains(Tag.기절))
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

    // 유닛의 소유상태를 반환하는 함수
    public bool IsMine() {
        return MyParty.TeamType == TeamType.Player && photonView.IsMine;
    }
    public bool IsAlly() {
        return MyParty.TeamType == TeamType.Player && !photonView.IsMine;
    }
    public bool IsEnemy() {
        return MyParty.TeamType == TeamType.Enemy;
    }

    // Data 관련 함수
    [PunRPC] protected void ApplyDataRPC(Unit.Data data) {
        this.MyData = data;
        UpdateAllStat();
        // 프로필 이미지도 적용
    }
    public void ApplyData(Unit.Data data) {
        ApplyDataRPC(data);
        if (photonView.IsMine) {    // 동기화 하지 않은 오브젝트일 경우 실행하지 않는다.
            photonView.RPC("ApplyDataRPC", RpcTarget.Others, data);
        }
    }

    // 데미지/회복 관련 함수
    [PunRPC] protected void CreateHealingFxRPC() {
        Instantiate(HealingFxPrefab, transform.position, Quaternion.identity, transform);
    }
    protected void CreateHealingFx() {
        photonView.RPC("CreateHealingFxRPC", RpcTarget.All);
    }
    public void RecoverHp(float baseAmount) {
        Ref<float> amount = new Ref<float>(baseAmount * GetFinalStat(StatType.Healing));

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
        CreateHealingFx();
        CreateDmgText(amount.Value, new Vector3(0, 1, 0));

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

            CreateDmgText(applyDmg, new Vector3(0.49f, 0.78f, 0.94f), damageCalculator.CriStack);
            yield return StartCoroutine(barrier.TakeDmg(applyDmg));
            dmg -= applyDmg;
        }

        // 배리어를 까고 남은 데미지를 hp에 적용한다
        if(0 < dmg) {
            float applyDmg = Mathf.Min(dmg, Hp);
            float overDmg = Mathf.Max(0, dmg - applyDmg);

            Hp -= applyDmg;
            CreateDmgText(dmg, new Vector3(1, 0.92f, 0.016f), damageCalculator.CriStack);

            // 공격자의 CoOnHitDamage 이벤트를 실행한다
            yield return StartCoroutine(GameManager.CoInvoke(damageCalculator.Attacker.CoOnHitHp, this, applyDmg, overDmg));

            if (Hp <= 0) {
                IsDie = true;
                Murderer = damageCalculator.Attacker;

                yield return StartCoroutine(GameManager.CoInvoke(Murderer.CoOnKill, this));
                yield return StartCoroutine(GameManager.CoInvoke(CoOnDie));
            }
        }

    }

    // 데미지 텍스쳐 생성
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
        barrier.Target = null;
    }
    public void ClearAllBarrier() {
        while (0 < Barriers.Count) {
            Barriers.Last().Destroy();
        }
    }

    // 파티 관련 함수
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

    // 삭제
    [PunRPC] private void DestroyRPC() {
        Destroy(gameObject);
    }
    public void Destroy() {
        photonView.RPC("DestroyRPC", RpcTarget.AllBuffered);
    }

    // 기본 능력을 이벤트에 등록
    protected void GainStunResistance(StunTurnDebuff stun) {
        StatTurnStatusEffect.Create(this, this, StatForm.AbnormalMul, StatType.StunSen, StatusEffectForm.Buff, 0.5f, stun.Turn + 3);
    }

}
