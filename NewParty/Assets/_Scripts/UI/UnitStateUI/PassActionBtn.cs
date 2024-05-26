using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassActionBtn : FixedActionBtn
{
    protected override void UpdateBtn() {
        //targetUnit 과 현재 턴을 가진 유닛이 같으면 활성화 한다.
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
