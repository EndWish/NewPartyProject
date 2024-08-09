using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VampireBiteSkill : ActiveSkill
{
    [SerializeField] protected float dmgCoefficient;

    [SerializeField] protected VampireBiteAttack attackPrefab;

    protected override void Awake() {
        base.Awake();
        Name = "���� ����";
    }

    public override IEnumerator CoUse() {
        yield return StartCoroutine(Owner.UseSelectedTokens());

        // ������ �����Ѵ�
        Unit target = BattleSelectable.Units[0];
        VampireBiteAttack attack = VampireBiteAttack.Create(Owner, target, CalculateDmg());

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

    public override string GetDescriptionText() {
        return string.Format("������ {0}�������� �ְ�, HP�� �� ���ط� ��ŭ ü���� ȸ���Ѵ�.",
            TooltipText.SetDamageFont(CalculateDmg()));
    }
    public override string GetDetailedDescriptionText() {
        return string.Format("������ {0} = ({1}{2}%)������({3})�� �ְ�, HP�� �� ���ط� ��ŭ ü���� ȸ���Ѵ�.",
            TooltipText.SetDamageFont(CalculateDmg()),
            TooltipText.GetIcon(StatType.Str),
            TooltipText.GetFlexibleFloat(dmgCoefficient * 100f),
            Tags.GetString(attackPrefab.InitTags));
    }

    protected float CalculateDmg() {
        return Owner.GetFinalStat(StatType.Str) * dmgCoefficient;
    }

}
