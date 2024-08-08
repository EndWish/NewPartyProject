using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum TextIconIndex
{
    BarrierAmount = 63,
}

public static class TooltipText
{
    public static string GetFlexibleFloat(float num) {
        if (num < 10f)
            return num.ToString("F2");
        if (num < 100f)
            return num.ToString("F1");
        return num.ToString("G");
    }

    public static string GetIcon(StatType statType) {
        return "<sprite=" + (int)statType + ">";
    }
    public static string GetIcon(TextIconIndex textIconIndex) {
        return "<sprite=" + (int)textIconIndex + ">";
    }

    public static string SetFont(string str, bool bold, bool italic, string color) { 
        if(bold) {
            str = "<b>" + str + "</b>";
        }

        if (italic) {
            str = "<i>" + str + "</i>";
        }

        if (!string.IsNullOrEmpty(color)) {
            str = "<color=" + color + ">" + str + "</color>";
        }

        return str;
    }
    public static string SetDamageFont(float dmg) {
        return "<b><color=red>" + GetFlexibleFloat(dmg) + "</b></color>";
    }
    public static string SetMulFont(float mul) {
        return "<b><color=orange>x" + mul.ToString("F2") + "</b></color>";
    }
    public static string SetPercentPointFont(float num) {
        num *= 100f;

        string numStr;
        if (num < 10f)
            numStr = num.ToString("F1");
        else
            numStr = num.ToString("G");

        return "<b><color=yellow>" + numStr + "%p</b></color>";
    }
    public static string SetCountFont(int num) {
        return "<b><color=white>" + num + "</b></color>";
    }
    public static string SetPercentFont(float num) {
        num *= 100f;

        string numStr;
        if (num < 10f)
            numStr = num.ToString("F1");
        else
            numStr = num.ToString("G");

        return "<b><color=white>" + numStr + "%</b></color>";
    }
    


}
