using Photon.Pun;
using Photon.Pun.Demo.Procedural;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaBarrierSkill : PassiveSkill
{
    [SerializeField] protected int turn;
    [SerializeField] protected float barrierCoefficient;

    protected override void Awake() {
        base.Awake();
        Name = "마나 배리어";
    }

    protected void OnDestroy() {
        if (Owner != null) Owner.OnUseActiveSkill -= this.OnUseActiveSkill;
    }

    [PunRPC]
    protected override void OwnerRPC(int viewId) {
        if (Owner != null) Owner.OnUseActiveSkill -= this.OnUseActiveSkill;
        base.OwnerRPC(viewId);
        if (Owner != null) Owner.OnUseActiveSkill += this.OnUseActiveSkill;
    }

    public override string GetDescription() {
        return string.Format("스킬을 사용할 때 마다 {0}만큼 {1}턴간 유지되는 배리어를 얻는다.",
            TooltipText.SetDamageFont(GetBarrierAmount()),
            TooltipText.SetCountFont(turn));
    }
    public override string GetDetailedDescription() {
        return string.Format("스킬을 사용할 때 마다 {0} = ({2}{3}%)만큼 {1}턴간 유지되는 배리어를 얻는다.",
            TooltipText.SetDamageFont(GetBarrierAmount()),
            TooltipText.SetCountFont(turn),
            TooltipText.GetIcon(StatType.Def),
            TooltipText.GetFlexibleFloat(barrierCoefficient * 100f));
    }

    protected void OnUseActiveSkill(ActiveSkill skill) {
        TurnBasedBarrier.Create(Owner, Owner, turn, GetBarrierAmount());
    }

    protected float GetBarrierAmount() {
        return Owner.GetFinalStat(StatType.Def) * barrierCoefficient;
    }

}
