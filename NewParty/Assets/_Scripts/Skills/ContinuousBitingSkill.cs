using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContinuousBitingSkill : ActiveSkill
{
    [SerializeField] protected float dmgCoefficient;
    [SerializeField] protected int hitNum;

    protected override void Awake() {
        base.Awake();
        Name = "���� ����";
    }

    public override IEnumerator CoUse() {
        yield return StartCoroutine(Owner.UseSelectedTokens());

        // ������ �����Ѵ�
        Unit target = BattleSelectable.Units[0];
        ContinuousBitingAttack attack = PhotonNetwork.Instantiate(GameManager.GetAttackPrefabPath("ContinuousBitingAttack"),
            target.transform.position, Quaternion.identity)
        .GetComponent<ContinuousBitingAttack>();

        float dmg = Owner.GetFinalStat(StatType.Str) * dmgCoefficient;
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
        return string.Format("������ {0}ȸ �������� ���ݷ��� {1} ������(#ġ��Ÿ ����  #�ٰŸ� #����)�� �ش�.",
            hitNum, Owner.GetFinalStat(StatType.Str) * dmgCoefficient);
    }
}
