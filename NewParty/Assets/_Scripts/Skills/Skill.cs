using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static System.Collections.Specialized.BitVector32;
using static UnityEngine.GraphicsBuffer;

public abstract class Skill : MonoBehaviourPun
{

    private Unit owner;

    public Sprite IconSp;

    public string Name { get; protected set; }
    //public int Cost;
    public bool IsPassive { get; protected set; }

    [PunRPC]
    protected virtual void OwnerRPC(int viewId) {
        owner = viewId == -1 ? null : PhotonView.Find(viewId).GetComponent<Unit>();
    }
    public Unit Owner {
        get { return owner; }
        set { 
            if (owner == value) return;
            photonView.RPC("OwnerRPC", RpcTarget.All, value?.photonView.ViewID ?? -1);
        }
    }

    public abstract string GetDescription();

    [PunRPC] protected void SetStatStatusEffectIconRPC(int viewId) {
        if(viewId == -1) return;
        StatStatusEffect statStatusEffect = PhotonView.Find(viewId).GetComponent<StatStatusEffect>();
        statStatusEffect.SetIconSp(IconSp);
    }
    protected void SetStatStatusEffectIcon(StatStatusEffect statStatusEffect) {
        photonView.RPC("SetStatStatusEffectIconRPC", RpcTarget.All, statStatusEffect.photonView.ViewID);
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
        statusEffect.Caster = Owner;
        SetStatStatusEffectIcon(statusEffect);
        statusEffect.InitIcon();

        return statusEffect;
    }
}
