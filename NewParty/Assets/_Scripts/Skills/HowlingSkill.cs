using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HowlingSkill : ActiveSkill
{
    [SerializeField] protected int turn;
    [SerializeField] protected float criChaMul;
    [SerializeField] protected float accMul;
    [SerializeField] protected int numDebuffsToRemove;
    [SerializeField] protected GameObject FxPrefab;

    protected override void Awake() {
        base.Awake();
        Name = "날갈기";
    }

    public override IEnumerator CoUse() {
        yield return StartCoroutine(Owner.UseSelectedTokens());
        CreateReinforceFX();

        // 능력치 버프 걸기
        Party targetParty = BattleSelectable.Parties[0];
        foreach (Unit target in targetParty.Units) {
            // 치확 버프
            StatTurnStatusEffect.Create(Owner, target, StatForm.AbnormalMul, StatType.CriCha, StatusEffectForm.Buff, criChaMul, turn);

            // 명중 버프
            StatTurnStatusEffect.Create(Owner, target, StatForm.AbnormalMul, StatType.Acc, StatusEffectForm.Buff, accMul, turn);

            // 랜덤한 디버프 제거
            for(int i = 0; i < numDebuffsToRemove; i++) {
                target.RemoveRandomStatusEffect((statusEffect) => statusEffect.Form == StatusEffectForm.Debuff);
            }

        }

        yield return new WaitForSeconds(0.5f);
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
        return Owner.MyParty.TeamType == unit.TeamType;
    }

    [PunRPC] protected void CreateReinforceFXRPC() {
        Instantiate(FxPrefab, Owner.transform.position, Quaternion.identity);
    }
    public void CreateReinforceFX() {
        photonView.RPC("CreateReinforceFXRPC", RpcTarget.All);
    }

    public override string GetDescriptionText() {
        return string.Format("아군 파티에게 {0}턴간 치명타 확률을 {1}, 명중을 {2} 상승시키고, 각자 랜덤한 디버프 {3}개를 제거한다",
            TooltipText.SetCountFont(turn),
            TooltipText.SetMulFont(criChaMul),
            TooltipText.SetMulFont(accMul),
            TooltipText.SetCountFont(numDebuffsToRemove));
    }

    public override string GetDetailedDescriptionText() {
        return string.Format("아군 파티에게 {0}턴간 치명타 확률을 {1}, 명중을 {2} 상승시키고, 각자 랜덤한 디버프 {3}개를 제거한다",
            TooltipText.SetCountFont(turn),
            TooltipText.SetMulFont(criChaMul),
            TooltipText.SetMulFont(accMul),
            TooltipText.SetCountFont(numDebuffsToRemove));
    }

}
