using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StatType
{

    //ü��, �ӵ�, ���ݷ�, ���� ���ݷ�, ��ų ���ݷ�, ��� �����, ġ��Ÿ Ȯ��, ġ��Ÿ ����, ����, �ǵ�, ���� �ǵ�, ����, ȸ��, ġ����, ��ū ����(3����)
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
        "�ִ� ü��", "�ӵ�", "���ݷ�", "���� ���ݷ�", "��ų ���ݷ�", "��� �����", "ġ��Ÿ Ȯ��", "ġ��Ÿ ����", "����", "�ǵ�", "���� �ǵ�", "����", "ȸ��", "ġ����",
        "���� ��ū ����", "��ų ��ū ����", "�ǵ� ��ū ����", 
        
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