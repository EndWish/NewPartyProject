using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public partial class Unit : MonoBehaviourPun
{
    // ���� Ŭ���� ////////////////////////////////////////////////////////////
    [Serializable]
    public class Data : SaveData, IMainSprite1x1, IMainSprite1x2
    {
        // ���� ����ȭ/������ȭ
        static public byte[] PhotonSerialize(object customObject) {
            Data data = (Data)customObject;

            // ��Ʈ���� �ʿ��� �޸� ������(Byte)
            MemoryStream ms = new MemoryStream(sizeof(UnitType) + sizeof(int));
            // �� �������� Byte �������� ��ȯ, �������� ���� ������
            ms.Write(BitConverter.GetBytes((int)data.Type), 0, sizeof(UnitType));
            ms.Write(BitConverter.GetBytes(data.GrowthLevel), 0, sizeof(int));

            // ������� ��Ʈ���� �迭 �������� ��ȯ
            return ms.ToArray();
        }
        static public object PhotonDeserialize(byte[] bytes) {
            Data data = new Data();
            // ����Ʈ �迭�� �ʿ��� ��ŭ �ڸ���, ���ϴ� �ڷ������� ��ȯ
            data.Type = (UnitType)BitConverter.ToInt32(bytes, 0);
            data.GrowthLevel = BitConverter.ToInt32(bytes, sizeof(UnitType));
            return data;
        }

        // ���� ���� //////////////////////////////////////////////////////////////
        [SerializeField] protected UnitSharedData sharedData;
        [SerializeField] protected int growthLevel;
        
        // ������ ///////////////////////////////////////////////////////////////////
        public Data() {
            sharedData = null;
            growthLevel = 0;
        }
        public Data(UnitType unitType) : this() {
            Type = unitType;
        }
        public Data(UnitType unitType, int growthLevel) : this(unitType) {
            GrowthLevel = growthLevel;
        }

        // �Լ� ///////////////////////////////////////////////////////////////////
        // ������Ƽ
        public UnitSharedData SharedData {
            get { return sharedData; }
        }
        public UnitType Type {
            get { return sharedData?.Type ?? UnitType.None; }
            set {
                sharedData = (value == UnitType.None) ? null : UnitSharedData.GetAsset(value);
                SaveSynced();
            }
        }
        public int GrowthLevel {
            get { return growthLevel; }
            set {
                growthLevel = value;
                SaveSynced();
            }
        }
        public string Name {
            get { return SharedData?.Name ?? ""; }
        }

        // ����/�ҷ����� ���� �Լ�
        protected UnitSaveFormat ToSaveFormat() {
            UnitSaveFormat saveFormat = new UnitSaveFormat();
            saveFormat.SaveKey = SaveKey;
            saveFormat.Type = Type;
            saveFormat.GrowthLevel = GrowthLevel;
            return saveFormat;
        }
        public Data From(UnitSaveFormat saveFormat) {
            SaveKey = saveFormat.SaveKey;
            Type = saveFormat.Type;
            GrowthLevel = saveFormat.GrowthLevel;
            return this;
        }
        public override void Save() {
            base.Save();
            ES3.Save<UnitSaveFormat>(SaveKey.ToString(), this.ToSaveFormat(),  UserData.Instance.Nickname);
        }

        // IIcon �Լ�
        public Sprite GetMainSprite1x1() {
            return SharedData?.GetMainSprite1x1() ?? Unit.NullIcon1x1;
        }
        public List<Sprite> GetMainSprites1x1() {
            return new List<Sprite> { GetMainSprite1x1() };
        }

        public Sprite GetMainSprite1x2() {
            return SharedData?.GetMainSprite1x2() ?? Unit.NullIcon1x2;
        }
        public List<Sprite> GetMainSprites1x2() {
            return new List<Sprite> { GetMainSprite1x2() };
        }

    }
}

public struct UnitSaveFormat
{
    public long SaveKey;
    public UnitType Type;
    public int GrowthLevel;
}