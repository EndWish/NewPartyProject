using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class EscCapture : MonoBehaviour
{
    static EscCapture capture = null;

    public UnityEvent OnEsc;

    private void OnEnable() {
        capture = this;
    }

    private void Update() {
        if (Input.GetKeyUp(KeyCode.Escape) && this == capture) {
            OnEsc.Invoke();
        }
    }

    public void GetCapture() {
        capture = this;
    }
}
