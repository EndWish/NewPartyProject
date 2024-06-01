using Photon.Pun.Demo.Procedural;
using System.Collections;
using UnityEngine;

public class DamageCalculator : MonoBehaviour
{
    public Unit Attacker { get; set; }
    public Unit Defender { get; set; }
    //public Attack Attack { get; set; }

    public float FinalDmg { get; set; }  // ���� ������
    public float DefRate { get; set; }   // ������
    public float CriRate { get; set; }   // ġ�����
    public int CriStack { get; set; } // ġ��Ÿ ��øȽ��

    public float Dmg { get; set; }       // ������
    public float DefPen { get; set; }    // �������
    public float CriCha { get; set; }  // ġ��Ÿ Ȯ��
    public float CriMul{ get; set; }  // ġ��Ÿ ���

    public float Def { get; set; }       // ����

    public IEnumerator Advance(float dmg, Unit attacker, Unit defender) {
        if (defender.IsDie)
            yield break;

        Attacker = attacker; 
        Defender = defender;

        // �ɷ�ġ�� �����´�
        Dmg = dmg;
        DefPen = attacker.GetFinalStat(StatType.DefPen);
        CriCha = attacker.GetFinalStat(StatType.CriCha);
        CriMul = attacker.GetFinalStat(StatType.CriMul);
        Def = defender.GetFinalStat(StatType.Def);

        // ���� ���θ� ����Ѵ�

        // ���� �������� ����Ѵ�.
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
