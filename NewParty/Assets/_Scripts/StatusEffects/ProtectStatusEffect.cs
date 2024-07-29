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
        return string.Format("{0}턴간 보호를 받습니다. 보호를 받는 유닛은 자신이 자신이 공격의 타겟이 되었을 때 스킬 시전자가 대신 타겟이 되어준다." +
            "\n만약 시전자와 동시에 공격의 타겟이 된다면 보호스킬이 발동되지 않는다." +
            "\n시전자가 기절할 경우 보호 스킬이 취소된다." +
            "\n\n시전자 : {1}",
            turn, Caster.Name);
    }

    protected IEnumerator CoOnBeginTurn() {
        Turn -= 1;
        yield break;
    }

    protected void OnBecomeAttackTarget(Attack attack, AttackTargetsSetting attackTargetsSetting) {
        // 발동하지 않는 조건 : 팀원의 공격, 시전자와 함께 공격대상이 되는 경우
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
