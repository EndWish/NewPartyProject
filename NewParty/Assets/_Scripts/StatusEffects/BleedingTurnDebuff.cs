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
            Target.Tags.SubTag(Tag.����);
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
            Target.Tags.SubTag(Tag.����);
        }
            
        base.TargetRPC(viewId);

        if (Target != null) {
            Target.CoOnBeginMyTurn += CoOnBeginTurn;
            Target.Tags.AddTag(Tag.����);
        }
    }

    public override string GetDescriptionText() {
        return string.Format("������ ������ {0:G}�ϰ� ���� ������ �� {1} �� (#����)�������� �ش�.", Turn, FloatToNormalStr(dmg));
    }

    protected IEnumerator CoOnBeginTurn() {
        // ���� ������ �����Ͽ� �������� �ش�.
        BleedingAttack attack = BleedingAttack.Create(Caster, Target, Dmg);
        yield return StartCoroutine(attack.Animate());

        Turn -= 1;
    }
}
