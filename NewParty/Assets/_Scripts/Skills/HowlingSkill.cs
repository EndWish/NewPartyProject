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
        Owner.RemoveSelectedToken();
        CreateReinforceFX();

        // �ɷ�ġ ���� �ɱ�
        Party targetParty = BattleSelectable.Parties[0];
        foreach (Unit target in targetParty.Units) {
            // ġȮ ����
            StatTurnStatusEffect criChaBuff = CreateStatTurnStatusEffect(StatForm.AbnormalMul, StatType.CriCha, StatusEffectForm.Buff, criChaMul, turn);
            target.AddStatusEffect(criChaBuff);

            // ���� ����
            StatTurnStatusEffect accBuff = CreateStatTurnStatusEffect(StatForm.AbnormalMul, StatType.Acc, StatusEffectForm.Buff, accMul, turn);
            target.AddStatusEffect(accBuff);

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

    public override string GetDescription() {
        return string.Format("�Ʊ� ��Ƽ���� {0}�ϰ� ġ��Ÿ Ȯ���� x{1}, ������ x{2} ��½�Ű��, ���� ������ ����� {3}���� �����Ѵ�", turn, criChaMul, accMul, numDebuffsToRemove);
    }
}
