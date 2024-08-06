using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StatusEffectForm
{
    None, Special, Buff, Debuff,
}

public abstract class StatusEffect : MonoBehaviourPun
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

    [SerializeField] protected StatusEffectIcon seIconPrefab;
    protected StatusEffectIcon seIcon = null;

    [SerializeField] public Sprite IconSp;
    public string Name;
    public StatusEffectForm form;

    public List<Tag> InitTags;
    public Tags Tags { get; set; } = new Tags();

    private Unit caster, target;

    protected virtual void Awake() {
        if (InitTags != null)
            Tags.AddTag(InitTags);

        seIcon = Instantiate(seIconPrefab);
    }

    protected virtual void Start() {
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

    [PunRPC]
    protected virtual void FormRPC(StatusEffectForm form) {
        this.form = form;
    }
    public StatusEffectForm Form {
        get { return form; }
        set { photonView.RPC("FormRPC", RpcTarget.All, value); }
    }

    public virtual void InitIcon() {
        seIcon.IconImg.sprite = this.IconSp;
        seIcon.GetTooltipTitle = () => Name;
        seIcon.GetTooltipRightUpperText = () => {
            switch (Form) {
                case StatusEffectForm.Buff:
                    return "버프";
                case StatusEffectForm.Debuff:
                    return "디버프";
                case StatusEffectForm.Special:
                    return "패시브";
                default:
                    return "형식 없음";
            }
        };
        switch (Form) {
            case StatusEffectForm.Buff:
                seIcon.BgImg.color = Color.green; break;
            case StatusEffectForm.Debuff:
                seIcon.BgImg.color = Color.red; break;
            case StatusEffectForm.Special:
                seIcon.BgImg.color = Color.gray; break;
            default:
                seIcon.BgImg.color = Color.black; break;
        }

        

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
