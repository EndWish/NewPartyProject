using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CountDownUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private Image fillImage;

    private float count;
    private bool isRunning;

    private void Update() {
        if (isRunning) {
            count -= Time.deltaTime;
            if (count <= 0) {
                isRunning = false;
            }
            else {
                countText.text = Mathf.CeilToInt(count).ToString();
                fillImage.fillAmount = 1f - (count - Mathf.Floor(count));
            }
        }
    }

    public void StartCount(float count) {
        this.count = count;
        isRunning = true;
    }
    public void Pause() {
        isRunning = false;
    }
    public void Resume() {
        isRunning = true;
    }

}
