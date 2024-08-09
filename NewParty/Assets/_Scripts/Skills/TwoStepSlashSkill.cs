using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class TwoStepSlashSkill : ActiveSkill
{
    [SerializeField] protected float dmgCoefficient;

    [SerializeField] protected TwoStepSlashAttack attackPrefab;

    protected override void Awake() {
        base.Awake();
        Name = "2단 베기";
    }

    public override IEnumerator CoUse() {
        yield return StartCoroutine(Owner.UseSelectedTokens());

        // 공격을 생성한다
        Unit target = BattleSelectable.Units[0];
        TwoStepSlashAttack attack = TwoStepSlashAttack.Create(Owner, target, CalculateDmg());

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
        return string.Format("적을 두번 베어 공격한다. {0}데미지를 두번 준다.",
            TooltipText.SetDamageFont(CalculateDmg()));
    }
    public override string GetDetailedDescriptionText() {
        return string.Format("적을 두번 베어 공격한다. {0} = ({1}{2}%)데미지({3})를 두번 준다.",
            TooltipText.SetDamageFont(CalculateDmg()),
            TooltipText.GetIcon(StatType.Str),
            TooltipText.GetFlexibleFloat(dmgCoefficient * 100f),
            Tags.GetString(attackPrefab.InitTags));
    }

    protected float CalculateDmg() {
        return Owner.GetFinalStat(StatType.Str) * dmgCoefficient;
    }

}
