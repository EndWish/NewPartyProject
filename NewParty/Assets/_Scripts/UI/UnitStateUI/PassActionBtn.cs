using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassActionBtn : FixedActionBtn
{
    protected override void UpdateBtn() {
        //targetUnit �� ���� ���� ���� ������ ������ Ȱ��ȭ �Ѵ�.
        if(targetUnit == BattleManager.Instance.UnitOfTurn) {
            Active = true;
        } else {
            Active = false;
        }
    }

    public override void OnClick() {
        throw new System.NotImplementedException();
    }
}
