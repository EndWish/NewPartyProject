using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public enum BattleSelectionType
{
    None, Unit, Party, Team, All,
}

public class BattleSelectable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public class CoroutineExecuter : MonoBehaviour { }

    // 공유 변수 //////////////////////////////////////////////////////////////
    static private CoroutineExecuter instance;
    static public CoroutineExecuter Instance {
        get { 
            if(instance == null) {
                instance = new GameObject("CoroutineExecuter").AddComponent<CoroutineExecuter>();
            }
            return instance; 
        }
    }

    static private Coroutine coroutine = null;

    static public bool IsRunning { get; private set; } = false;

    static public List<Unit> Units { get; private set; } = new List<Unit>();
    static public List<Party> Parties { get; private set; } = new List<Party>();
    static public bool allSelected = false;

    static public Predicate<Unit> SelectionPredicate { get; private set; } = null;
    static public UnityAction OnStop;
    static private UnityAction OnCancel;

    static public BattleSelectionType SelectionType { get; private set; } = BattleSelectionType.None;
    static public int NumTarget { get; private set; } = 0;
    static public int MaxNumTarget { get; private set; } = 0;

    static public void RunSelectMode(BattleSelectionType selectionType, int maxNumTarget, Predicate<Unit> predicate, UnityAction onCompleteSelection, UnityAction onCancel) {
        if (selectionType == BattleSelectionType.None || maxNumTarget <= 0)    // 잘못된 입력
            return;

        InitRunning(selectionType, maxNumTarget, predicate, onCancel);

        coroutine = Instance.StartCoroutine(CoRunSelectMode(onCompleteSelection));
    }
    static private void InitRunning(BattleSelectionType selectionType, int maxNumTarget, Predicate<Unit> predicate, UnityAction onCancel) {
        IsRunning = true;
        BattleSelectable.SelectionType = selectionType;
        BattleSelectable.MaxNumTarget = maxNumTarget;
        BattleSelectable.NumTarget = 0;

        Units.Clear();
        Parties.Clear();
        allSelected = false;

        SelectionPredicate = predicate;
        OnCancel = onCancel;
    }
    static private IEnumerator CoRunSelectMode(UnityAction onCompleteSelection) {

        while (true) {
            // 엔터를 눌렀을 경우
            if(Input.GetKeyUp(KeyCode.Return)) {
                if (0 < NumTarget)
                    break;
            }

            // Esc를 눌렀을 경우
            else if (Input.GetKeyUp(KeyCode.Escape)) {
                if(0 < NumTarget) {
                    if(SelectionType == BattleSelectionType.Unit) {
                        Units[NumTarget - 1].GetComponent<BattleSelectable>().AddNumSelectionOfSelectionType(-1);
                        Units.RemoveAt(NumTarget - 1);
                    }
                    if (SelectionType == BattleSelectionType.Party) {
                        Parties[NumTarget - 1].Units[0].GetComponent<BattleSelectable>().AddNumSelectionOfSelectionType(-1);
                        Parties.RemoveAt(NumTarget - 1);
                    }
                    --NumTarget;
                } else {
                    StopSelectMode();
                    yield break;
                }
            }

            // 선택을 완료했는지 확인한다
            if (NumTarget == MaxNumTarget || allSelected) {
                break;
            }

            yield return null;
        }

        // 성공적으로 선택이 완료되었을 경우
        onCompleteSelection.Invoke();
        OnStop?.Invoke();
        IsRunning = false;
    }

    static public void StopSelectMode() {
        if (IsRunning) {
            Instance.StopCoroutine(coroutine);
            IsRunning = false;
            OnCancel?.Invoke();
            OnStop?.Invoke();
        }
    }

    // 연결 변수 //////////////////////////////////////////////////////////////
    private Unit myUnit;
    [SerializeField] private GameObject SelectionUI;
    [SerializeField] private TextMeshProUGUI SelectionUIText;

    // 개인 변수 //////////////////////////////////////////////////////////////
    private int numSelection = 0;

    // 유니티 함수 ////////////////////////////////////////////////////////////
    private void Start() {
        SelectionUI.SetActive(false);
        myUnit = GetComponent<Unit>();
    }

    // 함수 ///////////////////////////////////////////////////////////////////
    public int NumSelection {
        get { return numSelection; }
        private set {
            int prevValue = numSelection;
            numSelection = value;
            SelectionUI.SetActive(0 < numSelection);
            SelectionUIText.text = 1 < numSelection ? numSelection.ToString() : "";

            if (prevValue == 0 && 0 < numSelection)
                BattleSelectable.OnStop += ResetNumSelection;
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (IsRunning && (SelectionPredicate?.Invoke(myUnit) ?? true)) {
            AddNumSelectionOfSelectionType(+1);
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (IsRunning && (SelectionPredicate?.Invoke(myUnit) ?? true)) {
            AddNumSelectionOfSelectionType(-1);
        }
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (IsRunning && (SelectionPredicate?.Invoke(myUnit) ?? true) && NumTarget < MaxNumTarget) {
            AddNumSelectionOfSelectionType(+1);

            if (SelectionType == BattleSelectionType.Unit)
                Units.Add(myUnit);
            else if (SelectionType == BattleSelectionType.Party)
                Parties.Add(myUnit.MyParty);
            else
                allSelected = true;

            ++NumTarget;
        }
    }

    public void AddNumSelectionOfSelectionType(int num) {
        switch (SelectionType) {
            case BattleSelectionType.Unit:
                NumSelection += num;
                break;
            case BattleSelectionType.Party:
                foreach (Unit unit in myUnit.MyParty.Units)
                    unit.GetComponent<BattleSelectable>().NumSelection += num;
                break;
            case BattleSelectionType.Team:
                foreach (Party party in BattleManager.Instance.Parties[(int)myUnit.TeamType])
                    foreach (Unit unit in party.Units)
                        unit.GetComponent<BattleSelectable>().NumSelection += num;
                break;
            case BattleSelectionType.All:
                for (TeamType type = TeamType.None; type < TeamType.Num; ++type)
                    foreach (Party party in BattleManager.Instance.Parties[(int)type])
                        foreach (Unit unit in party.Units)
                            unit.GetComponent<BattleSelectable>().NumSelection += num;
                break;
        }
    }

    public void ResetNumSelection() {
        NumSelection = 0;
        BattleSelectable.OnStop -= ResetNumSelection;
    }

}
