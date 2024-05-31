using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BasicAttack : MonoBehaviourPun
{
    [SerializeField] protected GameObject fx;

    public Unit Caster { get; set; }
    public Unit Target { get; set; }

    public int TokenStack { get; set; } = 0;

    protected int remainHitNum = 1;
    
    private void Update() {
        if (fx == null) {
            Destroy(this.gameObject);
        }
    }

    public BasicAttack Init(Unit caster, Unit target, int tokenStack) {
        Caster = caster;
        Target = target;
        TokenStack = tokenStack;

        return this;
    }

    public IEnumerator Animate() {
        yield return StartCoroutine(Hit());
        yield return new WaitUntil(() => fx == null);
    }

    public IEnumerator Hit() {
        float dmg = Caster.GetFinalStat(StatType.Str) * (1f + Caster.GetFinalStat(StatType.StackStr) * TokenStack);

        DamageCalculator dc = new GameObject("DamageCalculator").AddComponent<DamageCalculator>();
        yield return StartCoroutine(dc.Advance(dmg, Caster, Target));
    }



}
