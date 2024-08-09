using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class MassTransfusionSkill : ActiveSkill
{
    [SerializeField] protected float hpConsumptionRate;
    [SerializeField] protected float recoverDefPenCoefficient;
    [SerializeField] protected float healingMul;
    [SerializeField] protected int turn;
    [SerializeField] protected GameObject fxPrefab;

    protected override void Awake() {
        base.Awake();
        Name = "단체 수혈";
    }

    public override IEnumerator CoUse() {
        yield return StartCoroutine(Owner.UseSelectedTokens());
        
        // 자신의 체력을 깎는다.
        float hpCost = CalculateHpCost();
        Owner.Hp -= hpCost;
        float recoverHp = CalculateRecoverHp(hpCost);
        CreateFX();
        yield return new WaitForSeconds(0.3f);

        // 회복 객체를 생성한다.
        List<MassTransfusionAttack> attackList = new List<MassTransfusionAttack>();
        foreach (Unit target in BattleSelectable.Parties[0].Units) {
            if (target == Owner)
                continue;

            
            MassTransfusionAttack attack = MassTransfusionAttack.Create(Owner, target, recoverHp);
            attackList.Add(attack);
            StartCoroutine(attack.Animate());
            yield return new WaitForSeconds(0.2f);
        }

        // 자신에게 회복력 버프를 건다.
        StatTurnStatusEffect.Create(Owner, Owner, StatForm.AbnormalMul, StatType.Healing, StatusEffectForm.Buff, healingMul, turn);

        while(0 < attackList.Count) {
            attackList.RemoveAll((x) => x == null);
            yield return null;
        }
    }

    public override BattleSelectionType GetSelectionType() {
        return BattleSelectionType.Party;
    }
    public override int GetSelectionNum() {
        return 1;
    }
    public override bool SelectionPred(Unit unit) {
        if (!base.SelectionPred(unit))
            return false;
        return Owner.MyParty == unit.MyParty;
    }

    [PunRPC] protected void CreateFXRPC() {
        Instantiate(fxPrefab, Owner.transform.position, Quaternion.identity);
    }
    public void CreateFX() {
        photonView.RPC("CreateFXRPC", RpcTarget.All);
    }

    public override string GetDescriptionText() {
        return string.Format("체력을 {0}소모하여 자신의 파티원들을 {1}만큼 체력을 회복시킨다. 그리고 자신에게 {2}턴간 회복력을 {3}증가시킨다.",
            TooltipText.SetDamageFont(CalculateHpCost()),
            TooltipText.SetDamageFont(CalculateRecoverHp(CalculateHpCost())),
            TooltipText.SetCountFont(turn),
            TooltipText.SetMulFont(healingMul));
    }
    public override string GetDetailedDescriptionText() {
        return string.Format("체력을 {0} = (현재{4}{5}%)소모하여 자신의 파티원들을 {1} = (소모한 체력 + {6}{7}%)만큼 체력을 회복시킨다. 그리고 자신에게 {2}턴간 회복력을 {3}증가시킨다.",
            TooltipText.SetDamageFont(CalculateHpCost()),
            TooltipText.SetDamageFont(CalculateRecoverHp(CalculateHpCost())),
            TooltipText.SetCountFont(turn),
            TooltipText.SetMulFont(healingMul),
            TooltipText.GetIcon(StatType.Hpm),
            TooltipText.GetFlexibleFloat(hpConsumptionRate * 100f),
            TooltipText.GetIcon(StatType.DefPen),
            TooltipText.GetFlexibleFloat(recoverDefPenCoefficient * 100f));
    }

    protected float CalculateHpCost() {
        return Owner.Hp * hpConsumptionRate;
    }
    protected float CalculateRecoverHp(float hpCost) {
        return hpCost + Owner.GetFinalStat(StatType.DefPen) * recoverDefPenCoefficient;
    }

}
