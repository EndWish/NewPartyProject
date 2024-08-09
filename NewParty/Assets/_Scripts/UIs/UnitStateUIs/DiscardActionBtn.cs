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

    public override string GetTooltipTitleText() {
        return "������ ��ū ������";
    }
    public override string GetTooltipRightUpperText() {
        return "��ū 1�� �̻� ����";
    }
    public override string GetDescriptionText() {
        return "������ ��ū�� ���� ������ ���� �����Ѵ�.";
    }

}
