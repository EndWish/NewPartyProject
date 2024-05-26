using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscardActionBtn : FixedActionBtn
{
    protected override void UpdateBtn() {
        //��ū�� �ϳ��� Ȱ��ȭ �Ǿ������� Ȱ��ȭ �ȴ�.
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
        throw new System.NotImplementedException();
    }
}
