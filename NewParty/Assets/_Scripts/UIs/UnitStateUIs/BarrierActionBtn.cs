using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrierActionBtn : FixedActionBtn
{
    protected override void UpdateBtn() {

        if (!MeetActiveBasicCondition()) {
            Active = false;
            return;
        }

        Active = targetUnit.BasicBarrierSkill.CanUse();
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
    protected override string GetTooltipDetailedDescription() {
        return targetUnit?.BasicBarrierSkill.GetDetailedDescription();
    }
}
