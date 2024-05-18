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
                BattleSelectable.StopSelectMode();  // StopSelectMode �� �ϸ� targetParty�� null �� �Ǳ� ������ {} �ȿ��� ����
                RunSelectMode();
                Debug.Log("�ٸ� ���� ��带 ���� ���� �Ҵ�");
            } else {
                BattleSelectable.StopSelectMode();
                Debug.Log("���� ����");
            }
        } else {
            RunSelectMode();
            Debug.Log("���� �Ҵ�");
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
