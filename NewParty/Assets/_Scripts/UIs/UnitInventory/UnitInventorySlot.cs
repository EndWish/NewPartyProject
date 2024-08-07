using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UnitInventorySlot : UnitSlot, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    private UnitSlot dragUnitSlot = null;

    public void OnBeginDrag(PointerEventData eventData) {
        dragUnitSlot = UseDragUnitSlot(this);
    }

    public void OnDrag(PointerEventData eventData) {
        dragUnitSlot.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData) {
        // ��Ƽ ���Կ� �巡�� ���
        PartySequenceSlot partySequenceSlot = eventData.pointerEnter?.GetComponent<PartySequenceSlot>();
        if (partySequenceSlot != null) {
            Unit.Data[] partySequence = UserData.Instance.PartySequence;

            // ��Ƽ�� �������� ������ �̹� ��Ƽ�� ������ �����Ѵ�.
            for (int i = 0; i < UserData.PartySequenceMax; ++i) {
                if (partySequence[i] == Data)
                    partySequence[i] = null;
            }

            // ��Ƽ�� ���°�� ������ �˾Ƴ���.
            int index = partySequenceSlot.DataIndex;

            // ��Ƽ ���� ����Ʈ�� �ش� �ε����� ������ �ִ´�.
            partySequence[index] = Data;

            UserData.Instance.SavePartySequence();

            // ��ȭ�� �巡�� ���
            UnitTorchSlot unitTorchSlot = eventData.pointerEnter?.GetComponent<UnitTorchSlot>();
            if (unitTorchSlot != null) {
                unitTorchSlot.SoulTorchUI.UnitData = Data;
            }

        }

        EndUseDragUnitSlot();
    }

}
