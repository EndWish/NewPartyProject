using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSkillStateUI : MonoBehaviour
{
    [SerializeField] private Transform skillIconsParent;
    private List<SkillIcon> skillIconList = new List<SkillIcon>();
    private int offset = 0;
    private int maxOffset = 0;

    [SerializeField] private GameObject leftArrow, rightArrow;

    private void Awake() {
        skillIconList.InsertRange(0, skillIconsParent.GetComponentsInChildren<SkillIcon>());
    }

    public void UpdatePage(Unit unit) {
        if (unit != null) {
            List<Skill> skillList = unit.Skills;

            maxOffset = Mathf.Max(0, skillList.Count - skillIconList.Count);
            offset = Mathf.Clamp(offset, 0, maxOffset);
            for (int iconIndex = 0; iconIndex < skillIconList.Count; ++iconIndex) {
                int iconableIndex = offset + iconIndex;
                if (iconableIndex < skillList.Count) {
                    skillIconList[iconIndex].gameObject.SetActive(true);
                    skillIconList[iconIndex].UpdateIcon(skillList[iconableIndex]);
                }
                else {
                    skillIconList[iconIndex].gameObject.SetActive(false);
                }
            }

            leftArrow.SetActive(offset != 0);
            rightArrow.SetActive(offset != maxOffset);
        }
        else {
            for (int iconIndex = 0; iconIndex < skillIconList.Count; ++iconIndex) {
                skillIconList[iconIndex].gameObject.SetActive(false);
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
