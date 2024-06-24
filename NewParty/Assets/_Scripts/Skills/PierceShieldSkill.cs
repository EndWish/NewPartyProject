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
        Name = "���� �ձ�";
    }

    public override IEnumerator CoUse() {
        Owner.RemoveSelectedToken();

        // ������ �����Ѵ�
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
        return string.Format("������ ���ݷ��� {0}%��  (#��ų���� #�ٰŸ� #����)�������� �ְ� {1}�ϰ� ������ x{2:F2} ���ҽ�Ų��.",
            Owner.GetFinalStat(StatType.Str) * dmgCoefficient, turn, defMul);
    }
}
