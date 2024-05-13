using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitSlot : PageViewSlot<Unit>
{
    [SerializeField] protected Image profileImage;
    [SerializeField] protected TextMeshProUGUI growthLevelText;

    public override void SlotUpdate(Unit unit, int index) {
        base.SlotUpdate(unit, index);

        // 유닛 이미지 적용
        if(unit == null) {
            profileImage.sprite = null;
            profileImage.color = new Color(0, 0, 0, 0);
        }
        else {
            profileImage.sprite = unit.profileRenderer.sprite;
            profileImage.color = Color.white;
        }

        // 성장 레벨 텍스트
        if (unit == null)
            growthLevelText.text = string.Empty;
        else
            growthLevelText.text = 0 < unit.GrowthLevel ? ("+" + unit.GrowthLevel.ToString()) : unit.GrowthLevel.ToString();
    }
}
