using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiSkillBuff : TurnStatusEffect
{
    public static AntiSkillBuff Create(Unit caster, Unit target, int turn, float dmgMul) {
        AntiSkillBuff statusEffect = StatusEffect.Instantiate<AntiSkillBuff>();

        statusEffect.DmgMul = dmgMul;
        statusEffect.Turn = turn;
        statusEffect.Caster = caster;
        target.AddStatusEffect(statusEffect);

        return statusEffect;
    }

    protected float dmgMul = 0f;
    [SerializeField] GameObject FXPrefab;

    protected void OnDestroy() {
        if (Target != null) {
            Target.CoOnBeginTick -= CoOnBeginTurn;
            Target.OnAfterCalculateDmg -= OnAfterCalculateDmg;
        }
    }

    [PunRPC]
    protected virtual void DmgMulRPC(float dmgMul) {
        this.dmgMul = Mathf.Max(0, dmgMul);
    }
    public float DmgMul {
        get { return dmgMul; }
        set { photonView.RPC("DmgMulRPC", RpcTarget.All, value); }
    }

    [PunRPC]
    protected override void TargetRPC(int viewId) {
        if (Target != null) {
            Target.CoOnBeginMyTurn -= CoOnBeginTurn;
            Target.OnAfterCalculateDmg -= OnAfterCalculateDmg;
        }

        base.TargetRPC(viewId);

        if (Target != null) {
            Target.CoOnBeginMyTurn += CoOnBeginTurn;
            Target.OnAfterCalculateDmg += OnAfterCalculateDmg;
            Instantiate(FXPrefab, Target.transform.position, Quaternion.identity);
        }
    }

    public override string GetDescriptionText() {
        return string.Format("{0}턴간 (#스킬 공격)에 피격시 피해량을 {1:F1}%낮춘다.", Turn, (1f - dmgMul) * 100f);
    }

    protected IEnumerator CoOnBeginTurn() {
        Turn -= 1;
        yield break;
    }
    protected void OnAfterCalculateDmg(DamageCalculator dc) {
        if(dc.Defender == Target && dc.Attack.Tags.Contains(Tag.스킬공격)) {
            dc.FinalDmg *= dmgMul;
        }
    }


}
