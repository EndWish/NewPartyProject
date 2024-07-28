using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WoundClawSkill : PassiveSkill
{
    [SerializeField] protected float dmgMul;

    protected override void Awake() {
        base.Awake();
        Name = "��ó �ĺ��ı�";
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
        return string.Format("(#����)�� ������ �ִ� ��� ���ذ� {0}�����Ѵ�.",
            TooltipText.SetMulFont(dmgMul));
    }
    public override string GetDetailedDescription() {
        return string.Format("(#����)�� ������ �ִ� ��� ���ذ� {0}�����Ѵ�.",
            TooltipText.SetMulFont(dmgMul));
    }

    protected void OnBeforeCalculateDmg(DamageCalculator dc) {
        if (dc.Defender.Tags.Contains(Tag.����)) {
            dc.Dmg *= dmgMul;
        }
    }

}
