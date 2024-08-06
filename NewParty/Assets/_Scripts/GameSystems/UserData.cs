using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class UserData : MonoBehaviourSingleton<UserData>
{

    // ���� ���� //////////////////////////////////////////////////////////////
    static public int PartySequenceMax = 4;

    // ���� ���� //////////////////////////////////////////////////////////////
    public string Nickname { get; private set; }

    
    
    public HashSet<NodeName> ClearNodes { get; private set; }   // ���� ����

    private NodeName targetDungeon = NodeName.None;

    public List<Unit.Data> UnitDataList { get; private set; }   // ���� ����

    public Unit.Data[] PartySequence { get; private set; }      // ���� ����

    private int soulDust = 0;
    public List<SoulFragment> SoulFragmentList { get; private set; }    // Data�� ��ȯ�ؼ� ����

    

    // ����Ƽ �Լ� ////////////////////////////////////////////////////////////

    protected override void Awake() {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    // �Լ� ///////////////////////////////////////////////////////////////////

    public void SetNewPlayerData(string nickname) {
        // �г���
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
    
    // ���� ���� �Լ�
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

        // ���ο� ������ ��� �ٷ� �����Ѵ�.
        if(unitData.SaveKey == -1) {
            unitData.Save();
            SaveUnitDataKeyList();
        }
    }
    public void RemoveUnitData(Unit.Data unitData) {
        unitData.DeleteSaveData();
        UnitDataList.Remove(unitData);
        SaveUnitDataKeyList();

        // ��Ƽ���Կ� ������ ������� �����ϰ� �����Ѵ�.
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

    // Ŭ���� ��� �߰�
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

    // �ҿ� ����
    private void SetDefaultSoulFragment() {
        SoulFragmentList = new List<SoulFragment>();

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        this.AddSoulFragment(new SoulFragment(UnitType.Garuda, 10));
#endif
    }
    public void AddSoulFragment(SoulFragment soulFragment) {
        SoulFragmentList.Add(soulFragment);

        // ���ο� ������ ��� �ٷ� �����Ѵ�.
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

    // �ҿ� ����
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

    // ��Ƽ ���� ����
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

    // ��ǥ ���� ����
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
