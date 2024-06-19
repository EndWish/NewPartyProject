using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedCriticalSkill : PassiveSkill
{
    [SerializeField] protected int turn;
    [SerializeField] protected float dmgCoefficient;

    protected override void Awake() {
        base.Awake();
        Name = "���� ġ��Ÿ";
    }

    protected void OnDestroy() {
        if (Owner != null) Owner.CoOnHitDmg -= this.CoOnHitDmg;
    }

    protected override void OnSetOwner(Unit prev, Unit current) {
        base.OnSetOwner(prev, current);
        if(prev != null) prev.CoOnHitDmg -= this.CoOnHitDmg;
        if(current != null) current.CoOnHitDmg += this.CoOnHitDmg;
    }

    public override string GetDescription() {
        return string.Format("ġ��Ÿ�� ������ ������ {0:G}�ϰ� ���ݷ��� {1:G}%�� (#����)�������� �ش�.", turn, dmgCoefficient * 100f);
    }

    protected IEnumerator CoOnHitDmg(Unit damagedUnit, DamageCalculator dc) {
        if(0 < dc.CriStack) {
            // ���� ������� �����ϰ� damagedUnit���� �����Ѵ�
            BleedingDebuff statusEffect = PhotonNetwork.Instantiate(GameManager.GetStatusEffectPrefabPath("BleedingDebuff"),
            damagedUnit.transform.position, Quaternion.identity)
            .GetComponent<BleedingDebuff>();
            statusEffect.Turn = turn;
            statusEffect.Dmg = dmgCoefficient * Owner.GetFinalStat(StatType.Str);
            statusEffect.Caster = Owner;
            damagedUnit.AddStatusEffect(statusEffect);
        }
        yield break;
    }

}
