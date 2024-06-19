using Photon.Pun.Demo.Procedural;
using System;
using System.Collections;
using UnityEngine;

public class DamageCalculator : MonoBehaviour
{
    public Unit Attacker { get; set; }
    public Unit Defender { get; set; }
    public Attack Attack { get; set; }

    public float FinalDmg { get; set; }  // 최종 데미지
    public float DefRate { get; set; }   // 방어비율
    public float CriRate { get; set; }   // 치명배율
    public int CriStack { get; set; }    // 치명타 중첩횟수

    public float Dmg { get; set; }       // 데미지
    public float DefPen { get; set; }    // 방어관통력
    public float CriCha { get; set; }  // 치명타 확률
    public float CriMul{ get; set; }  // 치명타 배수
    public float SkillStr { get; set; } // 스킬 공격력
    public float Acc { get; set; }
    
    public float Def { get; set; }       // 방어력
    public float Avoid { get; set; }    // 회피

    public bool CriOption { get; set; } = false;

    public IEnumerator Advance(float dmg, Unit attacker, Unit defender, Attack attack) {
        if (defender.IsDie)
            yield break;

        Attacker = attacker; 
        Defender = defender;
        Attack = attack;

        // 능력치를 가져온다
        Dmg = dmg;
        DefPen = attacker.GetFinalStat(StatType.DefPen);
        CriCha = attacker.GetFinalStat(StatType.CriCha);
        CriMul = attacker.GetFinalStat(StatType.CriMul);
        SkillStr = attacker.GetFinalStat(StatType.SkillStr);
        Acc = attacker.GetFinalStat(StatType.Acc);

        Def = defender.GetFinalStat(StatType.Def);
        Avoid = defender.GetFinalStat(StatType.Avoid);

        // 이벤트를 호출한다
        attacker.OnBeforeCalculateDmg?.Invoke(this);
        defender.OnBeforeCalculateDmg?.Invoke(this);

        // 최종 데미지를 계산한다.
        CalculateDefRate();
        CalculateCriRate();
        CalculateFinalDmg();

        // 이벤트를 호출한다
        attacker.OnAfterCalculateDmg?.Invoke(this);
        defender.OnAfterCalculateDmg?.Invoke(this);

        yield return StartCoroutine(defender.TakeDmg(FinalDmg, this));

        StartCoroutine(GameManager.CoInvoke(attacker.CoOnHitDmg, defender, this));

        Destroy(this.gameObject);
    }

    public void CalculateFinalDmg() {
        FinalDmg = Dmg * (1f - DefRate);
        FinalDmg *= CriRate;

        // 스킬 공격일 경우 스킬 공격력 적용
        if (Attack.Tags.Contains(Tag.스킬공격)) {
            FinalDmg *= SkillStr;
        }
            
    }
    public void CalculateDefRate() {
        DefRate = Mathf.Min(1f, (DefPen + Def == 0) ? 0 : Def / (DefPen + Def));
    }
    public void CalculateCriRate() {
        // 기본 공격 또는 치명타 적용이 될 경우 크리티컬 적용
        if (Attack.Tags.ContainsAtLeastOne(new Tags(Tag.기본공격, Tag.치명타적용))) {
            int integerPart = (int)CriCha;
            float decimalPart = CriCha - integerPart;
            CriStack = integerPart + (UnityEngine.Random.Range(0f, 1f) < decimalPart ? 1 : 0);
            CriRate = Mathf.Max(1, Mathf.Pow(CriMul, CriStack));
        }
        else {
            CriStack = 0;
            CriRate = 1f;
        }
    }

}
