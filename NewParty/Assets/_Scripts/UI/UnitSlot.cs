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

        // ���� �̹��� ����
        if(unit == null) {
            profileImage.sprite = null;
            profileImage.color = new Color(0, 0, 0, 0);
        }
        else {
            profileImage.sprite = unit.profileRenderer.sprite;
            profileImage.color = Color.white;
        }

        // ���� ���� �ؽ�Ʈ
        if (unit == null)
            growthLevelText.text = string.Empty;
        else
            growthLevelText.text = 0 < unit.GrowthLevel ? ("+" + unit.GrowthLevel.ToString()) : unit.GrowthLevel.ToString();
    }
}
