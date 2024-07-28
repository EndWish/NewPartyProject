using Photon.Pun;
using Photon.Pun.Demo.Procedural;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicAttackSkill : ActiveSkill
{
    [SerializeField] protected BasicAttack attackPrefab;

    public enum BasicAttackType {
        Melee, Ranged,
    }

    public BasicAttackType Type;

    protected override void Awake() {
        base.Awake();
        Name = "�⺻ ����";
    }

    public override IEnumerator CoUse() {
        // ��ū�� ������ ���� �����Ѵ�
        int tokenStack = Owner.GetSelectedTokensNum();
        yield return StartCoroutine(Owner.UseSelectedTokens());

        // ������ �����Ѵ�
        Unit target = BattleSelectable.Units[0];
        BasicAttack attack = PhotonNetwork.Instantiate(GameManager.GetAttackPrefabPath(Type.ToString() + "BasicAttack"),
            target.transform.position, Quaternion.identity)
        .GetComponent<BasicAttack>();
        float dmg = CalculateDmg(tokenStack);
        attack.Init(Owner, target, tokenStack, dmg);

        yield return StartCoroutine(attack.Animate());
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
        return Owner.TeamType != unit.TeamType;
    }

    protected override bool MeetTokenCost() {
        int count = 0;
        foreach (var token in Owner.Tokens) {
            if (!token.IsSelected)
                continue;

            if (token.Type != TokenType.Atk) {
                return false;
            }

            ++count;
        }

        if (count == 0)
            return false;

        return true;
    }

    protected float CalculateDmg(int tokenStack) {
        return Owner.GetFinalStat(StatType.Str) * (1f + Owner.GetFinalStat(StatType.StackStr) * (tokenStack - 1));
    }

    public override string GetDescription() {
        return string.Format("���� �����Ͽ� {0}�� �������� �ش�.", 
            TooltipText.SetDamageFont(CalculateDmg(Mathf.Max(1, Owner.Tokens.FindAll(token => token.IsSelected && token.Type == TokenType.Atk).Count))));
    }
    public override string GetDetailedDescription() {
        return string.Format("���� �����Ͽ� {0} = ({1}100% + {1}100% x {2} x �߰���ū)�� ������({3})�� �ش�.",
            TooltipText.SetDamageFont(CalculateDmg(Mathf.Max(1, Owner.Tokens.FindAll(token => token.IsSelected && token.Type == TokenType.Atk).Count))),
            TooltipText.GetIcon(StatType.Str),
            TooltipText.GetIcon(StatType.StackStr),
            Tags.GetString(attackPrefab.InitTags));
    }

}
