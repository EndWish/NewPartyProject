using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnBasedBarrier : Barrier
{
    private int turn;

    protected void OnDestroy() {
        if (Target != null) {
            Target.CoOnBeginMyTurn -= OnBeginUnitTurn;
        }
    }

    public override Unit Target {
        set {
            if (Target != null) {
                Target.CoOnBeginMyTurn -= OnBeginUnitTurn;
            }
            base.Target = value;
            if (Target != null) {
                Target.CoOnBeginMyTurn += OnBeginUnitTurn;
            }
        }
    }

    [PunRPC] private void TurnRPC(int turn) {
        this.turn = turn;
    }
    public int Turn {
        get { return turn; }
        set { photonView.RPC("TurnRPC", RpcTarget.All, value); }
    }

    protected IEnumerator OnBeginUnitTurn() {
        if(--Turn == 0) {
            Destroy();
        }
        yield break;
    }

    public override float GetPriority() {
        return Turn;
    }
}
