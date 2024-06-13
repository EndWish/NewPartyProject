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
            if( soulFragment != null && unit != null 
                && soulFragment.GetUnitType() != unit.MyData.Type) {
                unit = null;
            }
            BtnLayerUpdate();
        }
    }

    private Unit unit;
    public Unit Unit {
        get { return unit; }
        set { 
            unit = value;
            if (soulFragment != null && unit != null
                && soulFragment.GetUnitType() != unit.MyData.Type) {
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
        unitTorchSlot.SlotUpdate(Unit, 0);
    }

    private void OnDestroy() {
        if (yesNoPopup != null)
            Destroy(yesNoPopup.gameObject);
    }

    private void BtnLayerUpdate() {
        if (yesNoPopup != null)
            Destroy(yesNoPopup.gameObject);

        bool hasUnit = Unit != null;
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

        if(Unit.NumSoulFragmentRequiredForSummon <= SoulFragment.GetNum()) {
            Unit.Data unitData = new Unit.Data(SoulFragment.GetUnitType(), Unit.GrowthLevelWhenSummoned);
            Unit newUnit = UserData.Instance.AddUnitData(unitData);
            SoulFragment.AddNum(-Unit.NumSoulFragmentRequiredForSummon);
            if (SoulFragment.GetNum() <= 0) {
                UserData.Instance.SoulFragmentList.Remove(SoulFragment);
                SoulFragment = null;
            }

            Unit = newUnit;
        }
    }
    public void OnClickSoulDecompositionBtn() {
        if (yesNoPopup != null)
            return;

        Debug.Log("입력이 있는 팝업창을 띄운다");
        yesNoPopup = Instantiate(yesNoInputPopupPrefab, new Vector3(Screen.width / 2, Screen.height / 2), Quaternion.identity,
    this.transform.parent);

        YesNoInputPopup yesNoInputPopup = yesNoPopup as YesNoInputPopup;

        yesNoInputPopup.SetQuestionText(string.Format("해당 영혼 파편을 분해하시겠 습니까?\n\n{0:G}개 분해 시 {1:G} 개의 가루를 얻습니다.", 0, 0));

        yesNoInputPopup.SetContentType(TMPro.TMP_InputField.ContentType.IntegerNumber);
        
        yesNoInputPopup.AddActionToInputField((str) => {
            int num = 0;
            int dustAmount = 0;
            if (int.TryParse(str, out num)) {
                dustAmount = num * SoulFragment.Target.SoulFragmentValueAsDust / 2;
            }
            yesNoInputPopup.SetQuestionText(string.Format("해당 영혼 파편을 분해하시겠 습니까?\n\n{0:G}개 분해 시 {1:G} 개의 가루를 얻습니다.", num, dustAmount));
        });
        yesNoInputPopup.AddActionToInputField(yesNoInputPopup.GetInputIntegerLimitAction(0, SoulFragment.GetNum()));

        yesNoInputPopup.SetYesText("분해하기");
        yesNoInputPopup.AddActionToYes(yesNoInputPopup.GetCloseAction());
        yesNoInputPopup.AddActionToYes(() => {
            if (SoulFragment == null)
                return;

            int num;
            if (int.TryParse(yesNoInputPopup.GetInputFieldText(), out num)) {
                if (num <= 0 || SoulFragment.GetNum() < num)
                    return;

                UserData userData = UserData.Instance;

                int dustAmount = num * SoulFragment.Target.SoulFragmentValueAsDust / 2;
                userData.SoulDust += dustAmount;

                SoulFragment.AddNum(-num);
                if (SoulFragment.GetNum() <= 0) {
                    userData.SoulFragmentList.Remove(SoulFragment);
                    SoulFragment = null;
                }
            }


        });

        yesNoInputPopup.SetNoText("취소");
        yesNoInputPopup.AddActionToNo(yesNoInputPopup.GetCloseAction());

        Canvas.ForceUpdateCanvases();
        //LayoutRebuilder.ForceRebuildLayoutImmediate(yesNoInputPopup.transform as RectTransform);
        yesNoInputPopup.GetComponent<VerticalLayoutGroup>().enabled = false;
        yesNoInputPopup.GetComponent<VerticalLayoutGroup>().enabled = true;
    }

    // Layer02 (유닛 슬롯만 올려놓은 경우)
    public void OnClickGrowthBtnWithDust() {
        if (yesNoPopup != null)
            return;

        UserData userData = UserData.Instance;
        int cost = GetCostSoulFragmentForGrowth(Unit.GrowthLevel) * Unit.NumSoulFragmentRequiredForSummon;
        if (cost <= userData.SoulDust) {
            // 영혼파편 사용
            userData.SoulDust -= cost;

            // 확률적으로 성장
            if (Random.Range(0f, 100f) <= GetSuccessProbabilityForGrowth(Unit.GrowthLevel)) {
                Unit.GrowthLevel += 1;
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

        int dustAmount = GetDecompositionDustAmount(Unit);
        yesNoPopup.SetQuestionText(string.Format("해당 유닛을 분해하시겠 습니까?\n\n분해 시 {0:G} 개의 가루를 얻습니다.", dustAmount));
        yesNoPopup.SetYesText("분해하기");
        yesNoPopup.AddActionToYes(() => {
            if (Unit == null)
                return;

            UserData userData = UserData.Instance;

            userData.SoulDust += GetDecompositionDustAmount(Unit);
            userData.RemoveUnitData(Unit);
            Unit = null;
        });
        yesNoPopup.AddActionToYes(yesNoPopup.GetCloseAction());

        yesNoPopup.SetNoText("취소");
        yesNoPopup.AddActionToNo(yesNoPopup.GetCloseAction());

        Canvas.ForceUpdateCanvases();
        //LayoutRebuilder.ForceRebuildLayoutImmediate(yesNoPopup.transform as RectTransform);
        yesNoPopup.GetComponent<VerticalLayoutGroup>().enabled = false;
        yesNoPopup.GetComponent<VerticalLayoutGroup>().enabled = true;
    }
    
    // Layer03 ()
    public void OnClickGrowthBtnWithFragment() {
        if (yesNoPopup != null)
            return;

        int cost = GetCostSoulFragmentForGrowth(Unit.GrowthLevel);
        if (cost <= SoulFragment.GetNum()) {
            // 영혼파편 사용
            SoulFragment.AddNum(-cost);
            if(SoulFragment.GetNum() <= 0) {
                UserData.Instance.SoulFragmentList.Remove(SoulFragment);
                SoulFragment = null;
            }

            // 확률적으로 성장
            if (Random.Range(0f, 100f) <= GetSuccessProbabilityForGrowth(Unit.GrowthLevel)) {
                Unit.GrowthLevel += 1;
            }
            else {
                Debug.Log("영혼 파편을 이용한 성장 실패");
            }
        }
    }

    private int GetCostSoulFragmentForGrowth(int level) {
        return Mathf.Max(0, 10 + (level - Unit.GrowthLevelWhenSummoned));
    }
    private float GetSuccessProbabilityForGrowth(int level) {
        if (level < 0)
            return 100f;
        return 100f * Mathf.Pow(0.92f, level);
    }
    private int GetDecompositionDustAmount(Unit unit) {
        return unit.SoulFragmentValueAsDust * Unit.NumSoulFragmentRequiredForSummon / 2;
    }

}
