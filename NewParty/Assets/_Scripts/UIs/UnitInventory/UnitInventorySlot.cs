using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UnitInventorySlot : UnitSlot, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    static private Image dragImg = null;
    static private int nUnitSlot = 0;

    [SerializeField] private Image dragImgPrefab;

    protected void Awake() {
        ++nUnitSlot;
    }

    protected void Start() {
        if(dragImg == null) {
            dragImg = Instantiate(dragImgPrefab, StaticOverlayCanvas.Instance.transform);
            dragImg.gameObject.SetActive(false);
        }
    }

    protected void OnDestroy() {
        --nUnitSlot;
        if (nUnitSlot == 0 && dragImg != null) {
            Destroy(dragImg.gameObject);
        }
    }

    public void OnBeginDrag(PointerEventData eventData) {
        dragImg.sprite = Data?.ProfileSprite;
        dragImg.gameObject.SetActive(true);
    }

    public void OnDrag(PointerEventData eventData) {
        dragImg.transform.position = eventData.position;
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
            int index = partySequenceSlot.dataIndex;

            // ��Ƽ ���� ����Ʈ�� �ش� �ε����� ������ �ִ´�.
            partySequence[index] = Data;

            UserData.Instance.SavePartySequence();
        }

        // ��ȭ�� �巡�� ���
        UnitTorchSlot unitTorchSlot = eventData.pointerEnter?.GetComponent<UnitTorchSlot>();
        if (unitTorchSlot != null) {
            unitTorchSlot.SoulTorchUI.UnitData = Data;
        }

        dragImg.gameObject.SetActive(false);
    }

}
