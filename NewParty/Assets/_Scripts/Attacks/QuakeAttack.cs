using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;

public class QuakeAttack : DmgAttack, IStunAttack
{
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

    public void Init(Unit caster, Unit target, float dmg) {
        Caster = caster;
        if (Targets.Count == 0)
            Targets.Add(target);
        else
            Targets[0] = target;
        Dmg = dmg;
    }

    public override IEnumerator Animate() {
        foreach (Unit Target in new AttackTargetsSetting(this, Targets)) {
            bool isHit = CalculateHit(Target);
            if (isHit) {
                yield return StartCoroutine(Hit(Target));
                StunCha = DamageCalculator.CalculateDefRate(Caster.GetFinalStat(StatType.Def), Target.GetFinalStat(StatType.DefPen));
                if (UnityEngine.Random.Range(0f, 1f) <= StunCha) {
                    StunTurnDebuff statusEffect = PhotonNetwork.Instantiate(GameManager.GetStatusEffectPrefabPath("StunTurnDebuff"),
                    Target.transform.position, Quaternion.identity)
                    .GetComponent<StunTurnDebuff>();
                    statusEffect.Turn = turn;
                    statusEffect.Caster = Caster;
                    Target.AddStatusEffect(statusEffect);
                }
            }
            else {
                yield return StartCoroutine(HitMiss(Target));
            }
        }


        yield return new WaitUntil(() => fx == null);

        this.Destroy();
    }



}
