using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class ActionBtn : MonoBehaviour
{
    [SerializeField] protected Image iconImg;
    [SerializeField] protected Image bgImg;
    protected Color activedBgColor;
    protected Color disactivedBgColor;

    protected Tooltip tooltip;

    protected Unit targetUnit;

    [SerializeField] protected Image outlineImg;
    private Unit actionUnit;

    private bool active = true;

    protected virtual void Awake() {
        activedBgColor = bgImg.color;
        disactivedBgColor = new Color(0.2f, 0.2f, 0.2f);
    }
    protected void Update() {
        if(Input.GetKeyDown(KeyCode.LeftShift) && tooltip != null) {
            tooltip.DescriptionText.text = GetTooltipDetailedDescription();

            for (int i = 0; i < 2; ++i) {
                LayoutRebuilder.ForceRebuildLayoutImmediate(tooltip.GetComponent<RectTransform>());
            }
        }
        if (Input.GetKeyUp(KeyCode.LeftShift) && tooltip != null) {
            tooltip.DescriptionText.text = GetTooltipDescription() + "\n\n[L Shift] 자세히 보기";

            for (int i = 0; i < 2; ++i) {
                LayoutRebuilder.ForceRebuildLayoutImmediate(tooltip.GetComponent<RectTransform>());
            }
        }
    }

    protected bool Active {
        get { return active; }
        set {
            if (active == value)
                return;

            active = value;
            if (active)
                OnActive();
            else
                OnDisactive();
        }
    }
    protected Unit ActionUnit {
        get { return actionUnit; }
        set { 
            actionUnit = value;
            if (actionUnit == null)
                outlineImg.color = new Color(1, 1, 1);
            else
                outlineImg.color = new Color(1, 1, 0);
        }
    }

    protected bool MeetActiveBasicCondition() {
        return targetUnit != null && targetUnit.HasTurn() && targetUnit.IsMine();
    }
    public virtual void OnActive() {
        bgImg.color = activedBgColor;
    }
    public virtual void OnDisactive() {
        bgImg.color = disactivedBgColor;
    }

    protected virtual bool MeetClickCondition() {
        BattleManager battleManager = BattleManager.Instance;
        return Active && targetUnit != null && targetUnit.IsMine() && targetUnit == battleManager.UnitOfTurn && battleManager.ActionCoroutine == null;
    }
    public abstract void OnClick();

    protected virtual void OnCompleteSelection() {
        ActionUnit = null;
    }
    protected virtual void OnCancel() {
        ActionUnit = null;
    }

    protected abstract string GetTooltipTitle();
    protected abstract string GetTooltipRightUpperText();
    protected abstract string GetTooltipDescription();
    protected abstract string GetTooltipDetailedDescription();

    public void OnPointerEnter() {
        tooltip = Tooltip.Instance;
        tooltip.IconImg.sprite = iconImg.sprite;
        tooltip.TitleText.text = GetTooltipTitle();
        tooltip.RightUpperText.text = GetTooltipRightUpperText();
        tooltip.DescriptionText.text = GetTooltipDescription() + "\n\n[L Shift] 자세히 보기";

        tooltip.transform.position = Input.mousePosition;
        tooltip.gameObject.SetActive(true);

        for (int i = 0; i < 2; ++i) {
            LayoutRebuilder.ForceRebuildLayoutImmediate(tooltip.GetComponent<RectTransform>());
        }

    }
    public void OnPointerExit() {
        Tooltip.Instance.gameObject.SetActive(false);
        tooltip = null;
    }

}
