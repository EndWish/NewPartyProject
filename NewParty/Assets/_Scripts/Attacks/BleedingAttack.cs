using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BleedingAttack : Attack
{
    [SerializeField] protected GameObject fx;

    public void Init(Unit caster, Unit target, float dmg) {
        Caster = caster;
        if (Targets.Count == 0)
            Targets.Add(target);
        else
            Targets[0] = target;
        Dmg = dmg;
    }

    public override IEnumerator Animate() {
        yield return StartCoroutine(Hit(Targets[0]));
        yield return new WaitUntil(() => fx == null);

        this.Destroy();
    }
}
