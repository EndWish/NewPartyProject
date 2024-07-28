using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfectionSkill : PassiveSkill
{
    [SerializeField] protected float chance;
    [SerializeField] protected int removeNum;
    [SerializeField] protected GameObject prevTargetMark;
    [SerializeField] protected GameObject fxPrefab;

    protected Unit prevTarget;

    protected override void Awake() {
        base.Awake();
        Name = "감염";

        prevTargetMark.SetActive(false);
    }

    protected void Update() {
        if (prevTargetMark.activeSelf && prevTarget != null) {
            prevTargetMark.transform.position = prevTarget.transform.position;
        }
    }

    protected void OnDestroy() {
        if (Owner != null) { 
            Owner.CoOnHit -= this.CoOnHit;
            Owner.CoOnBeginMyTurn -= this.CoOnBeginMyTurn;
            Owner.CoOnEndMyTurn -= this.CoOnEndMyTurn;
        }
    }

    [PunRPC]
    protected override void OwnerRPC(int viewId) {
        if (Owner != null) {
            Owner.CoOnHit -= this.CoOnHit;
            Owner.CoOnBeginMyTurn -= this.CoOnBeginMyTurn;
            Owner.CoOnEndMyTurn -= this.CoOnEndMyTurn;
        }
        base.OwnerRPC(viewId);
        if (Owner != null) {
            Owner.CoOnHit += this.CoOnHit;
            Owner.CoOnBeginMyTurn += this.CoOnBeginMyTurn;
            Owner.CoOnEndMyTurn += this.CoOnEndMyTurn;
        }
        else {
            prevTargetMark.SetActive(false);
        }
    }

    public override string GetDescription() {
        return string.Format("기본 공격 또는 스킬 공격을 최근에 공격하지 않은 적에게 맞추면 {0} 확률로 랜덤한 토큰 {1}개를 감염시켜 제거한다.",
            TooltipText.SetPercentFont(chance),
            TooltipText.SetCountFont(removeNum));
    }
    public override string GetDetailedDescription() {
        return string.Format("기본 공격 또는 스킬 공격을 최근에 공격하지 않은 적에게 맞추면 {0} 확률로 랜덤한 토큰 {1}개를 감염시켜 제거한다.",
            TooltipText.SetPercentFont(chance),
            TooltipText.SetCountFont(removeNum));
    }

    protected IEnumerator CoOnHit(Unit target, Attack attack) {
        if(attack.Tags.ContainsAtLeastOne(new Tags(Tag.기본공격, Tag.스킬공격 ))) {
            if(prevTarget != target && Random.Range(0f,1f) <= chance) {
                yield return StartCoroutine(Owner.DiscardRandomToken());
                CreateFX(target);
            }
            prevTarget = target;
        }
        yield break;
    }

    protected IEnumerator CoOnBeginMyTurn() {
        if(prevTarget != null) {
            prevTargetMark.SetActive(true);
        }
        yield break;
    }
    protected IEnumerator CoOnEndMyTurn() {
        prevTargetMark.SetActive(false);
        yield break;
    }

    [PunRPC] protected void CreateFXRPC(int viewId) {
        if (viewId == -1)
            return;

        Vector3 pos = PhotonView.Find(viewId).transform.position;
        Instantiate(fxPrefab, pos, Quaternion.identity);
    }
    public void CreateFX(Unit target) {
        photonView.RPC("CreateFXRPC", RpcTarget.All, target?.photonView.ViewID ?? -1);
    }

}
