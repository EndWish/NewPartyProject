using ExitGames.Client.Photon;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using static LogInManager;

public class GameManager : MonoBehaviourPunCallbacksSingleton<GameManager>
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

    static public string GetUnitPrefabPath() {
        return "Prefabs/Units/";
    }

    // 연결 변수 //////////////////////////////////////////////////////////////
    public List<ClientData> ClientDataList { get; protected set; } = new List<ClientData>();
    public ClientData MyClientData { get; protected set; } = null;

    public DungeonNodeInfo DungeonInfo { get; protected set; } = null;
    public UnityAction<DungeonNodeInfo> OnChangeDungeonInfo;

    // 유니티 함수 ////////////////////////////////////////////////////////////

    protected override void Awake() {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    // 포톤 함수 //////////////////////////////////////////////////////////////

    public override void OnJoinedRoom() {
        MyClientData = PhotonNetwork.Instantiate("Prefabs/Network/ClientData", Vector3.zero, Quaternion.identity).GetComponent<ClientData>();
    }

    public override void OnLeftRoom() {
        PhotonNetwork.Destroy(MyClientData.gameObject);
    }

    // 함수 ///////////////////////////////////////////////////////////////////
    public void AddClientData(ClientData clientData) {
        ClientDataList.Add(clientData);
    }
    public void RemoveClientData(ClientData clientData) {
        ClientDataList.Remove(clientData);
    }

    [PunRPC]
    protected void SetDungeonInfoRPC(string dungeonName) {
        DungeonInfo = Resources.Load<DungeonNodeInfo>("NodeInfo/" + dungeonName + "NodeInfo");
        OnChangeDungeonInfo?.Invoke(DungeonInfo);
    }
    public void SetDungeonInfo(DungeonNodeInfo dungeonNodeInfo) {
        photonView.RPC("SetDungeonInfoRPC", RpcTarget.AllBufferedViaServer, dungeonNodeInfo.Name.ToString());
        foreach (var clientData in ClientDataList)
            clientData.IsReady = false;
    }

    public bool IsReadyAllClient() {
        bool result = true;
        foreach(var clientData in ClientDataList) {
            if (clientData == MyClientData)
                continue;

            if (!clientData.IsReady) {
                result = false;
                break;
            }
        }
        return result;
    }

}
