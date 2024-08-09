using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStatusEffectIconable : IMainSprite1x1, ITooltipable
{
    public bool IsSEVisible();

    public Color GetBgColor();
}
