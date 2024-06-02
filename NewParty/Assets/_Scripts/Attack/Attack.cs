using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public abstract class Attack : MonoBehaviourPun
{
    public Unit Caster { get; set; }
    public List<Unit> Targets { get; set; } = new List<Unit>();
    public float Dmg { get; set; }

    public List<Tag> InitTags;
    public Tags Tags { get; set; } = new Tags();

    protected virtual void Awake() {
        if(InitTags != null)
            Tags.AddTag(InitTags);
    }

    public abstract IEnumerator Animate();

    protected IEnumerator Hit(Unit target) {
        DamageCalculator dc = new GameObject("DamageCalculator").AddComponent<DamageCalculator>();
        yield return StartCoroutine(dc.Advance(Dmg, Caster, target, this));
    }

}
