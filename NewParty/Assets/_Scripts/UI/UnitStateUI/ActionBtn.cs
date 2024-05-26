using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class ActionBtn : MonoBehaviour
{
    [SerializeField] protected Image bg;
    protected Color activedBgColor;
    protected Color disactivedBgColor;

    protected Unit targetUnit;

    private bool active = true;

    protected virtual void Awake() {
        activedBgColor = bg.color;
        disactivedBgColor = new Color(0.2f, 0.2f, 0.2f);
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

    public virtual void OnActive() {
        bg.color = activedBgColor;
    }
    public virtual void OnDisactive() {
        bg.color = disactivedBgColor;
    }

    public abstract void OnClick();

}
