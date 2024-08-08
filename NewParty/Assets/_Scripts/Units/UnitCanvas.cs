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

    [SerializeField] protected Transform statusEffectIconParent;
    [SerializeField] protected StatusEffectIcon2 statusEffectIconPrefab;
    protected List<StatusEffectIcon2> statusEffectIconList = new List<StatusEffectIcon2>();

    private void Awake() {
        unit = transform.parent.GetComponent<Unit>();
        ProfileImage.sprite = unit.SharedData.ProfileSprite;
    }

    private void Update() {
        // 게이지
        hpGaugeFill.localScale = new Vector3(unit.Hp / unit.GetFinalStat(StatType.Hpm), 1, 1);
        hpGaugeBgFill.localScale = new Vector3(MathF.Max(hpGaugeBgFill.localScale.x - Time.deltaTime * 2, hpGaugeFill.localScale.x), 1, 1);
        actionGaugeFill.localScale = new Vector3(unit.ActionGauge / Unit.MaxActionGauge, 1, 1);
        barrierGaugeFill.localScale = new Vector3(Mathf.Min(1, unit.GetBarriersAmount() / unit.GetFinalStat(StatType.Hpm)), 1, 1);

        // 깃발 테두기 색상
        if (unit.IsClicked()) { flagBorderImage.color = new Color(1, 1, 0); } else if (unit.IsOverOnMouse()) { flagBorderImage.color = new Color(1, 0.7f, 0); } else { flagBorderImage.color = new Color(1, 1, 1); }

        // 성장 레벨 텍스트
        growthLevelText.text = GrowthLevelToStr(unit.GrowthLevel);

        // 상태이상 아이콘
        List<IStatusEffectIconable> seIconableList = GetVisibleSEIconables();
        while (statusEffectIconList.Count < seIconableList.Count) {
            statusEffectIconList.Add(Instantiate(statusEffectIconPrefab, statusEffectIconParent));
        }
        for(int i = 0; i < statusEffectIconList.Count; ++i) {
            if(i < seIconableList.Count) {
                statusEffectIconList[i].UpdateIcon(seIconableList[i]);
                statusEffectIconList[i].gameObject.SetActive(true);
            }
            else {
                statusEffectIconList[i].gameObject.SetActive(false);
            }
        }
    }

    public List<IStatusEffectIconable> GetVisibleSEIconables() {
        List<IStatusEffectIconable> seIconableList = new List<IStatusEffectIconable>();
        UsefulMethod.ActionAll(unit.StatusEffects, (statusEffect) => {
            if (statusEffect.IsSEVisible())
                seIconableList.Add(statusEffect);
        });
        UsefulMethod.ActionAll(unit.Skills, (skill) => {
            if (skill is IStatusEffectIconable && ((IStatusEffectIconable)skill).IsSEVisible())
                seIconableList.Add(skill as IStatusEffectIconable);
        });
        return seIconableList;
    }

    public string GrowthLevelToStr(int level) {
        return 0 <= level ? ("+" + level.ToString()) : level.ToString();
    }

} 
