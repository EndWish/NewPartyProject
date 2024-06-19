using Photon.Pun.Demo.Procedural;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VampireBiteAttack : Attack
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
        yield return new WaitForSeconds(0.25f);
        if (!Caster.IsDie) Caster.CoOnHitHp += this.CoOnHitHp;
        yield return StartCoroutine(CalculateAndHit(Targets[0]));
        if (!Caster.IsDie) Caster.CoOnHitHp -= this.CoOnHitHp;
        yield return new WaitUntil(() => fx == null);

        this.Destroy();
    }

    protected IEnumerator CoOnHitHp(Unit damagedUnit, float dmg, float overDmg) {
        if (Caster.IsDie)
            yield break;

        Caster.RecoverHp(dmg);
    }

}
