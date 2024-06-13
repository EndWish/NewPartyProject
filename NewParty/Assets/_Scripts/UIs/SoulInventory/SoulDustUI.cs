using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SoulDustUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI numText;

    private void Update() {
        UserData userData = UserData.Instance;
        if (userData != null) {
            numText.text = userData.SoulDust.ToString();
        }
    }
}
