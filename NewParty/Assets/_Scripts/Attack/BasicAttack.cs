using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class BasicAttack : Attack
{
    [SerializeField] protected GameObject fx;

    public int TokenStack { get; set; } = 0;

    protected int remainHitNum = 1;

    public void Init(Unit caster, Unit target, int tokenStack, float dmg) {
        Caster = caster;
        if (Targets.Count == 0)
            Targets.Add(target);
        else
            Targets[0] = target;
        TokenStack = tokenStack;
        Dmg = dmg;
    }

    public override IEnumerator Animate() {
        yield return StartCoroutine(Hit(Targets[0]));
        yield return new WaitUntil(() => fx == null);

        PhotonNetwork.Destroy(this.gameObject);
    }

}
