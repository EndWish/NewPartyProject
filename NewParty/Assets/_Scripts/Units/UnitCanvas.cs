using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitCanvas : MonoBehaviour
{
    Unit unit;
    [SerializeField] protected RectTransform actionGaugeFill;
    [SerializeField] protected RectTransform hpGaugeFill;
    [SerializeField] protected RectTransform hpGaugeBgFill;
    [SerializeField] protected RectTransform barrierGaugeFill;

    [SerializeField] protected Image flagBorderImage;
    [SerializeField] protected Image profileImage;
    public Image ProfileImage { get { return profileImage; } }

    [SerializeField] protected TextMeshProUGUI growthLevelText;

    private void Awake() {
        unit = transform.parent.GetComponent<Unit>();
        ProfileImage.sprite = unit.StaticData.ProfileSprite;
    }

    private void Update() {
        // 게이지
        hpGaugeFill.localScale = new Vector3(unit.Hp / unit.GetFinalStat(StatType.Hpm), 1, 1);
        hpGaugeBgFill.localScale = new Vector3(MathF.Max(hpGaugeBgFill.localScale.x - Time.deltaTime * 2, hpGaugeFill.localScale.x), 1, 1);
        actionGaugeFill.localScale = new Vector3(unit.ActionGauge / Unit.MaxActionGauge, 1, 1);
        float sumBarrierAmount = 0;
        UsefulMethod.ActionAll(unit.Barriers, (barrier) => { sumBarrierAmount += barrier.Amount; });
        barrierGaugeFill.localScale = new Vector3(Mathf.Min(1, sumBarrierAmount / unit.GetFinalStat(StatType.Hpm)), 1, 1);

        // 깃발 테두기 색상
        if (unit.IsClicked()) { flagBorderImage.color = new Color(1, 1, 0); } else if (unit.IsOverOnMouse()) { flagBorderImage.color = new Color(1, 0.7f, 0); } else { flagBorderImage.color = new Color(1, 1, 1); }

        // 성장 레벨 텍스트
        growthLevelText.text = GrowthLevelToStr(unit.GrowthLevel);
    }

    public string GrowthLevelToStr(int level) {
        return 0 <= level ? ("+" + level.ToString()) : level.ToString();
    }

}
