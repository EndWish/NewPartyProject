using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPartyUnits : MonoBehaviour
{
    public Unit.Data[] unitDataList = new Unit.Data[UserData.PartySequenceMax];

    private void Start() {
        for(int i = 0; i < Mathf.Min(UserData.PartySequenceMax, unitDataList.Length); ++i) {
            Unit.Data unitData = unitDataList[i];
            if (unitData == null || unitData.Type == UnitType.None)
                continue;

            UserData.Instance.PartySequence[i] = unitData;
        }
    }

}
