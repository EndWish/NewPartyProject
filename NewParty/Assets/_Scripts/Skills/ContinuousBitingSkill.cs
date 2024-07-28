using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContinuousBitingSkill : ActiveSkill
{
    [SerializeField] protected float dmgCoefficient;
    [SerializeField] protected int hitNum;

    [SerializeField] protected ContinuousBitingAttack attackPrefab;

    protected override void Awake() {
        base.Awake();
        Name = "연속 물기";
    }

    public override IEnumerator CoUse() {
        yield return StartCoroutine(Owner.UseSelectedTokens());

        // 공격을 생성한다
        Unit target = BattleSelectable.Units[0];
        ContinuousBitingAttack attack = PhotonNetwork.Instantiate(GameManager.GetAttackPrefabPath("ContinuousBitingAttack"),
            target.transform.position, Quaternion.identity)
        .GetComponent<ContinuousBitingAttack>();

        float dmg = CalculateDmg();
        attack.Init(Owner, target, dmg, hitNum);

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
        return string.Format("적에게 {0}회 연속으로 공격하여 각각 {1}의 데미지를 준다.",
            TooltipText.SetCountFont(hitNum),
            TooltipText.SetDamageFont(CalculateDmg()));
    }
    public override string GetDetailedDescription() {
        return string.Format("적에게 {0}회 연속으로 공격하여 각각 {1} = ({2}{3}%)의 데미지({4})를 준다.",
            TooltipText.SetCountFont(hitNum),
            TooltipText.SetDamageFont(CalculateDmg()),
            TooltipText.GetIcon(StatType.Str),
            TooltipText.GetFlexibleFloat(dmgCoefficient * 100f),
            Tags.GetString(attackPrefab.InitTags));
    }

    protected float CalculateDmg() {
        return Owner.GetFinalStat(StatType.Str) * dmgCoefficient;
    }

}
