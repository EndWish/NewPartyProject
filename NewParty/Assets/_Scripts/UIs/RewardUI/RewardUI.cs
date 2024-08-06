using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RewardUI : PageView<IItem>
{
    [SerializeField] protected TextMeshProUGUI pageText;

    protected override void Awake() {
        Datas = RewardManager.Instance.ItemList;
        base.Awake();
    }

    private void Update() {
        UpdatePage(page);
    }

    public override void UpdatePage(int page) {
        base.UpdatePage(page);
        pageText.text = this.page + "/" + MaxPage;
    }

}