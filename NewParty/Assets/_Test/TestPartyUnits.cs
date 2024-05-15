using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPartyUnits : MonoBehaviour
{
    [ArrayElementTitle("Type")]
    public Unit.Data[] unitDataList = new Unit.Data[UserData.MaxPartyUnit];

    private void Start() {
        for(int i = 0; i < UserData.MaxPartyUnit; ++i) {
            Unit.Data unitData = unitDataList[i];
            if (unitData == null || unitData.Type == UnitType.None)
                continue;

            Unit newUnit = UserData.Instance.AddUnitData(unitData);
            UserData.Instance.PartyUnitList[i] = newUnit;
        }
    }

}
