using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class AccelerationBuff : StatStatusEffect, ITickStatusEffect
{
    protected int tick = 1;
    protected float speedMul = 1f;

    protected override void OnDestroy() {
        base.OnDestroy();
        if (Target != null) Target.CoOnBeginTick -= CoOnBeginTick;
    }

    [PunRPC] protected virtual void TickRPC(int tick) {
        this.tick = tick;
        seIcon.RightLowerText.text = tick.ToString();
    }
    public int Tick {
        get { return tick; }
        set {
            photonView.RPC("TickRPC", RpcTarget.All, value);
            if (value <= 0)
                this.Destroy();
        }
    }

    [PunRPC] protected virtual void SpeedMulRPC(float speedMul) {
        ReversApply();
        this.speedMul = speedMul;
        Apply();
    }
    public float SpeedMul {
        get { return speedMul; }
        set { photonView.RPC("SpeedMulRPC", RpcTarget.All, value); }
    }

    [PunRPC] protected override void TargetRPC(int viewId) {
        if (Target != null) 
            Target.CoOnBeginTick -= CoOnBeginTick;
        base.TargetRPC(viewId);
        if (Target != null)
            Target.CoOnBeginTick += CoOnBeginTick;
    }

    public override void Apply() {
        if (Target == null) return;
        Target.Stats[(int)StatForm.AbnormalMul, (int)StatType.Speed] *= speedMul;
        Target.UpdateFinalStat(StatType.Speed);
    }
    public override void ReversApply() {
        if (Target == null) return;
        Target.Stats[(int)StatForm.AbnormalMul, (int)StatType.Speed] /= speedMul;
        Target.UpdateFinalStat(StatType.Speed);
    }

    public override string GetDescription() {
        return string.Format( "{0:G}틱 동안 속도가 x{1:F1} 로 상승한다.", Tick, speedMul);
    }

    protected IEnumerator CoOnBeginTick() {
        Tick -= 1;
        yield break;
    }

}
