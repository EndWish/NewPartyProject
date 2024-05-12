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

public enum UnitType : int
{
    None, Garuda,
}

public enum StatType
{

    //ü��, �ӵ�, ���ݷ�, ���� ���ݷ�, ��� �����, ġ��Ÿ Ȯ��, ġ��Ÿ ����, ����, �ǵ�, ���� �ǵ�, ����, ȸ��, ġ����, ��ū ����(3����)
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
    // ���� Ŭ���� ////////////////////////////////////////////////////////////
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

        // Photon ����ȭ
        public static byte[] Serialize(object customObject) {
            Data data = (Data)customObject;

            // ��Ʈ���� �ʿ��� �޸� ������(Byte)
            MemoryStream ms = new MemoryStream(sizeof(UnitType) + sizeof(int));
            // �� �������� Byte �������� ��ȯ, �������� ���� ������
            ms.Write(BitConverter.GetBytes((int)data.Type), 0, sizeof(UnitType));
            ms.Write(BitConverter.GetBytes(data.GrowthLevel), 0, sizeof(int));

            // ������� ��Ʈ���� �迭 �������� ��ȯ
            return ms.ToArray();
        }

        // Photon ������ȭ
        public static object Deserialize(byte[] bytes) {
            Data data = new Data();
            // ����Ʈ �迭�� �ʿ��� ��ŭ �ڸ���, ���ϴ� �ڷ������� ��ȯ
            data.Type = (UnitType)BitConverter.ToInt32(bytes, 0);
            data.GrowthLevel = BitConverter.ToInt32(bytes, sizeof(UnitType));
            return data;
        }

    }

    // ���� ���� //////////////////////////////////////////////////////////////
    [SerializeField] protected Token tokenPrefab;
    [SerializeField] protected Transform tokensParent;
    public SpriteRenderer profileRenderer;

    // ���� ���� //////////////////////////////////////////////////////////////
    // �̸�
    public string Name;

    // ���忡 �ʿ��� ����
    public Data MyData;

    // �ɷ�ġ ���� ����
    [SerializeField, EnumNamedArrayAttribute(typeof(StatType))] protected float[] initStats = new float[(int)StatType.Num];
    protected float[,] stats = new float[(int)StatForm.Num, (int)StatType.Num];
    public UnityAction<Unit>[] OnUpdateFinalStat = new UnityAction<Unit>[(int)StatType.Num];
    public float Hp { get; set; }

    // ��ū ���� ����
    protected int maxTokens = 5;
    protected List<Token> tokens = new List<Token>();
    public UnityAction<Unit, Token> OnCreateToken;
    public UnityAction<Unit, Token> OnRemoveToken;

    // ��ų ���� ����

    // ��� ���� ����

    // �����̻� ���� ����

    // ��Ʋ�������� ������ �̺�Ʈ ����
    public UnityAction<Unit> OnBeginMyTurn, OnEndMyTurn;

    // ����Ƽ �Լ� ////////////////////////////////////////////////////////////
    protected void Awake() {
        UpdateAllStat();

        Hp = GetFinalStat(StatType.Hpm);
    }

    // �Լ� ///////////////////////////////////////////////////////////////////
    // ������Ƽ
    public int GrowthLevel {
        get { return MyData.GrowthLevel; }
        set { 
            MyData.GrowthLevel = value;
            UpdateBaseStat(true);
        }
    }

    // �ɷ�ġ ���� �Լ�
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
        // �⺻ �ɷ�ġ ���
        for (StatType type = 0; type < StatType.Num; ++type) {
            stats[(int)StatForm.Base, (int)type] = initStats[(int)type] * (1f + 0.01f * GrowthLevel);
        }

        // ���� �ɷ�ġ�� ����
        if (updateFinalStat)
            UpdateFinalStat();
    }
    public void UpdateEquipmentStat(bool updateFinalStat) {
        // �ɷ�ġ �ʱ�ȭ
        for (StatType type = 0; type < StatType.Num; ++type) {
            stats[(int)StatForm.EquipmentAdd, (int)type] = 0;
            stats[(int)StatForm.EquipmentMul, (int)type] = 1f;
        }

        // [�߰�] ����� �ɷ�ġ�� ����

        // ���� �ɷ�ġ�� ����
        if (updateFinalStat)
            UpdateFinalStat();
    }
    public void UpdateAbnormalStat(bool updateFinalStat) {
        // �ɷ�ġ �ʱ�ȭ
        for (StatType type = 0; type < StatType.Num; ++type) {
            stats[(int)StatForm.AbnormalAdd, (int)type] = 0;
            stats[(int)StatForm.AbnormalMul, (int)type] = 1f;
        }

        // [�߰�] �����̻��� �ɷ�ġ�� ����

        // ���� �ɷ�ġ�� ����
        if (updateFinalStat)
            UpdateFinalStat();
    }

    public void UpdateAllStat() {
        UpdateBaseStat(false);
        UpdateEquipmentStat(false);
        UpdateAbnormalStat(false);
        UpdateFinalStat();
    }

    // ��ū ���� �Լ�
    [PunRPC]
    protected void CreateTokenRPC(TokenType type) {
        Token newToken = Instantiate(tokenPrefab, tokensParent);
        tokens.Add(newToken);
        newToken.Owner = this;
        newToken.Type = type;
    }
    public void CreateRandomToken() {
        if (tokens.Count >= maxTokens)  // �ִ밳���� �Ѿ ���� �� ����.
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
        if (tokens.Count >= maxTokens)  // �ִ밳���� �Ѿ ���� �� ����.
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

    [PunRPC]
    protected void ApplyDataRPC(Unit.Data data) {
        this.MyData = data;
        UpdateAllStat();
    }
    public void ApplyData(Unit.Data data) {
        this.MyData = data;
        UpdateAllStat();
        if (photonView.IsMine) {    // ����ȭ ���� ���� ������Ʈ�� ���
            photonView.RPC("ApplyDataRPC", RpcTarget.Others, data);
        }
    }

}