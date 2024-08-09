using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitActionBtnStateUI : MonoBehaviour
{
    [SerializeField] private List<FixedActionBtn> fixedActionBtns;

    [SerializeField] private Transform skillActionsParent;
    private List<SkillActionBtn> skillActionBtns = new List<SkillActionBtn>();
    private int skillActionBtnOffset = 0;
    private int skillActionBtnMaxOffset = 0;

    [SerializeField] private GameObject leftArrow, rightArrow;

    private void Awake() {
        skillActionBtns.InsertRange(0, skillActionsParent.GetComponentsInChildren<SkillActionBtn>());
    }

    private void Update() {
        // 액션 버튼 업데이트
        Unit UnitOfTurn = BattleManager.Instance.UnitOfTurn;
        foreach (var actionBtn in fixedActionBtns) {
            actionBtn.UpdateBtn(UnitOfTurn);
        }

        skillActionBtnMaxOffset = Mathf.Max(0, (UnitOfTurn?.Skills.Count ?? 0) - skillActionBtns.Count);
        skillActionBtnOffset = Mathf.Clamp(skillActionBtnOffset, 0, skillActionBtnMaxOffset);
        for (int btnIndex = 0; btnIndex < skillActionBtns.Count; ++btnIndex) {
            int skillIndex = skillActionBtnOffset + btnIndex;
            Skill skill = (skillIndex < UnitOfTurn?.Skills.Count) ? UnitOfTurn.Skills[skillIndex] : null;
            skillActionBtns[btnIndex].UpdateBtn(UnitOfTurn, skill);
        }

        leftArrow.SetActive(skillActionBtnOffset != 0);
        rightArrow.SetActive(skillActionBtnOffset != skillActionBtnMaxOffset);

        // 키보드 입력
        List<KeyCode> keyCodes = new List<KeyCode> { KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.T, KeyCode.Y, KeyCode.U };
        for (int i = 0; i < keyCodes.Count; i++) {
            KeyCode keyCode = keyCodes[i];
            if (Input.GetKeyUp(keyCode)) {
                if (i < fixedActionBtns.Count)
                    fixedActionBtns[i].OnClick();
                else
                    skillActionBtns[i - fixedActionBtns.Count].OnClick();
            }
        }
    }

    public void RaiseSkillActionBtnOffset() {
        skillActionBtnOffset = Mathf.Min(skillActionBtnOffset + 1, skillActionBtnMaxOffset);
    }
    public void DownSkillActionBtnOffset() {
        skillActionBtnOffset = Mathf.Max(skillActionBtnOffset - 1, 0);
    }
    public void OnScrollSkillActionBtns() {
        Vector2 wheelInput = Input.mouseScrollDelta;
        if (wheelInput.y > 0) {    // 휠 위로
            DownSkillActionBtnOffset();
        }
        else if (wheelInput.y < 0) {   // 휠 아래로
            RaiseSkillActionBtnOffset();
        }
    }

}
