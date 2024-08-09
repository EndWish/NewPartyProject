using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SoulSlot : PageViewSlot<SoulFragment>
{
    [SerializeField] protected MultiLayerImageController Images;
    [SerializeField] protected TextMeshProUGUI numText;

    public override void SlotUpdate(SoulFragment soulFragment, int index) {
        base.SlotUpdate(soulFragment, index);

        // 유닛 이미지 적용
        Images.Sprites = Data?.GetMainSprites1x1() ?? new List<Sprite> { SoulFragment.NullIcon1x1 };

        // 텍스트 적용
        numText.text = Data?.Num.ToString();

    }

}
