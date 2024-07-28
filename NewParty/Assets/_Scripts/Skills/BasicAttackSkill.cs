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
        Name = "기본 공격";
    }

    public override IEnumerator CoUse() {
        // 토큰을 개수를 세고 삭제한다
        int tokenStack = Owner.GetSelectedTokensNum();
        yield return StartCoroutine(Owner.UseSelectedTokens());

        // 공격을 생성한다
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
        return string.Format("적을 공격하여 {0}의 데미지를 준다.", 
            TooltipText.SetDamageFont(CalculateDmg(Mathf.Max(1, Owner.Tokens.FindAll(token => token.IsSelected && token.Type == TokenType.Atk).Count))));
    }
    public override string GetDetailedDescription() {
        return string.Format("적을 공격하여 {0} = ({1}100% + {1}100% x {2} x 추가토큰)의 데미지({3})를 준다.",
            TooltipText.SetDamageFont(CalculateDmg(Mathf.Max(1, Owner.Tokens.FindAll(token => token.IsSelected && token.Type == TokenType.Atk).Count))),
            TooltipText.GetIcon(StatType.Str),
            TooltipText.GetIcon(StatType.StackStr),
            Tags.GetString(attackPrefab.InitTags));
    }

}
