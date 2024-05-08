using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoBehaviourPunCallbacksSingleton<T> : MonoBehaviourPunCallbacks where T : MonoBehaviourPunCallbacksSingleton<T>, new() {
    static T _instance = null;

    public static T Instance { get { return _instance; } }

    virtual protected void Awake(){
        if (_instance == null)
            _instance = this as T;

        if (_instance != this)
            Destroy(this.gameObject);
    }

}
