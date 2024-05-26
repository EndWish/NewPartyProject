using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtkActionBtn : FixedActionBtn
{
    protected override void UpdateBtn() {
        //atk 토큰만 활성화 되어 있을 경우 이 버튼이 활성화 된다.
        bool result = false;
        foreach(Token token in targetUnit.Tokens) {
            if (!token.IsSelected)
                continue;

            if(token.Type == TokenType.Atk) {
                result = true;
            }
            else {
                result = false;
                break;
            }
        }

        Active = result;
    }

    public override void OnClick() {
        throw new System.NotImplementedException();
    }
}
