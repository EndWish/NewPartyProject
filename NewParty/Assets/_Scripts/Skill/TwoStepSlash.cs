using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoStepSlash : Skill
{
    private void Awake() {
        Name = "2단 베기";
        //Cost = 3 - 3;
        IsPassive = false;
    }

    public override bool SelectionPred(Unit unit) {
        return Owner.TeamType != unit.TeamType;
    }
    public override BattleSelectionType GetSelectionType() {
        return BattleSelectionType.Unit;
    }
    public override int GetSelectionNum() {
        return 1;
    }

    public override IEnumerator CoUse() {
        Debug.Log("TwoStepSlash 코루틴 작동");
        yield return null;
    }


}
