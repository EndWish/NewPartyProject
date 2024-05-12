using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitInventorySlot : PageViewSlot<Unit>
{
    [SerializeField] private Image profileImage;
    [SerializeField] private TextMeshProUGUI growthLevelText;

    public override void SlotUpdate(Unit unit, int index) {
        base.SlotUpdate(unit, index);
        profileImage.sprite = unit.profileRenderer.sprite; // 유닛 이미지 적용
        growthLevelText.text = 0 < unit.GrowthLevel ? ("+"+ unit.GrowthLevel.ToString()) : unit.GrowthLevel.ToString();
    }
}
