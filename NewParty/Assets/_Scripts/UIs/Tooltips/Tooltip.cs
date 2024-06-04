using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviourSingleton<Tooltip>
{
    [SerializeField] private Image iconImg;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI rightUpperText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    private RectTransform rectTransform;

    protected override void Awake() {
        base.Awake();
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start() {
        gameObject.SetActive(false);
    }

    private void OnEnable() {
        if(Input.mousePosition.x <= Screen.width / 2f)
            rectTransform.pivot = new Vector2(0,0);
        else
            rectTransform.pivot = new Vector2(1, 0);
        transform.position = Input.mousePosition;

        float top = rectTransform.position.y + rectTransform.rect.height;
        if (Screen.height < top) {
            Vector3 positon = rectTransform.position;
            positon = new Vector3(positon.x, positon.y - (top - Screen.height), positon.z);
            rectTransform.position = positon;
        }

    }

    public Image IconImg { get { return iconImg; } }
    public TextMeshProUGUI TitleText { get { return titleText; } }
    public TextMeshProUGUI RightUpperText { get { return rightUpperText; } }
    public TextMeshProUGUI DescriptionText { get { return descriptionText; } }
}
