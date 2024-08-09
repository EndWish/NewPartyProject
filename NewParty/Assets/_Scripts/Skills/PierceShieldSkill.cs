using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PierceShieldSkill : ActiveSkill
{
    [SerializeField] protected float dmgCoefficient;
    [SerializeField] protected float defMul;
    [SerializeField] protected int turn;

    [SerializeField] protected PierceShieldAttack attackPrefab;

    protected override void Awake() {
        base.Awake();
        Name = "방패 뚫기";
    }

    public override IEnumerator CoUse() {
        yield return StartCoroutine(Owner.UseSelectedTokens());

        // 공격을 생성한다
        Unit target = BattleSelectable.Units[0];
        PierceShieldAttack attack = PierceShieldAttack.Create(Owner, target, defMul, turn, CalculateDmg());

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
        return string.Format("적에게 {0}데미지를 주고 {1}턴간 방어력을 {2}감소시킨다.",
            TooltipText.SetDamageFont(CalculateDmg()),
            TooltipText.SetCountFont(turn),
            TooltipText.SetMulFont(defMul));
    }
    public override string GetDetailedDescriptionText() {
        return string.Format("적에게 {0} = ({3}{4}%)데미지({5})를 주고 {1}턴간 방어력을 {2}감소시킨다.",
            TooltipText.SetDamageFont(CalculateDmg()),
            TooltipText.SetCountFont(turn),
            TooltipText.SetMulFont(defMul),
            TooltipText.GetIcon(StatType.Str),
            TooltipText.GetFlexibleFloat(dmgCoefficient * 100f),
            Tags.GetString(attackPrefab.InitTags));
    }

    protected float CalculateDmg() {
        return Owner.GetFinalStat(StatType.Str) * dmgCoefficient;
    }

}
