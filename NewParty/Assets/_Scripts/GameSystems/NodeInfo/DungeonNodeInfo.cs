using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "Dungeon Node Info", menuName = "Scriptable Object/Dungeon Node Info")]
public class DungeonNodeInfo : NodeInfo
{
    // 서브 클래스 ////////////////////////////////////////////////////////////
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
        [Range(0f, 100f)] public float Importance; // 비중
    }

    [Serializable]
    public class WaveInfo {
        public int Wave;
        public List<UnitInfo> UnitInfoList;
    }

    // 개인 정보 //////////////////////////////////////////////////////////////
    public Sprite BackgroundImg;

    // 랜덤 출현 정보
    [ArrayElementTitle("UnitInfo.UnitType")]
    public List<RandomUnitInfo> RandomUnitInfoList;

    // 특정 웨이브 출현 정보
    [ArrayElementTitle("Wave")]
    public List<WaveInfo> WaveInfoList;


    // 함수 ///////////////////////////////////////////////////////////////////
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
