using Photon.Pun;
using Photon.Pun.Demo.Procedural;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Barrier : MonoBehaviourPun
{
    [SerializeField] protected DamageText damageTextPrefab;

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
            Unit prev = caster;
            photonView.RPC("CasterRPC", RpcTarget.All, value?.photonView.ViewID ?? -1);
            OnSetCaster(prev, caster);
        }
    }
    protected virtual void OnSetCaster(Unit prev, Unit current) {

    }

    [PunRPC] protected void TargetRPC(int viewId) {
        target = (viewId == -1 ? null : PhotonView.Find(viewId).GetComponent<Unit>());
        transform.parent = target?.transform;
        transform.localPosition = Vector3.zero;
    }
    public Unit Target {
        get { return target; }
        set {
            if (target == value) return;
            Unit prev = target;
            photonView.RPC("TargetRPC", RpcTarget.All, value?.photonView.ViewID ?? -1);
            OnSetTarget(prev, target);
        }
    }
    protected virtual void OnSetTarget(Unit prev, Unit current) {

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

    public void Destroy() {
        if (Target != null)
            Target.RemoveBarrier(this);
        PhotonNetwork.Destroy(gameObject);
    }

    public abstract float GetPriority();

}
