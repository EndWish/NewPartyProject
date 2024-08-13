using JetBrains.Annotations;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientData : MonoBehaviourPun
{
    public string Nickname {  get; set; }
    private bool isReady = false;
    private bool hasLastRpc = false;
    private bool isGivingUp = false;
    private bool isLoaded  = false;
    

    private void Start() {
        Nickname = photonView.Owner.NickName;
        transform.SetParent(GameManager.Instance.transform);
        GameManager.Instance.AddClientData(this);
    }

    private void OnDestroy() {
        GameManager.Instance.RemoveClientData(this);
    }

    // 함수 ///////////////////////////////////////////////////////////////////

    // 포기 관련 함수
    [PunRPC]
    private void IsGivingUpRPC(bool result) {
        isGivingUp = result;
        if (isGivingUp)
            isReady = false;
    }
    public bool IsGivingUp {
        get { return isGivingUp; }
        set { photonView.RPC("IsGivingUpRPC", RpcTarget.AllBufferedViaServer, value); }
    }
    public void ToggleGivingUp() {
        photonView.RPC("IsGivingUpRPC", RpcTarget.AllBufferedViaServer, !IsGivingUp);
    }

    // 레디 관련 함수
    [PunRPC] private void IsReadyRPC(bool result) {
        isReady = result;
        if (isReady)
            isGivingUp = false;
    }
    public bool IsReady { 
        get { return isReady; }
        set { photonView.RPC("IsReadyRPC", RpcTarget.AllBufferedViaServer, value); }
    }
    public void ToggleReady() {
        photonView.RPC("IsReadyRPC", RpcTarget.AllBufferedViaServer, !IsReady);
    }

    [PunRPC] private void HasLastRpcRPC(bool result) {
        hasLastRpc = result;
    }
    public bool HasLastRpc {
        get { return hasLastRpc; }
        set { photonView.RPC("HasLastRpcRPC", RpcTarget.All, value); }
    }

    [PunRPC] private void IsLoadedRPC(bool result) {
        isLoaded = result;
    }
    public bool IsLoaded {
        get { return isLoaded; }
        set { photonView.RPC("IsLoadedRPC", RpcTarget.All, value); }
    }

}
