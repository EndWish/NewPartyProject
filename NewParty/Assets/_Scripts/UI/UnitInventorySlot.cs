using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UnitInventorySlot : UnitSlot, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [SerializeField] Image draggedProfileImage;

    protected void Start() {
        draggedProfileImage.gameObject.SetActive(false);
        draggedProfileImage.transform.SetParent(StaticOverlayCanvas.Instance.transform);
    }

    protected void OnDestroy() {
        if(draggedProfileImage != null) {
            Destroy(draggedProfileImage.gameObject);
        }
    }

    public override void SlotUpdate(Unit unit, int index) {
        base.SlotUpdate(unit, index);

        draggedProfileImage.sprite = unit?.profileRenderer.sprite;
    }

    public void OnBeginDrag(PointerEventData eventData) {
        draggedProfileImage.gameObject.SetActive(true);
    }

    public void OnDrag(PointerEventData eventData) {
        draggedProfileImage.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData) {
        UnitPartySlot unitPartySlot = eventData.pointerEnter?.GetComponent<UnitPartySlot>();
        if (unitPartySlot != null) {
            Unit[] partyUnitList = UserData.Instance.PartyUnitList;

            // 파티에 넣을려는 유닛이 이미 파티에 있으면 제거한다.
            for (int i = 0; i < UserData.MaxPartyUnit; ++i) {
                if (partyUnitList[i] == Data)
                    partyUnitList[i] = null;
            }

            // 파티의 몇번째에 넣을지 알아낸다.
            int index = unitPartySlot.dataIndex;

            // 파티 유닛 리스트의 해당 인덱스에 유닛을 넣는다.
            partyUnitList[index] = Data;
        }

        draggedProfileImage.gameObject.SetActive(false);
    }

}
