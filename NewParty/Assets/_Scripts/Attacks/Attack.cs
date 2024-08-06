using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Attack : MonoBehaviourPun
{
    static public string GetPrefabPath() {
        return "Prefabs/Attacks/";
    }
    static public string GetPrefabPath(string prefabName) {
        return GetPrefabPath() + prefabName;
    }

    protected static T Instantiate<T>(Vector3 pos, Quaternion quaternion) where T : Attack {
        return PhotonNetwork.Instantiate(Attack.GetPrefabPath(typeof(T).Name), pos, quaternion).GetComponent<T>();
    }

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
            StunTurnDebuff.Create(Caster, target, turn);
        }
    }

    [PunRPC]
    protected void DestroyRPC() {
        Destroy(gameObject);
    }
    public void Destroy() {
        photonView.RPC("DestroyRPC", RpcTarget.AllBuffered);
    }

}
