using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoulFragment
{
    public class Data {
        public UnitType UnitType = UnitType.None;
        public int Num { get; set; } = 0;

        public Data() { }
        public Data(UnitType type) : this() {
            UnitType = type;
        }
        public Data(UnitType type, int num) : this(type) {
            Num = num;
        }

    }

    // 연결 정보
    public Unit Target { get; protected set; }

    // 개인 정보
    private Data data;

    public SoulFragment() {
        data = new Data();
    }
    public SoulFragment(Data data) {
        this.data = data ?? new Data();
        SettingTarget();
    }

    public Data GetData() { 
        return data;
    }
    public void SetUnitType(UnitType type) {
        data.UnitType = type;
        SettingTarget();
    }
    public int GetNum() {
        return data.Num;
    }
    public void SetNum(int num) {
        data.Num = num;
    }
    public void AddNum(int num) {
        data.Num += num;
    }

    public void SettingTarget() {
        if(data.UnitType == UnitType.None) {
            Target = null;
            return;
        }
        Target = Resources.Load<Unit>( GameManager.GetUnitPrefabPath(data.UnitType.ToString()));
    }

}
