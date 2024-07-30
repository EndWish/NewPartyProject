using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDungeonInfoSetter : MonoBehaviour
{
#if DEVELOPMENT_BUILD || UNITY_EDITOR
    public DungeonNodeInfo TestDungeonInfo { get; set; }

    [SerializeField] private BattleManager battleManager;
    [SerializeField] private DungeonNodeInfo dungeonNodeInfo;

    private void Awake() {
        if (battleManager != null)
            battleManager.TestDungeonInfo = dungeonNodeInfo;
    }
#endif
}
