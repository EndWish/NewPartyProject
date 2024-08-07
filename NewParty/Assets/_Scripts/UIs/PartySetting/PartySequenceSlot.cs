using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PartySequenceSlot : UnitSlot, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerClickHandler
{
    private UnitSlot dragUnitSlot = null;

    public void OnBeginDrag(PointerEventData eventData) {
        if (Data != null && eventData.button == PointerEventData.InputButton.Left) {
            dragUnitSlot = UseDragUnitSlot(this);
        }
    }

    public void OnDrag(PointerEventData eventData) {
        if (dragUnitSlot == null)
            return;

        dragUnitSlot.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData) {
        if (dragUnitSlot == null)
            return;

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
        }

        EndUseDragUnitSlot();
        dragUnitSlot = null;
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (eventData.button == PointerEventData.InputButton.Right) {
            UserData.Instance.PartySequence[DataIndex] = null;
            UserData.Instance.SavePartySequence();
        }
    }
}
