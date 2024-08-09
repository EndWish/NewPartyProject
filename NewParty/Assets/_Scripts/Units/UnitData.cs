using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public partial class Unit : MonoBehaviourPun
{
    // 서브 클래스 ////////////////////////////////////////////////////////////
    [Serializable]
    public class Data : SaveData, IMainSprite1x1, IMainSprite1x2
    {
        // 포톤 직렬화/역직렬화
        static public byte[] PhotonSerialize(object customObject) {
            Data data = (Data)customObject;

            // 스트림에 필요한 메모리 사이즈(Byte)
            MemoryStream ms = new MemoryStream(sizeof(UnitType) + sizeof(int));
            // 각 변수들을 Byte 형식으로 변환, 마지막은 개별 사이즈
            ms.Write(BitConverter.GetBytes((int)data.Type), 0, sizeof(UnitType));
            ms.Write(BitConverter.GetBytes(data.GrowthLevel), 0, sizeof(int));

            // 만들어진 스트림을 배열 형식으로 반환
            return ms.ToArray();
        }
        static public object PhotonDeserialize(byte[] bytes) {
            Data data = new Data();
            // 바이트 배열을 필요한 만큼 자르고, 원하는 자료형으로 변환
            data.Type = (UnitType)BitConverter.ToInt32(bytes, 0);
            data.GrowthLevel = BitConverter.ToInt32(bytes, sizeof(UnitType));
            return data;
        }

        // 개인 정보 //////////////////////////////////////////////////////////////
        [SerializeField] protected UnitSharedData sharedData;
        [SerializeField] protected int growthLevel;
        
        // 생성자 ///////////////////////////////////////////////////////////////////
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

        // 함수 ///////////////////////////////////////////////////////////////////
        // 프로퍼티
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

        // 저장/불러오기 관련 함수
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

        // IIcon 함수
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