using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMainSprite1x1 : IMainSprites1x1
{
    public Sprite GetMainSprite1x1();
}

public interface IMainSprites1x1
{
    public List<Sprite> GetMainSprites1x1();
}

public interface IMainSprite1x2 : IMainSprites1x2
{
    public Sprite GetMainSprite1x2();
}

public interface IMainSprites1x2
{
    public List<Sprite> GetMainSprites1x2();
}


