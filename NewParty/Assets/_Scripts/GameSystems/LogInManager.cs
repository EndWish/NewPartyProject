using Photon.Pun.Demo.PunBasics;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;
using System.Text;

public class LogInManager : MonoBehaviourPunCallbacksSingleton<LogInManager>
{
    public enum OperatingState
    {
        None, Hosting, Joining,
    }

    readonly int NICKNAME_BYTE_LIMIT = 16;

    private OperatingState operatingState = OperatingState.None;
    private string ownerOfJoiningRoom;

    public GameObject LogInPanel;
    public TMP_InputField LogInNicknameInputField;

    public GameObject NetworkAccessPanel;
    public TextMeshProUGUI NicknameText;
    public TMP_InputField JoinNicknameInputField;

    // ����Ƽ �Լ� ////////////////////////////////////////////////////////////

    protected override void Awake() {
        base.Awake();

        Screen.SetResolution(1920 / 2, 1080 / 2, false);
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start() {
        ApplyInputLimit(LogInNicknameInputField);
        ApplyInputLimit(JoinNicknameInputField);

        LogInPanel.SetActive(true);
        NetworkAccessPanel.SetActive(false);
    }

    // ���� �Լ� //////////////////////////////////////////////////////////////

    public override void OnConnectedToMaster() {    // ������ �Ǿ��� ���
        base.OnConnectedToMaster();

        PhotonNetwork.NickName = UserData.Instance.Nickname;

        switch (operatingState) {
            case OperatingState.Hosting:
                PhotonNetwork.CreateRoom(UserData.Instance.Nickname, new RoomOptions { MaxPlayers = 4 }, null);
                break;

            case OperatingState.Joining:
                PhotonNetwork.JoinRoom(ownerOfJoiningRoom);
                break;

            default:
                break;
        }
    }

    public override void OnJoinedRoom() {   // �濡 �������� ��� (������� ��쵵 ȣ���)
        base.OnJoinedRoom();

        switch (operatingState) {
            case OperatingState.Hosting:
                PhotonNetwork.LoadLevel("VillageScene");
                operatingState = OperatingState.None;
                break;

            case OperatingState.Joining:
                operatingState = OperatingState.None;
                break;

            default:
                break;
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message) { // ����� ����
        base.OnCreateRoomFailed(returnCode, message);

        Debug.Log(message);
        operatingState = OperatingState.None;
    }

    public override void OnJoinRoomFailed(short returnCode, string message) {   // ������ ����
        base.OnJoinRoomFailed(returnCode, message);

        Debug.Log(message);
        operatingState = OperatingState.None;
    }

    // �Լ� ///////////////////////////////////////////////////////////////////

    public void LogIn() {
        if (UserData.Instance.Load(LogInNicknameInputField.text)) {  // �ҷ����⿡ �������� ���
            NicknameText.text = UserData.Instance.Nickname;
            LogInPanel.SetActive(false);
            NetworkAccessPanel.SetActive(true);
        } else {
            Debug.Log("�α��ο� �����߽��ϴ�.");
        }
    }

    public void NewStart() {
        if (LogInNicknameInputField.text == null || LogInNicknameInputField.text.Length == 0) {
            Debug.Log("�г����� �Է��Ͻÿ�");
            return;
        }

        if (ES3.FileExists(LogInNicknameInputField.text)) {
            Debug.Log("�ش� �г����� ���� �����Ͱ� �̹� �����մϴ�. ���� �г����� ����ϰ� ���� ��� ������ ���� �����͸� �����Ͻʽÿ�.");
            return;
        }

        UserData.Instance.SetNewPlayerData(LogInNicknameInputField.text);
        NicknameText.text = UserData.Instance.Nickname;
        LogInPanel.SetActive(false);
        NetworkAccessPanel.SetActive(true);
    }

    public void PlayHost() {
        operatingState = OperatingState.Hosting;
        if (PhotonNetwork.IsConnected)
            PhotonNetwork.CreateRoom(UserData.Instance.Nickname, new RoomOptions { MaxPlayers = 3 }, null);
        else
            PhotonNetwork.ConnectUsingSettings();   // ������ �õ��Ѵ�.
    }

    public void Join() {
        operatingState = OperatingState.Joining;
        ownerOfJoiningRoom = JoinNicknameInputField.text;

        if (PhotonNetwork.IsConnected) {
            PhotonNetwork.JoinRoom(ownerOfJoiningRoom);
        } else
            PhotonNetwork.ConnectUsingSettings();   // ������ �õ��Ѵ�.
    }

    private void ApplyInputLimit(TMP_InputField inputField) {
        inputField.onValueChanged.AddListener(
            (word) => {
                string newWord = Regex.Replace(word, @"[^0-9a-zA-Z��-�R]", "");
                while (NICKNAME_BYTE_LIMIT < Encoding.Unicode.GetByteCount(newWord))
                    newWord = newWord.Substring(0, newWord.Length - 1);
                inputField.text = newWord;
            }
        );
    }

}
