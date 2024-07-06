using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PartySettingUI : MonoBehaviour
{
    [SerializeField] protected Transform slotsParent;
    public PartySequenceSlot[] UnitSlots { get; set; } = new PartySequenceSlot[UserData.PartySequenceMax];

    // 유니티 함수 ////////////////////////////////////////////////////////////
    private void Awake() {
        UnitSlots = slotsParent.GetComponentsInChildren<PartySequenceSlot>();
    }

    private void Update() {
        for(int i = 0; i < UserData.PartySequenceMax; ++i) {
            UnitSlots[i].SlotUpdate(UserData.Instance.PartySequence[i], i);
        }
    }

}
