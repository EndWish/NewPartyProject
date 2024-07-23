using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuakeSkill : ActiveSkill
{
    [SerializeField] protected float dmgCoefficient;

    protected override void Awake() {
        base.Awake();
        Name = "����";
    }

    public override IEnumerator CoUse() {
        yield return StartCoroutine(Owner.UseSelectedTokens());

        // ������ �����Ѵ�
        Unit target = BattleSelectable.Units[0];
        QuakeAttack attack = PhotonNetwork.Instantiate(GameManager.GetAttackPrefabPath("QuakeAttack"),
            target.transform.position, Quaternion.identity)
        .GetComponent<QuakeAttack>();

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
        return string.Format("{0} �������� �ְ�, ������� ���� Ȯ���� ���� ���� ��Ų��. \n ����� = ���� / (���� + ����� ��� �����)",
            Owner.GetFinalStat(StatType.Str) * dmgCoefficient);
    }

}
