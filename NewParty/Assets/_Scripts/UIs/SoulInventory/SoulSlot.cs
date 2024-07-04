using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SoulSlot : PageViewSlot<SoulFragment>
{
    [SerializeField] protected Image iconImage;
    [SerializeField] protected Image profileImage;
    [SerializeField] protected TextMeshProUGUI numText;

    public override void SlotUpdate(SoulFragment soulFragment, int index) {
        base.SlotUpdate(soulFragment, index);

        // 유닛 이미지 적용
        if (Data == null) {
            iconImage.color = new Color(0, 0, 0, 0);
            profileImage.sprite = null;
            profileImage.color = new Color(0, 0, 0, 0);
        } else {
            iconImage.color = Color.white;
            profileImage.sprite = Data.Target.ProfileImage.sprite;
            profileImage.color = Color.white;
        }

        // 성장 레벨 텍스트
        numText.text = Data?.GetNum().ToString();

    }




}
