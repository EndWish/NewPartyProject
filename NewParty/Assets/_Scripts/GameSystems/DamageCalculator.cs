using Photon.Pun.Demo.Procedural;
using System;
using System.Collections;
using UnityEngine;

public class DamageCalculator : MonoBehaviour
{
    public Unit Attacker { get; set; }
    public Unit Defender { get; set; }
    public Attack Attack { get; set; }

    public float FinalDmg { get; set; }  // ���� ������
    public float DefRate { get; set; }   // ������
    public float CriRate { get; set; }   // ġ�����
    public int CriStack { get; set; }    // ġ��Ÿ ��øȽ��

    public float Dmg { get; set; }       // ������
    public float DefPen { get; set; }    // �������
    public float CriCha { get; set; }  // ġ��Ÿ Ȯ��
    public float CriMul{ get; set; }  // ġ��Ÿ ���
    public float SkillStr { get; set; } // ��ų ���ݷ�
    public float Acc { get; set; }
    
    public float Def { get; set; }       // ����
    public float Avoid { get; set; }    // ȸ��

    public bool CriOption { get; set; } = false;

    public IEnumerator Advance(float dmg, Unit attacker, Unit defender, Attack attack) {
        if (defender.IsDie)
            yield break;

        Attacker = attacker; 
        Defender = defender;
        Attack = attack;

        // �ɷ�ġ�� �����´�
        Dmg = dmg;
        DefPen = attacker.GetFinalStat(StatType.DefPen);
        CriCha = attacker.GetFinalStat(StatType.CriCha);
        CriMul = attacker.GetFinalStat(StatType.CriMul);
        SkillStr = attacker.GetFinalStat(StatType.SkillStr);
        Acc = attacker.GetFinalStat(StatType.Acc);

        Def = defender.GetFinalStat(StatType.Def);
        Avoid = defender.GetFinalStat(StatType.Avoid);

        // �̺�Ʈ�� ȣ���Ѵ�
        attacker.OnBeforeCalculateDmg?.Invoke(this);
        defender.OnBeforeCalculateDmg?.Invoke(this);

        // ���� �������� ����Ѵ�.
        CalculateDefRate();
        CalculateCriRate();
        CalculateFinalDmg();

        // �̺�Ʈ�� ȣ���Ѵ�
        attacker.OnAfterCalculateDmg?.Invoke(this);
        defender.OnAfterCalculateDmg?.Invoke(this);

        yield return StartCoroutine(defender.TakeDmg(FinalDmg, this));

        StartCoroutine(GameManager.CoInvoke(attacker.CoOnHitDmg, defender, this));

        Destroy(this.gameObject);
    }

    public void CalculateFinalDmg() {
        FinalDmg = Dmg * (1f - DefRate);
        FinalDmg *= CriRate;

        // ��ų ������ ��� ��ų ���ݷ� ����
        if (Attack.Tags.Contains(Tag.��ų����)) {
            FinalDmg *= SkillStr;
        }
            
    }
    public void CalculateDefRate() {
        DefRate = Mathf.Min(1f, (DefPen + Def == 0) ? 0 : Def / (DefPen + Def));
    }
    public void CalculateCriRate() {
        // �⺻ ���� �Ǵ� ġ��Ÿ ������ �� ��� ũ��Ƽ�� ����
        if (Attack.Tags.ContainsAtLeastOne(new Tags(Tag.�⺻����, Tag.ġ��Ÿ����))) {
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
