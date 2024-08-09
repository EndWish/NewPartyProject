using UnityEngine;

public abstract class BasicAttackSkill : ActiveSkill
{
    [SerializeField] protected BasicAttack attackPrefab;

    protected override void Awake() {
        base.Awake();
        Name = "기본 공격";
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

    public override string GetDescriptionText() {
        return string.Format("적을 공격하여 {0}의 데미지를 준다.", 
            TooltipText.SetDamageFont(CalculateDmg(Mathf.Max(1, Owner.Tokens.FindAll(token => token.IsSelected && token.Type == TokenType.Atk).Count))));
    }
    public override string GetDetailedDescriptionText() {
        return string.Format("적을 공격하여 {0} = ({1}100% + {1}100% x {2} x 추가토큰)의 데미지({3})를 준다.",
            TooltipText.SetDamageFont(CalculateDmg(Mathf.Max(1, Owner.Tokens.FindAll(token => token.IsSelected && token.Type == TokenType.Atk).Count))),
            TooltipText.GetIcon(StatType.Str),
            TooltipText.GetIcon(StatType.StackStr),
            Tags.GetString(attackPrefab.InitTags));
    }

}
