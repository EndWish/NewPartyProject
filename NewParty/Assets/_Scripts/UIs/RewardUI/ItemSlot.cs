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

        // ���� �̹��� ����
        Images.Sprites = Data.GetMainSprites1x1();

        // �ؽ�Ʈ ����
        numText.text = Data?.Num.ToString();
        nameText.text = Data?.Name;

    }

}