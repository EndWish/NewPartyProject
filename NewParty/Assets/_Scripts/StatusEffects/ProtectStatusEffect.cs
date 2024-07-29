using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtectStatusEffect : TurnStatusEffect
{
    public static ProtectStatusEffect Create(Unit caster, Unit target, int turn) {
        ProtectStatusEffect statusEffect = StatusEffect.Instantiate<ProtectStatusEffect>();

        statusEffect.Turn = turn;
        statusEffect.Caster = caster;
        target.AddStatusEffect(statusEffect);

        return statusEffect;
    }

    protected override void OnDestroy() {
        base.OnDestroy();
        if (Target != null) {
            Target.CoOnBeginTick -= CoOnBeginTurn;
            Target.OnBecomeAttackTarget -= OnBecomeAttackTarget;
        }
    }

    [PunRPC]
    protected override void TargetRPC(int viewId) {
        if (Target != null) {
            Target.CoOnBeginMyTurn -= CoOnBeginTurn;
            Target.OnBecomeAttackTarget -= OnBecomeAttackTarget;
        }

        base.TargetRPC(viewId);

        if (Target != null) {
            Target.CoOnBeginMyTurn += CoOnBeginTurn;
            Target.OnBecomeAttackTarget += OnBecomeAttackTarget;
        }
    }

    public override string GetDescription() {
        return string.Format("{0}�ϰ� ��ȣ�� �޽��ϴ�. ��ȣ�� �޴� ������ �ڽ��� �ڽ��� ������ Ÿ���� �Ǿ��� �� ��ų �����ڰ� ��� Ÿ���� �Ǿ��ش�." +
            "\n���� �����ڿ� ���ÿ� ������ Ÿ���� �ȴٸ� ��ȣ��ų�� �ߵ����� �ʴ´�." +
            "\n�����ڰ� ������ ��� ��ȣ ��ų�� ��ҵȴ�." +
            "\n\n������ : {1}",
            turn, Caster.Name);
    }

    protected IEnumerator CoOnBeginTurn() {
        Turn -= 1;
        yield break;
    }

    protected void OnBecomeAttackTarget(Attack attack, AttackTargetsSetting attackTargetsSetting) {
        // �ߵ����� �ʴ� ���� : ������ ����, �����ڿ� �Բ� ���ݴ���� �Ǵ� ���
        if(Target.TeamType == attack.Caster.TeamType) {
            return;
        }
        List<Unit> targetList = attackTargetsSetting.TargetList;
        foreach (Unit unit in targetList) {
            if (unit == Caster)
                return;
        }

        int index = targetList.FindIndex(unit => unit == Target);
        targetList[index] = Caster;
        Caster.GetComponent<UnitController>().SetPosition(Target.transform.position);
    }

}
