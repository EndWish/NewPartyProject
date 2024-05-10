using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;

public enum UnitType
{
    None, Garuda,
}

public enum StatType
{
    Hpm, Speed, Str, StackStr, SkillStr, DefPen, CriCha, CriMul, Def, Shield, StackShield, Acc, Avoid, Healing,
    AtkTokenShare, SkillTokenShare, ShiledTokenShare,

    Num
}

public enum StatForm
{
    Base, Final, EquipmentAdd, EquipmentMul, AbnormalAdd, AbnormalMul,
    
    Num
}

public class Unit : MonoBehaviourPun
{
    //ü��, �ӵ�, ���ݷ�, ���� ���ݷ�, ��� �����, ġ��Ÿ Ȯ��, ġ��Ÿ ����, ����, �ǵ�, ���� �ǵ�, ����, ȸ��, ġ����, ��ū ����

    // ���� Ŭ���� ////////////////////////////////////////////////////////////
    [Serializable]
    public class Data {
        public UnitType Type = UnitType.None;
        public int GrowthLevel { get; set; } = 0;
    }

    // ���� ���� //////////////////////////////////////////////////////////////
    // �̸�
    public string Name;

    // ���忡 �ʿ��� ����
    [SerializeField] protected Data data;

    // �ɷ�ġ ���� ����
    [SerializeField, EnumNamedArrayAttribute(typeof(StatType))] protected float[] baseStats = new float[(int)StatType.Num];
    protected float[,] stats = new float[(int)StatForm.Num, (int)StatType.Num];
    public UnityAction<Unit>[] OnUpdateFinalStat = new UnityAction<Unit>[(int)StatType.Num];
    public float Hp { get; set; }

    // ��ū ���� ����

    // ��ų ���� ����

    // ��� ���� ����

    // �����̻� ���� ����

    // ����Ƽ �Լ� ////////////////////////////////////////////////////////////
    protected void Awake() {
        UpdateFinalStat();
        // ���� �����̻� ���� �ɷ�ġ�� ����Ѵ�.

        Hp = GetFinalStat(StatType.Hpm);

    }

    // �Լ� ///////////////////////////////////////////////////////////////////
    public float GetFinalStat(StatType type) {
        return stats[(int)StatForm.Final, (int)type];
    }

    public void UpdateFinalStat(StatType type) {
        int index = (int)type;
        stats[(int)StatForm.Final, index] =
            (stats[(int)StatForm.Base, index] * (1f + 0.01f * data.GrowthLevel)
            + stats[(int)StatForm.EquipmentAdd, index]
            + stats[(int)StatForm.AbnormalAdd, index])
            * stats[(int)StatForm.EquipmentMul, index]
            * stats[(int)StatForm.AbnormalMul, index];

        OnUpdateFinalStat[index]?.Invoke(this);
    }
    public void UpdateFinalStat() {
        for(StatType type = 0; type < StatType.Num; ++type) {
            UpdateFinalStat(type);
        }
    }



}
