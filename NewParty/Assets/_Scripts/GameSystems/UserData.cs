using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserData : MonoBehaviourSingleton<UserData>
{
    public string Nickname { get; private set; }
    
    public HashSet<NodeName> ClearNodes { get; set; }

    protected override void Awake() {
        base.Awake();
        DontDestroyOnLoad(this);
    }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
    private void Update() {
        if (Input.GetKeyDown(KeyCode.F1)) {
            Save();
            OnlyDebug.Log("저장되었습니다.");
        }

    }
#endif

    public void SetNewPlayerData(string nickname) {
        Nickname = nickname;
        ClearNodes = GetInitCloearNodes();

        Save();
    }
    public bool Load(string nickname) {
        if (!ES3.FileExists(nickname))
            return false;

        Nickname = ES3.Load<string>("Nickname", nickname);

        HashSet<NodeName> cloearNoes = new HashSet<NodeName>();
        ClearNodes = ES3.Load<HashSet<NodeName>>("ClearNodes", nickname, GetInitCloearNodes());

        return true;
    }
    public void Save() {
        ES3.Save<string>("Nickname", Nickname, Nickname);
        ES3.Save<HashSet<NodeName>>("ClearNodes", ClearNodes, Nickname);
    }

    private HashSet<NodeName> GetInitCloearNodes() {
        HashSet<NodeName> clearNodes = new HashSet<NodeName>();
        clearNodes.Add(NodeName.Village);
        return clearNodes;
    }

}
