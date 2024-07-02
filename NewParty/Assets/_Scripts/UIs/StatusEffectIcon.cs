using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusEffectIcon : MonoBehaviour
{
    [SerializeField] protected Image iconImg;
    [SerializeField] protected Image bgImg;
    [SerializeField] protected TextMeshProUGUI rightLowerText;
    public Func<string> GetTooltipDescription;
    public Func<string> GetTooltipRightUpperText;
    public Func<string> GetTooltipTitle;

    public Image IconImg { get { return iconImg; } }
    public Image BgImg { get { return bgImg; } }
    public TextMeshProUGUI RightLowerText { get { return rightLowerText; } }

    public void OnPointerEnter() {
        Tooltip tooltip = Tooltip.Instance;
        tooltip.IconImg.sprite = iconImg.sprite;
        tooltip.TitleText.text = GetTooltipTitle?.Invoke();
        tooltip.RightUpperText.text = GetTooltipRightUpperText?.Invoke();
        tooltip.DescriptionText.text = GetTooltipDescription?.Invoke();

        tooltip.transform.position = Input.mousePosition;
        tooltip.gameObject.SetActive(true);

        for (int i = 0; i < 2; ++i) {
            LayoutRebuilder.ForceRebuildLayoutImmediate(tooltip.GetComponent<RectTransform>());
        }
        
    }
    public void OnPointerExit() {
        Tooltip.Instance.gameObject.SetActive(false);
    }

}
