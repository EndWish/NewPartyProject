using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveSwitch : MonoBehaviour
{
    [Serializable]
    protected struct GameObjectAndKey
    {
        public GameObject GameObject;
        public KeyCode KeyCode;
    }

    [SerializeField] protected List<GameObjectAndKey> GameObjectAndKeyList = new List<GameObjectAndKey>();

    private void Update() {
        foreach (var gameObjectAndKey in GameObjectAndKeyList) {
            if (Input.GetKeyUp(gameObjectAndKey.KeyCode)) {
                gameObjectAndKey.GameObject.SetActive(!gameObjectAndKey.GameObject.activeSelf);
            }
        }
    }

}
