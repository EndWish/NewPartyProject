using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Sprites Shared Data", menuName = "Scriptable Object/Sprites Shared Data")]
public class SpritesSharedData : ScriptableObject
{
    public List<Sprite> Sprites;
}
