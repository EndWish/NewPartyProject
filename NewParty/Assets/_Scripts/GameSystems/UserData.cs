using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserData : MonoBehaviourSingleton<UserData>
{

    // 공유 정보 //////////////////////////////////////////////////////////////
    static public int MaxPartyUnit = 4;

    // 개인 정보 //////////////////////////////////////////////////////////////
    public string Nickname { get; private set; }
    
    public HashSet<NodeName> ClearNodes { get; private set; }   // 저장 정보

    public List<Unit.Data> UnitDataList { get; private set; }   // 저장 정보
    public List<Unit> UnitList { get; private set; } = new List<Unit>();

    public Unit[] PartyUnitList { get; private set; } = new Unit[MaxPartyUnit];

    public List<SoulFragment> SoulFragmentList { get; private set; }    // Data로 변환해서 저장

    // 유니티 함수 ////////////////////////////////////////////////////////////

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

    // 함수 ///////////////////////////////////////////////////////////////////

    public void SetNewPlayerData(string nickname) {
        Nickname = nickname;
        ClearNodes = GetDefaultCloearNodes();
        UnitDataList = GetDefaultUnitDataList();
        UnitList = CreateUnitListFrom(UnitDataList);
        SoulFragmentList = new List<SoulFragment>();

        Save();
    }
    public bool Load(string nickname) {
        if (!ES3.FileExists(nickname))
            return false;

        Nickname = ES3.Load<string>("Nickname", nickname);

        HashSet<NodeName> cloearNoes = new HashSet<NodeName>();
        ClearNodes = ES3.Load<HashSet<NodeName>>("ClearNodes", nickname, GetDefaultCloearNodes());
        UnitDataList = ES3.Load<List<Unit.Data>>("UnitDataList", nickname, GetDefaultUnitDataList());
        UnitList = CreateUnitListFrom(UnitDataList);

        SoulFragmentList = new List<SoulFragment> { 
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            new SoulFragment(new SoulFragment.Data(UnitType.Garuda, 15)) 
#endif
        };
        List<SoulFragment.Data> soulFragmentDataList = ES3.Load("SoulFragmentDataList", nickname, new List<SoulFragment.Data>());
        UsefulMethod.ActionAll(soulFragmentDataList, (x) => { SoulFragmentList.Add(new SoulFragment(x)); });

        LoadPartyUnit(nickname);

        return true;
    }

    public void Save() {
        ES3.Save<string>("Nickname", Nickname, Nickname);
        ES3.Save<HashSet<NodeName>>("ClearNodes", ClearNodes, Nickname);
        ES3.Save<List<Unit.Data>>("UnitDataList", UnitDataList, Nickname);

        List<SoulFragment.Data> soulFragmentDataList = new List<SoulFragment.Data>();
        UsefulMethod.ActionAll(SoulFragmentList, (x) => { soulFragmentDataList.Add(x.GetData()); });
        ES3.Save<List<SoulFragment.Data>>("SoulFragmentDataList", soulFragmentDataList, Nickname);

        SavePartyUnit();
    }

    public void SavePartyUnit() {
        int[] partyUnitIndices = GetDefaultPartyUnitIndices();

        for (int i = 0; i < MaxPartyUnit; ++i) {
            Unit partyUnit = PartyUnitList[i];
            if (partyUnit != null)
                partyUnitIndices[i] = UnitDataList.IndexOf(partyUnit.MyData);
        }
        ES3.Save<int[]>("PartyUnitIndices", partyUnitIndices, Nickname);
    }
    public void LoadPartyUnit(string nickname) {
        int[] partyUnitIndices = ES3.Load<int[]>("PartyUnitIndices", nickname, GetDefaultPartyUnitIndices());

        for(int i = 0; i < MaxPartyUnit; ++i) {
            int index = partyUnitIndices[i];
            if (index != -1) {
                Unit.Data unitData = UnitDataList[index];
                PartyUnitList[i] = UnitList.Find(unit => unit.MyData == unitData);
            }
        }
    }

    // Default 정보 불러오는 함수
    private HashSet<NodeName> GetDefaultCloearNodes() {
        HashSet<NodeName> clearNodes = new HashSet<NodeName>();
        clearNodes.Add(NodeName.Village);
        return clearNodes;
    }
    private List<Unit.Data> GetDefaultUnitDataList() {
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
    private int[] GetDefaultPartyUnitIndices() {
        int[] partyUnitIndices = new int[MaxPartyUnit];
        Array.Fill(partyUnitIndices, -1);
        return partyUnitIndices;
    }

    // 유닛 관련 함수
    private Unit CreateUnitFrom(Unit.Data unitData) {
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

    public Unit AddUnitData(Unit.Data unitData) {
        UnitDataList.Add(unitData);
        Unit unit = CreateUnitFrom(unitData);
        UnitList.Add(unit);
        return unit;
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
