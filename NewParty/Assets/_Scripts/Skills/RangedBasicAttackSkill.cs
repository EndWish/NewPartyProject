using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedBasicAttackSkill : BasicAttackSkill
{
    public override IEnumerator CoUse() {
        // 토큰을 개수를 세고 삭제한다
        int tokenStack = Owner.GetSelectedTokensNum();
        yield return StartCoroutine(Owner.UseSelectedTokens());

        // 공격을 생성한다
        Unit target = BattleSelectable.Units[0];
        BasicAttack attack = RangedBasicAttack.Create(Owner, target, tokenStack, CalculateDmg(tokenStack));

        yield return StartCoroutine(attack.Animate());
    }
}
