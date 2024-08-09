using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[CreateAssetMenu(fileName = "Unit Shared Data", menuName = "Scriptable Object/Unit Shared Data")]
public class UnitSharedData : ScriptableObject, IMainSprite1x1, IMainSprite1x2
{
    static public UnitSharedData GetAsset(UnitType unitType) {
        StringBuilder sb = new StringBuilder("UnitSharedData/").Append(unitType).Append("SharedData");
        return Resources.Load<UnitSharedData>(sb.ToString());
    }

    [Header("������ �ֿ� ����")]
    public Sprite ProfileSprite;
    public Sprite IconSprite;
    public string Name;
    public UnitType Type;

    public float SpeciesMul;
    [SerializeField, EnumNamedArrayAttribute(typeof(StatType))]
    public float[] InitStats = new float[(int)StatType.Num];

    public List<Tag> InitTags;

    [Header("���� ��� ����")]
    public int SoulFragmentValueAsDust;

    // �Լ� ///////////////////////////////////////////////////////////////////
    public Sprite GetMainSprite1x1() {
        return IconSprite;
    }

    public List<Sprite> GetMainSprites1x1() {
        return new List<Sprite> { GetMainSprite1x1() };
    }

    public Sprite GetMainSprite1x2() {
        return ProfileSprite;
    }

    public List<Sprite> GetMainSprites1x2() {
        return new List<Sprite> { GetMainSprite1x2() };
    }
}
