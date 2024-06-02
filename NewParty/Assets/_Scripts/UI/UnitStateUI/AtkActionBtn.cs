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

        //atk ��ū�� Ȱ��ȭ �Ǿ� ���� ��� �� ��ư�� Ȱ��ȭ �ȴ�.
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

        BattleSelectable.RunSelectMode(BattleSelectionType.Unit, 1,
            ActionUnit.BasicAtkSkill.SelectionPred,
            OnCompleteSelection,
            OnCancel);
    }

    protected override void OnCompleteSelection() {
        ActionUnit.BasicAtkSkill.Use();
        base.OnCompleteSelection();
    }

}
