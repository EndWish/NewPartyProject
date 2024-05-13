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

            // ��Ƽ�� �������� ������ �̹� ��Ƽ�� ������ �����Ѵ�.
            for (int i = 0; i < UserData.MaxPartyUnit; ++i) {
                if (partyUnitList[i] == Data)
                    partyUnitList[i] = null;
            }

            // ��Ƽ�� ���°�� ������ �˾Ƴ���.
            int index = unitPartySlot.dataIndex;

            // ��Ƽ ���� ����Ʈ�� �ش� �ε����� ������ �ִ´�.
            partyUnitList[index] = Data;
        }

        draggedProfileImage.gameObject.SetActive(false);
    }

}
