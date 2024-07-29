using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class AccelerationSkill : ActiveSkill
{
    [SerializeField] protected float tickCoefficient;
    [SerializeField] protected float speedMul;
    [SerializeField] protected GameObject reinforceFXPrefab;

    protected override void Awake() {
        base.Awake();
        Name = "����";
    }

    public override IEnumerator CoUse() {
        yield return StartCoroutine(Owner.UseSelectedTokens());

        CreateReinforceFX();
        AccelerationBuff.Create(Owner, Owner, GetTickNum(), speedMul);

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
        return string.Format("{0} ƽ ���� �ӵ��� {1} ����Ѵ�.",
            BattleManager.Instance == null ? "-" : TooltipText.SetCountFont(GetTickNum()),
            TooltipText.SetMulFont(speedMul));
    }
    public override string GetDetailedDescription() {
        return string.Format("{0} = (���� ���� ������ �� x {1})ƽ ���� �ӵ��� {2}����Ѵ�.",
            BattleManager.Instance == null ? "-" : TooltipText.SetCountFont(GetTickNum()),
            tickCoefficient,
            TooltipText.SetMulFont(speedMul));
    }

    protected int GetTickNum() {
        return (int)Mathf.Ceil(BattleManager.Instance.GetUnitNum() * tickCoefficient);
    }

}
