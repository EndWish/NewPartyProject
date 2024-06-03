using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class BasicBarrier : Barrier
{

    public override Unit Target {
        set {
            base.Target = value;

            // �⺻ �踮� ��ø���� �ʵ��� �ٸ� �踮� �����Ѵ�.
            Barrier otherBarrier = Target?.Barriers.Find((barrier) => barrier is BasicBarrier && barrier != this);
            if (otherBarrier != null) {
                Amount = Mathf.Max(Amount, otherBarrier.Amount);
                Target.RemoveBarrier(otherBarrier);
                otherBarrier.Destroy();
            }
        }
    }

    public override float GetPriority() {
        return float.MaxValue;
    }

}
