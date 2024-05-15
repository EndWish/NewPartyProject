using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "Dungeon Node Info", menuName = "Scriptable Object/Dungeon Node Info")]
public class DungeonNodeInfo : NodeInfo
{
    // ���� Ŭ���� ////////////////////////////////////////////////////////////
    [Serializable]
    public class UnitInfo {
        public UnitType UnitType;
        public int2 GrowthLevelRange;

        public Unit.Data ToUnitData() {
            Unit.Data unitData = new Unit.Data(UnitType, UnityEngine.Random.Range(GrowthLevelRange.x, GrowthLevelRange.y + 1));
            return unitData;
        }
    }

    [Serializable]
    public class RandomUnitInfo {
        public UnitInfo UnitInfo;
        [Range(0f, 100f)] public float Importance; // ����
    }

    [Serializable]
    public class WaveInfo {
        public int Wave;
        public List<UnitInfo> UnitInfoList;
    }

    // ���� ���� //////////////////////////////////////////////////////////////
    public Sprite BackgroundImg;

    // ���� ���� ����
    [ArrayElementTitle("UnitInfo.UnitType")]
    public List<RandomUnitInfo> RandomUnitInfoList;

    // Ư�� ���̺� ���� ����
    [ArrayElementTitle("Wave")]
    public List<WaveInfo> WaveInfoList;


    // �Լ� ///////////////////////////////////////////////////////////////////
    public List<UnitInfo> GetRandomUnitInfo(int num) {
        List<UnitInfo> unitInfoList = new List<UnitInfo>();
        float importanceSum = RandomUnitInfoList.Sum(info => info.Importance);

        for(int i = 0; i < num; ++i) {
            float random = UnityEngine.Random.Range(0f, importanceSum);
            int index = 0;
            while (index < RandomUnitInfoList.Count && random <= RandomUnitInfoList[index].Importance)
                ++index;
            --index;

            unitInfoList.Add(RandomUnitInfoList[index].UnitInfo);
        }
        return unitInfoList;
    }

}
