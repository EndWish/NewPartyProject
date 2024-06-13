using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitTorchSlot : UnitSlot, IPointerClickHandler
{
    [SerializeField] private SoulTorchUI soulTorchUI;
    public SoulTorchUI SoulTorchUI { get { return soulTorchUI; } }

    public void OnPointerClick(PointerEventData eventData) {
        if (eventData.button == PointerEventData.InputButton.Right) {
            SoulTorchUI.Unit = null;
        }
    }
}
