using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicBarrier : Barrier
{

    protected override void OnSetTarget(Unit prev, Unit current) {
        base.OnSetTarget(prev, current);

        // 다른 BasicBarrier가 있을 경우
        Barrier otherBarrier = current.Barriers.Find((barrier) => barrier is BasicBarrier && barrier != this);
        if (otherBarrier != null) {
            Amount = Mathf.Max(Amount, otherBarrier.Amount);
            current.RemoveBarrier(otherBarrier);
        }
    }

    public override float GetPriority() {
        return float.MaxValue;
    }

}
