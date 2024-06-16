using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class AccelerationSkill : Skill
{
    [SerializeField] protected float tickCoefficient;
    [SerializeField] protected float speedMul;
    [SerializeField] protected GameObject reinforceFXPrefab;

    private void Awake() {
        Name = "가속";
        IsPassive = false;
    }

    public override IEnumerator CoUse() {
        Owner.RemoveSelectedToken();
        CreateReinforceFX();
        // TODO : 가속 상태이상 생성하고 Owner에게 적용하기
        AccelerationBuff statusEffect = PhotonNetwork.Instantiate(GameManager.GetStatusEffectPrefabPath("AccelerationBuff"),
            transform.position, Quaternion.identity)
            .GetComponent<AccelerationBuff>();
        statusEffect.SpeedMul = speedMul;
        statusEffect.Tick = GetTickNum();
        statusEffect.Caster = Owner;
        Owner.AddStatusEffect(statusEffect);
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
        return Owner == unit;
    }

    [PunRPC] protected void CreateReinforceFXRPC() {
        Instantiate(reinforceFXPrefab, Owner.transform.position, Quaternion.identity);
    }
    public void CreateReinforceFX() {
        photonView.RPC("CreateReinforceFXRPC", RpcTarget.All);
    }

    public override string GetDescription() {
        return string.Format("{0} 틱 동안 속도가 x{1:F1} 상승한다.", 
            BattleManager.Instance == null ? new StringBuilder("(전투 중인 유닛의 수) x ").Append(tickCoefficient).ToString() : GetTickNum(), 
            speedMul);
    }

    protected int GetTickNum() {
        return (int)Mathf.Ceil(BattleManager.Instance.GetUnitNum() * tickCoefficient);
    }

}
