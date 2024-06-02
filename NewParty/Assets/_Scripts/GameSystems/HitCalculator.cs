using Photon.Pun.Demo.Procedural;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class HitCalculator : MonoBehaviour
{
    public Unit Attacker { get; set; }
    public Unit Defender { get; set; }
    public Attack Attack { get; set; }

    public float IsHit { get; set; }       // ����

    public float Acc { get; set; }       // ����

    public float Avoid { get; set; }    // ȸ��

    public bool Calculate(Unit attacker, Unit defender, Attack attack) {
        Attacker = attacker;
        Defender = defender;
        Attack = attack;

        // �ɷ�ġ�� �����´�
        Acc = Attacker.GetFinalStat(StatType.Acc);
        Avoid = Defender.GetFinalStat(StatType.Avoid);

        // �̺�Ʈ�� ȣ���Ѵ�
        attacker.OnBeforeCalculateHit?.Invoke(this);
        defender.OnBeforeCalculateHit?.Invoke(this);

        // ���� Ȯ�� ���
        float accChance = (Acc + Avoid == 0) ? 0 : (Acc * 4f) / (Acc * 4f + Avoid);

        Destroy(this.gameObject);
        return Random.Range(0f, 1f) < accChance;
    }

}
