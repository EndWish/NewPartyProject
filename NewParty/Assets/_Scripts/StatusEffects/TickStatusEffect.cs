using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TickStatusEffect : StatusEffect, ITickStatusEffect
{
    protected int tick = 1;

    [PunRPC]
    protected virtual void TickRPC(int tick) {
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
}