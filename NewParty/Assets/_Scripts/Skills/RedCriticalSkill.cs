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

    [PunRPC]
    protected override void OwnerRPC(int viewId) {
        if (Owner != null) Owner.CoOnHitDmg -= this.CoOnHitDmg;
        base.OwnerRPC(viewId);
        if (Owner != null) Owner.CoOnHitDmg += this.CoOnHitDmg;
    }

    public override string GetDescription() {
        return string.Format("ġ��Ÿ�� ������ ������ {0:G}�ϰ� ���ݷ��� {1:G}%�� (#����)�������� �ش�.", turn, dmgCoefficient * 100f);
    }

    protected IEnumerator CoOnHitDmg(Unit damagedUnit, DamageCalculator dc) {
        if(0 < dc.CriStack) {
            // ���� ������� �����ϰ� damagedUnit���� �����Ѵ�
            BleedingTurnDebuff statusEffect = PhotonNetwork.Instantiate(GameManager.GetStatusEffectPrefabPath("BleedingTurnDebuff"),
            damagedUnit.transform.position, Quaternion.identity)
            .GetComponent<BleedingTurnDebuff>();
            statusEffect.Turn = turn;
            statusEffect.Dmg = dmgCoefficient * Owner.GetFinalStat(StatType.Str);
            statusEffect.Caster = Owner;
            damagedUnit.AddStatusEffect(statusEffect);
        }
        yield break;
    }

}
