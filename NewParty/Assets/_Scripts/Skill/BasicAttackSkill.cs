using Photon.Pun;
using Photon.Pun.Demo.Procedural;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicAttackSkill : Skill
{
    public enum BasicAttackType {
        Melee, Ranged,
    }

    public BasicAttackType Type;
    [SerializeField] private BasicAttack basicAttackPrefab;

    private void Awake() {
        Name = "�⺻ ����";
        IsPassive = false;
    }

    public override IEnumerator CoUse() {
        // ��ū�� ������ ���� �����Ѵ�
        int tokenStack = Owner.Tokens.FindAll(token => token.IsSelected).Count;
        Owner.RemoveSelectedToken();

        // ������ �����Ѵ�
        Unit target = BattleSelectable.Units[0];
        BasicAttack attack = PhotonNetwork.Instantiate(GameManager.GetAttackPrefabPath(Type.ToString() + "BasicAttack"),
            target.transform.position, Quaternion.identity)
        .GetComponent<BasicAttack>();
        float dmg = Owner.GetFinalStat(StatType.Str) * (1f + Owner.GetFinalStat(StatType.StackStr) * (tokenStack - 1));
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
        return Owner.TeamType != unit.TeamType;
    }
}
