using ExitGames.Client.Photon;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameManager
{
    static public void InitGameSetting() {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        Screen.SetResolution(1920 / 2, 1080 / 2, false);
#endif

        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
        PhotonNetwork.AutomaticallySyncScene = true;

        PhotonPeer.RegisterType(typeof(Unit.Data), 0, Unit.Data.Serialize, Unit.Data.Deserialize);
    }
}
