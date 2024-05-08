using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviourSingleton<T>, new() {
    static T _instance = null;

    public static T Instance { get { return _instance; } }

    protected virtual void Awake() {
        if (_instance == null)
            _instance = this as T;

        if (_instance != this)
            Destroy(this.gameObject);
    }

}
