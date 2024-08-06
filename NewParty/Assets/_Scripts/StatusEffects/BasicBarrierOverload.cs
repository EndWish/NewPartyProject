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

            // �����̻��� ��ø���� �ʵ��� �Ѵ�
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
            .Append("�⺻ �踮� ����� ������ ������ ������ ����Ѵ�.\n")
            .Append("������ ���� �ϳ��� �⺻ �踮���� ���� x{0:F2}�� �� �ȴ�.\n\n")

            .Append("���� ���� : {1:G} / ���� ���� : {2:F1}%")
            .ToString()

            , coefficient, stack, shieldMul * 100f);
    }

    protected void OnAddBarrier(Barrier barrier) {
        if(barrier is BasicBarrier) {
            barrier.Amount *= shieldMul;
        }
    }

}
