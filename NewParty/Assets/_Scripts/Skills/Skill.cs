using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using static System.Collections.Specialized.BitVector32;
using static UnityEngine.GraphicsBuffer;

public abstract class Skill : MonoBehaviourPun, IIcon1x1
{

    private Unit owner;

    [FormerlySerializedAs("IconSp")]
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

    public abstract string GetDescription();
    public abstract string GetDetailedDescription();

    public Sprite GetIcon1x1() {
        return iconSprite;
    }
    public List<Sprite> GetIcons1x1() {
        return new List<Sprite> { iconSprite };
    }
}
