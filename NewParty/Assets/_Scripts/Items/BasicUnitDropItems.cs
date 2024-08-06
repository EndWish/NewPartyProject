using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicUnitDropItems : UnitDropItems
{
    [PunRPC]
    protected override void AddToRewardsRPC() {
        RewardManager rewardManager = RewardManager.Instance;

        // 영혼 가루 추가 (파편2개가치 * 레벨비례{레벨당 +1%p})
        rewardManager.Add(new SoulDust((int)((unit.SharedData.SoulFragmentValueAsDust * 2f) * (1f + unit.GrowthLevel / 100f))));

        // 영혼 파편 추가 (2~4)
        int soulFragmentNum = UnityEngine.Random.Range(2, 4 + 1);
        rewardManager.Add(new SoulFragment(unit.SharedData.Type, soulFragmentNum));

    }
}
