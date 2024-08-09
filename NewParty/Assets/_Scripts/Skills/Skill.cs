using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class Skill : MonoBehaviourPun, IMainSprite1x1, ITooltipable, IDetailedDescription
{

    private Unit owner;

    [SerializeField] protected Sprite iconSprite;

    public string Name { get; protected set; }
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

    // IMainSprite1x1
    public Sprite GetMainSprite1x1() {
        return iconSprite;
    }
    public List<Sprite> GetMainSprites1x1() {
        return new List<Sprite> { iconSprite };
    }

    // ITooltipable
    public string GetTooltipTitleText() {
        return Name;
    }
    public abstract string GetTooltipRightUpperText();
    public abstract string GetDescriptionText();
    public abstract string GetDetailedDescriptionText();
}
