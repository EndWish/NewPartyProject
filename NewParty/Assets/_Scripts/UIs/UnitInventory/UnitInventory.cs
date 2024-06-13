using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

public class UnitInventory : PageView<Unit>
{
    [SerializeField] protected TextMeshProUGUI pageText;
    [SerializeField] protected GameObject sortList;

    protected override void Awake() {
        Datas = UserData.Instance.UnitList;
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
        Datas.Sort((unit1, unit2) => { return String.Compare(unit1.Name, unit2.Name); });
    }
    public void SortByNameInReverse() {
        Datas.Sort((unit1, unit2) => { return String.Compare(unit2.Name, unit1.Name); });
    }
    public void SortByHighestGrowthLevel() {
        Datas.Sort((unit1, unit2) => { return unit2.GrowthLevel - unit1.GrowthLevel; });
    }
    public void SortByLowestGrowthLevel() {
        Datas.Sort((unit1, unit2) => { return unit1.GrowthLevel - unit2.GrowthLevel; });
    }

}
