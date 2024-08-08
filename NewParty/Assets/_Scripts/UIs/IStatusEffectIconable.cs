using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStatusEffectIconable : IIcon1x1
{
    public bool IsSEVisible();

    public Color GetBgColor();

    public string GetTooltipTitleText();
    public string GetTooltipRightUpperText();
    public string GetDescriptionText();

}
