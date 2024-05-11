using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static LogInManager;

public class AutoLogInTester : MonoBehaviourPunCallbacksSingleton<AutoLogInTester>
{
    [SerializeField] private string nickname;

    protected override void Awake() {
        base.Awake();

        Screen.SetResolution(1920 / 2, 1080 / 2, false);
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
        PhotonNetwork.AutomaticallySyncScene = true;

        LogIn(nickname);
    }

    public void LogIn(string nickname) {
        if (!UserData.Instance.Load(nickname)) {  // �ҷ����⿡ �������� ���
            Debug.Log("�α��ο� �����߽��ϴ�.");
            return;
        }

        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster() {    // ������ �Ǿ��� ���
        base.OnConnectedToMaster();

        PhotonNetwork.NickName = UserData.Instance.Nickname;
        PhotonNetwork.CreateRoom(UserData.Instance.Nickname, new RoomOptions { MaxPlayers = 4 }, null);
    }

}
