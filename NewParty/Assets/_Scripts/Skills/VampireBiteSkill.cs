using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VampireBiteSkill : ActiveSkill
{
    [SerializeField] protected float dmgCoefficient;

    protected override void Awake() {
        base.Awake();
        Name = "���� ����";
    }

    public override IEnumerator CoUse() {
        Owner.RemoveSelectedToken();

        // ������ �����Ѵ�
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
        return string.Format("������ ���ݷ��� {0}�� (#ġ��Ÿ���� #��ų���� #�ٰŸ� #����)�������� �ְ�, HP�� �� ���ط� ��ŭ ü���� ȸ���Ѵ�.",
            Owner.GetFinalStat(StatType.Str) * dmgCoefficient);
    }
}
