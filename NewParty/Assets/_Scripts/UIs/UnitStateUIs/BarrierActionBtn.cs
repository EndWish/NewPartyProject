using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrierActionBtn : FixedActionBtn
{
    protected override void UpdateBtn() {
        BattleManager battleManager = BattleManager.Instance;

        if (!MeetActiveBasicCondition()) {
            Active = false;
            return;
        }

        //atk ��ū�� Ȱ��ȭ �Ǿ� ���� ��� �� ��ư�� Ȱ��ȭ �ȴ�.
        bool result = false;
        foreach (Token token in targetUnit.Tokens) {
            if (!token.IsSelected)
                continue;

            if (token.Type == TokenType.Barrier) {
                result = true;
            } 
            else {
                result = false;
                break;
            }
        }

        Active = result;
    }

    public override void OnClick() {
        BattleManager battleManager = BattleManager.Instance;

        if (MeetClickCondition()) {
            BattleSelectable.StopSelectMode();
            battleManager.ActionCoroutine = targetUnit.BasicBarrierSkill.CoUse();
        }
    }

    protected override string GetTooltipTitle() {
        return targetUnit?.BasicBarrierSkill.Name;
    }

    protected override string GetTooltipRightUpperText() {
        return "��� ��ū 1�� �̻�";
    }

    protected override string GetTooltipDescription() {
        return targetUnit?.BasicBarrierSkill.GetDescription();
    }
}
