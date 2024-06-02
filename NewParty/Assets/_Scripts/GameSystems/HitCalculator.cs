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

    public float IsHit { get; set; }       // 명중

    public float Acc { get; set; }       // 명중

    public float Avoid { get; set; }    // 회피

    public bool Calculate(Unit attacker, Unit defender, Attack attack) {
        Attacker = attacker;
        Defender = defender;
        Attack = attack;

        // 능력치를 가져온다
        Acc = Attacker.GetFinalStat(StatType.Acc);
        Avoid = Defender.GetFinalStat(StatType.Avoid);

        // 이벤트를 호출한다
        attacker.OnBeforeCalculateHit?.Invoke(this);
        defender.OnBeforeCalculateHit?.Invoke(this);

        // 최종 확률 계산
        float accChance = (Acc + Avoid == 0) ? 0 : (Acc * 4f) / (Acc * 4f + Avoid);

        Destroy(this.gameObject);
        return Random.Range(0f, 1f) < accChance;
    }

}
