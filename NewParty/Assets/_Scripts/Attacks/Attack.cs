using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public abstract class Attack : MonoBehaviourPun
{
    public Unit Caster { get; set; }
    public List<Unit> Targets { get; set; } = new List<Unit>();

    public List<Tag> InitTags;
    public Tags Tags { get; set; } = new Tags();

    public Sprite IconSp;

    protected virtual void Awake() {
        if(InitTags != null)
            Tags.AddTag(InitTags);
    }

    public abstract IEnumerator Animate();

    public bool CalculateHit(Unit target) {
        HitCalculator hc = new GameObject("HitCalculator").AddComponent<HitCalculator>();
        return hc.Calculate(Caster, target, this);
    }
    protected virtual IEnumerator Hit(Unit target) {
        yield return StartCoroutine(GameManager.CoInvoke(Caster.CoOnHit, target, this));
    }
    protected IEnumerator HitMiss(Unit target) {
        yield return StartCoroutine(GameManager.CoInvoke(Caster.CoOnHitMiss, target, this));
        yield return StartCoroutine(GameManager.CoInvoke(target.CoOnAvoid));
    }
    protected IEnumerator CalculateAndHit(Unit target) {
        bool isHit = CalculateHit(target);
        if (isHit) {
            yield return StartCoroutine(Hit(target));
        } else {
            yield return StartCoroutine(HitMiss(target));
        }
    }

    protected void HitStun(float basicStunCha, Unit target, int turn) {
        IStunAttack stunAttack = (IStunAttack)this;

        stunAttack.StunCha = basicStunCha * target.GetFinalStat(StatType.StunSen);
        if (UnityEngine.Random.Range(0f, 1f) <= stunAttack.StunCha) {
            StunTurnDebuff statusEffect = PhotonNetwork.Instantiate(GameManager.GetStatusEffectPrefabPath("StunTurnDebuff"),
            target.transform.position, Quaternion.identity)
            .GetComponent<StunTurnDebuff>();
            statusEffect.Turn = turn;
            statusEffect.Caster = Caster;
            target.AddStatusEffect(statusEffect);
        }
    }

    [PunRPC]
    protected void DestroyRPC() {
        Destroy(gameObject);
    }
    public void Destroy() {
        photonView.RPC("DestroyRPC", RpcTarget.AllBuffered);
    }

    protected StatTurnStatusEffect CreateStatTurnStatusEffect(StatForm statForm, StatType statType, StatusEffectForm statusEffectForm, float value, int turn) {
        StatTurnStatusEffect statusEffect = PhotonNetwork.Instantiate(GameManager.GetStatusEffectPrefabPath("StatTurnStatusEffect"),
            transform.position, Quaternion.identity)
            .GetComponent<StatTurnStatusEffect>();
        statusEffect.StatForm = statForm;
        statusEffect.StatType = statType;
        statusEffect.Form = statusEffectForm;
        statusEffect.Value = value;
        statusEffect.Turn = turn;
        statusEffect.Caster = Caster;

        return statusEffect;
    }



}
