using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static System.Collections.Specialized.BitVector32;
using static UnityEngine.GraphicsBuffer;

public class ShockBlowSkill : PassiveSkill
{
    protected int stack = 0;
    [SerializeField] protected int maxStack;
    [SerializeField] protected int turn;
    [SerializeField] protected float strMul;

    [SerializeField] protected GameObject fx;
    [SerializeField] protected StatusEffectIcon seIconPrefab;
    protected StatusEffectIcon seIcon = null;

    protected override void Awake() {
        base.Awake();
        Name = "충격타";

        seIcon = Instantiate(seIconPrefab);
        InitIcon();
    }

    protected void OnDestroy() {
        if (Owner != null) Owner.CoOnUseToken -= this.CoOnUseToken;
        if (Owner != null) Owner.CoOnHitDmg -= this.CoOnHitDmg;
        if (Owner != null) Owner.CoOnHitMiss -= this.CoOnHitMiss;
    }

    [PunRPC]
    protected virtual void StackRPC(int stack) {
        this.stack = Mathf.Min(stack, maxStack);
        if(0 < stack) {
            seIcon.gameObject.SetActive(true);
            seIcon.BgImg.color = Color.gray;
            seIcon.RightLowerText.text = stack.ToString();
            if (stack == maxStack) {
                seIcon.BgImg.color = Color.magenta;
            }
        }
        else {
            seIcon.gameObject.SetActive(false);
        }
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

            seIcon.transform.SetParent(null);
        }
        base.OwnerRPC(viewId);
        if (Owner != null) {
            Owner.CoOnUseToken += this.CoOnUseToken;
            Owner.CoOnHitDmg += this.CoOnHitDmg;
            Owner.CoOnHitMiss += this.CoOnHitMiss;

            seIcon.transform.SetParent(Owner.StatusEffectIconParent);
            seIcon.transform.localScale = Vector3.one;
        }
    }

    public override string GetDescription() {
        return string.Format("공격 토큰을 {0}개 이상 사용할 경우 충전 상태가 된다. 충전상태에서 기본 공격 적중 시 {1}턴간 대상의 공격력을 x{2:F2} 감소시킨다.", maxStack, turn, strMul);
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
            StatTurnStatusEffect strDebuff = CreateStatTurnStatusEffect(StatForm.AbnormalMul, StatType.Str, StatusEffectForm.Debuff, strMul, turn);
            damagedUnit.AddStatusEffect(strDebuff);

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

    public virtual void InitIcon() {
        seIcon.IconImg.sprite = this.IconSp;
        seIcon.GetTooltipTitle = () => Name;
        seIcon.GetTooltipRightUpperText = () => "패시브";
        seIcon.BgImg.color = Color.gray;
        
        seIcon.GetTooltipDescription = GetDescription;

        Stack = Stack; // 아이콘 내용을 적용하기 위해서 호출
    }

}
