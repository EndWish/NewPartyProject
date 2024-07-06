using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveData
{
    static private long saveKeyCount = -1;
    public long SaveKey { get; set; } = -1;
    
    public void SaveSynced() {
        if (SaveKey != -1)
            Save();
    }
    public virtual void Save() {
        if(SaveKey == -1) {
            if(saveKeyCount == -1) {
                saveKeyCount = ES3.Load<long>("SaveKeyCount", UserData.Instance.Nickname, 0);
            }

            SaveKey = ++saveKeyCount;
            ES3.Save<long>("SaveKeyCount", saveKeyCount, UserData.Instance.Nickname);
        }
    }
    public void DeleteSaveData() {
        ES3.DeleteKey(SaveKey.ToString());
    }

}