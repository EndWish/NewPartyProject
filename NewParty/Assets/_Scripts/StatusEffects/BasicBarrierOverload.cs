using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class BasicBarrierOverload : StatStatusEffect
{
    static protected float coefficient = 0.9f;

    int stack = 1;
    float shieldMul = 1f;

    [PunRPC] protected virtual void StackRPC(int stack) {
        ReversApply();
        this.stack = stack;
        Apply();
    }
    public int Stack {
        get { return stack; }
        set { photonView.RPC("StackRPC", RpcTarget.All, value); }
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

    public override void Apply() {
        if (Target == null) return;
        shieldMul = Mathf.Pow(coefficient, stack);
        Target.Stats[(int)StatForm.AbnormalMul, (int)StatType.Shield] *= shieldMul;
        Target.UpdateFinalStat(StatType.Shield);

        seIcon.RightLowerText.text = stack.ToString();
    }
    public override void ReversApply() {
        if (Target == null) return;
        Target.Stats[(int)StatForm.AbnormalMul, (int)StatType.Shield] /= shieldMul;
        Target.UpdateFinalStat(StatType.Shield);
    }

    public override string GetDescription() {
        return string.Format(
            new StringBuilder()
            .Append("�⺻ �踮� ����� ������ ������ ������ ����Ѵ�.\n")
            .Append("������ ���� �ϳ��� ���尡 x{0:F2}�� �� �ȴ�.\n\n")

            .Append("���� ���� : {1:G} / ���� ���� : {2:F1}%")
            .ToString()

            , coefficient, stack, shieldMul * 100f);
    }
}
