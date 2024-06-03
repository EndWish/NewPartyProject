using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ClientStateUI : MonoBehaviour
{
    [SerializeField] private ClientStateBanner bannerPrefab;
    private List<ClientStateBanner> bannerList = new List<ClientStateBanner>();



    private void Update() {
        // 클라이언트 상태 배너 적용
        List<ClientData> clientDataList = GameManager.Instance.ClientDataList;
        while (bannerList.Count < clientDataList.Count) {
            bannerList.Add(Instantiate(bannerPrefab, this.transform));
        }

        for (int i = 0; i < bannerList.Count; ++i) {
            if (i < clientDataList.Count) {
                bannerList[i].gameObject.SetActive(true);
                bannerList[i].UpdateBanner(clientDataList[i]);
            } else {
                bannerList[i].gameObject.SetActive(false);
            }
        }
    }


}
