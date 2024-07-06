using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitSlot : PageViewSlot<Unit.Data>
{
    [SerializeField] protected Image profileImage;
    [SerializeField] protected TextMeshProUGUI growthLevelText;

    public override void SlotUpdate(Unit.Data unitData, int index) {
        base.SlotUpdate(unitData, index);

        // ���� �̹��� ����
        if(unitData == null) {
            profileImage.sprite = null;
            profileImage.color = new Color(0, 0, 0, 0);
        }
        else {
            profileImage.sprite = unitData.ProfileSprite;
            profileImage.color = Color.white;
        }

        // ���� ���� �ؽ�Ʈ
        if (unitData == null)
            growthLevelText.text = string.Empty;
        else
            growthLevelText.text = 0 <= unitData.GrowthLevel ? ("+" + unitData.GrowthLevel.ToString()) : unitData.GrowthLevel.ToString();
    }
}
