using System.Collections;
using UnityEngine;

public abstract class BasicAttack : DmgAttack
{
    [SerializeField] protected GameObject fx;

    public int TokenStack { get; set; } = 0;

    public override IEnumerator Animate() {
        foreach(Unit Target in new AttackTargetsSetting(this, Targets)) {
            yield return StartCoroutine(CalculateAndHit(Target));
        }

        yield return new WaitUntil(() => fx == null);

        this.Destroy();
    }

}
