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
        return "패스";
    }

    protected override string GetTooltipRightUpperText() {
        return "";
    }

    protected override string GetTooltipDescription() {
        return "아무런 행동을 하지 않고 턴을 종료한다.";
    }
    protected override string GetTooltipDetailedDescription() {
        return "아무런 행동을 하지 않고 턴을 종료한다.";
    }

}
