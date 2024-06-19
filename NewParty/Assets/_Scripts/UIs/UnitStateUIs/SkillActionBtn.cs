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
        CostText.text = targetSkill is ActiveSkill ? ((ActiveSkill)targetSkill).Cost.ToString() : "";

        BattleManager battleManager = BattleManager.Instance;

        if (!MeetActiveBasicCondition()) {
            Active = false;
            return;
        }

        Active = targetSkill is ActiveSkill && ((ActiveSkill)targetSkill).CanUse();

        if (targetSkill != null && targetSkill is PassiveSkill)
            bgImg.color = new Color(0.9f, 0, 0.9f);
    }

    public override void OnClick() {
        BattleManager battleManager = BattleManager.Instance;

        if (MeetClickCondition()) {
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
        ActionUnit = targetUnit;
        ActiveSkill activeSkill = (ActiveSkill)targetSkill;

        BattleSelectable.RunSelectMode(activeSkill.GetSelectionType(),
            activeSkill.GetSelectionNum(),
            activeSkill.SelectionPred,
            OnCompleteSelection,
            OnCancel);
    }

    protected override void OnCompleteSelection() {
        ActiveSkill activeSkill = (ActiveSkill)targetSkill;

        activeSkill.Use();
        base.OnCompleteSelection();
    }

}
