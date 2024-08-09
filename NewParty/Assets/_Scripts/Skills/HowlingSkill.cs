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
        Name = "������";
    }

    public override IEnumerator CoUse() {
        yield return StartCoroutine(Owner.UseSelectedTokens());
        CreateReinforceFX();

        // �ɷ�ġ ���� �ɱ�
        Party targetParty = BattleSelectable.Parties[0];
        foreach (Unit target in targetParty.Units) {
            // ġȮ ����
            StatTurnStatusEffect.Create(Owner, target, StatForm.AbnormalMul, StatType.CriCha, StatusEffectForm.Buff, criChaMul, turn);

            // ���� ����
            StatTurnStatusEffect.Create(Owner, target, StatForm.AbnormalMul, StatType.Acc, StatusEffectForm.Buff, accMul, turn);

            // ������ ����� ����
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
        return string.Format("�Ʊ� ��Ƽ���� {0}�ϰ� ġ��Ÿ Ȯ���� {1}, ������ {2} ��½�Ű��, ���� ������ ����� {3}���� �����Ѵ�",
            TooltipText.SetCountFont(turn),
            TooltipText.SetMulFont(criChaMul),
            TooltipText.SetMulFont(accMul),
            TooltipText.SetCountFont(numDebuffsToRemove));
    }

    public override string GetDetailedDescriptionText() {
        return string.Format("�Ʊ� ��Ƽ���� {0}�ϰ� ġ��Ÿ Ȯ���� {1}, ������ {2} ��½�Ű��, ���� ������ ����� {3}���� �����Ѵ�",
            TooltipText.SetCountFont(turn),
            TooltipText.SetMulFont(criChaMul),
            TooltipText.SetMulFont(accMul),
            TooltipText.SetCountFont(numDebuffsToRemove));
    }

}
