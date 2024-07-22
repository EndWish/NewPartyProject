using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaknessBleeding : PassiveSkill
{
    [SerializeField] protected float dmgCoefficient;

    protected override void Awake() {
        base.Awake();
        Name = "���� : ����";
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
        return string.Format("���� �� �� �޴� �������� {0}% �����Ѵ�.", dmgCoefficient * 100f);
    }

    protected void OnBeforeCalculateDmg(DamageCalculator dc) {
        if (dc.Defender == Owner && Owner.Tags.Contains(Tag.����)) {
            dc.Dmg *= 1f + dmgCoefficient;
        }
    }
}
