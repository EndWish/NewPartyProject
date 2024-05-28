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
    // 연결 정보 //////////////////////////////////////////////////////////////
    public List<Unit> Units { get; private set; } = new List<Unit>();
    private BoxCollider2D boxCollider2d;

    // 개인 정보 //////////////////////////////////////////////////////////////
    public TeamType TeamType = TeamType.None;
    private bool isMoveStop = false;
    private int remainRestSkill = 0;

    // 유니티 함수 ////////////////////////////////////////////////////////////
    private void Awake() {
        boxCollider2d = GetComponent<BoxCollider2D>();
    }

    private void Update() {
        // 인덱스에 따라 위치, 스케일 변동
        if(TeamType != TeamType.None) {
            // 위치 이동
            Vector3 targetPos = BattleManager.Instance.GetPartyPos(TeamType);

            int sign = 0;
            if (TeamType == TeamType.Player) sign = -1;
            else if (TeamType == TeamType.Enemy) sign = 1;

            // a x (1-r^n)/(1 -r) : 등비수열의 합공식
            int index = GetIndex();
            float r = 0.9f;
            float geometricSeqSum = (1f - Mathf.Pow(r, index) ) / (1f - r);
            targetPos += new Vector3(1f * sign * geometricSeqSum, 1.2f * geometricSeqSum, 0);

            float distance = Vector3.Distance(transform.position, targetPos);
            transform.position = Vector3.MoveTowards(transform.position, targetPos, (10f + distance) * Time.deltaTime);

            if (transform.position == targetPos) isMoveStop = true;
            else isMoveStop = false;

            // 스케일 변경
            Vector3 targetScale = Vector3.one * Mathf.Pow(r, index);
            transform.localScale = Vector3.MoveTowards(transform.localScale, targetScale, (1f + distance/10f) * Time.deltaTime);

        }
    }

    // 함수 ///////////////////////////////////////////////////////////////////

    public int GetIndex() {
        return BattleManager.Instance.Parties[(int)TeamType].IndexOf(this);
    }

    // 유닛 추가
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

    // 유닛 빼기
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

    // 파티 스킬 횟수
    [PunRPC]
    private void RemainRestSkillRPC(int value) {
        remainRestSkill = value;
    }
    public int RemainRestSkill {
        get { return remainRestSkill; }
        set { photonView.RPC("RemainRestSkillRPC", RpcTarget.All, value); }
    }

    // 팀설정
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

    // IScroll, 오브젝트 위에서 스크롤 되었을 때
    public void OnScroll(PointerEventData eventData) {
        if (isMoveStop) {
            Vector2 wheelInput = eventData.scrollDelta;
            if (wheelInput.y > 0) {    // 휠 위로
                BattleManager.Instance.RotateParties(TeamType, 1);
            } else if (wheelInput.y < 0) {   // 휠 아래로
                BattleManager.Instance.RotateParties(TeamType, -1);
            }
        }
    }
}
