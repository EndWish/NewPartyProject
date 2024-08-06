using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoulDust : IItem
{
    private static Sprite icon1x1;
    public static Sprite Icon1x1 {
        get {
            if (icon1x1 == null)
                icon1x1 = Resources.Load<Sprite>("Image/img_ic_item_soulDust");
            return icon1x1;
        }
    }

    protected int num;

    public SoulDust(int num) {
        Num = num;
    }

    public string Name {
        get { return "¿µÈ¥°¡·ç"; }
    }
    public int Num {
        get { return num; }
        set { num = value; }
    }

    public List<Sprite> GetIcons1x1() {
        return new List<Sprite> { Icon1x1 };
    }

    public void InsertTo(UserData userData) {
        userData.SoulDust += Num;
    }
    public void InsertTo(List<IItem> list) {
        SoulDust soulDust = (SoulDust)list.Find((item) => item is SoulDust);
        if (soulDust == null) {
            list.Add(this);
        }
        else {
            soulDust.Num += this.Num;
        }
    }


}
