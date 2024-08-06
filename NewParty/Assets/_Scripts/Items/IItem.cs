using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IItem : IIcons1x1
{
    public string Name { get; }
    public int Num { get; }

    public void InsertTo(UserData userData);
    public void InsertTo(List<IItem> list);

}
