using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class ProtectSkill : ActiveSkill
{
    [SerializeField] protected int turn;
    protected ProtectStatusEffect protectSE;

    protected override void Awake() {
        base.Awake();
        Name = "��ȣ";
    }

    protected void OnDestroy() {
        if (Owner != null) Owner.OnStun -= this.OnStun;
    }

    [PunRPC]
    protected override void OwnerRPC(int viewId) {
        if (Owner != null) Owner.OnStun -= this.OnStun;
        base.OwnerRPC(viewId);
        if (Owner != null) Owner.OnStun += this.OnStun;
    }

    public override IEnumerator CoUse() {
        yield return StartCoroutine(Owner.UseSelectedTokens());

        Unit target = BattleSelectable.Units[0];
        ProtectStatusEffect statusEffect = PhotonNetwork.Instantiate(GameManager.GetStatusEffectPrefabPath("ProtectStatusEffect"),
            transform.position, Quaternion.identity)
            .GetComponent<ProtectStatusEffect>();
        statusEffect.Turn = turn;
        statusEffect.Caster = Owner;
        target.AddStatusEffect(statusEffect);

        if(protectSE != null)
            protectSE.Destroy();
        SetProtectSE(statusEffect);

        yield return new WaitForSeconds(0.5f);
    }

    public override BattleSelectionType GetSelectionType() {
        return BattleSelectionType.Unit;
    }
    public override int GetSelectionNum() {
        return 1;
    }
    public override bool SelectionPred(Unit unit) {
        if (!base.SelectionPred(unit))
            return false;
        return Owner.TeamType == unit.TeamType && Owner != unit;
    }

    public override string GetDescription() {
        return string.Format("������ �Ʊ��� {0}�ϰ� ��ȣ�Ѵ�. ��ȣ�� �޴� ������ �ڽ��� �ڽ��� ������ Ÿ���� �Ǿ��� �� ��ų �����ڰ� ��� Ÿ���� �Ǿ��ش�." +
            "\n���� ��ȣ ���� ���ÿ� ������ Ÿ���� �ȴٸ� ��ȣ��ų�� �ߵ����� �ʴ´�." +
            "\n��ȣ ��ų�� ���������� �ٽ� ����� ��� ������ ��ȣ�ϴ� ����� ��ҵȴ�. ���� �����ڰ� ������ ��� ��ȣ ��ų�� ��ҵȴ�.",
            turn);
    }

    [PunRPC]
    protected void SetProtectSERPC(int viewId) {
        protectSE = viewId == -1 ? null : PhotonView.Find(viewId).GetComponent<ProtectStatusEffect>();
    }
    protected void SetProtectSE(ProtectStatusEffect protectSE) {
        photonView.RPC("SetProtectSERPC", RpcTarget.All, protectSE?.photonView.ViewID ?? -1);
    }

    protected void OnStun(StunTurnDebuff stun) {
        if(protectSE != null) {
            protectSE.Destroy();
        }
    }

}
