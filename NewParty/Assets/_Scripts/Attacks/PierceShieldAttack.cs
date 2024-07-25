using Photon.Pun;
using Photon.Pun.Demo.Procedural;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PierceShieldAttack : DmgAttack
{
    [SerializeField] protected GameObject fx;
    protected float defMul = 1f;
    protected int turn = 1;

    public void Init(Unit caster, Unit target, float defMul, int turn, float dmg) {
        Caster = caster;
        if (Targets.Count == 0)
            Targets.Add(target);
        else
            Targets[0] = target;
        this.defMul = defMul;
        this.turn = turn;
        Dmg = dmg;
    }

    public override IEnumerator Animate() {
        foreach (Unit Target in new AttackTargetsSetting(this, Targets)) {
            bool isHit = CalculateHit(Target);
            if (isHit) {
                yield return StartCoroutine(Hit(Target));
                StatTurnStatusEffect defDebuff = CreateStatTurnStatusEffect(StatForm.AbnormalMul, StatType.Def, StatusEffectForm.Debuff, defMul, turn);
                Target.AddStatusEffect(defDebuff);
            }
            else {
                yield return StartCoroutine(HitMiss(Target));
            }
        }

        yield return new WaitUntil(() => fx == null);

        this.Destroy();
    }

}
