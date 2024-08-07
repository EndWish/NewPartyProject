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

        // 파티 슬롯에 드래한 경우
        PartySequenceSlot partySequenceSlot = eventData.pointerEnter?.GetComponent<PartySequenceSlot>();
        if (partySequenceSlot != null) {
            Unit.Data[] partySequence = UserData.Instance.PartySequence;

            // 파티에 넣을려는 유닛이 이미 파티에 있으면 제거한다.
            for (int i = 0; i < UserData.PartySequenceMax; ++i) {
                if (partySequence[i] == Data)
                    partySequence[i] = null;
            }

            // 파티의 몇번째에 넣을지 알아낸다.
            int index = partySequenceSlot.DataIndex;

            // 파티 유닛 리스트의 해당 인덱스에 유닛을 넣는다.
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
