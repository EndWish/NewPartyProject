using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public abstract class Skill : MonoBehaviourPun
{

    private Unit owner;

    public Sprite IconSp;

    public string Name { get; protected set; }
    public int Cost;
    public bool IsPassive { get; protected set; }
    
    public Unit Owner {
        get { return owner; }
        set { 
            if (owner == value) return;
            Unit prev = owner;
            owner = value;
            OnSetTarget(prev, owner);
        }
    }
    protected virtual void OnSetTarget(Unit prev, Unit current) {

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
        if (IsPassive)
            return false;
        return true;
    }
    
    public virtual bool SelectionPred(Unit unit) { return true; }
    public virtual bool SelectionPred(Party party) { return true; }
    public abstract BattleSelectionType GetSelectionType();
    public abstract int GetSelectionNum();

    public void Use() {
        BattleManager.Instance.ActionCoroutine = CoUse();
    }
    public abstract IEnumerator CoUse();

    public abstract string GetDescription();

}
