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
    // 서브 클래스 ////////////////////////////////////////////////////////////
    [Serializable]
    protected class KeyMappingEvent {
        public KeyCode keyCode;
        public UnityEvent OnKeyDown;
    }

    // 공유 변수 //////////////////////////////////////////////////////////////
    static private List<Capture> captures = new List<Capture>();
    static private Capture CapturedInstance {
        get { return 0 < captures.Count ? captures[captures.Count - 1] : null; }
    }
    static private bool captureLock = false;

    // 개인 변수 //////////////////////////////////////////////////////////////
    [SerializeField, ArrayElementTitle("keyCode")] 
    private List<KeyMappingEvent> keyMappingEvents = new List<KeyMappingEvent>();
    public bool IsFrontWhenCaptured = false;

    // 유니티 함수 ////////////////////////////////////////////////////////////
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

    // 변수 ///////////////////////////////////////////////////////////////////
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
