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
            battleManager.ActionCoroutine = targetUnit.CoPassAction();
        }
    }
}
