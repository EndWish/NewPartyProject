using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BleedingTurnDebuff : TurnStatusEffect
{
    public static BleedingTurnDebuff Create(Unit caster, Unit target, int turn, float dmg) {
        BleedingTurnDebuff statusEffect = StatusEffect.Instantiate<BleedingTurnDebuff>();

        statusEffect.dmg = dmg;
        statusEffect.Turn = turn;
        statusEffect.Caster = caster;
        target.AddStatusEffect(statusEffect);

        return statusEffect;
    }

    protected float dmg = 0f;

    protected void OnDestroy() {
        if (Target != null) {
            Target.CoOnBeginMyTurn -= CoOnBeginTurn;
            Target.Tags.SubTag(Tag.출혈);
        }
    }

    [PunRPC]
    protected virtual void DmgRPC(float dmg) {
        this.dmg = dmg;
    }
    public float Dmg {
        get { return dmg; }
        set { photonView.RPC("DmgRPC", RpcTarget.All, value); }
    }

    [PunRPC]
    protected override void TargetRPC(int viewId) {
        if (Target != null) {
            Target.CoOnBeginMyTurn -= CoOnBeginTurn;
            Target.Tags.SubTag(Tag.출혈);
        }
            
        base.TargetRPC(viewId);

        if (Target != null) {
            Target.CoOnBeginMyTurn += CoOnBeginTurn;
            Target.Tags.AddTag(Tag.출혈);
        }
    }

    public override string GetDescriptionText() {
        return string.Format("출혈을 일으켜 {0:G}턴간 턴이 시작할 때 {1} 의 (#출혈)데미지를 준다.", Turn, FloatToNormalStr(dmg));
    }

    protected IEnumerator CoOnBeginTurn() {
        // 출혈 공격을 생성하여 데미지를 준다.
        BleedingAttack attack = BleedingAttack.Create(Caster, Target, Dmg);
        yield return StartCoroutine(attack.Animate());

        Turn -= 1;
    }
}
