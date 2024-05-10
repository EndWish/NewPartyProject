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
    //체력, 속도, 공격력, 스택 공격력, 방어 관통력, 치명타 확률, 치명타 배율, 방어력, 실드, 스택 실드, 명중, 회피, 치유력, 토큰 지분

    // 서브 클래스 ////////////////////////////////////////////////////////////
    [Serializable]
    public class Data {
        public UnitType Type = UnitType.None;
        public int GrowthLevel { get; set; } = 0;
    }

    // 개인 정보 //////////////////////////////////////////////////////////////
    // 이름
    public string Name;

    // 저장에 필요한 변수
    [SerializeField] protected Data data;

    // 능력치 관련 변수
    [SerializeField, EnumNamedArrayAttribute(typeof(StatType))] protected float[] baseStats = new float[(int)StatType.Num];
    protected float[,] stats = new float[(int)StatForm.Num, (int)StatType.Num];
    public UnityAction<Unit>[] OnUpdateFinalStat = new UnityAction<Unit>[(int)StatType.Num];
    public float Hp { get; set; }

    // 토큰 관련 변수

    // 스킬 관련 변수

    // 장비 관련 변수

    // 상태이상 관련 변수

    // 유니티 함수 ////////////////////////////////////////////////////////////
    protected void Awake() {
        UpdateFinalStat();
        // 장비와 상태이상 관련 능력치도 계산한다.

        Hp = GetFinalStat(StatType.Hpm);

    }

    // 함수 ///////////////////////////////////////////////////////////////////
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
