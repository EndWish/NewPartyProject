using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StatFeatures;
using static UnityEngine.Rendering.HDROutputUtils;

public enum StatType
{

    //ü��, �ӵ�, ���ݷ�, ���� ���ݷ�, ��ų ���ݷ�, ��� �����, ġ��Ÿ Ȯ��, ġ��Ÿ ����, ����, �ǵ�, ���� �ǵ�, ����, ȸ��, ġ����, ��ū ����(3����)
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
        new StatFeature("�ִ� ü��", 1f, StatOperation.Figure), //Hpm
        new StatFeature("�ӵ�", 1f, StatOperation.Figure), //Speed
        new StatFeature("���ݷ�", 0f, StatOperation.Figure), //Str
        new StatFeature("���� ���ݷ�", 0f, StatOperation.PercentPoint), //StackStr
        new StatFeature("��ų ���ݷ�", 1f, StatOperation.Mul ), //SkillStr
        new StatFeature("��� �����", 0f, StatOperation.Figure ), //DefPen
        new StatFeature("ġ��Ÿ Ȯ��", 0f, StatOperation.Percent ), //CriCha
        new StatFeature("ġ��Ÿ ����", 1f, StatOperation.Mul ), //CriMul
        new StatFeature("����", 0f, StatOperation.Figure ), //Def
        new StatFeature("�ǵ�", 0f, StatOperation.Figure ), //Shield
        new StatFeature("���� �ǵ�", 0f, StatOperation.PercentPoint  ), //StackShield
        new StatFeature("����", 0f, StatOperation.Figure ), //Acc
        new StatFeature("ȸ��", 0f, StatOperation.Figure ), //Avoid
        new StatFeature("ġ����", 0f, StatOperation.Mul), //Healing
        new StatFeature("���� ��ū ����", 0f, StatOperation.Figure ), //AtkTokenWeight
        new StatFeature("��ų ��ū ����", 0f, StatOperation.Figure ), //SkillTokenWeight
        new StatFeature("�ǵ� ��ū ����", 0f, StatOperation.Figure ), //ShieldTokenWeight
        new StatFeature("���� ����", 0f, StatOperation.Mul), //StunSensitivity
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
