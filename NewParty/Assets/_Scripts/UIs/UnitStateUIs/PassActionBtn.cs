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

    public override string GetTooltipTitleText() {
        return "패스";
    }
    public override string GetTooltipRightUpperText() {
        return null;
    }
    public override string GetDescriptionText() {
        return "아무런 행동을 하지 않고 턴을 종료한다.";
    }

}
