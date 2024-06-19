using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BleedingDebuff : StatusEffect, ITurnStatusEffect
{
    protected int turn = 1;
    protected float dmg = 0f;

    protected override void OnDestroy() {
        base.OnDestroy();
        if (Target != null) {
            Target.CoOnBeginTick -= CoOnBeginTurn;
            Target.Tags.SubTag(Tag.����);
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

    public override string GetDescription() {
        return string.Format("������ ������ {0:G}�ϰ� ���ݷ��� {1}�� (#����)�������� �ش�.", Turn, FloatToNormalStr(dmg));
    }

    protected IEnumerator CoOnBeginTurn() {
        // ���� ������ �����Ͽ� �������� �ش�.
        BleedingAttack attack = PhotonNetwork.Instantiate(GameManager.GetAttackPrefabPath("BleedingAttack"),
            Target.transform.position, Quaternion.identity)
        .GetComponent<BleedingAttack>();

        attack.Init(Caster, Target, Dmg);
        yield return StartCoroutine(attack.Animate());

        Turn -= 1;
    }
}
