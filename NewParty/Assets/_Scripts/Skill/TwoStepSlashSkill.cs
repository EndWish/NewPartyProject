using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoStepSlashSkill : Skill
{
    [SerializeField] protected float coefficient;

    private void Awake() {
        Name = "2�� ����";
        IsPassive = false;
    }

    public override IEnumerator CoUse() {
        Owner.RemoveSelectedToken();

        // ������ �����Ѵ�
        Unit target = BattleSelectable.Units[0];
        TwoStepSlashAttack attack = PhotonNetwork.Instantiate(GameManager.GetAttackPrefabPath("TwoStepSlashAttack"),
            target.transform.position, Quaternion.identity)
        .GetComponent<TwoStepSlashAttack>();

        float dmg = Owner.GetFinalStat(StatType.Str) * coefficient;
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
        return Owner.TeamType != unit.TeamType;
    }

}
