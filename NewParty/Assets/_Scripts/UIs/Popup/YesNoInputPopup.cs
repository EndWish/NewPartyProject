using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class YesNoInputPopup : YesNoPopup
{
    [SerializeField] protected TMP_InputField inputField;

    public void AddActionToInputField(UnityAction<string> action) {
        inputField.onValueChanged.AddListener(action);
    }

    public void SetContentType(TMP_InputField.ContentType type) {
        inputField.contentType = type;
    }
    public UnityAction<string> GetInputIntegerLimitAction(int min, int max) {
        return (str) => {
            int num;
            if (int.TryParse(str, out num)) {
                
                if (num < min)
                    inputField.text = min.ToString();
                else if(max < num)
                    inputField.text = max.ToString();
            }
        };
    }
    public string GetInputFieldText() {
        return inputField.text;
    }
    public void SetInputFieldText(string text) {
        inputField.text = text;
    }
}
