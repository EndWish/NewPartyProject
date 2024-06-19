using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PassiveSkill : Skill
{
    protected virtual void Awake() {
        IsPassive = true;
    }
}
