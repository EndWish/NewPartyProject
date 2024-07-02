using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        ActiveSkill useSkill;
        int nAtk, nSkill, nShield;
        ResetTokenSelection(unit, out nAtk, out nSkill, out nShield);

        // 스킬 토큰이 많을 경우 사용한다
        useSkill = FindHighCostUseableSkill(unit, nSkill);
        if (4 <= nSkill && useSkill != null) {
            SelectTokens(unit, TokenType.Skill, useSkill.Cost);
            return ActionResult.Skill;
        }

        // 방어 토큰이 많을 경우
        // 배리어를 사용하면 효과가 있을 때 사용한다
        if (4 <= nShield && IsEffectiveBarrier(unit, nShield)) {
            SelectAllTokens(unit, TokenType.Barrier);
            return ActionResult.Shield;
        }

        // 공격 토큰이 하나라도 있으면 사용
        if (0 < nAtk) {
            SelectAllTokens(unit, TokenType.Atk);
            return ActionResult.Atk;
        }

        // 배리어를 사용하면 효과가 있을 때 사용한다
        if(0 < nShield && IsEffectiveBarrier(unit, nShield)) {
            SelectAllTokens(unit, TokenType.Barrier);
            return ActionResult.Shield;
        }

        // 스킬을 사용할 수 있으면 스킬을 사용한다
        if(useSkill != null) {
            SelectTokens(unit, TokenType.Skill, useSkill.Cost);
            return ActionResult.Skill;
        }

        // 토큰 3개가 비도록 버린다.
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
        nShield = nTokens[(int)TokenType.Barrier];
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
    static protected ActiveSkill FindHighCostUseableSkill(Unit unit, int nSkill) {
        ActiveSkill useSkill = null;
        foreach (Skill skill in unit.Skills) {
            if (skill is not ActiveSkill)
                continue;

            ActiveSkill activeSkill = (ActiveSkill)skill;
            if (activeSkill.Cost <= nSkill && activeSkill.MeetUniqueConditions())
                if (useSkill == null || useSkill.Cost < activeSkill.Cost)
                    useSkill = activeSkill;
        }
        return useSkill;
    }

    // 개인 변수 //////////////////////////////////////////////////////////////
    public SelectMode MySelectMode = SelectMode.None;

    public IEnumerator AutoSelect() {
        Unit unit = GetComponent<Unit>();
        ActionResult actionResult = modeMapping[MySelectMode].Invoke(unit);

        ActiveSkill selectedSkill = null;

        if (actionResult == ActionResult.Atk
            && unit.BasicAtkSkill.CanUse()
            && SelectRandomTarget(unit.BasicAtkSkill)) {
            unit.BasicAtkSkill.Use();
        }
        else if (actionResult == ActionResult.Shield) {
            BattleManager.Instance.ActionCoroutine = unit.BasicBarrierSkill.CoUse();
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
    protected bool SelectRandomTarget(ActiveSkill activeSkill) {
        if(activeSkill == null) 
            return false;

        BattleSelectable.StopSelectMode();
        BattleSelectable.Units.Clear();
        BattleSelectable.Parties.Clear();

        int maxSelection = activeSkill.GetSelectionNum();
        int nSelection = 0;
        switch (activeSkill.GetSelectionType()) {
            case BattleSelectionType.Unit:
                while (nSelection < maxSelection) {
                    List<Unit> units = new List<Unit>();
                    BattleManager.Instance.ActionAllUnit((unit) => {
                        if(activeSkill.SelectionPred(unit))
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
                    BattleManager.Instance.ActionAllUnit((unit) => {
                        if (activeSkill.SelectionPred(unit))
                            parties.Add(unit.MyParty);
                    });
                    parties = parties.Distinct().ToList();

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
    protected ActiveSkill SelectRandomSkill(Unit unit, out ActiveSkill selectedSkill) {
        List<ActiveSkill> activeSkills = new List<ActiveSkill>();
        foreach(Skill skill in unit.Skills) {
            if (skill is not ActiveSkill)
                continue;

            ActiveSkill activeSkill = (ActiveSkill)skill;
            if (activeSkill.CanUse())
                activeSkills.Add(activeSkill);
        }

        if(activeSkills.Count == 0) {
            selectedSkill = null;
        } else {
            selectedSkill = activeSkills.PickRandom();
        }
        
        return selectedSkill;
    }

}
