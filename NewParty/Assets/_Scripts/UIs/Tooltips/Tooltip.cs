using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviourSingleton<Tooltip>
{
    [SerializeField] private Image iconImg;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI rightUpperText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    private RectTransform rectTransform;

    private ITooltipable target;

    protected override void Awake() {
        base.Awake();
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start() {
        gameObject.SetActive(false);
    }

    private void Update() {
        if(target != null && target is IDetailedDescription) {
            IDetailedDescription detailedDescription = (IDetailedDescription)target;

            if (Input.GetKeyDown(KeyCode.LeftShift)) {
                DescriptionText.text = detailedDescription.GetDetailedDescriptionText();
            }
            if (Input.GetKeyUp(KeyCode.LeftShift)) {
                DescriptionText.text = target.GetDescriptionText() + "\n\n[L Shift] 자세히 보기";
            }

            for (int i = 0; i < 2; ++i) {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            }
        }
    }

    private void OnDisable() {
        target = null;
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

    public void UpdatePage(ITooltipable tooltipable) {
        target = tooltipable;

        IconImg.sprite = tooltipable.GetMainSprite1x1();
        TitleText.text = tooltipable.GetTooltipTitleText();
        RightUpperText.text = tooltipable.GetTooltipRightUpperText();

        var detailedDescription = tooltipable as IDetailedDescription;

        if(detailedDescription == null) {
            DescriptionText.text = tooltipable.GetDescriptionText();
        }
        else {
            DescriptionText.text = tooltipable.GetDescriptionText() + "\n\n[L Shift] 자세히 보기";
        }

        for (int i = 0; i < 2; ++i) {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }
    }

}
