using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static System.Runtime.CompilerServices.RuntimeHelpers;

public class UnitStateUI : MonoBehaviour
{
    [SerializeField] private Image profileImg;
    [SerializeField] private TextMeshProUGUI growthLevelText;

    [SerializeField, EnumNamedArrayAttribute(typeof(StatType))] 
    private TextMeshProUGUI[] statTexts;
    [SerializeField] private TextMeshProUGUI barrierText;

    [SerializeField] UnitStatusEffectStateUI unitSEStateUI;
    [SerializeField] UnitSkillStateUI unitSkillStateUI;

    private void Update() {
        BattleManager battleManager = BattleManager.Instance;
        Unit UnitOfTurn = battleManager.UnitOfTurn;
        Unit targetUnit = battleManager.UnitOnMouse;
        targetUnit ??= battleManager.UnitClicked;
        targetUnit ??= battleManager.UnitOfTurn;

        // 유닛 상태이상 아이콘들 업데이트
        unitSEStateUI.UpdatePage(targetUnit);

        // 유닛 스킬 아이콘들 업데이트
        unitSkillStateUI.UpdatePage(targetUnit);

        // 프로필 업데이트
        profileImg.sprite = targetUnit?.GetMainSprite1x2();
        growthLevelText.text = targetUnit == null ? "" : TooltipText.SetPositivePrefix(targetUnit.GrowthLevel);

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
        barrierText.text = targetUnit == null ? "-" : "(" + TooltipText.GetFlexibleFloat(targetUnit.GetBarriersAmount()) + ")";

        // 키보드 입력
        if ((Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.S))
            && (UnitOfTurn?.IsMine() ?? false) && battleManager.ActionCoroutine == null) {
            StartCoroutine(UnitOfTurn.GetComponent<TokenSelector>().AutoSelect());
        }

    }

}
