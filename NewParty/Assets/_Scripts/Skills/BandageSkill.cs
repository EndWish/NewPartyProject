using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BandageSkill : ActiveSkill
{
    [SerializeField] protected float minCoefficient;
    [SerializeField] protected float maxCoefficient;

    protected override void Awake() {
        base.Awake();
        Name = "붕대 감기";
    }

    public override IEnumerator CoUse() {
        yield return StartCoroutine(Owner.UseSelectedTokens());

        float amount = Owner.GetFinalStat(StatType.Hpm) * Random.Range(minCoefficient, maxCoefficient);
        Owner.RecoverHp(amount);

        yield return new WaitForSeconds(0.5f);
    }

    public override BattleSelectionType GetSelectionType() {
        return BattleSelectionType.Unit;
    }
    public override int GetSelectionNum() {
        return 1;
    }
    public override bool SelectionPred(Unit unit) {
        if (!base.SelectionPred(unit))
            return false;
        return Owner == unit;
    }

    public override string GetDescriptionText() {
        float hpm = Owner.GetFinalStat(StatType.Hpm);
        return string.Format("{0}~{1} 만큼 회복한다.",
            TooltipText.SetDamageFont(hpm * minCoefficient),
            TooltipText.SetDamageFont(hpm * maxCoefficient));
    }
    public override string GetDetailedDescriptionText() {
        float hpm = Owner.GetFinalStat(StatType.Hpm);
        return string.Format("{0}~{1} 만큼 회복한다.",
            TooltipText.SetDamageFont(hpm * minCoefficient),
            TooltipText.SetDamageFont(hpm * maxCoefficient));
    }
}
