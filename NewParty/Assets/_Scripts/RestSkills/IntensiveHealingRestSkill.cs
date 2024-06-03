using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntensiveHealingRestSkill : RestSkill
{
    private float amount = 0.50f;


    protected override void RunSelectMode() {
        base.RunSelectMode();
        BattleSelectable.RunSelectMode(BattleSelectionType.Unit, 1, Predicate, OnCompleteSelection, OnCancel);
    }

    private bool Predicate(Unit unit) {
        return unit.MyParty == UserParty;
    }

    protected override void OnCompleteSelection() {
        Unit unit = BattleSelectable.Units[0];
        unit.RecoverHp(unit.GetFinalStat(StatType.Hpm) * amount);
        base.OnCompleteSelection();
    }

}
