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

    // À¯´ÏÆ¼ ÇÔ¼ö ////////////////////////////////////////////////////////////

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

    // Æ÷Åæ ÇÔ¼ö //////////////////////////////////////////////////////////////

    public override void OnConnectedToMaster() {    // ¿¬°áÀÌ µÇ¾úÀ» °æ¿ì
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

    public override void OnJoinedRoom() {   // ¹æ¿¡ Á¢¼ÓÇßÀ» °æ¿ì (¸¸µé¾úÀ» °æ¿ìµµ È£ÃâµÊ)
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

    public override void OnCreateRoomFailed(short returnCode, string message) { // ¹æ»ý¼º ½ÇÆÐ
        base.OnCreateRoomFailed(returnCode, message);

        Debug.Log(message);
        operatingState = OperatingState.None;
    }

    public override void OnJoinRoomFailed(short returnCode, string message) {   // ¹æÀÔÀå ½ÇÆÐ
        base.OnJoinRoomFailed(returnCode, message);

        Debug.Log(message);
        operatingState = OperatingState.None;
    }

    // ÇÔ¼ö ///////////////////////////////////////////////////////////////////

    public void LogIn() {
        if (UserData.Instance.Load(LogInNicknameInputField.text)) {  // ºÒ·¯¿À±â¿¡ ¼º°øÇßÀ» °æ¿ì
            NicknameText.text = UserData.Instance.Nickname;
            LogInPanel.SetActive(false);
            NetworkAccessPanel.SetActive(true);
        } else {
            Debug.Log("·Î±×ÀÎ¿¡ ½ÇÆÐÇß½À´Ï´Ù.");
        }
    }

    public void NewStart() {
        if (LogInNicknameInputField.text == null || LogInNicknameInputField.text.Length == 0) {
            Debug.Log("´Ð³×ÀÓÀ» ÀÔ·ÂÇÏ½Ã¿À");
            return;
        }

        if (ES3.FileExists(LogInNicknameInputField.text)) {
            Debug.Log("ÇØ´ç ´Ð³×ÀÓÀÇ ÀúÀå µ¥ÀÌÅÍ°¡ ÀÌ¹Ì Á¸ÀçÇÕ´Ï´Ù. °°Àº ´Ð³×ÀÓÀ» »ç¿ëÇÏ°í ½ÍÀ» °æ¿ì ±âÁ¸ÀÇ ÀúÀå µ¥ÀÌÅÍ¸¦ »èÁ¦ÇÏ½Ê½Ã¿À.");
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
            PhotonNetwork.ConnectUsingSettings();   // ¿¬°áÀ» ½ÃµµÇÑ´Ù.
    }

    public void Join() {
        operatingState = OperatingState.Joining;
        ownerOfJoiningRoom = JoinNicknameInputField.text;

        if (PhotonNetwork.IsConnected) {
            PhotonNetwork.JoinRoom(ownerOfJoiningRoom);
        } else
            PhotonNetwork.ConnectUsingSettings();   // ¿¬°áÀ» ½ÃµµÇÑ´Ù.
    }

    private void ApplyInputLimit(TMP_InputField inputField) {
        inputField.onValueChanged.AddListener(
            (word) => {
                string newWord = Regex.Replace(word, @"[^0-9a-zA-Z°¡-ÆR]", "");
                while (NICKNAME_BYTE_LIMIT < Encoding.Unicode.GetByteCount(newWord))
                    newWord = newWord.Substring(0, newWord.Length - 1);
                inputField.text = newWord;
            }
        );
    }

}
