using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private Unit caster, target;

    protected virtual void OnDestroy() {
        ReversApply();
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
    }
    public virtual Unit Target {
        get { return target; }
        set { photonView.RPC("TargetRPC", RpcTarget.All, value?.photonView.ViewID ?? -1); }
    }

    public virtual void Apply() {
        if (Target == null) return;
        if(seIcon == null) { 
            seIcon = Instantiate(seIconPrefab);
            InitIcon();
        }
        seIcon.transform.SetParent(Target.StatusEffectIconParent);
    }
    public abstract void ReversApply();
    protected virtual void InitIcon() {
        seIcon.IconImg.sprite = this.IconSp;
        seIcon.GetTooltipTitle = () => Name;
        seIcon.GetTooltipRightUpperText = () => {
            switch (Form) {
                case StatusEffectForm.Buff:
                    return "����";
                case StatusEffectForm.Debuff:
                    return "�����";
                case StatusEffectForm.Passive:
                    return "�нú�";
                default:
                    return "���� ����";
            }
        };
        seIcon.GetTooltipDescription = GetDescription;

    }

    public abstract string GetDescription();

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