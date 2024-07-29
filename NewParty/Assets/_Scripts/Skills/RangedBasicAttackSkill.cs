using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedBasicAttackSkill : BasicAttackSkill
{
    public override IEnumerator CoUse() {
        // ��ū�� ������ ���� �����Ѵ�
        int tokenStack = Owner.GetSelectedTokensNum();
        yield return StartCoroutine(Owner.UseSelectedTokens());

        // ������ �����Ѵ�
        Unit target = BattleSelectable.Units[0];
        BasicAttack attack = RangedBasicAttack.Create(Owner, target, tokenStack, CalculateDmg(tokenStack));

        yield return StartCoroutine(attack.Animate());
    }
}
