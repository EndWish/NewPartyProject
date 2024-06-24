using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaknessBleeding : PassiveSkill
{
    [SerializeField] protected float dmgCoefficient;

    protected override void Awake() {
        base.Awake();
        Name = "약점 : 출혈";
    }

    protected void OnDestroy() {
        if (Owner != null) Owner.OnBeforeCalculateDmg -= this.OnBeforeCalculateDmg;
    }

    protected override void OnSetOwner(Unit prev, Unit current) {
        base.OnSetOwner(prev, current);
        if (prev != null) prev.OnBeforeCalculateDmg -= this.OnBeforeCalculateDmg;
        if (current != null) current.OnBeforeCalculateDmg += this.OnBeforeCalculateDmg;
    }

    public override string GetDescription() {
        return string.Format("출혈 중 때 받는 데미지가 {0}% 증가한다.", dmgCoefficient * 100f);
    }

    protected void OnBeforeCalculateDmg(DamageCalculator dc) {
        if (dc.Defender == Owner && Owner.Tags.Contains(Tag.출혈)) {
            dc.Dmg *= 1f + dmgCoefficient;
        }
    }
}
