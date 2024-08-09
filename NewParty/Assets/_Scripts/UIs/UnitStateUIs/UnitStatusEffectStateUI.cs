using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitStatusEffectStateUI : MonoBehaviour
{
    [SerializeField] private Transform seIconsParent;
    private List<StatusEffectIcon> seIconList = new List<StatusEffectIcon>();
    private int offset = 0;
    private int maxOffset = 0;

    [SerializeField] private GameObject leftArrow, rightArrow;

    private void Awake() {
        seIconList.InsertRange(0, seIconsParent.GetComponentsInChildren<StatusEffectIcon>());
    }

    public void UpdatePage(Unit unit) {
        if(unit != null) {
            List<IStatusEffectIconable> seIconableList = unit.UnitCanvas.GetVisibleSEIconables();

            maxOffset = Mathf.Max(0, seIconableList.Count - seIconList.Count);
            offset = Mathf.Clamp(offset, 0, maxOffset);
            for (int iconIndex = 0; iconIndex < seIconList.Count; ++iconIndex) {
                int iconableIndex = offset + iconIndex;
                if(iconableIndex < seIconableList.Count) {
                    seIconList[iconIndex].gameObject.SetActive(true);
                    seIconList[iconIndex].UpdateIcon(seIconableList[iconableIndex]);
                }
                else {
                    seIconList[iconIndex].gameObject.SetActive(false);
                }
            }

            leftArrow.SetActive(offset != 0);
            rightArrow.SetActive(offset != maxOffset);
        }
        else {
            for (int iconIndex = 0; iconIndex < seIconList.Count; ++iconIndex) {
                seIconList[iconIndex].gameObject.SetActive(false);
            }
            leftArrow.SetActive(false);
            rightArrow.SetActive(false);
        }
    }

    public void RaiseOffset() {
        offset = Mathf.Min(offset + 1, maxOffset);
    }
    public void DownOffset() {
        offset = Mathf.Max(offset - 1, 0);
    }
    public void OnScroll() {
        Vector2 wheelInput = Input.mouseScrollDelta;
        if (wheelInput.y > 0) {    // »Ÿ ¿ß∑Œ
            DownOffset();
        }
        else if (wheelInput.y < 0) {   // »Ÿ æ∆∑°∑Œ
            RaiseOffset();
        }
    }

}
