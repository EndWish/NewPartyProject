using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitController : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Unit unit;

    private void Awake() {
        unit = GetComponent<Unit>();
    }

    private void Update() {
        // ��ġ
        if (unit.TeamType != TeamType.None) {
            Vector3 pos = Vector3.zero;

            int sign = 0;
            if (unit.TeamType == TeamType.Player) sign = -1;
            else if (unit.TeamType == TeamType.Enemy) sign = 1;

            int index = unit.GetIndex();
            pos += new Vector3(1.5f * sign * index, 0, -1); // ������ �۾������� �ϰ� �ʹ�

            transform.localPosition = Vector3.MoveTowards(transform.localPosition, pos, 10f * Time.deltaTime);
        }

        // ũ��
        if (unit.HasTurn()) {
            transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.one * 1.3f, Time.deltaTime * 2f);
        }
        else {
            transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.one, Time.deltaTime * 2f);
        }
    }

    // Ŭ�� ���� �Լ�
    public void OnPointerClick(PointerEventData eventData) {
        if (BattleSelectable.IsRunning)
            return;

        BattleManager battleManager = BattleManager.Instance;
        if (battleManager.UnitClicked == unit)
            battleManager.UnitClicked = null;
        else
            battleManager.UnitClicked = unit;
    }
    public void OnPointerEnter(PointerEventData eventData) {
        BattleManager battleManager = BattleManager.Instance;
        battleManager.UnitOnMouse = unit;
    }
    public void OnPointerExit(PointerEventData eventData) {
        BattleManager battleManager = BattleManager.Instance;
        battleManager.UnitOnMouse = null;
    }

}
