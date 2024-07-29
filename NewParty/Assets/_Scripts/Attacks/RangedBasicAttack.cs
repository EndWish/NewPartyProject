using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedBasicAttack : BasicAttack
{
    public static RangedBasicAttack Create(Unit caster, Unit target, int tokenStack, float dmg) {
        RangedBasicAttack attack = Attack.Instantiate<RangedBasicAttack>(target.transform.position, Quaternion.identity);

        attack.Caster = caster;
        attack.Targets = new List<Unit> { target };
        attack.TokenStack = tokenStack;
        attack.Dmg = dmg;

        return attack;
    }
}
