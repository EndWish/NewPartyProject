using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SoulSlot : PageViewSlot<SoulFragment>
{
    [SerializeField] protected Image profileImage;
    [SerializeField] protected TextMeshProUGUI numText;

    public override void SlotUpdate(SoulFragment soulFragment, int index) {
        base.SlotUpdate(soulFragment, index);

        Debug.Log("SlotUpdate");

        if(Data == null) {
            gameObject.SetActive(false);
            return;
        } else {
            gameObject.SetActive(true);
        }

        // ���� �̹��� ����
        profileImage.sprite = Data.Target.profileImage.sprite;
        profileImage.color = Color.white;

        // ���� ���� �ؽ�Ʈ
        numText.text = Data?.GetNum().ToString();

    }




}
