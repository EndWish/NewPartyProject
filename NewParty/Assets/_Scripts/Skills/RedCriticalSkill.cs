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
            BleedingTurnDebuff.Create(Owner, damagedUnit, turn, CalculateDmg());
        }
        yield break;
    }

    protected float CalculateDmg() {
        return Owner.GetFinalStat(StatType.Str) * dmgCoefficient;
    }

}
