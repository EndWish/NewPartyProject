using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class ActionBtn : MonoBehaviour, ITooltipable
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

    protected bool Active {
        get { return active; }
        set {
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

    public void OnPointerEnter() {
        tooltip = Tooltip.Instance;
        tooltip.transform.position = Input.mousePosition;
        tooltip.gameObject.SetActive(true);
        tooltip.UpdatePage(this);
    }
    public void OnPointerExit() {
        Tooltip.Instance.gameObject.SetActive(false);
        tooltip = null;
    }

    // ITooltipable
    public abstract string GetTooltipTitleText();
    public abstract string GetTooltipRightUpperText();
    public abstract string GetDescriptionText();
    public Sprite GetMainSprite1x1() {
        return iconImg.sprite;
    }
    public List<Sprite> GetMainSprites1x1() {
        return new List<Sprite> { GetMainSprite1x1() };
    }
}
