using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillIcon : MonoBehaviour
{
    [SerializeField] protected Image iconImg;
    [SerializeField] protected Image bgImg;
    [SerializeField] protected TextMeshProUGUI rightLowerText;
    [SerializeField] protected TextMeshProUGUI rightUpperText;

    private Skill target;

    public Image IconImg { get { return iconImg; } }
    public Image BgImg { get { return bgImg; } }
    public TextMeshProUGUI RightLowerText { get { return rightLowerText; } }
    public TextMeshProUGUI RightUpperText { get { return rightUpperText; } }

    public void UpdateIcon(Skill target) {
        this.target = target;

        IconImg.sprite = target?.GetMainSprite1x1();
        BgImg.color = target is PassiveSkill ? new Color(0.9f, 0f, 0.9f) : new Color(0.2f, 0.2f, 0.2f);
        RightLowerText.text = (target as IIconRightLowerTextable)?.GetIconRightLowerText() ?? null;
        RightUpperText.text = (target as IIconRightUpperTextable)?.GetIconRightUpperText() ?? null;
    }

    public void OnPointerEnter() {
        if (target != null) {
            Tooltip tooltip = Tooltip.Instance;
            tooltip.transform.position = Input.mousePosition;
            tooltip.gameObject.SetActive(true);
            tooltip.UpdatePage(target);
        }
    }
    public void OnPointerExit() {
        Tooltip.Instance.gameObject.SetActive(false);
    }

}
