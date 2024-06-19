using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoStepSlashAttack : Attack
{
    [SerializeField] protected GameObject[] slashFx = new GameObject[2];

    protected int remainHitNum = 1;

    protected override void Awake() {
        base.Awake();
        slashFx[0].SetActive(false);
        slashFx[1].SetActive(false);
    }

    public void Init(Unit caster, Unit target, float dmg) {
        Caster = caster;
        if (Targets.Count == 0)
            Targets.Add(target);
        else
            Targets[0] = target;
        Dmg = dmg;
    }

    public override IEnumerator Animate() {
        ActiveSlashFx(0);
        yield return StartCoroutine(CalculateAndHit(Targets[0]));
        yield return new WaitUntil(() => slashFx[0] == null);

        ActiveSlashFx(1);
        yield return StartCoroutine(CalculateAndHit(Targets[0]));
        yield return new WaitUntil(() => slashFx[1] == null);

        this.Destroy();
    }

    [PunRPC] protected void ActiveSlashFxRPC(int index) {
        slashFx[index].SetActive(true);
    }
    protected void ActiveSlashFx(int index) {
        photonView.RPC("ActiveSlashFxRPC", RpcTarget.All, index);
    }


}
