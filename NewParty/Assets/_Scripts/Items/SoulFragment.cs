using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SoulFragment : SaveData, IItem
{
    private static Sprite mark;
    public static Sprite Mark {
        get {
            if (mark == null)
                mark = Resources.Load<Sprite>("Image/img_mark_item_soulFragment");
            return mark;
        }
    }

    private static Sprite nullIcon1x1;
    public static Sprite NullIcon1x1 {
        get {
            if (nullIcon1x1 == null)
                nullIcon1x1 = Resources.Load<Sprite>("Image/img_ic_item_soulFragment_null");
            return nullIcon1x1;
        }
    }

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
    public string Name { 
        get { return unitSharedData.Name + "의 영혼파편"; } 
    }
    public List<Sprite> GetIcons1x1() {
        return new List<Sprite>(unitSharedData.GetIcons1x1()) { Mark };
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

    public void InsertTo(UserData userData) {
        SoulFragment soulFragment = userData.SoulFragmentList.Find((x) => x.Type == Type);
        if(soulFragment == null) {
            userData.AddSoulFragment(this);
        }
        else {
            soulFragment.Num += this.Num;
        }
    }
    public void InsertTo(List<IItem> list) {
        SoulFragment soulFragment = (SoulFragment)list.Find((item) => item is SoulFragment && (item as SoulFragment).Type == Type);
        if (soulFragment == null) {
            list.Add(this);
        }
        else {
            soulFragment.Num += this.Num;
        }
    }

}

public struct SoulFragmentSaveFormat
{
    public long SaveKey;
    public UnitType Type;
    public int Num;
}
