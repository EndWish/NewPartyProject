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
        dragImg.sprite = Data?.ProfileImage.sprite;
        dragImg.gameObject.SetActive(true);
    }

    public void OnDrag(PointerEventData eventData) {
        dragImg.transform.position = eventData.position;
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

        UnitTorchSlot unitTorchSlot = eventData.pointerEnter?.GetComponent<UnitTorchSlot>();
        if (unitTorchSlot != null) {
            unitTorchSlot.SoulTorchUI.Unit = Data;
        }

        dragImg.gameObject.SetActive(false);
    }

}
