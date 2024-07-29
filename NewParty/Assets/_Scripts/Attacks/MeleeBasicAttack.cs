using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeBasicAttack : BasicAttack
{
    public static MeleeBasicAttack Create(Unit caster, Unit target, int tokenStack, float dmg) {
        MeleeBasicAttack attack = Attack.Instantiate<MeleeBasicAttack>(target.transform.position, Quaternion.identity);

        attack.Caster = caster;
        attack.Targets = new List<Unit> { target };
        attack.TokenStack = tokenStack;
        attack.Dmg = dmg;

        return attack;
    }
}
