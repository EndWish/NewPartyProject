using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtkActionBtn : FixedActionBtn
{


    protected override void UpdateBtn() {

        if (!MeetActiveBasicCondition()) {
            Active = false;
            return;
        }

        Active = targetUnit.BasicAtkSkill.CanUse();
    }

    public override void OnClick() {
        BattleManager battleManager = BattleManager.Instance;

        if (MeetClickCondition()) {
            if (BattleSelectable.IsRunning) {
                if (ActionUnit == null) {
                    BattleSelectable.StopSelectMode();
                    RunSelectMode();
                } else {
                    BattleSelectable.StopSelectMode();
                }
            } else {
                RunSelectMode();
            }

        }
    }

    protected void RunSelectMode() {
        ActionUnit = targetUnit;

        BattleSelectable.RunSelectMode(ActionUnit.BasicAtkSkill.GetSelectionType(),
            ActionUnit.BasicAtkSkill.GetSelectionNum(),
            ActionUnit.BasicAtkSkill.SelectionPred,
            OnCompleteSelection,
            OnCancel);
    }

    protected override void OnCompleteSelection() {
        ActionUnit.BasicAtkSkill.Use();
        base.OnCompleteSelection();
    }

    public override string GetTooltipTitleText() {
        return targetUnit?.BasicAtkSkill.Name;
    }
    public override string GetTooltipRightUpperText() {
        return "공격 토큰 1개 이상";
    }
    public override string GetDescriptionText() {
        return targetUnit?.BasicAtkSkill.GetDescriptionText();
    }
    public string GetDetailedDescriptionText() {
        return targetUnit?.BasicAtkSkill.GetDetailedDescriptionText();
    }

}
