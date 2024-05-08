using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserData : MonoBehaviourSingleton<UserData>
{
    public string Nickname { get; private set; }

    protected override void Awake() {
        base.Awake();
        DontDestroyOnLoad(this);
    }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
    private void Update() {
        if (Input.GetKeyDown(KeyCode.F1)) {
            Save();
            OnlyDebug.Log("����Ǿ����ϴ�.");
        }

    }
#endif

    public void SetNewPlayerData(string nickname) {
        Nickname = nickname;
        Save();
    }
    public bool Load(string nickname) {
        if (!ES3.FileExists(nickname))
            return false;

        // �г��� �ҷ�����
        Nickname = ES3.Load<string>("Nickname", nickname);

        return true;
    }
    public void Save() {
        // �г��� �����ϱ�
        ES3.Save<string>("Nickname", Nickname, Nickname);
    }


}
