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

    public override void UpdatePage(int page) {
        base.UpdatePage(page);
        pageText.text = this.page + "/" + MaxPage;
    }

    // 정렬 관련 함수
    public void ToggleSortList() {
        sortList.SetActive(!sortList.activeSelf);
    }
    public void SortByName() {
        Datas.Sort((soulFragment1, soulFragment2) => { return String.Compare(soulFragment1.Target.Name, soulFragment2.Target.Name); });
    }
    public void SortByNameInReverse() {
        Datas.Sort((soulFragment1, soulFragment2) => { return String.Compare(soulFragment1.Target.Name, soulFragment1.Target.Name); });
    }
    public void SortByHighestGrowthLevel() {
        Datas.Sort((soulFragment1, soulFragment2) => { return soulFragment2.Target.GrowthLevel - soulFragment1.Target.GrowthLevel; });
    }
    public void SortByLowestGrowthLevel() {
        Datas.Sort((soulFragment1, soulFragment2) => { return soulFragment1.Target.GrowthLevel - soulFragment2.Target.GrowthLevel; });
    }

}
