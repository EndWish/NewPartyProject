using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoStepSlashAttack : DmgAttack
{
    public static TwoStepSlashAttack Create(Unit caster, Unit target, float dmg) {
        TwoStepSlashAttack attack = Attack.Instantiate<TwoStepSlashAttack>(target.transform.position, Quaternion.identity);

        attack.Caster = caster;
        attack.Targets = new List<Unit> { target };
        attack.Dmg = dmg;

        return attack;
    }

    [SerializeField] protected GameObject[] slashFx = new GameObject[2];

    protected int remainHitNum = 1;

    protected override void Awake() {
        base.Awake();
        slashFx[0].SetActive(false);
        slashFx[1].SetActive(false);
    }

    public override IEnumerator Animate() {
        foreach (Unit Target in new AttackTargetsSetting(this, Targets)) {
            ActiveSlashFx(0);
            yield return StartCoroutine(CalculateAndHit(Target));
        }
        yield return new WaitUntil(() => slashFx[0] == null);

        foreach (Unit Target in new AttackTargetsSetting(this, Targets)) {
            ActiveSlashFx(1);
            yield return StartCoroutine(CalculateAndHit(Target));
        }
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
