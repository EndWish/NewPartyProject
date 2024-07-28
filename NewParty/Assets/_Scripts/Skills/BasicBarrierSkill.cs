using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicBarrierSkill : ActiveSkill
{
    protected override void Awake() {
        base.Awake();
        Name = "기본 배리어";
    }

    public override IEnumerator CoUse() {
        // 토큰을 개수를 세고 삭제한다
        int tokenStack = Owner.Tokens.FindAll(token => token.IsSelected).Count;
        yield return StartCoroutine(Owner.UseSelectedTokens());

        // 기본 배리어 생성 및 적용
        BasicBarrier barrier = PhotonNetwork.Instantiate(GameManager.GetBarrierPrefabPath("BasicBarrier"),
            transform.position, Quaternion.identity)
            .GetComponent<BasicBarrier>();
        barrier.Init(Owner, CalculateAmount(tokenStack));

        // 기본 배리어 과부하 상태이상 생성 및 적용
        BasicBarrierOverload statusEffect = PhotonNetwork.Instantiate(GameManager.GetStatusEffectPrefabPath("BasicBarrierOverload"),
            transform.position, Quaternion.identity)
            .GetComponent<BasicBarrierOverload>();
        statusEffect.Caster = Owner;
        Owner.AddStatusEffect(statusEffect);

        yield return new WaitForSeconds(0.15f);
    }

    public override BattleSelectionType GetSelectionType() {
        return BattleSelectionType.Unit;
    }
    public override int GetSelectionNum() {
        return 1;
    }
    public override bool SelectionPred(Unit unit) {
        if (!base.SelectionPred(unit))
            return false;
        return Owner == unit;
    }

    protected override bool MeetTokenCost() {
        int count = 0;
        foreach (var token in Owner.Tokens) {
            if (!token.IsSelected)
                continue;

            if (token.Type != TokenType.Barrier) {
                return false;
            }

            ++count;
        }

        if (count == 0)
            return false;

        return true;
    }

    protected float CalculateAmount(int tokenStack) {
        return Owner.GetFinalStat(StatType.Shield) * (1f + Owner.GetFinalStat(StatType.StackShield) * (tokenStack - 1));
    }

    public override string GetDescription() {
        return string.Format("{0}의 데미지를 막아주는 배리어를 생성한다. 기본 배리어는 중첩되지 않는다.",
            TooltipText.SetDamageFont(CalculateAmount(Mathf.Max(1, Owner.Tokens.FindAll(token => token.IsSelected && token.Type == TokenType.Barrier).Count))));
    }

    public override string GetDetailedDescription() {
        return string.Format("{0} = ({1}100% + {1}100% x {2} x 추가토큰)의 데미지를 막아주는 배리어를 생성한다. 기본 배리어는 중첩되지 않는다.",
            TooltipText.SetDamageFont(CalculateAmount(Mathf.Max(1, Owner.Tokens.FindAll(token => token.IsSelected && token.Type == TokenType.Barrier).Count))),
            TooltipText.GetIcon(StatType.Shield),
            TooltipText.GetIcon(StatType.StackShield));
    }

}
