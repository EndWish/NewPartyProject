using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Events;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public class Party : MonoBehaviourPunCallbacks, IScrollHandler
{
    // ���� ���� //////////////////////////////////////////////////////////////
    public List<Unit> Units { get; private set; } = new List<Unit>();
    private BoxCollider2D boxCollider2d;

    // ���� ���� //////////////////////////////////////////////////////////////
    public TeamType TeamType = TeamType.None;
    private bool isMoveStop = false;
    private int remainRestSkill = 0;

    // ����Ƽ �Լ� ////////////////////////////////////////////////////////////
    private void Awake() {
        boxCollider2d = GetComponent<BoxCollider2D>();
    }

    private void Update() {
        // �ε����� ���� ��ġ, ������ ����
        if(TeamType != TeamType.None) {
            // ��ġ �̵�
            Vector3 targetPos = BattleManager.Instance.GetPartyPos(TeamType);

            int sign = 0;
            if (TeamType == TeamType.Player) sign = -1;
            else if (TeamType == TeamType.Enemy) sign = 1;

            // a x (1-r^n)/(1 -r) : �������� �հ���
            int index = GetIndex();
            float r = 0.9f;
            float geometricSeqSum = (1f - Mathf.Pow(r, index) ) / (1f - r);
            targetPos += new Vector3(1f * sign * geometricSeqSum, 1.2f * geometricSeqSum, 0);

            float distance = Vector3.Distance(transform.position, targetPos);
            transform.position = Vector3.MoveTowards(transform.position, targetPos, (10f + distance) * Time.deltaTime);

            if (transform.position == targetPos) isMoveStop = true;
            else isMoveStop = false;

            // ������ ����
            Vector3 targetScale = Vector3.one * Mathf.Pow(r, index);
            transform.localScale = Vector3.MoveTowards(transform.localScale, targetScale, (1f + distance/10f) * Time.deltaTime);

        }
    }

    // �Լ� ///////////////////////////////////////////////////////////////////

    public int GetIndex() {
        return BattleManager.Instance.Parties[(int)TeamType].IndexOf(this);
    }

    // ���� �߰�
    [PunRPC]
    private void AddUnitRPC(int unitViewId, int index) {
        Unit unit = PhotonView.Find(unitViewId).GetComponent<Unit>();
        Units.Insert(index, unit);
        unit.transform.parent = this.transform;
        unit.MyParty = this;
    }
    public void AddUnit(Unit unit, int index) {
        photonView.RPC("AddUnitRPC", RpcTarget.All ,unit.photonView.ViewID, index);
    }
    public void AddUnit(Unit unit) {
        photonView.RPC("AddUnitRPC", RpcTarget.All, unit.photonView.ViewID, Units.Count);
    }

    // ���� ����
    [PunRPC]
    private void RemoveUnitRPC(int unitViewId) {
        Unit unit = PhotonView.Find(unitViewId).GetComponent<Unit>();
        Units.Remove(unit);
        unit.transform.parent = null;
        unit.MyParty = null;
    }
    public void RemoveUnit(Unit unit) {
        photonView.RPC("RemoveUnitRPC", RpcTarget.All, unit.photonView.ViewID);
    }

    // ��Ƽ ��ų Ƚ��
    [PunRPC]
    private void RemainRestSkillRPC(int value) {
        remainRestSkill = value;
    }
    public int RemainRestSkill {
        get { return remainRestSkill; }
        set { photonView.RPC("RemainRestSkillRPC", RpcTarget.All, value); }
    }

    // ������
    public void OnSetTeam(TeamType type) {
        TeamType = type;
        transform.position = BattleManager.Instance.GetPartyPos(TeamType);
        if(type == TeamType.Player) {
            boxCollider2d.offset = new Vector2(-2.25f, 0f);
        } else if(type == TeamType.Enemy) {
            boxCollider2d.offset = new Vector2(2.25f, 0f);
        }

        foreach (Unit unit in Units) {
            unit.OnSetTeam(type);
        }
    }

    // IScroll, ������Ʈ ������ ��ũ�� �Ǿ��� ��
    public void OnScroll(PointerEventData eventData) {
        if (isMoveStop) {
            Vector2 wheelInput = eventData.scrollDelta;
            if (wheelInput.y > 0) {    // �� ����
                BattleManager.Instance.RotateParties(TeamType, 1);
            } else if (wheelInput.y < 0) {   // �� �Ʒ���
                BattleManager.Instance.RotateParties(TeamType, -1);
            }
        }
    }
}
