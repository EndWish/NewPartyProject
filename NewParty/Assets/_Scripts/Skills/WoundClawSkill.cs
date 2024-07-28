using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WoundClawSkill : PassiveSkill
{
    [SerializeField] protected float dmgMul;

    protected override void Awake() {
        base.Awake();
        Name = "상처 후벼파기";
    }

    protected void OnDestroy() {
        if (Owner != null) Owner.OnBeforeCalculateDmg -= this.OnBeforeCalculateDmg;
    }

    [PunRPC]
    protected override void OwnerRPC(int viewId) {
        if (Owner != null) Owner.OnBeforeCalculateDmg -= this.OnBeforeCalculateDmg;
        base.OwnerRPC(viewId);
        if (Owner != null) Owner.OnBeforeCalculateDmg += this.OnBeforeCalculateDmg;
    }

    public override string GetDescription() {
        return string.Format("(#출혈)인 적에게 주는 모든 피해가 {0}증가한다.",
            TooltipText.SetMulFont(dmgMul));
    }
    public override string GetDetailedDescription() {
        return string.Format("(#출혈)인 적에게 주는 모든 피해가 {0}증가한다.",
            TooltipText.SetMulFont(dmgMul));
    }

    protected void OnBeforeCalculateDmg(DamageCalculator dc) {
        if (dc.Defender.Tags.Contains(Tag.출혈)) {
            dc.Dmg *= dmgMul;
        }
    }

}
