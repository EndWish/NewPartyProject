using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static System.Collections.Specialized.BitVector32;

public class ShockBlowSkill : PassiveSkill, IStatusEffectIconable, IIconRightUpperTextable
{
    protected int stack = 0;
    [SerializeField] protected int maxStack;
    [SerializeField] protected int turn;
    [SerializeField] protected float strMul;

    [SerializeField] protected GameObject fx;

    protected override void Awake() {
        base.Awake();
        Name = "충격타";
    }

    protected void OnDestroy() {
        if (Owner != null) Owner.CoOnUseToken -= this.CoOnUseToken;
        if (Owner != null) Owner.CoOnHitDmg -= this.CoOnHitDmg;
        if (Owner != null) Owner.CoOnHitMiss -= this.CoOnHitMiss;
    }

    [PunRPC]
    protected virtual void StackRPC(int stack) {
        this.stack = Mathf.Min(stack, maxStack);
    }
    public int Stack {
        get { return stack; }
        set { photonView.RPC("StackRPC", RpcTarget.All, value); }
    }

    [PunRPC]
    protected override void OwnerRPC(int viewId) {
        if (Owner != null) {
            Owner.CoOnUseToken -= this.CoOnUseToken;
            Owner.CoOnHitDmg -= this.CoOnHitDmg;
            Owner.CoOnHitMiss -= this.CoOnHitMiss;
        }
        base.OwnerRPC(viewId);
        if (Owner != null) {
            Owner.CoOnUseToken += this.CoOnUseToken;
            Owner.CoOnHitDmg += this.CoOnHitDmg;
            Owner.CoOnHitMiss += this.CoOnHitMiss;
        }
    }

    public override string GetDescriptionText() {
        return string.Format("공격 토큰을 {0}개 이상 사용할 경우 충전 상태가 된다. 충전상태에서 기본공격 적중 시 {1}턴간 대상의 공격력을 {2}감소시킨다.",
            TooltipText.SetCountFont(maxStack),
            TooltipText.SetCountFont(turn),
            TooltipText.SetMulFont(strMul));
    }
    public override string GetDetailedDescriptionText() {
        return string.Format("공격 토큰을 {0}개 이상 사용할 경우 충전 상태가 된다. 충전상태에서 기본공격 적중 시 {1}턴간 대상의 공격력을 {2}감소시킨다.",
            TooltipText.SetCountFont(maxStack),
            TooltipText.SetCountFont(turn),
            TooltipText.SetMulFont(strMul));
    }

    protected IEnumerator CoOnUseToken(Token token) {
        if (token.Type == TokenType.Atk) {
            Stack = Mathf.Min(maxStack, Stack + 1);
        }
        yield break;
    }
    protected IEnumerator CoOnHitDmg(Unit damagedUnit, DamageCalculator dc) {
        Attack attack = dc.Attack;
        if (maxStack <= Stack && attack.Tags.Contains(Tag.기본공격)) {
            // 공격력 디버프
            StatTurnStatusEffect.Create(Owner, damagedUnit, StatForm.AbnormalMul, StatType.Str, StatusEffectForm.Debuff, strMul, turn);

            // 이펙트 생성
            Instantiate(fx, damagedUnit.transform.position, Quaternion.identity);

            Stack = 0;
        }
        yield break;
    }
    protected IEnumerator CoOnHitMiss(Unit damagedUnit, Attack attack) {
        if (maxStack <= Stack && attack.Tags.Contains(Tag.기본공격)) {
            Stack = 0;
        }
        yield break;
    }

    // IStatusEffectIconable, IRightUpperTextableIcon
    public bool IsSEVisible() {
        return 0 < stack;
    }
    public Color GetBgColor() {
        if (stack == maxStack) {
            return Color.magenta;
        }
        return Color.gray;
    }
    
    public string GetIconRightUpperText() {
        return Stack.ToString();
    }
}
