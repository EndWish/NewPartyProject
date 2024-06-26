using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static System.Collections.Specialized.BitVector32;

public enum StatusEffectForm
{
    None, Passive, Buff, Debuff,
}

public abstract class StatusEffect : MonoBehaviourPun
{
    [SerializeField] protected StatusEffectIcon seIconPrefab;
    protected StatusEffectIcon seIcon = null;

    [SerializeField] public Sprite IconSp;
    public string Name;
    public StatusEffectForm Form;

    public List<Tag> InitTags;
    public Tags Tags { get; set; } = new Tags();

    private Unit caster, target;

    protected virtual void Awake() {
        if (InitTags != null)
            Tags.AddTag(InitTags);

        seIcon = Instantiate(seIconPrefab);
        InitIcon();
    }

    protected virtual void OnDestroy() {
        Destroy(seIcon.gameObject);
    }

    [PunRPC] protected virtual void CasterRPC(int viewId) {
        if(viewId == -1) {
            caster = null;
        } else {
            caster = PhotonView.Find(viewId).GetComponent<Unit>();
        }
    }
    public virtual Unit Caster {
        get { return caster; }
        set { photonView.RPC("CasterRPC", RpcTarget.All, value?.photonView.ViewID ?? -1); } 
    }

    [PunRPC] protected virtual void TargetRPC(int viewId) {
        target = (viewId == -1 ? null : PhotonView.Find(viewId).GetComponent<Unit>());
        transform.parent = target?.transform;
        transform.localPosition = Vector3.zero;

        seIcon.transform.SetParent(Target?.StatusEffectIconParent);
        seIcon.transform.localScale = Vector3.one;
    }
    public virtual Unit Target {
        get { return target; }
        set { photonView.RPC("TargetRPC", RpcTarget.All, value?.photonView.ViewID ?? -1); }
    }


    protected virtual void InitIcon() {
        seIcon.IconImg.sprite = this.IconSp;
        seIcon.GetTooltipTitle = () => Name;
        seIcon.GetTooltipRightUpperText = () => {
            switch (Form) {
                case StatusEffectForm.Buff:
                    return "버프";
                case StatusEffectForm.Debuff:
                    return "디버프";
                case StatusEffectForm.Passive:
                    return "패시브";
                default:
                    return "형식 없음";
            }
        };
        seIcon.GetTooltipDescription = GetDescription;

    }

    public abstract string GetDescription();

    protected string FloatToNormalStr(float value) {
        if (100 <= value)
            return string.Format("{0:G}", value);
        return string.Format("{0:F2}", value);
    }

    protected T FindSameStatusEffect<T>(Unit unit) where T : StatusEffect {
        return (T)unit?.StatusEffects.Find((statusEffect) => statusEffect is T && statusEffect != this);
    }

    [PunRPC] protected void DestroyRPC() {
        Destroy(gameObject);
    }
    public void Destroy() {
        Target?.RemoveStatusEffect(this);
        photonView.RPC("DestroyRPC", RpcTarget.AllBuffered);
    }

}
