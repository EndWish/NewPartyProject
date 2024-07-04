using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Unit Static Data", menuName = "Scriptable Object/Unit Static Data")]
public class UnitStaticData : ScriptableObject
{
    [Header("������ �ֿ� ����")]
    public Sprite ProfileSprite;
    public string Name;
    public UnitType Type;

    [SerializeField, EnumNamedArrayAttribute(typeof(StatType))]
    public float[] InitStats = new float[(int)StatType.Num];

    public List<Tag> InitTags;

    [Header("������ �߰� ����")]
    public int SoulFragmentValueAsDust;


}
