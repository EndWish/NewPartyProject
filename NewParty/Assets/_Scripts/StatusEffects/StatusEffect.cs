using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public enum StatusEffectForm
{
    None, Special, Buff, Debuff,
}

public abstract class StatusEffect : MonoBehaviourPun, IStatusEffectIconable
{
    static public string GetPrefabPath() {
        return "Prefabs/StatusEffects/";
    }
    static public string GetPrefabPath(string prefabName) {
        return GetPrefabPath() + prefabName;
    }

    protected static T Instantiate<T>() where T : StatusEffect {
        T statusEffect = PhotonNetwork.Instantiate(StatusEffect.GetPrefabPath(typeof(T).Name),
            Vector3.zero, Quaternion.identity)
            .GetComponent<T>();

        return statusEffect;
    }

    [SerializeField] protected Sprite iconSprite;
    public string Name;
    public StatusEffectForm form;

    public List<Tag> InitTags;
    public Tags Tags { get; set; } = new Tags();

    private Unit caster, target;

    protected virtual void Awake() {
        if (InitTags != null)
            Tags.AddTag(InitTags);
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

    [PunRPC]
    protected virtual void FormRPC(StatusEffectForm form) {
        this.form = form;
    }
    public StatusEffectForm Form {
        get { return form; }
        set { photonView.RPC("FormRPC", RpcTarget.All, value); }
    }

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

    // IStatusEffectIconable
    public Sprite GetMainSprite1x1() {
        return iconSprite;
    }
    public List<Sprite> GetMainSprites1x1() {
        return new List<Sprite> { iconSprite };
    }
    public bool IsSEVisible() {
        return true;
    }
    public Color GetBgColor() {
        switch (Form) {
            case StatusEffectForm.Buff:
                return Color.green;
            case StatusEffectForm.Debuff:
                return Color.red;
            case StatusEffectForm.Special:
                return Color.gray;
            default:
                return Color.black;
        }
    }
    public string GetTooltipTitleText() {
        return Name;
    }
    public string GetTooltipRightUpperText() {
        switch (Form) {
            case StatusEffectForm.Buff:
                return "버프";
            case StatusEffectForm.Debuff:
                return "디버프";
            case StatusEffectForm.Special:
                return "특수";
            default:
                return "형식 없음";
        }
    }
    public abstract string GetDescriptionText();
}
