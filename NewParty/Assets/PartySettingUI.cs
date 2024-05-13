using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PartySettingUI : MonoBehaviour
{
    [SerializeField] protected Transform slotsParent;
    public UnitPartySlot[] UnitSlots { get; set; } = new UnitPartySlot[UserData.MaxPartyUnit];

    // 유니티 함수 ////////////////////////////////////////////////////////////
    private void Awake() {
        UnitSlots = slotsParent.GetComponentsInChildren<UnitPartySlot>();
    }

    private void Update() {
        for(int i = 0; i < UserData.MaxPartyUnit; ++i) {
            UnitSlots[i].SlotUpdate(UserData.Instance.PartyUnitList[i], i);
        }
    }

}
