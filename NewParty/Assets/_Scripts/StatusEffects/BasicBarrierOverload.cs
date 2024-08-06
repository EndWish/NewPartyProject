using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class BasicBarrierOverload : StatStatusEffect
{
    static protected float coefficient = 0.9f;

    public static BasicBarrierOverload Create(Unit caster) {
        BasicBarrierOverload statusEffect = StatusEffect.Instantiate<BasicBarrierOverload>();

        statusEffect.Caster = caster;
        caster.AddStatusEffect(statusEffect);

        return statusEffect;
    }

    int stack = 1;
    float shieldMul = coefficient;

    protected override void OnDestroy() {
        base.OnDestroy();
        if (Target != null) {
            Target.OnAddBarrier -= OnAddBarrier;
        }
    }

    [PunRPC] 
    protected virtual void StackRPC(int stack) {
        this.stack = stack;
        shieldMul = Mathf.Pow(coefficient, stack);
    }
    public int Stack {
        get { return stack; }
        set { photonView.RPC("StackRPC", RpcTarget.All, value); }
    }

    public float ShieldMul {
        get {
            return shieldMul;
        }
    }

    [PunRPC]
    protected override void TargetRPC(int viewId) {
        if (Target != null) {
            Target.OnAddBarrier -= OnAddBarrier;
        }

        base.TargetRPC(viewId);

        if (Target != null) {
            Target.OnAddBarrier += OnAddBarrier;
        }
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

    public override string GetDescription() {
        return string.Format(
            new StringBuilder()
            .Append("기본 배리어를 사용할 때마다 과부하 스택이 상승한다.\n")
            .Append("과부하 스택 하나당 기본 배리어의 양이 x{0:F2}배 가 된다.\n\n")

            .Append("현재 스택 : {1:G} / 적용 배율 : {2:F1}%")
            .ToString()

            , coefficient, stack, shieldMul * 100f);
    }

    protected void OnAddBarrier(Barrier barrier) {
        if(barrier is BasicBarrier) {
            barrier.Amount *= shieldMul;
        }
    }

}
