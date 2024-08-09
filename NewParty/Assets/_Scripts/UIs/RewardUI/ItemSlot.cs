using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : PageViewSlot<IItem>
{

    [SerializeField] protected MultiLayerImageController Images;
    [SerializeField] protected TextMeshProUGUI numText;
    [SerializeField] protected TextMeshProUGUI nameText;

    public override void SlotUpdate(IItem item, int index) {
        base.SlotUpdate(item, index);

        // 유닛 이미지 적용
        Images.Sprites = Data.GetMainSprites1x1();

        // 텍스트 적용
        numText.text = Data?.Num.ToString();
        nameText.text = Data?.Name;

    }

}