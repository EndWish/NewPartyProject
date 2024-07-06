using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SoulInventory : PageView<SoulFragment>
{
    [SerializeField] protected TextMeshProUGUI pageText;
    [SerializeField] protected GameObject sortList;

    protected override void Awake() {
        Datas = UserData.Instance.SoulFragmentList;
        sortList.SetActive(false);
        base.Awake();
    }

    private void Update() {
        UpdatePage(page);
    }

    public override void UpdatePage(int page) {
        base.UpdatePage(page);
        pageText.text = this.page + "/" + MaxPage;
    }

    // 정렬 관련 함수
    public void ToggleSortList() {
        sortList.SetActive(!sortList.activeSelf);
    }
    public void SortByName() {
        Datas.Sort((soulFragment1, soulFragment2) => { return String.Compare(soulFragment1.UnitSharedData.Name, soulFragment2.UnitSharedData.Name); });
        UserData.Instance.SaveSoulFragmentKeyList();
    }
    public void SortByNameInReverse() {
        Datas.Sort((soulFragment1, soulFragment2) => { return String.Compare(soulFragment1.UnitSharedData.Name, soulFragment1.UnitSharedData.Name); });
        UserData.Instance.SaveSoulFragmentKeyList();
    }
    public void SortByHighestNum() {
        Datas.Sort((soulFragment1, soulFragment2) => { return soulFragment2.Num - soulFragment1.Num; });
        UserData.Instance.SaveSoulFragmentKeyList();
    }
    public void SortByLowestNum() {
        Datas.Sort((soulFragment1, soulFragment2) => { return soulFragment1.Num - soulFragment2.Num; });
        UserData.Instance.SaveSoulFragmentKeyList();
    }

}
