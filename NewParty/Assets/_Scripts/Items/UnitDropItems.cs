using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UnitDropItems : MonoBehaviourPun
{
    protected Unit unit;

    protected void Awake() {
        unit = GetComponent<Unit>();
        unit.CoOnDie += UnitCoOnDie;
    }

    protected void OnDestroy() {
        unit.CoOnDie -= UnitCoOnDie;
    }

    protected IEnumerator UnitCoOnDie() {
        AddToRewards();
        yield break;
    }
    protected void AddToRewards() {
        photonView.RPC("AddToRewardsRPC", RpcTarget.All);
    }

    [PunRPC]
    protected abstract void AddToRewardsRPC();

}
