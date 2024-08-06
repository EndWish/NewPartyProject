using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardManager : MonoBehaviourSingleton<RewardManager>
{
    protected override void Awake() {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    public int SoulDust { get; set; } = 0;

    public List<IItem> ItemList { get; } = new List<IItem>();

    public void Add(IItem item) {
        item.InsertTo(ItemList);
    }

    public void ReceiveReward() {
        UserData userData = UserData.Instance;
        userData.SoulDust += SoulDust;
        foreach (var item in ItemList) {
            item.InsertTo(userData);
        }
    }



    public void ClearReward() {
        SoulDust = 0;
        ItemList.Clear();
    }

}
