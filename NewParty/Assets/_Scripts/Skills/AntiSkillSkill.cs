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
        Name = "안티 스킬";
    }

    public override IEnumerator CoUse() {
        Owner.RemoveSelectedToken();

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
        return string.Format("선택한 파티에게 {0}턴간 (#스킬 공격)에 피격시 피해량을 {1:F2}%낮추는 버프를 부여한다.",
            turn, (1f - dmgMul) * 100f);
    }
}
