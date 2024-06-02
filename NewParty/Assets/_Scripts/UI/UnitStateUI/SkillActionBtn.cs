using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillActionBtn : ActionBtn
{
    [SerializeField] protected Image IconImg;
    [SerializeField] protected TextMeshProUGUI CostText;
    protected Skill targetSkill;

    public void UpdateBtn(Unit unit, Skill skill) {
        if(unit == null || skill == null) {
            gameObject.SetActive(false);
            return;
        } else {
            gameObject.SetActive(true);
        }

        targetUnit = unit;
        targetSkill = skill;
        IconImg.sprite = targetSkill.IconSp;
        CostText.text = targetSkill.IsPassive ? "" : targetSkill.Cost.ToString();

        BattleManager battleManager = BattleManager.Instance;

        if (!MeetActiveBasicCondition()) {
            Active = false;
            return;
        }

        //atk ��ū�� Ȱ��ȭ �Ǿ� ���� ��� �� ��ư�� Ȱ��ȭ �ȴ�.
        Active = targetSkill.CanUse();
    }

    public override void OnClick() {
        Debug.Log("OnClick");
        BattleManager battleManager = BattleManager.Instance;

        if (MeetClickCondition()) {
            Debug.Log("OnClick/MeetClickCondition()���");
            if (BattleSelectable.IsRunning) {
                if (ActionUnit == null) {
                    BattleSelectable.StopSelectMode();
                    RunSelectMode();
                } else {
                    BattleSelectable.StopSelectMode();
                }
            } else {
                RunSelectMode();
            }
        }
    }

    protected void RunSelectMode() {
        Debug.Log("RunSelectMode");

        ActionUnit = targetUnit;

        BattleSelectable.RunSelectMode(targetSkill.GetSelectionType(),
            targetSkill.GetSelectionNum(),
            targetSkill.SelectionPred,
            OnCompleteSelection,
            OnCancel);
    }

    protected override void OnCompleteSelection() {
        targetSkill.Use();
        base.OnCompleteSelection();
    }

}
