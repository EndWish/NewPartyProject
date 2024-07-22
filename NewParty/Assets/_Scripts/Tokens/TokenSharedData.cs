using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TokenSharedData", menuName = "Scriptable Object/Token Shared Data")]
public class TokenSharedData : ScriptableObject
{
    [SerializeField, EnumNamedArray(typeof(TokenType))] private List<Sprite> tokenIconSprites;
    [SerializeField, EnumNamedArray(typeof(TokenType))] private List<Sprite> tokenBackgroundSprites;

    public Sprite GetTokenIconSprite(TokenType type) {
        return tokenIconSprites[(int)type];
    }
    public Sprite GetTokenBackgroundSprites(TokenType type) {
        return tokenBackgroundSprites[(int)type];
    }

}
