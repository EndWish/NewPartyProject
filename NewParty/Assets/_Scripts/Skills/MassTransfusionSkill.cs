using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class MassTransfusionSkill : ActiveSkill
{
    [SerializeField] protected float hpConsumptionRate;
    [SerializeField] protected float recoverDefPenCoefficient;
    [SerializeField] protected float healingMul;
    [SerializeField] protected int turn;
    [SerializeField] protected GameObject fxPrefab;

    protected override void Awake() {
        base.Awake();
        Name = "��ü ����";
    }

    public override IEnumerator CoUse() {
        Owner.RemoveSelectedToken();
        
        // �ڽ��� ü���� ��´�.
        float hpCost = Owner.Hp * hpConsumptionRate;
        Owner.Hp -= hpCost;
        float recoverHp = hpCost + Owner.GetFinalStat(StatType.DefPen) * recoverDefPenCoefficient;
        CreateFX();
        yield return new WaitForSeconds(0.3f);

        // ȸ�� ��ü�� �����Ѵ�.
        List<MassTransfusionAttack> attackList = new List<MassTransfusionAttack>();
        foreach (Unit target in BattleSelectable.Parties[0].Units) {
            if (target == Owner)
                continue;

            MassTransfusionAttack attack = PhotonNetwork.Instantiate(GameManager.GetAttackPrefabPath("MassTransfusionAttack"),
                Owner.transform.position, Quaternion.identity)
                .GetComponent<MassTransfusionAttack>();
            attack.Init(Owner, target, recoverHp);
            attackList.Add(attack);
            StartCoroutine(attack.Animate());
            yield return new WaitForSeconds(0.2f);
        }

        // �ڽſ��� ȸ���� ������ �Ǵ�.
        StatTurnStatusEffect healingBuff = CreateStatTurnStatusEffect(StatForm.AbnormalMul, StatType.Healing, StatusEffectForm.Buff, healingMul, turn);
        Owner.AddStatusEffect(healingBuff);

        while(0 < attackList.Count) {
            attackList.RemoveAll((x) => x == null);
            yield return null;
        }
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
        return Owner.MyParty == unit.MyParty;
    }

    [PunRPC] protected void CreateFXRPC() {
        Instantiate(fxPrefab, Owner.transform.position, Quaternion.identity);
    }
    public void CreateFX() {
        photonView.RPC("CreateFXRPC", RpcTarget.All);
    }

    public override string GetDescription() {
        return string.Format("���� ü���� {0:G}%�� �Ҹ��Ͽ� �ڽ��� ��Ƽ������ (�Ҹ��� ü�� + ��� ������� {1:G}%)��ŭ ü���� ȸ����Ų��. �׸��� �ڽſ��� {2}�ϰ� ȸ������ x{3:F2}������Ų��.",
            hpConsumptionRate * 100f, recoverDefPenCoefficient * 100f, turn, healingMul);
    }
    
}
