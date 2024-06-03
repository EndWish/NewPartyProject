using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FixedActionBtn : ActionBtn
{
    public void UpdateBtn(Unit unit) {
        targetUnit = unit;
        UpdateBtn();
    }
    protected abstract void UpdateBtn();
}
