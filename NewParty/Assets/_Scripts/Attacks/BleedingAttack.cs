using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BleedingAttack : DmgAttack
{
    public static BleedingAttack Create(Unit caster, Unit target, float dmg) {
        BleedingAttack attack = Attack.Instantiate<BleedingAttack>(target.transform.position, Quaternion.identity);

        attack.Caster = caster;
        attack.Targets = new List<Unit> { target };
        attack.Dmg = dmg;

        return attack;
    }

    [SerializeField] protected GameObject fx;

    public override IEnumerator Animate() {
        foreach (Unit Target in new AttackTargetsSetting(this, Targets)) {
            yield return StartCoroutine(Hit(Target));
            yield return new WaitUntil(() => fx == null);
        }

        this.Destroy();
    }
}
