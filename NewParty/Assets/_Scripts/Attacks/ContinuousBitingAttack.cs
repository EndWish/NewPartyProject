using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class ContinuousBitingAttack : DmgAttack
{
    [SerializeField] protected GameObject biteFxPrefab;

    protected int hitNum = 1;

    protected override void Awake() {
        base.Awake();
    }

    public void Init(Unit caster, Unit target, float dmg, int hitNum) {
        Caster = caster;
        if (Targets.Count == 0)
            Targets.Add(target);
        else
            Targets[0] = target;
        Dmg = dmg;
        this.hitNum = hitNum;
    }

    public override IEnumerator Animate() {
        foreach (Unit Target in new AttackTargetsSetting(this, Targets)) {
            bool hitResult = CalculateHit(Target);

            for (int i = 0; i < hitNum; ++i) {
                CreateFxRPC();
                yield return new WaitForSeconds(0.25f);
                if (hitResult)
                    yield return StartCoroutine(Hit(Target));
                else
                    yield return StartCoroutine(HitMiss(Target));
            }
        }

        yield return new WaitForSeconds(0.15f);
        this.Destroy();
    }

    [PunRPC]
    protected void CreateFxRPC() {
        Vector3 position = Targets[0].transform.position + Random.insideUnitCircle.ToVector3();
        Instantiate(biteFxPrefab, position, Quaternion.identity, Targets[0].transform);;
    }
    protected void CreateFx() {
        photonView.RPC("CreateFxRPC", RpcTarget.All);
    }
}
