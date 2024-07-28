using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscardActionBtn : FixedActionBtn
{
    protected override void UpdateBtn() {

        if (!MeetActiveBasicCondition()) {
            Active = false;
            return;
        }

        Active = targetUnit.CanUseDiscardAction();
    }

    public override void OnClick() {
        BattleManager battleManager = BattleManager.Instance;

        if (MeetClickCondition()) {
            BattleSelectable.StopSelectMode();
            battleManager.ActionCoroutine = targetUnit.CoDiscardAction();
        }
    }

    protected override string GetTooltipTitle() {
        return "선택한 토큰 버리기";
    }

    protected override string GetTooltipRightUpperText() {
        return "토큰 1개 이상 선택";
    }

    protected override string GetTooltipDescription() {
        return "선택한 토큰을 전부 버리고 턴을 종료한다.";
    }
    protected override string GetTooltipDetailedDescription() {
        return "선택한 토큰을 전부 버리고 턴을 종료한다. (#기절)일 때 사용이 불가능 하다.";
    }
}
