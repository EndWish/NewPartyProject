using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnBasedBarrier : Barrier
{
    private int remainTurn;

    protected void OnDestroy() {
        if (Target != null) {
            Target.CoOnBeginMyTurn -= OnBeginUnitTurn;
        }
    }

    [PunRPC] private void RemainTurnRPC(int turn) {
        remainTurn = turn;
    }
    public int RemainTurn {
        get { return remainTurn; }
        set { photonView.RPC("RemainTurnRPC", RpcTarget.All, value); }
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

    protected IEnumerator OnBeginUnitTurn(Unit unit) {
        if(--RemainTurn == 0) {
            Destroy();
        }
        yield break;
    }

    public override float GetPriority() {
        return RemainTurn;
    }
}
