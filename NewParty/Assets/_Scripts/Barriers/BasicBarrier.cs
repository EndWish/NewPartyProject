using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class BasicBarrier : Barrier
{
    public static BasicBarrier Create(Unit caster, float amount) {

        BasicBarrier barrier = Barrier.Instantiate<BasicBarrier>();
        barrier.Amount = amount;
        barrier.Caster = caster;
        caster.AddBarrier(barrier);

        return barrier;
    }

    public override Unit Target {
        set {
            base.Target = value;

            // 기본 배리어가 중첩되지 않도록 다른 배리어를 제거한다.
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
