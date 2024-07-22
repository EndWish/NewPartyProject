using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicBarrierSkill : ActiveSkill
{
    protected override void Awake() {
        base.Awake();
        Name = "�⺻ �踮��";
    }

    public override IEnumerator CoUse() {
        // ��ū�� ������ ���� �����Ѵ�
        int tokenStack = Owner.Tokens.FindAll(token => token.IsSelected).Count;
        yield return StartCoroutine(Owner.UseSelectedTokens());

        // �⺻ �踮�� ���� �� ����
        BasicBarrier barrier = PhotonNetwork.Instantiate(GameManager.GetBarrierPrefabPath("BasicBarrier"),
            transform.position, Quaternion.identity)
            .GetComponent<BasicBarrier>();
        barrier.Init(Owner, CalculateAmount(tokenStack));

        // �⺻ �踮�� ������ �����̻� ���� �� ����
        BasicBarrierOverload statusEffect = PhotonNetwork.Instantiate(GameManager.GetStatusEffectPrefabPath("BasicBarrierOverload"),
            transform.position, Quaternion.identity)
            .GetComponent<BasicBarrierOverload>();
        statusEffect.Caster = Owner;
        Owner.AddStatusEffect(statusEffect);

        yield return new WaitForSeconds(0.15f);
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

    protected override bool MeetTokenCost() {
        bool result = true;
        int count = 0;
        foreach (var token in Owner.Tokens) {
            if (!token.IsSelected)
                continue;

            if (token.Type != TokenType.Barrier) {
                result = false;
                break;
            }

            ++count;
        }

        if (count == 0)
            result = false;

        return result;
    }

    protected float CalculateAmount(int tokenStack) {
        return Owner.GetFinalStat(StatType.Shield) * (1f + Owner.GetFinalStat(StatType.StackShield) * (tokenStack - 1));
    }

    public override string GetDescription() {
        return string.Format("{0}�� �������� �����ִ� �踮� �����Ѵ�. �⺻ �踮��� ��ø���� �ʴ´�.",
            CalculateAmount(Mathf.Max(1, Owner.Tokens.FindAll(token => token.IsSelected && token.Type == TokenType.Barrier).Count)));
    }
}