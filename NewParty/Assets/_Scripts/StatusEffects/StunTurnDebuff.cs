using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class StunTurnDebuff : TurnStatusEffect
{
    protected override void OnDestroy() {
        base.OnDestroy();
        if (Target != null) {
            Target.CoOnEndMyTurn -= CoOnBeginTurn;
            Target.Tags.SubTag(Tag.기절);
        }
    }

    [PunRPC]
    protected override void TargetRPC(int viewId) {
        if (Target != null) {
            Target.CoOnEndMyTurn -= CoOnBeginTurn;
            Target.Tags.SubTag(Tag.기절);
        }

        base.TargetRPC(viewId);

        if (Target != null) {
            Target.CoOnEndMyTurn += CoOnBeginTurn;
            Target.Tags.AddTag(Tag.기절);
        }
    }
    public override Unit Target {
        set { 
            base.Target = value;
            Target?.OnStun?.Invoke(this);
        }
    }

    public override string GetDescription() {
        return string.Format("{0} 턴이 끝날때 까지 아무런행동을 할 수 없다.", Turn);
    }

    protected IEnumerator CoOnBeginTurn() {
        Turn -= 1;
        yield break;
    }
}
