using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[CreateAssetMenu(fileName = "Unit Shared Data", menuName = "Scriptable Object/Unit Shared Data")]
public class UnitSharedData : ScriptableObject
{
    static public UnitSharedData GetAsset(UnitType unitType) {
        StringBuilder sb = new StringBuilder("UnitSharedData/").Append(unitType).Append("SharedData");
        return Resources.Load<UnitSharedData>(sb.ToString());
    }

    [Header("유닛의 주요 정보")]
    public Sprite ProfileSprite;
    public string Name;
    public UnitType Type;

    public float SpeciesMul;
    [SerializeField, EnumNamedArrayAttribute(typeof(StatType))]
    public float[] InitStats = new float[(int)StatType.Num];

    public List<Tag> InitTags;

    [Header("유닛의 추가 정보")]
    public int SoulFragmentValueAsDust;


}
