using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscardActionBtn : FixedActionBtn
{
    protected override void UpdateBtn() {
        BattleManager battleManager = BattleManager.Instance;

        if (!MeetActiveBasicCondition()) {
            Active = false;
            return;
        }

        //��ū�� �ϳ��� Ȱ��ȭ �Ǿ������� Ȱ��ȭ �ȴ�.
        bool result = false;
        foreach (Token token in targetUnit.Tokens) {
            if (token.IsSelected) {
                result = true;
                break;
            }
        }

        Active = result;
    }

    public override void OnClick() {
        BattleManager battleManager = BattleManager.Instance;

        if (MeetClickCondition()) {
            BattleSelectable.StopSelectMode();
            battleManager.ActionCoroutine = targetUnit.CoDiscardAction();
        }
    }

    protected override string GetTooltipTitle() {
        return "������ ��ū ������";
    }

    protected override string GetTooltipRightUpperText() {
        return "��ū 1�� �̻� ����";
    }

    protected override string GetTooltipDescription() {
        return "������ ��ū�� ���� ������ ���� �����Ѵ�.";
    }
}
