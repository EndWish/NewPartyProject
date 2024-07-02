using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtkActionBtn : FixedActionBtn
{
    protected override void UpdateBtn() {
        BattleManager battleManager = BattleManager.Instance;

        if (!MeetActiveBasicCondition()) {
            Active = false;
            return;
        }

        //atk 토큰만 활성화 되어 있을 경우 이 버튼이 활성화 된다.
        bool result = false;
        foreach(Token token in targetUnit.Tokens) {
            if (!token.IsSelected)
                continue;

            if(token.Type == TokenType.Atk) {
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

    protected override string GetTooltipTitle() {
        return targetUnit?.BasicAtkSkill.Name;
    }

    protected override string GetTooltipRightUpperText() {
        return "공격 토큰 1개 이상";
    }

    protected override string GetTooltipDescription() {
        return targetUnit?.BasicAtkSkill.GetDescription();
    }
}
