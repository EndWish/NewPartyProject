using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPartyUnits : MonoBehaviour
{
    [ArrayElementTitle("Type")]
    public Unit.Data[] unitDataList = new Unit.Data[UserData.PartySequenceMax];

    private void Start() {
        for(int i = 0; i < UserData.PartySequenceMax; ++i) {
            Unit.Data unitData = unitDataList[i];
            if (unitData == null || unitData.Type == UnitType.None)
                continue;

            UserData.Instance.PartySequence[i] = unitData;
        }
    }

}
