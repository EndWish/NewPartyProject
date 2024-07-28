using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class SharpenBladeSkill : ActiveSkill
{
    [SerializeField] protected int turn;
    [SerializeField] protected float criChaAdd;
    [SerializeField] protected GameObject reinforceFxPrefab;

    protected override void Awake() {
        base.Awake();
        Name = "������";
    }

    public override IEnumerator CoUse() {
        yield return StartCoroutine(Owner.UseSelectedTokens());
        CreateReinforceFX();

        // ġȮ ����
        StatTurnStatusEffect accBuff = CreateStatTurnStatusEffect(StatForm.AbnormalAdd, StatType.CriCha, StatusEffectForm.Buff, criChaAdd, turn);
        Owner.AddStatusEffect(accBuff);

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
        Instantiate(reinforceFxPrefab, Owner.transform.position, Quaternion.identity);
    }
    public void CreateReinforceFX() {
        photonView.RPC("CreateReinforceFXRPC", RpcTarget.All);
    }

    public override string GetDescription() {
        return string.Format("{0}�� ���� ġ��Ÿ Ȯ���� {1}����Ѵ�.",
            TooltipText.SetCountFont(turn),
            TooltipText.SetPercentPointFont(criChaAdd));
    }
    public override string GetDetailedDescription() {
        return string.Format("{0}�� ���� ġ��Ÿ Ȯ���� {1}����Ѵ�.",
            TooltipText.SetCountFont(turn),
            TooltipText.SetPercentPointFont(criChaAdd));
    }

}
