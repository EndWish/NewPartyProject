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
        return "������ ��ū ������";
    }

    protected override string GetTooltipRightUpperText() {
        return "��ū 1�� �̻� ����";
    }

    protected override string GetTooltipDescription() {
        return "������ ��ū�� ���� ������ ���� �����Ѵ�.";
    }
    protected override string GetTooltipDetailedDescription() {
        return "������ ��ū�� ���� ������ ���� �����Ѵ�. (#����)�� �� ����� �Ұ��� �ϴ�.";
    }
}
