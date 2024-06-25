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
        Vector3 randomOffset = Random.insideUnitCircle;
        Vector3 targetPos = Targets[0].transform.position;
        float speed = 5f;
        while (targetPos != transform.position) {
            transform.position = Vector3.MoveTowards(transform.position, transform.position + randomOffset, 10f * Time.deltaTime);
            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            speed +=10f * Time.deltaTime;

            yield return null;
        }

        Targets[0].RecoverHp(healingAmount);
        this.Destroy();
    }

}
