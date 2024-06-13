using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class YesNoPopup : MonoBehaviour
{
    [SerializeField] protected Button yesBtn;
    [SerializeField] protected TextMeshProUGUI yesText;
    [SerializeField] protected Button noBtn;
    [SerializeField] protected TextMeshProUGUI noText;
    [SerializeField] protected TextMeshProUGUI questionText;

    public void AddActionToYes(UnityAction action) {
        yesBtn.onClick.AddListener(action);
    }
    public void AddActionToNo(UnityAction action) {
        noBtn.onClick.AddListener(action);
    }
    public void SetYesText(string text) {
        yesText.text = text;
    }
    public void SetNoText(string text) {
        noText.text = text;
    }
    public void SetQuestionText(string text) {
        questionText.text = text;
    }

    public UnityAction GetCloseAction() {
        return () => {
            Destroy(this.gameObject);
        };
    }

}
