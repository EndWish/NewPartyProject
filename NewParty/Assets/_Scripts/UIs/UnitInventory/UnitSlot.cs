using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitSlot : PageViewSlot<Unit.Data>
{
    // 공유 정보 //////////////////////////////////////////////////////////////
    static private UnitSlot dragUnitSlot = null;
    static private UnitSlot DragUnitSlot {
        get {
            if (dragUnitSlot == null) {
                dragUnitSlot = Instantiate(Resources.Load<UnitSlot>("Prefabs/UI/UnitSlot"), StaticOverlayCanvas.Instance.transform);

                Color newColor = dragUnitSlot.profileImage.color;
                newColor.a = 0.5f;
                dragUnitSlot.profileImage.color = newColor;

                dragUnitSlot.gameObject.SetActive(false);
            }
            return dragUnitSlot;
        }
    }

    static private UnitSlot draggedSlot = null;

    static public UnitSlot UseDragUnitSlot(UnitSlot ownerSlot) {
        UnitSlot.draggedSlot = ownerSlot;
        DragUnitSlot.SlotUpdate(ownerSlot.Data, 0);
        DragUnitSlot.gameObject.SetActive(true);
        return DragUnitSlot;
    }
    static public void EndUseDragUnitSlot() {
        DragUnitSlot.gameObject.SetActive(false);
        draggedSlot = null;
    }

    // 연결 정보 //////////////////////////////////////////////////////////////
    [SerializeField] protected Image profileImage;
    [SerializeField] protected TextMeshProUGUI growthLevelText;

    protected virtual void OnDestroy() {
        if (draggedSlot == this)
            EndUseDragUnitSlot();
    }

    public override void SlotUpdate(Unit.Data unitData, int index) {
        base.SlotUpdate(unitData, index);

        // 유닛 이미지 적용
        profileImage.sprite = Data?.GetMainSprite1x2() ?? Unit.NullIcon1x2;

        // 성장 레벨 텍스트
        if (Data == null)
            growthLevelText.text = string.Empty;
        else
            growthLevelText.text = (0 <= Data.GrowthLevel) ? ("+" + Data.GrowthLevel.ToString()) : Data.GrowthLevel.ToString(); ;
    }
}
