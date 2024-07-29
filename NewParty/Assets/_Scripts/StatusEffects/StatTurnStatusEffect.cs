using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class StatTurnStatusEffect : StatStatusEffect, ITurnStatusEffect
{
    public static StatTurnStatusEffect Create(Unit caster, Unit target, StatForm statForm, StatType statType, StatusEffectForm statusEffectForm, float value, int turn) {
        StatTurnStatusEffect statusEffect = StatusEffect.Instantiate<StatTurnStatusEffect>();

        statusEffect.StatForm = statForm;
        statusEffect.StatType = statType;
        statusEffect.Form = statusEffectForm;
        statusEffect.Value = value;
        statusEffect.Turn = turn;
        statusEffect.Caster = caster;
        target.AddStatusEffect(statusEffect);

        return statusEffect;
    }

    protected int turn = 1;


    protected override void OnDestroy() {
        base.OnDestroy();
        if (Target != null) Target.CoOnBeginTick -= CoOnBeginTurn;
    }

    [PunRPC]
    protected virtual void TurnRPC(int turn) {
        this.turn = turn;
        seIcon.RightLowerText.text = turn.ToString();
    }
    public int Turn {
        get { return turn; }
        set {
            photonView.RPC("TurnRPC", RpcTarget.All, value);
            if (value <= 0)
                this.Destroy();
        }
    }

    [PunRPC]
    protected override void TargetRPC(int viewId) {
        if (Target != null)
            Target.CoOnBeginMyTurn -= CoOnBeginTurn;
        base.TargetRPC(viewId);
        if (Target != null)
            Target.CoOnBeginMyTurn += CoOnBeginTurn;
    }

    public override string GetDescription() {
        return new StringBuilder().Append(Turn).Append("ео ╣©╬х ").Append(base.GetDescription()).ToString();
    }

    protected IEnumerator CoOnBeginTurn() {
        Turn -= 1;
        yield break;
    }
}
