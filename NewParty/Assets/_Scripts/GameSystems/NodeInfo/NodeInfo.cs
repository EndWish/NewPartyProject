using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NodeName : int
{
    None, 
    Village = 1, 
    Seolhwa01 = 101, Seolhwa02, Seolhwa03, Seolhwa04, Seolhwa05,
}

[CreateAssetMenu(fileName = "Node Info", menuName = "Scriptable Object/Node Info")]
public class NodeInfo : ScriptableObject
{
    public NodeName Name;
    public Sprite Icon;
}
