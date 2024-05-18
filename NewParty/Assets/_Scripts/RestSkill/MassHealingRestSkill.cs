using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MassHealingRestSkill : RestSkill
{
    private float amount = 0.20f;

    protected override void RunSelectMode() {
        base.RunSelectMode();
        BattleSelectable.RunSelectMode(BattleSelectionType.Party, 1, Predicate, OnCompleteSelection, OnCancel);
    }

    private bool Predicate(Unit unit) {
        return unit.MyParty == UserParty;
    }

    protected override void OnCompleteSelection() {
        foreach (var unit in UserParty.Units) {
            unit.RecoverHp(unit.GetFinalStat(StatType.Hpm) * amount);
        }
        base.OnCompleteSelection();
    }

}
