using System;
using System.Collections;
using UnityEngine;



public abstract class DmgAttack : Attack, IDmgAttack
{
    private Ref<float> dmg = new Ref<float>();
    public Action<Ref<float>> OnSetDmg { get; set; }

    public float Dmg {
        get { return dmg.Value; }
        set { 
            dmg.Value = value;
            OnSetDmg?.Invoke(dmg);
        }
    }

    protected override IEnumerator Hit(Unit target) {
        yield return StartCoroutine(base.Hit(target));
        DamageCalculator dc = new GameObject("DamageCalculator").AddComponent<DamageCalculator>();
        yield return StartCoroutine(dc.Advance(Dmg, Caster, target, this));
    }

}
