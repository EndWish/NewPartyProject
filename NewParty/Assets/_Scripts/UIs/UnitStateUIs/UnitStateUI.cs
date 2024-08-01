using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static System.Runtime.CompilerServices.RuntimeHelpers;

public class UnitStateUI : MonoBehaviour
{
    [SerializeField] private List<FixedActionBtn> fixedActionBtns;

    [SerializeField] private Transform skillActionsParent;
    private List<SkillActionBtn> skillActionBtns = new List<SkillActionBtn>();
    private int skillActionBtnOffset = 0;
    private int skillActionBtnMaxOffset = 0;

    [SerializeField] private GameObject leftArrow, rightArrow;

    [SerializeField] private Image profileImg;
    [SerializeField] private TextMeshProUGUI growthLevelText;

    [SerializeField, EnumNamedArrayAttribute(typeof(StatType))] 
    private TextMeshProUGUI[] statTexts;

    private void Awake() {
        skillActionBtns.InsertRange(0, skillActionsParent.GetComponentsInChildren<SkillActionBtn>());
    }

    private void Update() {
        BattleManager battleManager = BattleManager.Instance;

        Unit targetUnit = battleManager.UnitOnMouse;
        targetUnit ??= battleManager.UnitClicked;
        targetUnit ??= battleManager.UnitOfTurn;

        // 액션 버튼 업데이트
        Unit UnitOfTurn = battleManager.UnitOfTurn;
        foreach (var actionBtn in fixedActionBtns) {
            actionBtn.UpdateBtn(UnitOfTurn);
        }

        skillActionBtnMaxOffset = Mathf.Max(0, (UnitOfTurn?.Skills.Count ?? 0) - skillActionBtns.Count);
        skillActionBtnOffset = Mathf.Clamp(skillActionBtnOffset, 0, skillActionBtnMaxOffset);
        for(int btnIndex = 0; btnIndex < skillActionBtns.Count; ++btnIndex) {
            int skillIndex = skillActionBtnOffset + btnIndex;
            Skill skill = (skillIndex < UnitOfTurn?.Skills.Count) ? UnitOfTurn.Skills[skillIndex] : null;
            skillActionBtns[btnIndex].UpdateBtn(UnitOfTurn, skill);
        }

        leftArrow.SetActive(skillActionBtnOffset != 0);
        rightArrow.SetActive(skillActionBtnOffset != skillActionBtnMaxOffset);

        // 프로필 업데이트
        profileImg.sprite = targetUnit?.ProfileImage.sprite;
        growthLevelText.text = targetUnit == null ? "" : GrowthLevelToStr(targetUnit.GrowthLevel);

        // 능력치 텍스트 표시
        float[] tokenWeight = new float[3] { targetUnit?.GetFinalStat(StatType.AtkTokenWeight) ?? 0,
            targetUnit?.GetFinalStat(StatType.SkillTokenWeight) ?? 0,
            targetUnit ?.GetFinalStat(StatType.ShieldTokenWeight) ?? 0 };
        float sumTokenWeight = tokenWeight[0] + tokenWeight[1] + tokenWeight[2];

        for (StatType statType = 0; statType < StatType.Num; ++statType) {
            int index = (int)statType;
            if (statTexts[index] == null)
                continue;

            if (targetUnit == null) {
                statTexts[index].text = "-";
                continue;
            }

            // 예외 처리
            switch (statType) {
                case StatType.Hpm:
                    statTexts[index].text = TooltipText.GetFlexibleFloat(targetUnit.Hp) + "/"
                        + TooltipText.GetFlexibleFloat(targetUnit.GetFinalStat(statType));
                    continue;
                case StatType.AtkTokenWeight:
                case StatType.SkillTokenWeight:
                case StatType.ShieldTokenWeight:
                    statTexts[index].text = string.Format("{0}% ({1})",
                        TooltipText.GetFlexibleFloat(targetUnit.GetFinalStat(statType) / sumTokenWeight * 100f),
                        TooltipText.GetFlexibleFloat(targetUnit.GetFinalStat(statType))); 
                    continue;
            }

            // 일괄 처리
            switch (StatFeatures.GetOperation(statType)) {
                case StatOperation.Figure:
                    statTexts[index].text = TooltipText.GetFlexibleFloat(targetUnit.GetFinalStat(statType));
                    break;
                case StatOperation.Mul:
                    statTexts[index].text = "x" + TooltipText.GetFlexibleFloat(targetUnit.GetFinalStat(statType));
                    break;
                case StatOperation.PercentPoint:
                    statTexts[index].text = TooltipText.GetFlexibleFloat(targetUnit.GetFinalStat(statType) * 100f) + "%<size=20>p</size>";
                    break;
                case StatOperation.Percent:
                    statTexts[index].text = TooltipText.GetFlexibleFloat(targetUnit.GetFinalStat(statType) * 100f) + "%";
                    break;
            }
        }

        // 키보드 입력
        List<KeyCode> keyCodes = new List<KeyCode> { KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.T, KeyCode.Y, KeyCode.U };
        for(int i = 0; i < keyCodes.Count; i++) {
            KeyCode keyCode = keyCodes[i];
            if (Input.GetKeyUp(keyCode)) {
                if(i < fixedActionBtns.Count)
                    fixedActionBtns[i].OnClick();
                else
                    skillActionBtns[i - fixedActionBtns.Count].OnClick();
            }
        }

        if ((Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.S))
            && (UnitOfTurn?.IsMine() ?? false) && battleManager.ActionCoroutine == null) {
            StartCoroutine(UnitOfTurn.GetComponent<TokenSelector>().AutoSelect());
        }

    }

    private string FloatToNormalStr(float value) {
        if (100 <= value)
            return string.Format("{0:G}", value);
        return string.Format("{0:F2}", value);
    }
    private string FloatToPercentStr(float value) {
        return string.Format("{0:F1}%", value * 100f);
    }

    public void RaiseSkillActionBtnOffset() {
        skillActionBtnOffset = Mathf.Min(skillActionBtnOffset + 1, skillActionBtnMaxOffset);
    }
    public void DownSkillActionBtnOffset() {
        skillActionBtnOffset = Mathf.Max(skillActionBtnOffset - 1, 0);
    }
    public void OnScrollSkillActionBtns() {
        Vector2 wheelInput = Input.mouseScrollDelta;
        if (wheelInput.y > 0) {    // 휠 위로
            DownSkillActionBtnOffset();
        } else if (wheelInput.y < 0) {   // 휠 아래로
            RaiseSkillActionBtnOffset();
        }
    }

    public string GrowthLevelToStr(int level) {
        return 0 <= level ? ("+" + level.ToString()) : level.ToString();
    }

}
