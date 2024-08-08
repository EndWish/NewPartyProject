using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusEffectIcon2 : MonoBehaviour
{
    [SerializeField] protected Image iconImg;
    [SerializeField] protected Image bgImg;
    [SerializeField] protected TextMeshProUGUI rightLowerText;
    [SerializeField] protected TextMeshProUGUI rightUpperText;

    private IStatusEffectIconable target;

    public Image IconImg { get { return iconImg; } }
    public Image BgImg { get { return bgImg; } }
    public TextMeshProUGUI RightLowerText { get { return rightLowerText; } }
    public TextMeshProUGUI RightUpperText { get { return rightUpperText; } }

    public void UpdateIcon(IStatusEffectIconable target) {
        this.target = target;

        IconImg.sprite = target?.GetIcon1x1();
        BgImg.color = target == null ? Color.black : target.GetBgColor();
        RightLowerText.text = (target is IRightLowerTextableIcon) ? ((IRightLowerTextableIcon)target).GetRightLowerText() : null;
        RightUpperText.text = (target is IRightUpperTextableIcon) ? ((IRightUpperTextableIcon)target).GetRightUpperText() : null;
    }

    public void OnPointerEnter() {
        if(target != null) {
            Tooltip tooltip = Tooltip.Instance;
            tooltip.IconImg.sprite = IconImg.sprite;
            tooltip.TitleText.text = target.GetTooltipTitleText();
            tooltip.RightUpperText.text = target.GetTooltipRightUpperText();
            tooltip.DescriptionText.text = target.GetDescriptionText();

            tooltip.transform.position = Input.mousePosition;
            tooltip.gameObject.SetActive(true);

            for (int i = 0; i < 2; ++i) {
                LayoutRebuilder.ForceRebuildLayoutImmediate(tooltip.GetComponent<RectTransform>());
            }
        }
    }
    public void OnPointerExit() {
        Tooltip.Instance.gameObject.SetActive(false);
    }

}
