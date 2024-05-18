using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using TMPro;

public enum UnitType : int
{
    None, Garuda,
}

public enum StatType
{

    //체력, 속도, 공격력, 스택 공격력, 방어 관통력, 치명타 확률, 치명타 배율, 방어력, 실드, 스택 실드, 명중, 회피, 치유력, 토큰 지분(3종류)
    Hpm, Speed, Str, StackStr, SkillStr, DefPen, CriCha, CriMul, Def, Shield, StackShield, Acc, Avoid, Healing,
    AtkTokenShare, SkillTokenShare, ShiledTokenShare,

    Num
}

public enum StatForm
{
    Base, Final, EquipmentAdd, EquipmentMul, AbnormalAdd, AbnormalMul,
    
    Num
}

public class Unit : MonoBehaviourPun
{
    // 서브 클래스 ////////////////////////////////////////////////////////////
    [Serializable]
    public class Data {
        public UnitType Type = UnitType.None;
        public int GrowthLevel { get; set; } = 0;

        public Data() { }
        public Data(UnitType type) {
            Type = type;
        }
        public Data(UnitType type, int growthLevel) : this(type) {
            GrowthLevel = growthLevel;
        }

        // Photon 직렬화
        public static byte[] Serialize(object customObject) {
            Data data = (Data)customObject;

            // 스트림에 필요한 메모리 사이즈(Byte)
            MemoryStream ms = new MemoryStream(sizeof(UnitType) + sizeof(int));
            // 각 변수들을 Byte 형식으로 변환, 마지막은 개별 사이즈
            ms.Write(BitConverter.GetBytes((int)data.Type), 0, sizeof(UnitType));
            ms.Write(BitConverter.GetBytes(data.GrowthLevel), 0, sizeof(int));

            // 만들어진 스트림을 배열 형식으로 반환
            return ms.ToArray();
        }

        // Photon 역직렬화
        public static object Deserialize(byte[] bytes) {
            Data data = new Data();
            // 바이트 배열을 필요한 만큼 자르고, 원하는 자료형으로 변환
            data.Type = (UnitType)BitConverter.ToInt32(bytes, 0);
            data.GrowthLevel = BitConverter.ToInt32(bytes, sizeof(UnitType));
            return data;
        }

    }

    // 연결 정보 //////////////////////////////////////////////////////////////
    [SerializeField] protected Token tokenPrefab;
    [SerializeField] protected Transform tokensParent;
    [SerializeField] protected TextMeshProUGUI growthLevelText;
    public SpriteRenderer profileRenderer;

    // 개인 정보 //////////////////////////////////////////////////////////////
    // 이름
    public string Name;

    // 저장에 필요한 변수
    public Data MyData;

    // 능력치 관련 변수
    [SerializeField, EnumNamedArrayAttribute(typeof(StatType))] protected float[] initStats = new float[(int)StatType.Num];
    protected float[,] stats = new float[(int)StatForm.Num, (int)StatType.Num];
    public UnityAction<Unit>[] OnUpdateFinalStat = new UnityAction<Unit>[(int)StatType.Num];

    public float Hp { get; set; }
    public UnityAction<Unit, Ref<float>> OnRecoverHp;   // 유닛, 회복량
    public UnityAction<Unit, float, float> OnRecoveredHp;   // 유닛, 회복량, 초과량

    // 토큰 관련 변수
    protected int maxTokens = 5;
    protected List<Token> tokens = new List<Token>();
    public UnityAction<Unit, Token> OnCreateToken;
    public UnityAction<Unit, Token> OnRemoveToken;

    // 파티 관련 변수
    public TeamType TeamType { get; set; } = TeamType.None;
    public Party MyParty { get; set; }

    // 스킬 관련 변수

    // 장비 관련 변수

    // 상태이상 관련 변수

    // 배틀페이지와 관련한 이벤트 변수
    public UnityAction<Unit> OnBeginMyTurn, OnEndMyTurn;

    // 유니티 함수 ////////////////////////////////////////////////////////////
    protected void Awake() {
        growthLevelText.text = GetGrowthLevelStr();
        UpdateAllStat();

        Hp = GetFinalStat(StatType.Hpm);
    }

    protected void Update() {
        if (TeamType != TeamType.None) {
            Vector3 pos = Vector3.zero;

            int sign = 0;
            if (TeamType == TeamType.Player) sign = -1;
            else if (TeamType == TeamType.Enemy) sign = 1;

            int index = GetIndex();
            pos += new Vector3(1.5f * sign * index, 0, 0); // 갈수록 작아지도록 하고 싶다

            transform.localPosition = Vector3.MoveTowards(transform.localPosition, pos, 10f * Time.deltaTime);
        }
    }

    // 함수 ///////////////////////////////////////////////////////////////////
    // 프로퍼티
    public int GrowthLevel {
        get { return MyData.GrowthLevel; }
        set { 
            MyData.GrowthLevel = value;
            growthLevelText.text = GetGrowthLevelStr();
            UpdateBaseStat(true);
        }
    }

    // 능력치 관련 함수
    public float GetFinalStat(StatType type) {
        return stats[(int)StatForm.Final, (int)type];
    }

    public void UpdateFinalStat(StatType type) {
        int index = (int)type;
        stats[(int)StatForm.Final, index] =
            (stats[(int)StatForm.Base, index]
            + stats[(int)StatForm.EquipmentAdd, index]
            + stats[(int)StatForm.AbnormalAdd, index])
            * stats[(int)StatForm.EquipmentMul, index]
            * stats[(int)StatForm.AbnormalMul, index];

        OnUpdateFinalStat[index]?.Invoke(this);
    }
    public void UpdateFinalStat() {
        for(StatType type = 0; type < StatType.Num; ++type) {
            UpdateFinalStat(type);
        }
    }

    public void UpdateBaseStat(bool updateFinalStat) {
        // 기본 능력치 계산
        for (StatType type = 0; type < StatType.Num; ++type) {
            stats[(int)StatForm.Base, (int)type] = initStats[(int)type] * (1f + 0.01f * GrowthLevel);
        }

        // 최종 능력치에 적용
        if (updateFinalStat)
            UpdateFinalStat();
    }
    public void UpdateEquipmentStat(bool updateFinalStat) {
        // 능력치 초기화
        for (StatType type = 0; type < StatType.Num; ++type) {
            stats[(int)StatForm.EquipmentAdd, (int)type] = 0;
            stats[(int)StatForm.EquipmentMul, (int)type] = 1f;
        }

        // [추가] 장비의 능력치들 적용

        // 최종 능력치에 적용
        if (updateFinalStat)
            UpdateFinalStat();
    }
    public void UpdateAbnormalStat(bool updateFinalStat) {
        // 능력치 초기화
        for (StatType type = 0; type < StatType.Num; ++type) {
            stats[(int)StatForm.AbnormalAdd, (int)type] = 0;
            stats[(int)StatForm.AbnormalMul, (int)type] = 1f;
        }

        // [추가] 상태이상의 능력치들 적용

        // 최종 능력치에 적용
        if (updateFinalStat)
            UpdateFinalStat();
    }

    public void UpdateAllStat() {
        UpdateBaseStat(false);
        UpdateEquipmentStat(false);
        UpdateAbnormalStat(false);
        UpdateFinalStat();
    }

    // 토큰 관련 함수
    [PunRPC]
    protected void CreateTokenRPC(TokenType type) {
        Token newToken = Instantiate(tokenPrefab, tokensParent);
        tokens.Add(newToken);
        newToken.Owner = this;
        newToken.Type = type;
    }
    public void CreateRandomToken() {
        if (tokens.Count >= maxTokens)  // 최대개수를 넘어서 얻을 수 없다.
            return;

        float sum = GetFinalStat(StatType.AtkTokenShare) + GetFinalStat(StatType.SkillTokenShare) + GetFinalStat(StatType.ShiledTokenShare);
        float random = UnityEngine.Random.Range(0f, sum);

        TokenType type = TokenType.None;
        if (random <= GetFinalStat(StatType.AtkTokenShare))
            type = TokenType.Atk;
        else if(random <= GetFinalStat(StatType.AtkTokenShare) + GetFinalStat(StatType.SkillTokenShare))
            type = TokenType.Skill;
        else 
            type = TokenType.Shield;

        photonView.RPC("CreateTokenRPC", RpcTarget.All, type);

        OnCreateToken?.Invoke(this, tokens[tokens.Count - 1]);
    }
    public void CreateRandomToken(int num) {
        for(int i = 0; i < num; ++i) {
            CreateRandomToken();
        }
    }
    public void CreateToken(TokenType type) {
        if (tokens.Count >= maxTokens)  // 최대개수를 넘어서 얻을 수 없다.
            return;

        photonView.RPC("CreateTokenRPC", RpcTarget.All, type);

        OnCreateToken?.Invoke(this, tokens[tokens.Count - 1]);
    }
    public void CreateToken(TokenType type, int num) {
        for (int i = 0; i < num; ++i) {
            CreateToken(type);
        }
    }

    [PunRPC]
    protected void RemoveTokenRPC(int index) {
        Token token = tokens[index];
        tokens.RemoveAt(index);
        Destroy(token.gameObject);
    }
    public void RemoveToken(Token token) {
        OnRemoveToken?.Invoke(this, token);

        int index = tokens.IndexOf(token);
        photonView.RPC("RemoveTokenRPC", RpcTarget.All, index);
    }
    public void RemoveSelectedToken() {
        List<Token> selectedTokens = tokens.FindAll(token => token.IsSelected);
        foreach (Token token in selectedTokens)
            RemoveToken(token);
    }

    // Data 관련 함수
    [PunRPC]
    protected void ApplyDataRPC(Unit.Data data) {
        this.MyData = data;
        UpdateAllStat();
        growthLevelText.text = GetGrowthLevelStr();
        // 프로필 이미지도 적용
    }
    public void ApplyData(Unit.Data data) {
        ApplyDataRPC(data);
        if (photonView.IsMine) {    // 동기화 하지 않은 오브젝트일 경우 실행하지 않는다.
            photonView.RPC("ApplyDataRPC", RpcTarget.Others, data);
        }
    }

    // 체력 회복
    [PunRPC]
    private void RecoverHpRPC(float mount) {
        Hp += mount;
    }
    public void RecoverHp(float baseAmount) {
        Ref<float> amount = new Ref<float>(baseAmount);

        // 회복할때 (회복수치를 조정가능)
        OnRecoverHp?.Invoke(this, amount);

        // 회복 초과량 확인하기
        float overAmount = 0;
        float lostHp = GetFinalStat(StatType.Hpm) - Hp;
        if(lostHp < amount.Value) {
            overAmount = amount.Value - lostHp;
            amount.Value = lostHp;
        }

        // Hp에 적용
        photonView.RPC("RecoverHpRPC", RpcTarget.All, amount.Value);

        // 회복되었을 때 
        OnRecoveredHp?.Invoke(this, amount.Value, overAmount);
    }

    // 파티 관련 함수
    public void OnSetTeam(TeamType type) {
        TeamType = type;
        if(TeamType == TeamType.Player) {
            profileRenderer.flipX = false;
        } else if(TeamType == TeamType.Enemy) {
            profileRenderer.flipX = true;
        }
    }
    public int GetIndex() {
        return MyParty.Units.IndexOf(this);
    }

    // 기타
    protected string GetGrowthLevelStr() {
        return 0 <= GrowthLevel ? ("+" + GrowthLevel.ToString()) : GrowthLevel.ToString();
    }

}
