using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class UserData : MonoBehaviourSingleton<UserData>
{

    // 공유 정보 //////////////////////////////////////////////////////////////
    static public int PartySequenceMax = 4;

    // 개인 정보 //////////////////////////////////////////////////////////////
    public string Nickname { get; private set; }

    
    
    public HashSet<NodeName> ClearNodes { get; private set; }   // 저장 정보

    private NodeName targetDungeon = NodeName.None;

    public List<Unit.Data> UnitDataList { get; private set; }   // 저장 정보

    public Unit.Data[] PartySequence { get; private set; }      // 저장 정보

    private int soulDust = 0;
    public List<SoulFragment> SoulFragmentList { get; private set; }    // Data로 변환해서 저장

    

    // 유니티 함수 ////////////////////////////////////////////////////////////

    protected override void Awake() {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    // 함수 ///////////////////////////////////////////////////////////////////

    public void SetNewPlayerData(string nickname) {
        // 닉네임
        Nickname = nickname;
        ES3.Save<string>("Nickname", Nickname, Nickname);

        SetDefaultUnitData();
        SetDefaultClearNodes();
        SetDefaultSoulFragment();
        SoulDust = 0;

        SetDefaultPartySequence();

    }
    public bool Load(string nickname) {
        if (!ES3.FileExists(nickname))
            return false;

        Nickname = ES3.Load<string>("Nickname", nickname);
        if (!LoadUnitDataList()) SetDefaultUnitData();
        if (!LoadClearNodes()) SetDefaultClearNodes();
        if (!LoadSoulFragmentList()) SetDefaultSoulFragment();
        LoadSouldust();
        if (!LoadPartySequence()) SetDefaultPartySequence();
        if (!LoadTargetDungeon()) SetDefaultTargetDungeon();
        return true;
    }
    
    // 유닛 관련 함수
    private void SetDefaultUnitData() {
        UnitDataList = new List<Unit.Data>();

        this.AddUnitData(new Unit.Data(UnitType.Garuda, 0));

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        for (int i = 0; i < 5; ++i) {
            this.AddUnitData(new Unit.Data(UnitType.Garuda, i * 10));
            this.AddUnitData(new Unit.Data(UnitType.GrayWolf, i * 10));
            this.AddUnitData(new Unit.Data(UnitType.RedWolf, i * 10));
            this.AddUnitData(new Unit.Data(UnitType.AwlMosquito, i * 10));
            this.AddUnitData(new Unit.Data(UnitType.DrillMosquito, i * 10));
            this.AddUnitData(new Unit.Data(UnitType.StoneTurtle, i * 10));
            this.AddUnitData(new Unit.Data(UnitType.IronTurtle, i * 10));
        }

        this.AddUnitData(new Unit.Data(UnitType.Garuda, 0));
        this.AddUnitData(new Unit.Data(UnitType.Garuda, 0));
#endif
    }
    public void AddUnitData(Unit.Data unitData) {
        UnitDataList.Add(unitData);

        // 새로운 정보일 경우 바로 저장한다.
        if(unitData.SaveKey == -1) {
            unitData.Save();
            SaveUnitDataKeyList();
        }
    }
    public void RemoveUnitData(Unit.Data unitData) {
        unitData.DeleteSaveData();
        UnitDataList.Remove(unitData);
        SaveUnitDataKeyList();

        // 파티슬롯에 끼워져 있을경우 제거하고 저장한다.
        for (int i = 0; i < PartySequenceMax; ++i) {
            if(PartySequence[i] == unitData) {
                PartySequence[i] = null;
                SavePartySequence();
                break;
            }
        }
    }
    public void SaveUnitDataKeyList() {
        List<long> keyList = UnitDataList.Select(data => data.SaveKey).ToList();
        ES3.Save<List<long>>("UnitDataKeyList", keyList, Nickname);
    }
    private bool LoadUnitDataList() {
        if (ES3.KeyExists("UnitDataKeyList", Nickname)) {
            
            UnitDataList = new List<Unit.Data>();
            List<long> dataKeyList = ES3.Load<List<long>>("UnitDataKeyList", Nickname);
            foreach (long key in dataKeyList) {
                UnitSaveFormat unitSaveFormat = ES3.Load<UnitSaveFormat>(key.ToString(), Nickname);
                Unit.Data data = new Unit.Data().From(unitSaveFormat);
                this.AddUnitData(data);
            }
            return true;
        }
        return false;
    }

    // 클리어 노드 추가
    private void SetDefaultClearNodes() {
        ClearNodes = new HashSet<NodeName>();
        ClearNodes.Add(NodeName.Village);
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        ClearNodes.Add(NodeName.Seolhwa01);
        ClearNodes.Add(NodeName.Seolhwa02);
        ClearNodes.Add(NodeName.Seolhwa03);
        ClearNodes.Add(NodeName.Seolhwa04);
        ClearNodes.Add(NodeName.Seolhwa05);
#endif
        SaveClearNodes();
    }
    public void AddClearNode(NodeName node) {
        ClearNodes.Add(node);
        SaveClearNodes();
    }
    public void SaveClearNodes() {
        ES3.Save<HashSet<NodeName>>("ClearNodes", ClearNodes, Nickname);
    }
    private bool LoadClearNodes() {
        if (ES3.KeyExists("ClearNodes", Nickname)) {
            ClearNodes = ES3.Load<HashSet<NodeName>>("ClearNodes", Nickname);
            return true;
        }
        return false;
    }

    // 소울 파편
    private void SetDefaultSoulFragment() {
        SoulFragmentList = new List<SoulFragment>();

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        this.AddSoulFragment(new SoulFragment(UnitType.Garuda, 10));
#endif
    }
    public void AddSoulFragment(SoulFragment soulFragment) {
        SoulFragmentList.Add(soulFragment);

        // 새로운 정보일 경우 바로 저장한다.
        if (soulFragment.SaveKey == -1) {
            soulFragment.Save();
            SaveSoulFragmentKeyList();
        }
    }
    public void RemoveSoulFragment(SoulFragment soulFragment) {
        soulFragment.DeleteSaveData();
        SoulFragmentList.Remove(soulFragment);
        SaveSoulFragmentKeyList();
    }
    public void SaveSoulFragmentKeyList() {
        List<long> keyList = SoulFragmentList.Select(data => data.SaveKey).ToList();
        ES3.Save<List<long>>("SoulFragmentKeyList", keyList, Nickname);
    }
    private bool LoadSoulFragmentList() {
        if (ES3.KeyExists("SoulFragmentKeyList", Nickname)) {
            SoulFragmentList = new List<SoulFragment>();
            List<long> dataKeyList = ES3.Load<List<long>>("SoulFragmentKeyList", Nickname);
            foreach (long key in dataKeyList) {
                SoulFragmentSaveFormat unitSaveFormat = ES3.Load<SoulFragmentSaveFormat>(key.ToString(), Nickname);
                SoulFragment data = new SoulFragment().From(unitSaveFormat);
                this.AddSoulFragment(data);
            }
            return true;
        }
        return false;
    }

    // 소울 가루
    public int SoulDust {
        get { return soulDust; }
        set {
            soulDust = value;
            SaveSoulDust();
        }
    }
    private void SaveSoulDust() {
        ES3.Save("SoulDust", SoulDust, Nickname);
    }
    private void LoadSouldust() {
        SoulDust = ES3.Load<int>("SoulDust", Nickname, 0);
    }

    // 파티 순서 정보
    private void SetDefaultPartySequence() {
        PartySequence = new Unit.Data[PartySequenceMax];
        SavePartySequence();
    }
    public void SavePartySequence() {
        List<long> keyList = PartySequence.Select(data => data?.SaveKey ?? -1).ToList();
        ES3.Save<List<long>>("PartySequence", keyList, Nickname);
    }
    private bool LoadPartySequence() {
        if (ES3.KeyExists("PartySequence", Nickname)) {
            PartySequence = new Unit.Data[PartySequenceMax];
            List<long> keyList = ES3.Load<List<long>>("PartySequence", Nickname);
            for(int i = 0; i < PartySequenceMax; ++i) {
                long key = keyList[i];
                if (key == -1)
                    continue;
                PartySequence[i] = UnitDataList.Find((unitData) => unitData.SaveKey == key);
            }
            return true;
        }
        return false;
    }

    // 목표 던전 정보
    public NodeName TargetDungeon {
        get { return targetDungeon; }
        set {
            targetDungeon = value;
            SaveTargetDungeon();
        }
    }
    public void SaveTargetDungeon() {
        ES3.Save<NodeName>("TargetDungeon", TargetDungeon, Nickname);
    }
    private bool LoadTargetDungeon() {
        if (ES3.KeyExists("TargetDungeon", Nickname)) {
            targetDungeon = ES3.Load<NodeName>("TargetDungeon", Nickname);
            return true;
        }
        return false;
    }
    private void SetDefaultTargetDungeon() {
        TargetDungeon = NodeName.None;
    }

}
