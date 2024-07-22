using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiSkillBuff : StatusEffect, ITurnStatusEffect
{
    protected int turn = 1;
    protected float dmgMul = 0f;
    [SerializeField] GameObject FXPrefab;

    protected override void OnDestroy() {
        base.OnDestroy();
        if (Target != null) {
            Target.CoOnBeginTick -= CoOnBeginTurn;
            Target.OnAfterCalculateDmg -= OnAfterCalculateDmg;
        }
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

    public override string GetDescription() {
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
