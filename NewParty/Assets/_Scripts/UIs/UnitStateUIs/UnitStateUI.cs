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

        // ���� �����̻� �����ܵ� ������Ʈ
        unitSEStateUI.UpdatePage(targetUnit);

        // ���� ��ų �����ܵ� ������Ʈ
        unitSkillStateUI.UpdatePage(targetUnit);

        // ������ ������Ʈ
        profileImg.sprite = targetUnit?.GetMainSprite1x2();
        growthLevelText.text = targetUnit == null ? "" : TooltipText.SetPositivePrefix(targetUnit.GrowthLevel);

        // �ɷ�ġ �ؽ�Ʈ ǥ��
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

            // ���� ó��
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

            // �ϰ� ó��
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

        // Ű���� �Է�
        if ((Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.S))
            && (UnitOfTurn?.IsMine() ?? false) && battleManager.ActionCoroutine == null) {
            StartCoroutine(UnitOfTurn.GetComponent<TokenSelector>().AutoSelect());
        }

    }

}
