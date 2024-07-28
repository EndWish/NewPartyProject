using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuakeSkill : ActiveSkill
{
    [SerializeField] protected float dmgCoefficient;

    [SerializeField] protected QuakeAttack attackPrefab;

    protected override void Awake() {
        base.Awake();
        Name = "지진";
    }

    public override IEnumerator CoUse() {
        yield return StartCoroutine(Owner.UseSelectedTokens());

        // 공격을 생성한다
        Unit target = BattleSelectable.Units[0];
        QuakeAttack attack = PhotonNetwork.Instantiate(GameManager.GetAttackPrefabPath("QuakeAttack"),
            target.transform.position, Quaternion.identity)
        .GetComponent<QuakeAttack>();

        attack.Init(Owner, target, CalculateDmg());

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

    public override string GetDescription() {
        return string.Format("{0}를 주고, 방어율만큼의 확률로 적을 <b><i>기절</i></b> 시킨다.",
            TooltipText.SetDamageFont(CalculateDmg()));
    }
    public override string GetDetailedDescription() {
        return string.Format("{0} = ({1}{2}%)데미지({3})를 주고, 방어율 = ({4} / ({4} + 상대{5}))만큼의 확률로 적을 <b><i>기절</i></b> 시킨다.",
            TooltipText.SetDamageFont(CalculateDmg()),
            TooltipText.GetIcon(StatType.Str),
            TooltipText.GetFlexibleFloat(dmgCoefficient * 100f),
            Tags.GetString(attackPrefab.InitTags),
            TooltipText.GetIcon(StatType.Def),
            TooltipText.GetIcon(StatType.DefPen));
    }

    protected float CalculateDmg() {
        return Owner.GetFinalStat(StatType.Str) * dmgCoefficient;
    }

}
