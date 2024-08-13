using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReadyUI : MonoBehaviour
{
    [SerializeField] private ReadyBanner bannerPrefab;
    private List<ReadyBanner> bannerList = new List<ReadyBanner>();

    [SerializeField] private TextMeshProUGUI btnText;

    private DungeonNodeInfo dungeonNodeInfo;
    [SerializeField] private Image dungeonInfoImage;

    private void Start() {
        GameManager.Instance.OnChangeDungeonInfo += OnChangeDungeonInfo;

        NodeName targetDungeonName = UserData.Instance.TargetDungeon;
        if (targetDungeonName != NodeName.None) {
            GameManager.Instance.SetDungeonInfo(DungeonNodeInfo.Get(targetDungeonName));
        }
    }

    private void OnDestroy() {
        GameManager.Instance.OnChangeDungeonInfo -= OnChangeDungeonInfo;
    }

    private void Update() {
        if (!PhotonNetwork.InRoom)
            return;

        // 버튼 텍스트 적용
        if (PhotonNetwork.IsMasterClient) {
            btnText.text = "입장";
        } else {
            if(GameManager.Instance.MyClientData.IsReady)
                btnText.text = "준비 해제";
            else
                btnText.text = "준비";
        }

        // 배너 적용
        List<ClientData> clientDataList = GameManager.Instance.ClientDataList;
        if(bannerList.Count < clientDataList.Count) {
            bannerList.Add(Instantiate(bannerPrefab, this.transform));
        }

        for(int i = 0; i < bannerList.Count; ++i) {
            if (i < clientDataList.Count) {
                bannerList[i].gameObject.SetActive(true);
                bannerList[i].UpdateBanner(clientDataList[i]);
            } else {
                bannerList[i].gameObject.SetActive(false);
            }
        }
    }

    public void ToggleReady() {
        if (PhotonNetwork.IsMasterClient) {
            if (dungeonNodeInfo == null)
                return;

            if (!GameManager.Instance.IsReadyAllClient())
                return;

            PhotonNetwork.LoadLevel("BattleScene");
        } else {
            GameManager.Instance.MyClientData.ToggleReady();
        }
    }
    public void OnChangeDungeonInfo(DungeonNodeInfo info) {
        dungeonNodeInfo = info;
        if (info == null) {
            dungeonInfoImage.sprite = null;
        } else {
            dungeonInfoImage.sprite = info.Icon;
        }
    }

}
