using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StatFeatures;
using static UnityEngine.Rendering.HDROutputUtils;

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

public enum StatOperation
{
    Figure, Mul, PercentPoint, Percent, 
}

public static class StatFeatures
{
    private struct StatFeature {
        public string Korean;
        public float Min;
        public StatOperation Operation;

        public StatFeature(string korean, float min, StatOperation statOperation) {
            Korean = korean;
            Min = min;
            Operation = statOperation;
        }
    }  

    private static StatFeature[] statFeature = new StatFeature[(int)StatType.Num] {
        new StatFeature("최대 체력", 1f, StatOperation.Figure), //Hpm
        new StatFeature("속도", 1f, StatOperation.Figure), //Speed
        new StatFeature("공격력", 0f, StatOperation.Figure), //Str
        new StatFeature("스택 공격력", 0f, StatOperation.PercentPoint), //StackStr
        new StatFeature("스킬 공격력", 1f, StatOperation.Mul ), //SkillStr
        new StatFeature("방어 관통력", 0f, StatOperation.Figure ), //DefPen
        new StatFeature("치명타 확률", 0f, StatOperation.Percent ), //CriCha
        new StatFeature("치명타 배율", 1f, StatOperation.Mul ), //CriMul
        new StatFeature("방어력", 0f, StatOperation.Figure ), //Def
        new StatFeature("실드", 0f, StatOperation.Figure ), //Shield
        new StatFeature("스택 실드", 0f, StatOperation.PercentPoint  ), //StackShield
        new StatFeature("명중", 0f, StatOperation.Figure ), //Acc
        new StatFeature("회피", 0f, StatOperation.Figure ), //Avoid
        new StatFeature("치유력", 0f, StatOperation.Mul), //Healing
        new StatFeature("공격 토큰 비중", 0f, StatOperation.Figure ), //AtkTokenWeight
        new StatFeature("스킬 토큰 비중", 0f, StatOperation.Figure ), //SkillTokenWeight
        new StatFeature("실드 토큰 비중", 0f, StatOperation.Figure ), //ShieldTokenWeight
        new StatFeature("기절 감도", 0f, StatOperation.Mul), //StunSensitivity
        //Num
    };

    public static string GetKorean(StatType statType) {
        return statFeature[(int)statType].Korean;
    }
    public static float GetMin(StatType statType) {
        return statFeature[(int)statType].Min;
    }
    public static StatOperation GetOperation(StatType statType) {
        return statFeature[(int)statType].Operation;
    }

}
