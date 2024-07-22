using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WoundClawSkill : PassiveSkill
{
    [SerializeField] protected float dmgCoefficient;

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
        return string.Format("(#출혈)인 적에게 주는 모든 피해가 {0}% 증가한다.", dmgCoefficient * 100f);
    }

    protected void OnBeforeCalculateDmg(DamageCalculator dc) {
        if (dc.Defender.Tags.Contains(Tag.출혈)) {
            dc.Dmg *= (1f + dmgCoefficient);
        }
    }

}
