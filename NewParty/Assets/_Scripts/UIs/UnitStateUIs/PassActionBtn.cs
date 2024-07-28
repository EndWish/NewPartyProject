using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassActionBtn : FixedActionBtn
{
    protected override void UpdateBtn() {
        BattleManager battleManager = BattleManager.Instance;

        if (!MeetActiveBasicCondition()) {
            Active = false;
            return;
        }

        Active = true;
    }

    public override void OnClick() {
        BattleManager battleManager = BattleManager.Instance;

        if (MeetClickCondition()) {
            BattleSelectable.StopSelectMode();
            battleManager.ActionCoroutine = targetUnit.CoPassAction();
        }
    }

    protected override string GetTooltipTitle() {
        return "�н�";
    }

    protected override string GetTooltipRightUpperText() {
        return "";
    }

    protected override string GetTooltipDescription() {
        return "�ƹ��� �ൿ�� ���� �ʰ� ���� �����Ѵ�.";
    }
    protected override string GetTooltipDetailedDescription() {
        return "�ƹ��� �ൿ�� ���� �ʰ� ���� �����Ѵ�.";
    }

}
