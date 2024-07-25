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
            Target.Tags.SubTag(Tag.����);
        }
    }

    [PunRPC]
    protected override void TargetRPC(int viewId) {
        if (Target != null) {
            Target.CoOnEndMyTurn -= CoOnBeginTurn;
            Target.Tags.SubTag(Tag.����);
        }

        base.TargetRPC(viewId);

        if (Target != null) {
            Target.CoOnEndMyTurn += CoOnBeginTurn;
            Target.Tags.AddTag(Tag.����);
        }
    }
    public override Unit Target {
        set { 
            base.Target = value;
            Target?.OnStun?.Invoke(this);
        }
    }

    public override string GetDescription() {
        return string.Format("{0} ���� ������ ���� �ƹ����ൿ�� �� �� ����.", Turn);
    }

    protected IEnumerator CoOnBeginTurn() {
        Turn -= 1;
        yield break;
    }
}
