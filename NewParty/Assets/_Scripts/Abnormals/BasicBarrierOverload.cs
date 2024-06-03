using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicBarrierOverload : StatusEffect
{
    static protected float coefficient = 0.9f;

    int stack = 1;
    float shieldMul = 1f;

    [PunRPC] protected virtual void StackRPC(int stack) {
        ReversApply();
        this.stack = stack;
        Apply();
        Target?.UpdateFinalStat(StatType.Shield);
        Debug.Log("FinalStat : " + Target?.GetFinalStat(StatType.Shield));
    }
    public int Stack {
        get { return stack; }
        set { photonView.RPC("StackRPC", RpcTarget.All, value); }
    }

    [PunRPC] protected override void TargetRPC(int viewId) {
        ReversApply();
        base.TargetRPC(viewId);
        Apply();
        Target?.UpdateFinalStat(StatType.Shield);
        Debug.Log("FinalStat : " + Target?.GetFinalStat(StatType.Shield));
    }
    public override Unit Target {
        set {
            base.Target = value;

            // 상태이상이 중첩되지 않도록 한다
            BasicBarrierOverload statusEffect = FindSameStatusEffect<BasicBarrierOverload>(Target);
            if (statusEffect != null) {
                Stack += statusEffect.Stack;
                Target.RemoveStatusEffect(statusEffect);
                statusEffect.Destroy();
            }
        }
    }

    public override void Apply() {
        if (Target == null) return;
        shieldMul = Mathf.Pow(coefficient, stack);
        Target.Stats[(int)StatForm.AbnormalMul, (int)StatType.Shield] *= shieldMul;
        Debug.Log("AbnormalMul : " + Target.Stats[(int)StatForm.AbnormalMul, (int)StatType.Shield]);
    }
    public override void ReversApply() {
        if (Target == null) return;
        Target.Stats[(int)StatForm.AbnormalMul, (int)StatType.Shield] /= shieldMul;
    }

}
