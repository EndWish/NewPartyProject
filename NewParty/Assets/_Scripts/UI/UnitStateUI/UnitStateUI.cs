using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitStateUI : MonoBehaviour
{
    [SerializeField] List<FixedActionBtn> fixedActionBtns;

    private void Update() {
        BattleManager battleManager = BattleManager.Instance;

        Unit actionBtnUnit = battleManager.UnitOnMouse;
        actionBtnUnit ??= battleManager.UnitClicked;
        actionBtnUnit ??= battleManager.UnitOfTurn;

        foreach (var actionBtn in fixedActionBtns) {
            actionBtn.UpdateBtn(actionBtnUnit);
        }

    }

}
