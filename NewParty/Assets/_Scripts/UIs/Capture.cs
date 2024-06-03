using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Capture : MonoBehaviour
{
    // ���� Ŭ���� ////////////////////////////////////////////////////////////
    [Serializable]
    protected class KeyMappingEvent {
        public KeyCode keyCode;
        public UnityEvent OnKeyDown;
    }

    // ���� ���� //////////////////////////////////////////////////////////////
    static private List<Capture> captures = new List<Capture>();
    static private Capture CapturedInstance {
        get { return 0 < captures.Count ? captures[captures.Count - 1] : null; }
    }
    static private bool captureLock = false;

    // ���� ���� //////////////////////////////////////////////////////////////
    [SerializeField, ArrayElementTitle("keyCode")] 
    private List<KeyMappingEvent> keyMappingEvents = new List<KeyMappingEvent>();
    public bool IsFrontWhenCaptured = false;

    // ����Ƽ �Լ� ////////////////////////////////////////////////////////////
    private void OnEnable() {
        GetCapture();
    }

    private void OnDisable() {
        LostCapture();
    }

    private void OnDestroy() {
        LostCapture();
    }

    private void Update() {
        if (IsCaptured() && !captureLock) {
            foreach (var keyMappingEvent in keyMappingEvents) {
                if (Input.GetKeyUp(keyMappingEvent.keyCode)) {
                    keyMappingEvent.OnKeyDown.Invoke();
                }
            }
            captureLock = true;
        }
    }

    private void LateUpdate() {
        if (IsCaptured())
            captureLock = false;
    }

    // ���� ///////////////////////////////////////////////////////////////////
    public void GetCapture() {
        captures.Remove(this);
        captures.Add(this);
        if(IsFrontWhenCaptured)
            transform.SetAsLastSibling();
    }
    public void LostCapture() {
        bool wasCaptured = false;
        if (IsCaptured()) wasCaptured = true;

        captures.Remove(this);

        if(wasCaptured && CapturedInstance != null) {
            if (CapturedInstance.IsFrontWhenCaptured)
                CapturedInstance.transform.SetAsLastSibling();
        }
    }
    public bool IsCaptured() {
        return CapturedInstance == this;
    }

}
