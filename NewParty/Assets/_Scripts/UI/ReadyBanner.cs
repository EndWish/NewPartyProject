using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ReadyBanner : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nicknameText;
    [SerializeField] private GameObject readyMark;
    [SerializeField] private GameObject hostMark;

    public void UpdateBanner(ClientData clientData) {
        readyMark.SetActive(false);
        hostMark.SetActive(false);

        nicknameText.text = clientData.Nickname;

        if(PhotonNetwork.MasterClient.NickName == clientData.Nickname) {
            hostMark.SetActive(true);
        } else if(clientData.IsReady) {
            readyMark.SetActive(true);
        }
    }

}
