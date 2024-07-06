using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class RestSkill : MonoBehaviour
{
    [SerializeField] protected GameObject selectUI;
    private Party userParty = null;

    protected void Start() {
        selectUI?.SetActive(false);
    }

    protected Party UserParty {
        get { return userParty; }
        set {
            userParty = value;
            selectUI.SetActive(userParty != null);
        }
    }

    public void OnClick() {
        if (BattleSelectable.IsRunning) {
            if (UserParty == null) {
                BattleSelectable.StopSelectMode();  // StopSelectMode 를 하면 targetParty가 null 이 되기 때문에 {} 안에서 실행
                RunSelectMode();
            } else {
                BattleSelectable.StopSelectMode();
            }
        } else {
            RunSelectMode();
        }
    }

    protected virtual void RunSelectMode() {
        UserParty = GetFirstParty();
    }
    protected virtual void OnCompleteSelection() {
        --UserParty.RemainRestSkill;
        UserParty = null;
    }
    protected virtual void OnCancel() {
        UserParty = null;
    }

    protected Party GetFirstParty() {
        return BattleManager.Instance.Parties[(int)TeamType.Player][0];
    }

}
