using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SoulInventorySlot : SoulSlot, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    static private Transform dragTransform = null;
    static private MultiLayerImageController dragImages = null;
    
    static private int nSlot = 0;

    [SerializeField] private Transform dragPrefab;

    protected void Awake() {
        ++nSlot;
    }

    protected void Start() {
        if (dragTransform == null) {
            dragTransform = Instantiate(dragPrefab, StaticOverlayCanvas.Instance.transform);
            dragImages = dragTransform.GetComponent<MultiLayerImageController>();
            dragTransform.gameObject.SetActive(false);
        }
    }

    protected void OnDestroy() {
        --nSlot;
        if (nSlot == 0 && dragTransform != null) {
            Destroy(dragTransform.gameObject);
        }
    }

    public void OnBeginDrag(PointerEventData eventData) {
        dragImages.Sprites = Data.GetMainSprites1x1();
        dragTransform.gameObject.SetActive(true);
    }

    public void OnDrag(PointerEventData eventData) {
        dragTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData) {
        SoulTorchSlot soulTorchSlot = eventData.pointerEnter?.GetComponent<SoulTorchSlot>();
        if (soulTorchSlot != null) {
            soulTorchSlot.SoulTorchUI.SoulFragment = this.Data;
        }

        dragTransform.gameObject.SetActive(false);
    }
}