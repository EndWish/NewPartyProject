using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientData : MonoBehaviourPun
{
    public string Nickname {  get; set; }
    public bool IsReady { get; private set; } = false;

    private void Start() {
        Nickname = photonView.Owner.NickName;
        transform.SetParent(GameManager.Instance.transform);
        GameManager.Instance.AddClientData(this);
    }

    private void OnDestroy() {
        GameManager.Instance.RemoveClientData(this);
    }

    [PunRPC]
    private void ReadyRPC(bool result) {
        IsReady = result;
    }
    public void Ready(bool result) {
        photonView.RPC("ReadyRPC", RpcTarget.AllBufferedViaServer, result);
    }
    public void ToggleReady() {
        photonView.RPC("ReadyRPC", RpcTarget.AllBufferedViaServer, !IsReady);
    }

}
