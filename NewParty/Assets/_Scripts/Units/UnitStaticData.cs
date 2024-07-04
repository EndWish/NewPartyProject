using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Unit Static Data", menuName = "Scriptable Object/Unit Static Data")]
public class UnitStaticData : ScriptableObject
{
    [Header("유닛의 주요 정보")]
    public Sprite ProfileSprite;
    public string Name;
    public UnitType Type;

    [SerializeField, EnumNamedArrayAttribute(typeof(StatType))]
    public float[] InitStats = new float[(int)StatType.Num];

    public List<Tag> InitTags;

    [Header("유닛의 추가 정보")]
    public int SoulFragmentValueAsDust;


}
