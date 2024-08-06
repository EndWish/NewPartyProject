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
        profileImage.sprite = Data?.GetIcon1x2() ?? Unit.NullIcon1x2;

        // ���� ���� �ؽ�Ʈ
        if (Data == null)
            growthLevelText.text = string.Empty;
        else
            growthLevelText.text = (0 <= Data.GrowthLevel) ? ("+" + Data.GrowthLevel.ToString()) : Data.GrowthLevel.ToString(); ;
    }
}
