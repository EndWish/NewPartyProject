using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedCriticalSkill : PassiveSkill
{
    [SerializeField] protected int turn;
    [SerializeField] protected float dmgCoefficient;

    [SerializeField] protected BleedingAttack attackPrefab;

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
        return string.Format("치명타시 출혈을 일으켜 {0}턴간 {1}데미지를 준다.",
            TooltipText.SetCountFont(turn),
            TooltipText.SetDamageFont(CalculateDmg()));
    }
    public override string GetDetailedDescription() {
        return string.Format("치명타시 출혈을 일으켜 {0}턴간 {1} = ({2}{3}%)데미지({4})를 준다.",
            TooltipText.SetCountFont(turn),
            TooltipText.SetDamageFont(CalculateDmg()),
            TooltipText.GetIcon(StatType.Str),
            TooltipText.GetFlexibleFloat(dmgCoefficient * 100f),
            Tags.GetString(attackPrefab.InitTags));
    }

    protected IEnumerator CoOnHitDmg(Unit damagedUnit, DamageCalculator dc) {
        if(0 < dc.CriStack) {
            // 출혈 디버프를 생성하고 damagedUnit에게 부착한다
            BleedingTurnDebuff statusEffect = PhotonNetwork.Instantiate(GameManager.GetStatusEffectPrefabPath("BleedingTurnDebuff"),
            damagedUnit.transform.position, Quaternion.identity)
            .GetComponent<BleedingTurnDebuff>();
            statusEffect.Turn = turn;
            statusEffect.Dmg = CalculateDmg();
            statusEffect.Caster = Owner;
            damagedUnit.AddStatusEffect(statusEffect);
        }
        yield break;
    }

    protected float CalculateDmg() {
        return Owner.GetFinalStat(StatType.Str) * dmgCoefficient;
    }

}
