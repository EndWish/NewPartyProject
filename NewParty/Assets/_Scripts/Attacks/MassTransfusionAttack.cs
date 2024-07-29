using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MassTransfusionAttack : Attack
{
    public static MassTransfusionAttack Create(Unit caster, Unit target, float healingAmount) {
        MassTransfusionAttack attack = Attack.Instantiate<MassTransfusionAttack>(caster.transform.position, Quaternion.identity);

        attack.Caster = caster;
        attack.Targets = new List<Unit> { target };
        attack.healingAmount = healingAmount;

        return attack;
    }

    protected float healingAmount;

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
