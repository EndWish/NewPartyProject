using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StatType
{

    //체력, 속도, 공격력, 스택 공격력, 스킬 공격력, 방어 관통력, 치명타 확률, 치명타 배율, 방어력, 실드, 스택 실드, 명중, 회피, 치유력, 토큰 지분(3종류)
    Hpm, Speed, Str, StackStr, SkillStr, DefPen, CriCha, CriMul, Def, Shield, StackShield, Acc, Avoid, Healing,
    AtkTokenWeight, SkillTokenWeight, ShieldTokenWeight,

    Num
}

public enum StatForm
{
    Base, Final, EquipmentAdd, EquipmentMul, AbnormalAdd, AbnormalMul,

    Num
}

public class StatToKorean
{
    static private string[] mapping = { 
        "최대 체력", "속도", "공격력", "스택 공격력", "스킬 공격력", "방어 관통력", "치명타 확률", "치명타 배율", "방어력", "실드", "스택 실드", "명중", "회피", "치유력",
        "공격 토큰 비중", "스킬 토큰 비중", "실드 토큰 비중", 
        
        "ERROR"
    };

    static private bool[] isPercent = {
        false, false, false, true, true, false, true, true, false, false, true, false, false, true, false, false, false,

        false
    };

    static public string Get(StatType statType) {
        return mapping[(int)statType];
    }

    static public bool IsPercent(StatType statType) {
        return isPercent[(int)statType];
    }

}