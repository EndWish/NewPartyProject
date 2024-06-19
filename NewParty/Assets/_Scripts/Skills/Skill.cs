using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public abstract class Skill : MonoBehaviourPun
{

    private Unit owner;

    public Sprite IconSp;

    public string Name { get; protected set; }
    //public int Cost;
    public bool IsPassive { get; protected set; }
    
    public Unit Owner {
        get { return owner; }
        set { 
            if (owner == value) return;
            Unit prev = owner;
            owner = value;
            OnSetOwner(prev, owner);
        }
    }
    protected virtual void OnSetOwner(Unit prev, Unit current) {

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
        statusEffect.StatusEffectForm = statusEffectForm;
        statusEffect.Value = value;
        statusEffect.Turn = turn;
        statusEffect.Caster = Owner;
        SetStatStatusEffectIcon(statusEffect);

        return statusEffect;
    }
}
