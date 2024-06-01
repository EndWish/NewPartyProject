using Photon.Pun.Demo.Procedural;
using System.Collections;
using UnityEngine;

public class DamageCalculator : MonoBehaviour
{
    public Unit Attacker { get; set; }
    public Unit Defender { get; set; }
    //public Attack Attack { get; set; }

    public float FinalDmg { get; set; }  // 최종 데미지
    public float DefRate { get; set; }   // 방어비율
    public float CriRate { get; set; }   // 치명배율
    public int CriStack { get; set; } // 치명타 중첩횟수

    public float Dmg { get; set; }       // 데미지
    public float DefPen { get; set; }    // 방어관통력
    public float CriCha { get; set; }  // 치명타 확률
    public float CriMul{ get; set; }  // 치명타 배수

    public float Def { get; set; }       // 방어력

    public IEnumerator Advance(float dmg, Unit attacker, Unit defender) {
        if (defender.IsDie)
            yield break;

        Attacker = attacker; 
        Defender = defender;

        // 능력치를 가져온다
        Dmg = dmg;
        DefPen = attacker.GetFinalStat(StatType.DefPen);
        CriCha = attacker.GetFinalStat(StatType.CriCha);
        CriMul = attacker.GetFinalStat(StatType.CriMul);
        Def = defender.GetFinalStat(StatType.Def);

        // 명중 여부를 계산한다

        // 최종 데미지를 계산한다.
        CalculateDefRate();
        CalculateCriRate();
        CalculateFinalDmg();

        yield return StartCoroutine(defender.TakeDmg(FinalDmg, this));

        Destroy(this.gameObject);
    }

    public void CalculateFinalDmg() {
        FinalDmg = Dmg * (1f - DefRate) * CriRate;
    }
    public void CalculateDefRate() {
        DefRate = Mathf.Min(1f, (DefPen + Def == 0) ? 0 : Def / (DefPen + Def));
    }
    public void CalculateCriRate() {
        int integerPart = (int)CriCha;
        float decimalPart = CriCha - integerPart;
        int exponent = integerPart + (Random.Range(0f, 1f) < decimalPart ? 1 : 0);
        CriRate = Mathf.Max(1, Mathf.Pow(CriMul, exponent));
    }

}
