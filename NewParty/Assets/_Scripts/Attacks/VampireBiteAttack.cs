using Photon.Pun.Demo.Procedural;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VampireBiteAttack : DmgAttack
{
    public static VampireBiteAttack Create(Unit caster, Unit target, float dmg) {
        VampireBiteAttack attack = Attack.Instantiate<VampireBiteAttack>(target.transform.position, Quaternion.identity);

        attack.Caster = caster;
        attack.Targets = new List<Unit> { target };
        attack.Dmg = dmg;

        return attack;
    }

    [SerializeField] protected GameObject fx;

    public override IEnumerator Animate() {
        yield return new WaitForSeconds(0.25f);
        foreach (Unit Target in new AttackTargetsSetting(this, Targets)) {
            if (!Caster.IsDie) Caster.CoOnHitHp += this.CoOnHitHp;
            yield return StartCoroutine(CalculateAndHit(Target));
            if (!Caster.IsDie) Caster.CoOnHitHp -= this.CoOnHitHp;
        }
        yield return new WaitUntil(() => fx == null);

        this.Destroy();
    }

    protected IEnumerator CoOnHitHp(Unit damagedUnit, float dmg, float overDmg) {
        if (Caster.IsDie)
            yield break;

        Caster.RecoverHp(dmg);
    }

}
