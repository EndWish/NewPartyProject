using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class UnitDrawLayerController : MonoBehaviour
{
    private Unit unit;
    private Canvas canvas;
    private SortingGroup sortingGroup;

    private void Awake() {
        unit = GetComponent<Unit>();
        canvas = GetComponentInChildren<Canvas>();
        sortingGroup = GetComponent<SortingGroup>();
    }

    private void Update() {
        Party party = unit.MyParty;
        int partyIndex = party.GetIndex();

        canvas.sortingOrder = -partyIndex;
        sortingGroup.sortingOrder = -partyIndex;
    }

}
