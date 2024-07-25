using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MassTransfusionAttack : Attack
{
    protected float healingAmount;

    public void Init(Unit caster, Unit target, float healingAmount) {
        Caster = caster;
        if (Targets.Count == 0)
            Targets.Add(target);
        else
            Targets[0] = target;
        this.healingAmount = healingAmount;
    }

    public override IEnumerator Animate() {
        foreach (Unit Target in new AttackTargetsSetting(this, Targets)) {
            Vector3 randomOffset = Random.insideUnitCircle;
            
            float speed = 5f;
            while (true) {
                Vector3 targetPos = Target.transform.position;
                if (targetPos == transform.position)
                    break;

                transform.position = Vector3.MoveTowards(transform.position, transform.position + randomOffset, 10f * Time.deltaTime);
                transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
                speed += 10f * Time.deltaTime;

                yield return null;
            }

            Target.RecoverHp(healingAmount);
        }

        this.Destroy();
    }

}
