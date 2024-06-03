using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class TokenSelector : MonoBehaviour
{
    public enum ActionResult {
        None, Pass, Discard, Atk, Shield, Skill,
    }
    public enum SelectMode
    {
        None, BasicAtkMode, 
    }

    static private Dictionary<SelectMode, Func<Unit, ActionResult>> modeMapping = new Dictionary<SelectMode, Func<Unit, ActionResult>> {
        {SelectMode.None,  BasicAtkMode},
        {SelectMode.BasicAtkMode,  BasicAtkMode}
    };

    static public ActionResult BasicAtkMode(Unit unit) {
        Skill useSkill;
        int nAtk, nSkill, nShield;
        ResetTokenSelection(unit, out nAtk, out nSkill, out nShield);

        // ��ų ��ū�� ���� ��� ����Ѵ�
        useSkill = FindHighCostUseableSkill(unit, nSkill);
        if (4 <= nSkill && useSkill != null) {
            SelectTokens(unit, TokenType.Skill, useSkill.Cost);
            return ActionResult.Skill;
        }

        // ��� ��ū�� ���� ���
        // �踮� ����ϸ� ȿ���� ���� �� ����Ѵ�
        if (4 <= nShield && IsEffectiveBarrier(unit, nShield)) {
            SelectAllTokens(unit, TokenType.Shield);
            return ActionResult.Shield;
        }

        // ���� ��ū�� �ϳ��� ������ ���
        if (0 < nAtk) {
            SelectAllTokens(unit, TokenType.Atk);
            return ActionResult.Atk;
        }

        // �踮� ����ϸ� ȿ���� ���� �� ����Ѵ�
        if(0 < nShield && IsEffectiveBarrier(unit, nShield)) {
            SelectAllTokens(unit, TokenType.Shield);
            return ActionResult.Shield;
        }

        // ��ų�� ����� �� ������ ��ų�� ����Ѵ�
        if(useSkill != null) {
            SelectTokens(unit, TokenType.Skill, useSkill.Cost);
            return ActionResult.Skill;
        }

        // ��ū 3���� �񵵷� ������.
        int nTokens = nAtk + nSkill + nShield;
        int nBlank = unit.MaxTokens - nTokens;
        int nDiscard = 3 - nBlank;
        if(0 < nDiscard) {
            SelectTokens(unit, nDiscard);
            return ActionResult.Discard;
        }

        return ActionResult.Pass;
    }

    static protected void ResetTokenSelection(Unit unit) {
        UsefulMethod.ActionAll(unit.Tokens, (token) => {
            token.IsSelected = false;
        });
    }
    static protected void ResetTokenSelection(Unit unit, out int nAtk, out int nSkill, out int nShield) {
        int[] nTokens = new int[(int)TokenType.Num];
        UsefulMethod.ActionAll(unit.Tokens, (token) => {
            token.IsSelected = false;
            ++nTokens[(int)token.Type];
        });
        nAtk = nTokens[(int)TokenType.Atk];
        nSkill = nTokens[(int)TokenType.Skill];
        nShield = nTokens[(int)TokenType.Shield];
    }
    static protected void SelectAllTokens(Unit unit) {
        foreach (var token in unit.Tokens) {
            token.IsSelected = true;
        }
    }
    static protected void SelectAllTokens(Unit unit, TokenType type) {
        foreach (var token in unit.Tokens) {
            if(token.Type == type)
                token.IsSelected = true;
        }
    }
    static protected void SelectTokens(Unit unit, TokenType type, int num) {
        foreach (var token in unit.Tokens) {
            if (token.Type == type && 0 < num) {
                token.IsSelected = true;
                --num;
            }
        }
    }
    static protected void SelectTokens(Unit unit, int num) {
        foreach (var token in unit.Tokens) {
            if (0 < num) {
                token.IsSelected = true;
                --num;
            }
        }
    }
    static protected bool IsEffectiveBarrier(Unit unit, int nTokens) {
        Barrier barrier = unit.Barriers.Find((barrier) => { return barrier is BasicBarrier; });
        if (barrier == null)
            return true;

        float amount = unit.GetFinalStat(StatType.Shield);
        amount *= (1f + unit.GetFinalStat(StatType.StackShield) * (nTokens - 1));

        if (barrier.Amount <= amount * 0.7f)
            return true;

        return false;
    }
    static protected Skill FindHighCostUseableSkill(Unit unit, int nSkill) {
        Skill useSkill = null;
        foreach (Skill skill in unit.Skills) {
            if (skill.Cost <= nSkill && skill.MeetUniqueConditions())
                if (useSkill == null || useSkill.Cost < skill.Cost)
                    useSkill = skill;
        }
        return useSkill;
    }

    // ���� ���� //////////////////////////////////////////////////////////////
    public SelectMode MySelectMode = SelectMode.None;

    public IEnumerator AutoSelect() {
        Unit unit = GetComponent<Unit>();
        ActionResult actionResult = modeMapping[MySelectMode].Invoke(unit);

        Skill selectedSkill = null;

        if (actionResult == ActionResult.Atk
            && unit.BasicAtkSkill.CanUse()
            && SelectRandomTarget(unit.BasicAtkSkill)) {
            unit.BasicAtkSkill.Use();
        }
        else if (actionResult == ActionResult.Shield) {
            BattleManager.Instance.ActionCoroutine = unit.CoBasicBarrier();
        }
        else if (actionResult == ActionResult.Skill
            && SelectRandomTarget(SelectRandomSkill(unit, out selectedSkill))) {
            selectedSkill.Use();
        }
        else if (actionResult == ActionResult.Discard) {
            BattleManager.Instance.ActionCoroutine = unit.CoDiscardAction();
        }
        else {
            BattleManager.Instance.ActionCoroutine = unit.CoPassAction();
        }

        yield return new WaitForSeconds(0.5f);
    }
    protected bool SelectRandomTarget(Skill skill) {
        if(skill == null) 
            return false;

        BattleSelectable.StopSelectMode();
        BattleSelectable.Units.Clear();
        BattleSelectable.Parties.Clear();

        int maxSelection = skill.GetSelectionNum();
        int nSelection = 0;
        switch (skill.GetSelectionType()) {
            case BattleSelectionType.Unit:
                while (nSelection < maxSelection) {
                    List<Unit> units = new List<Unit>();
                    BattleManager.Instance.ActionAllUnit((unit) => {
                        if(skill.SelectionPred(unit))
                            units.Add(unit);
                    });

                    if (units.Count == 0)
                        break;

                    BattleSelectable.Units.Add(units.PickRandom());
                    ++nSelection;
                }
                break;

            case BattleSelectionType.Party:
                while (nSelection < maxSelection) {
                    List<Party> parties = new List<Party>();
                    BattleManager.Instance.ActionAllParty((party) => {
                        if (skill.SelectionPred(party))
                            parties.Add(party);
                    });

                    if (parties.Count == 0)
                        break;

                    BattleSelectable.Parties.Add(parties.PickRandom());
                    ++nSelection;
                }
                break;
        }

        if (nSelection == 0)
            return false;
        return true;
    }
    protected Skill SelectRandomSkill(Unit unit, out Skill selectedSkill) {
        List<Skill> skills = new List<Skill>();
        foreach(Skill skill in unit.Skills) {
            if(skill.CanUse())
                skills.Add(skill);
        }

        if(skills.Count == 0) {
            selectedSkill = null;
        } else {
            selectedSkill = skills.PickRandom();
        }
        
        return selectedSkill;
    }

}