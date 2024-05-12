using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserData : MonoBehaviourSingleton<UserData>
{
    public string Nickname { get; private set; }
    
    public HashSet<NodeName> ClearNodes { get; private set; }

    public List<Unit.Data> UnitDataList { get; private set; }
    public List<Unit> UnitList = new List<Unit>();

    protected override void Awake() {
        base.Awake();
        DontDestroyOnLoad(this);
    }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
    private void Update() {
        if (Input.GetKeyDown(KeyCode.F1)) {
            Save();
            OnlyDebug.Log("저장되었습니다.");
        }

    }
#endif

    public void SetNewPlayerData(string nickname) {
        Nickname = nickname;
        ClearNodes = GetInitCloearNodes();
        UnitDataList = GetInitUnitDataList();
        UnitList = CreateUnitListFrom(UnitDataList);

        Save();
    }
    public bool Load(string nickname) {
        if (!ES3.FileExists(nickname))
            return false;

        Nickname = ES3.Load<string>("Nickname", nickname);

        HashSet<NodeName> cloearNoes = new HashSet<NodeName>();
        ClearNodes = ES3.Load<HashSet<NodeName>>("ClearNodes", nickname, GetInitCloearNodes());
        UnitDataList = ES3.Load<List<Unit.Data>>("UnitDataList", nickname, GetInitUnitDataList());
        UnitList = CreateUnitListFrom(UnitDataList);

        return true;
    }
    public void Save() {
        ES3.Save<string>("Nickname", Nickname, Nickname);
        ES3.Save<HashSet<NodeName>>("ClearNodes", ClearNodes, Nickname);
        ES3.Save<List<Unit.Data>>("UnitDataList", UnitDataList, Nickname);
    }

    private HashSet<NodeName> GetInitCloearNodes() {
        HashSet<NodeName> clearNodes = new HashSet<NodeName>();
        clearNodes.Add(NodeName.Village);
        return clearNodes;
    }
    private List<Unit.Data> GetInitUnitDataList() {
        List<Unit.Data> unitDataList = new List<Unit.Data> {
            new Unit.Data(UnitType.Garuda, 0),
        };

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        for(int i = 0; i < 70; ++i) {
            unitDataList.Add(new Unit.Data(UnitType.Garuda, UnityEngine.Random.Range(-10, 20)));
        }
#endif

        return unitDataList;
    }

    public Unit CreateUnitFrom(Unit.Data unitData) {
        Unit prefab = Resources.Load<Unit>("Prefabs/Units/" + unitData.Type.ToString());
        Unit unit = Instantiate<Unit>(prefab, this.transform);
        unit.ApplyData(unitData);
        unit.gameObject.SetActive(false);

        return unit;
    }
    private List<Unit> CreateUnitListFrom(List<Unit.Data> unitDataList) {
        List<Unit> unitList = new List<Unit>();
        foreach (var unitData in unitDataList) {
            Unit unit = CreateUnitFrom(unitData);
            unitList.Add(unit);
        }
        return unitList;
    }

    public void AddUnitData(Unit.Data unitData) {
        Unit unit = CreateUnitFrom(unitData);
        UnitDataList.Add(unitData);
    }

    public void RemoveUnitData(Unit.Data unitData) {
        // 리스트에서 제거
        UnitDataList.Remove(unitData);

        Unit unit = UnitList.Find(unit => unit.MyData == unitData);
        UnitList.Remove(unit);

        // 유닛 삭제
        Destroy(unit.gameObject);
    }
    public void RemoveUnitData(Unit unit) {
        // 리스트에서 제거
        UnitDataList.Remove(unit.MyData);
        UnitList.Remove(unit);

        // 유닛 삭제
        Destroy(unit.gameObject);
    }

}
