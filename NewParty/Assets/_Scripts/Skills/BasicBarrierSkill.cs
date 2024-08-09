using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicBarrierSkill : ActiveSkill
{
    protected override void Awake() {
        base.Awake();
        Name = "�⺻ �踮��";
    }

    public override IEnumerator CoUse() {
        // ��ū�� ������ ���� �����Ѵ�
        int tokenStack = Owner.Tokens.FindAll(token => token.IsSelected).Count;
        yield return StartCoroutine(Owner.UseSelectedTokens());

        // �⺻ �踮�� ���� �� ����
        BasicBarrier.Create(Owner, CalculateAmount(tokenStack));

        // �⺻ �踮�� ������ �����̻� ���� �� ����
        BasicBarrierOverload.Create(Owner);

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

    public override string GetDescriptionText() {
        float basicAmount = CalculateAmount(Mathf.Max(1, Owner.Tokens.FindAll(token => token.IsSelected && token.Type == TokenType.Barrier).Count));
        float amountOverloadApplied = basicAmount * GetBasicBarrierOverloadCoefficient();

        return string.Format("{0}�� �������� �����ִ� �踮� �����Ѵ�. �⺻ �踮��� ��ø���� �ʴ´�. \n[�⺻ �踮�� ������] �����̻��� ������ ��� {1}�� �������� �����ش�.",
            TooltipText.SetDamageFont(basicAmount), 
            TooltipText.SetDamageFont(amountOverloadApplied));
    }

    public override string GetDetailedDescriptionText() {
        float basicAmount = CalculateAmount(Mathf.Max(1, Owner.Tokens.FindAll(token => token.IsSelected && token.Type == TokenType.Barrier).Count));
        float amountOverloadApplied = basicAmount * GetBasicBarrierOverloadCoefficient();

        return string.Format("{0} = ({2}100% + {2}100% x {3} x �߰���ū)�� �������� �����ִ� �踮� �����Ѵ�. �⺻ �踮��� ��ø���� �ʴ´�. \n[�⺻ �踮�� ������] �����̻��� ������ ��� {1}�� �������� �����ش�.",
            TooltipText.SetDamageFont(basicAmount),
            TooltipText.SetDamageFont(amountOverloadApplied),
            TooltipText.GetIcon(StatType.Shield),
            TooltipText.GetIcon(StatType.StackShield));
    }

    protected float GetBasicBarrierOverloadCoefficient() {
        BasicBarrierOverload basicBarrierOverload = (BasicBarrierOverload)Owner.StatusEffects.Find((statusEffect) => (statusEffect is BasicBarrierOverload));
        return basicBarrierOverload?.ShieldMul ?? 1f;
    }

}
