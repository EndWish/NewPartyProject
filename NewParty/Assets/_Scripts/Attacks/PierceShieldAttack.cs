using Photon.Pun;
using Photon.Pun.Demo.Procedural;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PierceShieldAttack : Attack
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
        bool isHit = CalculateHit(Targets[0]);
        if (isHit) {
            yield return StartCoroutine(Hit(Targets[0]));
            StatTurnStatusEffect defDebuff = CreateStatTurnStatusEffect(StatForm.AbnormalMul, StatType.Def, StatusEffectForm.Debuff, defMul, turn);
            Targets[0].AddStatusEffect(defDebuff);
        }
        else {
            yield return StartCoroutine(GameManager.CoInvoke(Targets[0].CoOnAvoid));
        }
        yield return new WaitUntil(() => fx == null);

        this.Destroy();
    }

}
