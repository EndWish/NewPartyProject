using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuakeAttack : DmgAttack, IStunAttack
{
    public static QuakeAttack Create(Unit caster, Unit target, float dmg) {
        QuakeAttack attack = Attack.Instantiate<QuakeAttack>(target.transform.position, Quaternion.identity);

        attack.Caster = caster;
        attack.Targets = new List<Unit> { target };
        attack.Dmg = dmg;

        return attack;
    }

    [SerializeField] protected GameObject fx;
    protected float defMul = 1f;
    protected int turn = 1;

    private Ref<float> stunCha = new Ref<float>();
    public Action<Ref<float>> OnSetStunCha { get; set; }

    public float StunCha {
        get { return stunCha.Value; }
        set {
            stunCha.Value = value;
            OnSetStunCha?.Invoke(stunCha);
        }
    }

    public override IEnumerator Animate() {
        foreach (Unit Target in new AttackTargetsSetting(this, Targets)) {
            bool isHit = CalculateHit(Target);
            if (isHit) {
                yield return StartCoroutine(Hit(Target));
                HitStun(DamageCalculator.CalculateDefRate(Caster.GetFinalStat(StatType.Def), Target.GetFinalStat(StatType.DefPen)), Target, turn);
            }
            else {
                yield return StartCoroutine(HitMiss(Target));
            }
        }


        yield return new WaitUntil(() => fx == null);

        this.Destroy();
    }



}
