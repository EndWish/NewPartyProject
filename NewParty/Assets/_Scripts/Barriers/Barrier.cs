using Photon.Pun;
using Photon.Pun.Demo.Procedural;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Barrier : MonoBehaviourPun
{
    private Unit caster;
    private Unit target;

    private float amount;

    [PunRPC] protected void CasterRPC(int viewId) {
        caster = (viewId == -1 ? null : PhotonView.Find(viewId).GetComponent<Unit>());
    }
    public Unit Caster {
        get { return caster; } 
        set {
            if (caster == value) return;
            photonView.RPC("CasterRPC", RpcTarget.All, value?.photonView.ViewID ?? -1);
        }
    }

    [PunRPC] protected virtual void TargetRPC(int viewId) {
        target = (viewId == -1 ? null : PhotonView.Find(viewId).GetComponent<Unit>());
        transform.parent = target?.transform;
        transform.localPosition = Vector3.zero;
    }
    public virtual Unit Target {
        get { return target; }
        set { photonView.RPC("TargetRPC", RpcTarget.All, value?.photonView.ViewID ?? -1); }
    }

    [PunRPC] protected void AmountRPC(float value) {
        amount = value;
    }
    public float Amount {
        get { return amount; }
        set { photonView.RPC("AmountRPC", RpcTarget.All, value); }
    }

    public IEnumerator TakeDmg(float dmg) {
        Amount -= dmg;

        if(Amount <= 0) {
            Destroy();
        }

        yield break;
    }

    [PunRPC] protected void DestroyRPC() {
        Destroy(gameObject);
    }
    public void Destroy() {
        Target?.RemoveBarrier(this);
        photonView.RPC("DestroyRPC", RpcTarget.AllBuffered);
    }

    public abstract float GetPriority();

}
