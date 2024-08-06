using Photon.Pun;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StaticOverlayCanvas : MonoBehaviourSingleton<StaticOverlayCanvas>
{


    public TextMeshProUGUI NetwortStatusText;



    protected override void Awake() {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    private void Update() {
        NetwortStatusText.text = PhotonNetwork.NetworkClientState.ToString();
    }

}
