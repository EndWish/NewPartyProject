using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonNode : Node
{
    protected new DungeonNodeInfo nodeInfo {
        get {  return (DungeonNodeInfo)base.nodeInfo; }
        set { base.nodeInfo = value; }
    }


}
