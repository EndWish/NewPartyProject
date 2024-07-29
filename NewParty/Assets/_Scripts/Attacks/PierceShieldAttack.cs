using Photon.Pun;
using Photon.Pun.Demo.Procedural;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PierceShieldAttack : DmgAttack
{
    public static PierceShieldAttack Create(Unit caster, Unit target, float defMul, int turn, float dmg) {
        PierceShieldAttack attack = Attack.Instantiate<PierceShieldAttack>(target.transform.position, Quaternion.identity);

        attack.Caster = caster;
        attack.Targets = new List<Unit> { target };
        attack.defMul = defMul;
        attack.turn = turn;
        attack.Dmg = dmg;

        return attack;
    }

    [SerializeField] protected GameObject fx;
    protected float defMul = 1f;
    protected int turn = 1;

    public override IEnumerator Animate() {
        foreach (Unit target in new AttackTargetsSetting(this, Targets)) {
            bool isHit = CalculateHit(target);
            if (isHit) {
                yield return StartCoroutine(Hit(target));
                StatTurnStatusEffect.Create(Caster, target, StatForm.AbnormalMul, StatType.Def, StatusEffectForm.Debuff, defMul, turn);
            }
            else {
                yield return StartCoroutine(HitMiss(target));
            }
        }

        yield return new WaitUntil(() => fx == null);

        this.Destroy();
    }

}
