using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class AntiSkillSkill : ActiveSkill
{
    [SerializeField] protected float dmgMul;
    [SerializeField] protected int turn;

    protected override void Awake() {
        base.Awake();
        Name = "��Ƽ ��ų";
    }

    public override IEnumerator CoUse() {
        yield return StartCoroutine(Owner.UseSelectedTokens());

        Party targetParty = BattleSelectable.Parties[0];
        foreach(Unit target in targetParty.Units) {
            AntiSkillBuff statusEffect = PhotonNetwork.Instantiate(GameManager.GetStatusEffectPrefabPath("AntiSkillBuff"),
                transform.position, Quaternion.identity)
                .GetComponent<AntiSkillBuff>();
            statusEffect.DmgMul = dmgMul;
            statusEffect.Turn = turn;
            statusEffect.Caster = Owner;
            target.AddStatusEffect(statusEffect);
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
        return Owner.TeamType == unit.TeamType;
    }

    public override string GetDescription() {
        return string.Format("������ ��Ƽ�� {0}�ϰ� (#��ų ����)�� �ǰݽ� �޴� ���ط��� {1}�����Ѵ�.",
            TooltipText.SetCountFont(turn),
            TooltipText.SetMulFont(dmgMul));
    }
    public override string GetDetailedDescription() {
        return string.Format("������ ��Ƽ�� {0}�ϰ� (#��ų ����)�� �ǰݽ� �޴� ���ط��� {1}�����Ѵ�.",
            TooltipText.SetCountFont(turn),
            TooltipText.SetMulFont(dmgMul));
    }

}
