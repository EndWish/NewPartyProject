using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IIcon1x1 : IIcons1x1
{
    public Sprite GetIcon1x1();
}

public interface IIcons1x1
{
    public List<Sprite> GetIcons1x1();
}

public interface IIcon1x2 : IIcons1x2
{
    public Sprite GetIcon1x2();
}

public interface IIcons1x2
{
    public List<Sprite> GetIcons1x2();
}

public interface IRightLowerTextableIcon
{
    public string GetRightLowerText();
}

public interface IRightUpperTextableIcon
{
    public string GetRightUpperText();
}
