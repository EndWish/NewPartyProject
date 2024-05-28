using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscardActionBtn : FixedActionBtn
{
    protected override void UpdateBtn() {
        BattleManager battleManager = BattleManager.Instance;

        if (!MeetActiveBasicCondition()) {
            Active = false;
            return;
        }

        //토큰이 하나라도 활성화 되어있으면 활성화 된다.
        bool result = false;
        foreach (Token token in targetUnit.Tokens) {
            if (token.IsSelected) {
                result = true;
                break;
            }
        }

        Active = result;
    }

    public override void OnClick() {
        BattleManager battleManager = BattleManager.Instance;

        if (MeetClickCondition()) {
            battleManager.ActionCoroutine = targetUnit.CoDiscardAction();
        }
    }
}
