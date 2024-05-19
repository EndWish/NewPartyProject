using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ClientStateBanner : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nicknameText;
    [SerializeField] private GameObject hostMark;
    [SerializeField] private GameObject readyMark;

    public void UpdateBanner(ClientData clientData) {
        nicknameText.text = clientData.Nickname;
        hostMark.SetActive(PhotonNetwork.MasterClient.NickName == clientData.Nickname);
        readyMark.SetActive(clientData.IsReady);
    }
}
