using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActiveSkill : Skill, IIconRightLowerTextable
{
    public int Cost;

    protected virtual void Awake() {
        IsPassive = false;
    }

    public bool CanUse() {
        return MeetUniqueConditions() && MeetTokenCost();
    }
    protected virtual bool MeetTokenCost() {
        bool result = true;
        int count = 0;
        foreach (var token in Owner.Tokens) {
            if (!token.IsSelected)
                continue;

            if (token.Type != TokenType.Skill) {
                result = false;
                break;
            }

            ++count;
        }

        if (count != Cost)
            result = false;

        return result;
    }
    public virtual bool MeetUniqueConditions() {
        if (Owner.Tags.Contains(Tag.기절))
            return false;
        return true;
    }

    public virtual bool SelectionPred(Unit unit) { return true; }
    public abstract BattleSelectionType GetSelectionType();
    public abstract int GetSelectionNum();

    public void Use() {
        if(this is not BasicAttackSkill && this is not BasicBarrierSkill) {
            Owner.OnUseActiveSkill?.Invoke(this);
        }
        BattleManager.Instance.ActionCoroutine = CoUse();
    }
    public abstract IEnumerator CoUse();

    public string GetIconRightLowerText() {
        return Cost.ToString();
    }

    public override string GetTooltipRightUpperText() {
        return string.Format("액티브(비용 : {0})", Cost);
    }
}
