using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VampireBiteSkill : ActiveSkill
{
    [SerializeField] protected float dmgCoefficient;

    protected override void Awake() {
        base.Awake();
        Name = "흡혈 물기";
    }

    public override IEnumerator CoUse() {
        Owner.RemoveSelectedToken();

        // 공격을 생성한다
        Unit target = BattleSelectable.Units[0];
        VampireBiteAttack attack = PhotonNetwork.Instantiate(GameManager.GetAttackPrefabPath("VampireBiteAttack"),
            target.transform.position, Quaternion.identity)
        .GetComponent<VampireBiteAttack>();

        float dmg = Owner.GetFinalStat(StatType.Str) * dmgCoefficient;
        attack.Init(Owner, target, dmg);

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
        return string.Format("적에게 공격력의 {0}의 (#치명타적용 #스킬공격 #근거리 #관통)데미지를 주고, HP에 준 피해량 만큼 체력을 회복한다.",
            Owner.GetFinalStat(StatType.Str) * dmgCoefficient);
    }
}
