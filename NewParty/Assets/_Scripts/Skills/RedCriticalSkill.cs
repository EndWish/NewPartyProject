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
        Name = "붉은 치명타";
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
        return string.Format("치명타시 출혈을 일으켜 {0:G}턴간 공격력의 {1:G}%의 (#출혈)데미지를 준다.", turn, dmgCoefficient * 100f);
    }

    protected IEnumerator CoOnHitDmg(Unit damagedUnit, DamageCalculator dc) {
        if(0 < dc.CriStack) {
            // 출혈 디버프를 생성하고 damagedUnit에게 부착한다
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
