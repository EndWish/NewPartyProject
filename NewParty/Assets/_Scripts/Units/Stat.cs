using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StatType
{

    //체력, 속도, 공격력, 스택 공격력, 스킬 공격력, 방어 관통력, 치명타 확률, 치명타 배율, 방어력, 실드, 스택 실드, 명중, 회피, 치유력, 토큰 지분(3종류)
    Hpm, Speed, Str, StackStr, SkillStr, DefPen, CriCha, CriMul, Def, Shield, StackShield, Acc, Avoid, Healing,
    AtkTokenWeight, SkillTokenWeight, ShieldTokenWeight, StunSen,

    Num
}

public enum StatForm
{
    Base, Final, EquipmentAdd, EquipmentMul, AbnormalAdd, AbnormalMul,

    Num
}

public static class StatClamp
{
    public static float[] MinStats { get; set; } = new float[(int)StatType.Num] {
        1f, //Hpm
        1f, //Speed
        0f, //Str
        1f, //StackStr
        0f, //SkillStr
        0f, //DefPen
        0f, //CriCha
        1f, //CriMul
        0f, //Def
        0f, //Shield
        1f, //StackShield
        0f, //Acc
        0f, //Avoid
        0f, //Healing
        0f, //AtkTokenWeight
        0f, //SkillTokenWeight
        0f, //ShieldTokenWeight
        0f, //StunSensitivity
        //Num
    };
}

public class StatToKorean
{
    static private string[] mapping = { 
        "최대 체력", "속도", "공격력", "스택 공격력", "스킬 공격력", "방어 관통력", "치명타 확률", "치명타 배율", "방어력", "실드", "스택 실드", "명중", "회피", "치유력",
        "공격 토큰 비중", "스킬 토큰 비중", "실드 토큰 비중", "기절 감도",
        
        "ERROR"
    };

    static private bool[] isPercent = {
        false, false, false, true, true, false, true, true, false, false, true, false, false, true, false, false, false, true, 

        false
    };

    static public string Get(StatType statType) {
        return mapping[(int)statType];
    }

    static public bool IsPercent(StatType statType) {
        return isPercent[(int)statType];
    }

}