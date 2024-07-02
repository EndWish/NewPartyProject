using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldActionBtn : FixedActionBtn
{
    protected override void UpdateBtn() {
        BattleManager battleManager = BattleManager.Instance;

        if (!MeetActiveBasicCondition()) {
            Active = false;
            return;
        }

        //atk 토큰만 활성화 되어 있을 경우 이 버튼이 활성화 된다.
        bool result = false;
        foreach (Token token in targetUnit.Tokens) {
            if (!token.IsSelected)
                continue;

            if (token.Type == TokenType.Shield) {
                result = true;
            } else {
                result = false;
                break;
            }
        }

        Active = result;
    }

    public override void OnClick() {
        BattleManager battleManager = BattleManager.Instance;

        if (MeetClickCondition()) {
            BattleSelectable.StopSelectMode();
            battleManager.ActionCoroutine = targetUnit.CoBasicBarrier();
        }
    }

    protected override string GetTooltipTitle() {
        return "기본 방어막 생성";
    }

    protected override string GetTooltipRightUpperText() {
        return "방어 토큰 1개 이상";
    }

    protected override string GetTooltipDescription() {
        return "쉴드의 100% 만큼 방어막을 생성한다. 방어 토큰을 추가로 사용할 경우 개당 스택 쉴드 만큼 방어막이 늘어난다.";
    }
}
