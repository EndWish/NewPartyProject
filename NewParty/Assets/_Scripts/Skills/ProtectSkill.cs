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
        Name = "보호";
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
        return string.Format("선택한 아군을 {0}턴간 보호한다. 보호를 받는 유닛은 자신이 자신이 공격의 타겟이 되었을 때 스킬 시전자가 대신 타겟이 되어준다." +
            "\n만약 보호 대상과 동시에 공격의 타겟이 된다면 보호스킬이 발동되지 않는다." +
            "\n보호 스킬이 끝나기전에 다시 사용할 경우 이전에 보호하던 대상은 취소된다. 또한 시전자가 기절할 경우 보호 스킬이 취소된다.",
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
