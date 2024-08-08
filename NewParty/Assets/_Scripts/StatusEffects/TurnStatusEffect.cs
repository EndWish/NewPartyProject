using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TurnStatusEffect : StatusEffect, ITurnStatusEffect, IRightLowerTextableIcon
{
    protected int turn = 1;

    [PunRPC]
    protected virtual void TurnRPC(int turn) {
        this.turn = turn;
    }
    public int Turn {
        get { return turn; }
        set {
            photonView.RPC("TurnRPC", RpcTarget.All, value);
            if (value <= 0)
                this.Destroy();
        }
    }

    public string GetRightLowerText() {
        return turn.ToString();
    }

}
