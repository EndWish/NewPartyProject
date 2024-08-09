using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrierActionBtn : FixedActionBtn, IDetailedDescription
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

    public override string GetTooltipTitleText() {
        return targetUnit?.BasicBarrierSkill.Name;
    }
    public override string GetTooltipRightUpperText() {
        return "방어 토큰 1개 이상";
    }
    public override string GetDescriptionText() {
        return targetUnit?.BasicBarrierSkill.GetDescriptionText();
    }
    public string GetDetailedDescriptionText() {
        return targetUnit?.BasicBarrierSkill.GetDetailedDescriptionText();
    }
}
