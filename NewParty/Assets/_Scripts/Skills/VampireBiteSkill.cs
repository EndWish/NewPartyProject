using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VampireBiteSkill : ActiveSkill
{
    [SerializeField] protected float dmgCoefficient;

    [SerializeField] protected VampireBiteAttack attackPrefab;

    protected override void Awake() {
        base.Awake();
        Name = "흡혈 물기";
    }

    public override IEnumerator CoUse() {
        yield return StartCoroutine(Owner.UseSelectedTokens());

        // 공격을 생성한다
        Unit target = BattleSelectable.Units[0];
        VampireBiteAttack attack = VampireBiteAttack.Create(Owner, target, CalculateDmg());

        yield return StartCoroutine(attack.Animate());
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
        return Owner.TeamType != unit.TeamType;
    }

    public override string GetDescriptionText() {
        return string.Format("적에게 {0}데미지를 주고, HP에 준 피해량 만큼 체력을 회복한다.",
            TooltipText.SetDamageFont(CalculateDmg()));
    }
    public override string GetDetailedDescriptionText() {
        return string.Format("적에게 {0} = ({1}{2}%)데미지({3})를 주고, HP에 준 피해량 만큼 체력을 회복한다.",
            TooltipText.SetDamageFont(CalculateDmg()),
            TooltipText.GetIcon(StatType.Str),
            TooltipText.GetFlexibleFloat(dmgCoefficient * 100f),
            Tags.GetString(attackPrefab.InitTags));
    }

    protected float CalculateDmg() {
        return Owner.GetFinalStat(StatType.Str) * dmgCoefficient;
    }

}
