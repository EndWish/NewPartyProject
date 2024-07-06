using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SoulFragment : SaveData
{
    // 개인 정보
    protected UnitSharedData unitSharedData;
    protected int num;
    

    public SoulFragment() {
        unitSharedData = null;
        num = 0;
    }
    public SoulFragment(UnitType type) : this() {
        Type = type;
    }
    public SoulFragment(UnitType type, int num) : this(type) {
        Num = num;
    }

    public UnitSharedData UnitSharedData {
        get { return unitSharedData; }
    }
    public UnitType Type {
        get { return unitSharedData?.Type ?? UnitType.None; }
        set { 
            unitSharedData = (value == UnitType.None) ? null : UnitSharedData.GetAsset(value);
            SaveSynced();
        }
    }
    public int Num {
        get { return num; } 
        set {  
            num = value;
            SaveSynced();
        }
    }
    public Sprite UnitProfileSprite {
        get { return UnitSharedData?.ProfileSprite; }
    }


    protected SoulFragmentSaveFormat ToSaveFormat() {
        SoulFragmentSaveFormat saveFormat = new SoulFragmentSaveFormat();
        saveFormat.SaveKey = SaveKey;
        saveFormat.Type = Type;
        saveFormat.Num = Num;
        return saveFormat;
    }
    public SoulFragment From(SoulFragmentSaveFormat saveFormat) {
        SaveKey = saveFormat.SaveKey;
        Type = saveFormat.Type;
        Num = saveFormat.Num;
        return this;
    }
    public override void Save() {
        base.Save();
        ES3.Save<SoulFragmentSaveFormat>(SaveKey.ToString(), this.ToSaveFormat(), UserData.Instance.Nickname);
    }

}

public struct SoulFragmentSaveFormat
{
    public long SaveKey;
    public UnitType Type;
    public int Num;
}
