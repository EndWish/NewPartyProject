using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private TextMeshProUGUI HpmText, SpeedText, StrText, StackStrText, DefPenText, SkillStrText;
    [SerializeField] private TextMeshProUGUI DefText, ShieldText, StackShieldText, CriChaText, CriMulText, HealingText;
    [SerializeField] private TextMeshProUGUI AccText, AvoidText, AtkTokenWeightText, SkillTokenWeightText, ShiledTokenWeightText;

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
        profileImg.sprite = targetUnit?.profileImage.sprite;
        growthLevelText.text = targetUnit?.GetGrowthLevelStr();

        // 능력치 텍스트 표시
        HpmText.text = targetUnit == null ? "-" : FloatToNormalStr(targetUnit.Hp) + "/" + FloatToNormalStr(targetUnit.GetFinalStat(StatType.Hpm));

        float speed = targetUnit?.GetFinalStat(StatType.Speed) ?? 0;
        SpeedText.text = targetUnit == null ? "-" : FloatToNormalStr(speed) + string.Format(" ({0:F2}s)", (Unit.MaxActionGauge - targetUnit.ActionGauge) / speed);

        StrText.text = targetUnit == null ? "-" : FloatToNormalStr(targetUnit.GetFinalStat(StatType.Str));
        StackStrText.text = targetUnit == null ? "-" : FloatToPercentStr(targetUnit.GetFinalStat(StatType.StackStr));
        DefPenText.text = targetUnit == null ? "-" : FloatToNormalStr(targetUnit.GetFinalStat(StatType.DefPen));
        SkillStrText.text = targetUnit == null ? "-" : "x" + FloatToNormalStr(targetUnit.GetFinalStat(StatType.SkillStr));

        DefText.text = targetUnit == null ? "-" : FloatToNormalStr(targetUnit.GetFinalStat(StatType.Def));
        ShieldText.text = targetUnit == null ? "-" : FloatToNormalStr(targetUnit.GetFinalStat(StatType.Shield));
        StackShieldText.text = targetUnit == null ? "-" : FloatToPercentStr(targetUnit.GetFinalStat(StatType.StackShield));
        CriChaText.text = targetUnit == null ? "-" : FloatToPercentStr(targetUnit.GetFinalStat(StatType.CriCha));
        CriMulText.text = targetUnit == null ? "-" : "x" + FloatToNormalStr(targetUnit.GetFinalStat(StatType.CriMul));
        HealingText.text = targetUnit == null ? "-" : "x" + FloatToNormalStr(targetUnit.GetFinalStat(StatType.Healing));

        AccText.text = targetUnit == null ? "-" : FloatToNormalStr(targetUnit.GetFinalStat(StatType.Acc));
        AvoidText.text = targetUnit == null ? "-" : FloatToNormalStr(targetUnit.GetFinalStat(StatType.Avoid));

        float[] tokenWeight = new float[3] { targetUnit?.GetFinalStat(StatType.AtkTokenWeight) ?? 0, 
            targetUnit?.GetFinalStat(StatType.SkillTokenWeight) ?? 0, 
            targetUnit ?.GetFinalStat(StatType.ShieldTokenWeight) ?? 0 };
        float sumTokenWeight = tokenWeight[0] + tokenWeight[1] + tokenWeight[2];
        AtkTokenWeightText.text = targetUnit == null ? "-" : string.Format("{0:F1} ({1:F1}%)", tokenWeight[0], tokenWeight[0] / sumTokenWeight * 100f);
        SkillTokenWeightText.text = targetUnit == null ? "-" : string.Format("{0:F1} ({1:F1}%)", tokenWeight[1], tokenWeight[1] / sumTokenWeight * 100f);
        ShiledTokenWeightText.text = targetUnit == null ? "-" : string.Format("{0:F1} ({1:F1}%)", tokenWeight[2], tokenWeight[2] /sumTokenWeight * 100f);

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

}
