using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoulTorchUI : MonoBehaviour
{
    [SerializeField] private SoulTorchSlot soulTorchSlot;
    [SerializeField] private UnitTorchSlot unitTorchSlot;

    [SerializeField, Header("영혼/유닛/영혼+유닛 순서")] 
    private GameObject[] btnLayers = new GameObject[3];

    [SerializeField] private YesNoPopup yesNoPopupPrefab;
    [SerializeField] private YesNoInputPopup yesNoInputPopupPrefab;

    private YesNoPopup yesNoPopup;

    private SoulFragment soulFragment;
    public SoulFragment SoulFragment {
        get { return soulFragment; }
        set {
            soulFragment = value;
            if( soulFragment != null && unitData != null 
                && soulFragment.Type != unitData.Type) {
                unitData = null;
            }
            BtnLayerUpdate();
        }
    }

    private Unit.Data unitData;
    public Unit.Data UnitData {
        get { return unitData; }
        set { 
            unitData = value;
            if (soulFragment != null && unitData != null
                && soulFragment.Type != unitData.Type) {
                soulFragment = null;
            }
            BtnLayerUpdate();
        }
    }

    private void Start() {
        BtnLayerUpdate();
    }

    private void Update() {
        soulTorchSlot.SlotUpdate(SoulFragment, 0);
        unitTorchSlot.SlotUpdate(UnitData, 0);
    }

    private void OnDestroy() {
        if (yesNoPopup != null)
            Destroy(yesNoPopup.gameObject);
    }

    private void BtnLayerUpdate() {
        if (yesNoPopup != null)
            Destroy(yesNoPopup.gameObject);

        bool hasUnit = UnitData != null;
        bool hasSoulFragment = SoulFragment != null;

        GameObject targetLayer = null;
        if (!hasUnit && hasSoulFragment) {
            targetLayer = btnLayers[0];
        } 
        else if (hasUnit && !hasSoulFragment) {
            targetLayer = btnLayers[1];
        } 
        else if (hasUnit && hasSoulFragment) {
            targetLayer = btnLayers[2];
        }

        foreach(var btnLayer in btnLayers) {
            if (btnLayer == targetLayer)
                btnLayer.SetActive(true);
            else
                btnLayer.SetActive(false);
        }

    }

    // Layer01 (영혼 슬롯만 올려놓은 경우)
    public void OnClickSummonsBtn() {
        if (yesNoPopup != null)
            return;

        if(Unit.NumSoulFragmentRequiredForSummon <= SoulFragment.Num) {
            // 비용 지불
            SoulFragment.Num -= Unit.NumSoulFragmentRequiredForSummon;
            if (SoulFragment.Num <= 0) {
                UserData.Instance.RemoveSoulFragment(SoulFragment);
                SoulFragment = null;
            }

            Unit.Data unitData = new Unit.Data(SoulFragment.Type, 0);
            UserData.Instance.AddUnitData(unitData);
        }
    }
    public void OnClickSoulDecompositionBtn() {
        if (yesNoPopup != null)
            return;

        Debug.Log("입력이 있는 팝업창을 띄운다");
        yesNoPopup = Instantiate(yesNoInputPopupPrefab, new Vector3(Screen.width / 2, Screen.height / 2), Quaternion.identity,
            this.transform.parent);

        YesNoInputPopup yesNoInputPopup = yesNoPopup as YesNoInputPopup;

        // 질문
        yesNoInputPopup.SetQuestionText(string.Format("해당 영혼 파편을 분해하시겠 습니까?\n\n{0:G}개 분해 시 {1:G} 개의 가루를 얻습니다.", 0, 0));

        // 입력 필드 설정
        yesNoInputPopup.SetContentType(TMPro.TMP_InputField.ContentType.IntegerNumber);
        yesNoInputPopup.AddActionToInputField((str) => {
            int num = 0;
            int dustAmount = 0;
            if (int.TryParse(str, out num)) {
                dustAmount = num * SoulFragment.UnitSharedData.SoulFragmentValueAsDust / 2;
            }
            yesNoInputPopup.SetQuestionText(string.Format("해당 영혼 파편을 분해하시겠 습니까?\n\n{0:G}개 분해 시 {1:G} 개의 가루를 얻습니다.", num, dustAmount));
        });
        yesNoInputPopup.AddActionToInputField(yesNoInputPopup.GetInputIntegerLimitAction(0, SoulFragment.Num));

        // Yes 버튼 설정
        yesNoInputPopup.SetYesText("분해하기");
        yesNoInputPopup.AddActionToYes(yesNoInputPopup.GetCloseAction());
        yesNoInputPopup.AddActionToYes(() => {
            if (SoulFragment == null)
                return;

            int num;
            if (int.TryParse(yesNoInputPopup.GetInputFieldText(), out num)) {
                if (num <= 0 || SoulFragment.Num < num)
                    return;

                UserData userData = UserData.Instance;

                int dustAmount = num * SoulFragment.UnitSharedData.SoulFragmentValueAsDust / 2;
                userData.SoulDust += dustAmount;

                SoulFragment.Num -= num;
                if (SoulFragment.Num <= 0) {
                    userData.RemoveSoulFragment(SoulFragment);
                    SoulFragment = null;
                }
            }


        });

        // No 버튼 설정
        yesNoInputPopup.SetNoText("취소");
        yesNoInputPopup.AddActionToNo(yesNoInputPopup.GetCloseAction());

        for (int i = 0; i < 2; ++i) {
            LayoutRebuilder.ForceRebuildLayoutImmediate(yesNoInputPopup.GetComponent<RectTransform>());
        }

    }

    // Layer02 (유닛 슬롯만 올려놓은 경우)
    public void OnClickGrowthBtnWithDust() {
        if (yesNoPopup != null)
            return;

        UserData userData = UserData.Instance;
        int cost = GetCostSoulFragmentForGrowth(UnitData.GrowthLevel) * Unit.NumSoulFragmentRequiredForSummon;
        if (cost <= userData.SoulDust) {
            // 영혼파편 사용
            userData.SoulDust -= cost;

            // 확률적으로 성장
            if (Random.Range(0f, 100f) <= GetSuccessProbabilityForGrowth(UnitData.GrowthLevel)) {
                UnitData.GrowthLevel += 1;
            }
            else {
                Debug.Log("영혼 가루을 이용한 성장 실패");
            }
        }

    }
    public void OnClickUnitDecompositionBtn() {
        if (yesNoPopup != null)
            return;

        Debug.Log("팝업창을 띄운다");
        yesNoPopup = Instantiate(yesNoPopupPrefab, new Vector3(Screen.width / 2, Screen.height / 2), Quaternion.identity, 
            this.transform.parent);

        int dustAmount = GetDecompositionDustAmount(UnitData.SharedData);
        yesNoPopup.SetQuestionText(string.Format("해당 유닛을 분해하시겠 습니까?\n\n분해 시 {0:G} 개의 가루를 얻습니다.", dustAmount));
        yesNoPopup.SetYesText("분해하기");
        yesNoPopup.AddActionToYes(() => {
            if (UnitData == null)
                return;

            UserData userData = UserData.Instance;

            userData.SoulDust += GetDecompositionDustAmount(UnitData.SharedData);
            userData.RemoveUnitData(UnitData);
            UnitData = null;
        });
        yesNoPopup.AddActionToYes(yesNoPopup.GetCloseAction());

        yesNoPopup.SetNoText("취소");
        yesNoPopup.AddActionToNo(yesNoPopup.GetCloseAction());

        for (int i = 0; i < 2; ++i) {
            LayoutRebuilder.ForceRebuildLayoutImmediate(yesNoPopup.GetComponent<RectTransform>());
        }

    }
    
    // Layer03 ()
    public void OnClickGrowthBtnWithFragment() {
        if (yesNoPopup != null)
            return;

        int cost = GetCostSoulFragmentForGrowth(UnitData.GrowthLevel);
        if (cost <= SoulFragment.Num) {
            // 영혼파편 사용
            SoulFragment.Num -= cost;
            if(SoulFragment.Num <= 0) {
                UserData.Instance.RemoveSoulFragment(SoulFragment);
                SoulFragment = null;

            }

            // 확률적으로 성장
            if (Random.Range(0f, 100f) <= GetSuccessProbabilityForGrowth(UnitData.GrowthLevel)) {
                UnitData.GrowthLevel += 1;
            }
            else {
                Debug.Log("영혼 파편을 이용한 성장 실패");
            }
        }
    }

    private int GetCostSoulFragmentForGrowth(int level) {
        return Mathf.Max(0, 10 + level);
    }
    private float GetSuccessProbabilityForGrowth(int level) {
        if (level < 0)
            return 100f;
        return 100f * Mathf.Pow(0.92f, level);
    }
    private int GetDecompositionDustAmount(UnitSharedData unitSharedData) {
        return unitSharedData.SoulFragmentValueAsDust * Unit.NumSoulFragmentRequiredForSummon / 2;
    }

}
