using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldActionBtn : FixedActionBtn
{
    protected override void UpdateBtn() {
        //atk ��ū�� Ȱ��ȭ �Ǿ� ���� ��� �� ��ư�� Ȱ��ȭ �ȴ�.
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
        throw new System.NotImplementedException();
    }
}
