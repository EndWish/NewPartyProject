using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PierceShieldSkill : ActiveSkill
{
    [SerializeField] protected float dmgCoefficient;
    [SerializeField] protected float defMul;
    [SerializeField] protected int turn;

    protected override void Awake() {
        base.Awake();
        Name = "방패 뚫기";
    }

    public override IEnumerator CoUse() {
        Owner.RemoveSelectedToken();

        // 공격을 생성한다
        Unit target = BattleSelectable.Units[0];
        PierceShieldAttack attack = PhotonNetwork.Instantiate(GameManager.GetAttackPrefabPath("PierceShieldAttack"),
            target.transform.position, Quaternion.identity)
        .GetComponent<PierceShieldAttack>();

        float dmg = Owner.GetFinalStat(StatType.Str) * dmgCoefficient;
        attack.Init(Owner, target, defMul, turn, dmg);

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
        return string.Format("적에게 공격력의 {0}%의  (#스킬공격 #근거리 #관통)데미지를 주고 {1}턴간 방어력을 x{2:F2} 감소시킨다.",
            Owner.GetFinalStat(StatType.Str) * dmgCoefficient, turn, defMul);
    }
}
