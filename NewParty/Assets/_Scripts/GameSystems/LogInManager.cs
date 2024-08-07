using Photon.Pun.Demo.PunBasics;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;
using System.Text;
using ExitGames.Client.Photon;

public class LogInManager : MonoBehaviourPunCallbacksSingleton<LogInManager>
{
    public enum OperatingState
    {
        None, Hosting, Joining,
    }

    readonly int NICKNAME_BYTE_LIMIT = 16;

    // 연결 정보 /////////////////////////////////////////////////////////////

    public GameObject LogInPanel;
    public TMP_InputField LogInNicknameInputField;

    public GameObject NetworkAccessPanel;
    public TextMeshProUGUI NicknameText;
    public TMP_InputField JoinNicknameInputField;

    // 개인 정보 //////////////////////////////////////////////////////////////

    private OperatingState operatingState = OperatingState.None;
    private string ownerOfJoiningRoom;

    // 유니티 함수 ////////////////////////////////////////////////////////////

    protected override void Awake() {
        base.Awake();

        GameManager.InitGameSetting();
    }

    private void Start() {
        ApplyInputLimit(LogInNicknameInputField);
        ApplyInputLimit(JoinNicknameInputField);

        LogInPanel.SetActive(true);
        NetworkAccessPanel.SetActive(false);
    }

    // 포톤 함수 //////////////////////////////////////////////////////////////

    public override void OnConnectedToMaster() {    // 연결이 되었을 경우
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

    public override void OnJoinedRoom() {   // 방에 접속했을 경우 (만들었을 경우도 호출됨)
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

    public override void OnCreateRoomFailed(short returnCode, string message) { // 방생성 실패
        base.OnCreateRoomFailed(returnCode, message);

        Debug.Log(message);
        operatingState = OperatingState.None;
    }

    public override void OnJoinRoomFailed(short returnCode, string message) {   // 방입장 실패
        base.OnJoinRoomFailed(returnCode, message);

        Debug.Log(message);
        operatingState = OperatingState.None;
    }

    // 함수 ///////////////////////////////////////////////////////////////////

    public void LogIn() {
        if (UserData.Instance.Load(LogInNicknameInputField.text)) {  // 불러오기에 성공했을 경우
            NicknameText.text = UserData.Instance.Nickname;
            LogInPanel.SetActive(false);
            NetworkAccessPanel.SetActive(true);
        } else {
            Debug.Log("로그인에 실패했습니다.");
        }
    }

    public void NewStart() {
        if (LogInNicknameInputField.text == null || LogInNicknameInputField.text.Length == 0) {
            Debug.Log("닉네임을 입력하시오");
            return;
        }

        if (ES3.FileExists(LogInNicknameInputField.text)) {
            Debug.Log("해당 닉네임의 저장 데이터가 이미 존재합니다. 같은 닉네임을 사용하고 싶을 경우 기존의 저장 데이터를 삭제하십시오.");
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
            PhotonNetwork.ConnectUsingSettings();   // 연결을 시도한다.
    }

    public void Join() {
        operatingState = OperatingState.Joining;
        ownerOfJoiningRoom = JoinNicknameInputField.text;

        if (PhotonNetwork.IsConnected) {
            PhotonNetwork.JoinRoom(ownerOfJoiningRoom);
        } else
            PhotonNetwork.ConnectUsingSettings();   // 연결을 시도한다.
    }

    private void ApplyInputLimit(TMP_InputField inputField) {
        inputField.onValueChanged.AddListener(
            (word) => {
                string newWord = Regex.Replace(word, @"[^0-9a-zA-Z가-힣]", "");
                while (NICKNAME_BYTE_LIMIT < Encoding.Unicode.GetByteCount(newWord))
                    newWord = newWord.Substring(0, newWord.Length - 1);
                inputField.text = newWord;
            }
        );
    }

}
