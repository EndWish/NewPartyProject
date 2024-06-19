using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class TwoStepSlashSkill : ActiveSkill
{
    [SerializeField] protected float dmgCoefficient;

    protected override void Awake() {
        base.Awake();
        Name = "2단 베기";
    }

    public override IEnumerator CoUse() {
        Owner.RemoveSelectedToken();

        // 공격을 생성한다
        Unit target = BattleSelectable.Units[0];
        TwoStepSlashAttack attack = PhotonNetwork.Instantiate(GameManager.GetAttackPrefabPath("TwoStepSlashAttack"),
            target.transform.position, Quaternion.identity)
        .GetComponent<TwoStepSlashAttack>();

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
        return string.Format("적을 두번 베어 공격한다. {0} 데미지를 두번 준다.",
            Owner.GetFinalStat(StatType.Str) * dmgCoefficient);
    }

}
