using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class StatStatusEffect : StatusEffect
{
    protected StatType statType;
    protected StatForm statForm;
    protected StatusEffectForm statusEffectForm;
    protected float value = 1f;

    protected override void OnDestroy() {
        base.OnDestroy();
        ReversApply();
    }

    [PunRPC] protected override void TargetRPC(int viewId) {
        ReversApply();
        base.TargetRPC(viewId);
        Apply();
    }

    [PunRPC] protected virtual void StatTypeRPC(StatType statType) {
        ReversApply();
        this.statType = statType;
        Name = StatToKorean.Get(statType) + " �����̻�";
        Apply();
    }
    public StatType StatType {
        get { return statType; }
        set { photonView.RPC("StatTypeRPC", RpcTarget.All, value); }
    }

    [PunRPC] protected virtual void StatFormRPC(StatForm statForm) {
        ReversApply();
        this.statForm = statForm;
        Apply();
    }
    public StatForm StatForm {
        get { return statForm; }
        set {
            if (value == StatForm.AbnormalAdd || value == StatForm.AbnormalMul)
                photonView.RPC("StatFormRPC", RpcTarget.All, value);
            else
                OnlyDebug.Log("StatForm ERROR");
        }
    }

    [PunRPC] protected virtual void StatusEffectFormRPC(StatusEffectForm statusEffectForm) {
        this.statusEffectForm = statusEffectForm;
    }
    public StatusEffectForm StatusEffectForm {
        get { return statusEffectForm; }
        set { photonView.RPC("StatusEffectFormRPC", RpcTarget.All, value); }
    }

    [PunRPC] protected virtual void ValueRPC(float value) {
        ReversApply();
        this.value = value;
        Apply();
    }
    public float Value {
        get { return value; }
        set { photonView.RPC("ValueRPC", RpcTarget.All, value); }
    }

    public virtual void Apply() {
        if (Target == null) return;
        if(statForm == StatForm.AbnormalAdd)
            Target.Stats[(int)statForm, (int)statType] += Value;
        else if(statForm == StatForm.AbnormalMul)
            Target.Stats[(int)statForm, (int)statType] *= Value;
        Target.UpdateFinalStat(statType);
    }
    public virtual void ReversApply() {
        if (Target == null) return;
        if (statForm == StatForm.AbnormalAdd)
            Target.Stats[(int)statForm, (int)statType] -= Value;
        else if (statForm == StatForm.AbnormalMul)
            Target.Stats[(int)statForm, (int)statType] /= Value;
        Target.UpdateFinalStat(statType);
    }

    public void SetIconSp(Sprite sprite) {
        IconSp = sprite;
        seIcon.IconImg.sprite = sprite;
    }

    public override string GetDescription() {
        string verb = statusEffectForm == StatusEffectForm.Buff ? "���" : "����";

        if (statForm == StatForm.AbnormalAdd) {
            if (StatToKorean.IsPercent(statType))
                return string.Format("{0}�� {1:G}%p {2}�Ѵ�.", StatToKorean.Get(statType), value * 100f, verb);
            else
                return string.Format("{0}�� {1:G} {2}�Ѵ�.", StatToKorean.Get(statType), value, verb);
        }
        else if(statForm == StatForm.AbnormalMul) {
            return string.Format("{0}�� x{1:G} {2}�Ѵ�.", StatToKorean.Get(statType), value, verb);
        }

        return "StatForm ERROR";
    }
}