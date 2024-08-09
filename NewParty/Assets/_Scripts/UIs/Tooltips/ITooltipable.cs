using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITooltipable : IMainSprite1x1
{
    public string GetTooltipTitleText();
    public string GetTooltipRightUpperText();
    public string GetDescriptionText();
}

public interface IDetailedDescription
{
    public string GetDetailedDescriptionText();
}