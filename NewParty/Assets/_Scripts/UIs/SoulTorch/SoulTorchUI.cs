using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoulTorchUI : MonoBehaviour
{
    [SerializeField] private SoulTorchSlot soulTorchSlot;
    [SerializeField] private UnitTorchSlot unitTorchSlot;

    [SerializeField, Header("��ȥ/����/��ȥ+���� ����")] 
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

    // Layer01 (��ȥ ���Ը� �÷����� ���)
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

        Debug.Log("�Է��� �ִ� �˾�â�� ����");
        yesNoPopup = Instantiate(yesNoInputPopupPrefab, new Vector3(Screen.width / 2, Screen.height / 2), Quaternion.identity,
    this.transform.parent);

        YesNoInputPopup yesNoInputPopup = yesNoPopup as YesNoInputPopup;

        yesNoInputPopup.SetQuestionText(string.Format("�ش� ��ȥ ������ �����Ͻð� ���ϱ�?\n\n{0:G}�� ���� �� {1:G} ���� ���縦 ����ϴ�.", 0, 0));

        yesNoInputPopup.SetContentType(TMPro.TMP_InputField.ContentType.IntegerNumber);
        
        yesNoInputPopup.AddActionToInputField((str) => {
            int num = 0;
            int dustAmount = 0;
            if (int.TryParse(str, out num)) {
                dustAmount = num * SoulFragment.Target.SoulFragmentValueAsDust / 2;
            }
            yesNoInputPopup.SetQuestionText(string.Format("�ش� ��ȥ ������ �����Ͻð� ���ϱ�?\n\n{0:G}�� ���� �� {1:G} ���� ���縦 ����ϴ�.", num, dustAmount));
        });
        yesNoInputPopup.AddActionToInputField(yesNoInputPopup.GetInputIntegerLimitAction(0, SoulFragment.GetNum()));

        yesNoInputPopup.SetYesText("�����ϱ�");
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

        yesNoInputPopup.SetNoText("���");
        yesNoInputPopup.AddActionToNo(yesNoInputPopup.GetCloseAction());

        Canvas.ForceUpdateCanvases();
        //LayoutRebuilder.ForceRebuildLayoutImmediate(yesNoInputPopup.transform as RectTransform);
        yesNoInputPopup.GetComponent<VerticalLayoutGroup>().enabled = false;
        yesNoInputPopup.GetComponent<VerticalLayoutGroup>().enabled = true;
    }

    // Layer02 (���� ���Ը� �÷����� ���)
    public void OnClickGrowthBtnWithDust() {
        if (yesNoPopup != null)
            return;

        UserData userData = UserData.Instance;
        int cost = GetCostSoulFragmentForGrowth(Unit.GrowthLevel) * Unit.NumSoulFragmentRequiredForSummon;
        if (cost <= userData.SoulDust) {
            // ��ȥ���� ���
            userData.SoulDust -= cost;

            // Ȯ�������� ����
            if (Random.Range(0f, 100f) <= GetSuccessProbabilityForGrowth(Unit.GrowthLevel)) {
                Unit.GrowthLevel += 1;
            }
            else {
                Debug.Log("��ȥ ������ �̿��� ���� ����");
            }
        }

    }
    public void OnClickUnitDecompositionBtn() {
        if (yesNoPopup != null)
            return;

        Debug.Log("�˾�â�� ����");
        yesNoPopup = Instantiate(yesNoPopupPrefab, new Vector3(Screen.width / 2, Screen.height / 2), Quaternion.identity, 
            this.transform.parent);

        int dustAmount = GetDecompositionDustAmount(Unit);
        yesNoPopup.SetQuestionText(string.Format("�ش� ������ �����Ͻð� ���ϱ�?\n\n���� �� {0:G} ���� ���縦 ����ϴ�.", dustAmount));
        yesNoPopup.SetYesText("�����ϱ�");
        yesNoPopup.AddActionToYes(() => {
            if (Unit == null)
                return;

            UserData userData = UserData.Instance;

            userData.SoulDust += GetDecompositionDustAmount(Unit);
            userData.RemoveUnitData(Unit);
            Unit = null;
        });
        yesNoPopup.AddActionToYes(yesNoPopup.GetCloseAction());

        yesNoPopup.SetNoText("���");
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
            // ��ȥ���� ���
            SoulFragment.AddNum(-cost);
            if(SoulFragment.GetNum() <= 0) {
                UserData.Instance.SoulFragmentList.Remove(SoulFragment);
                SoulFragment = null;
            }

            // Ȯ�������� ����
            if (Random.Range(0f, 100f) <= GetSuccessProbabilityForGrowth(Unit.GrowthLevel)) {
                Unit.GrowthLevel += 1;
            }
            else {
                Debug.Log("��ȥ ������ �̿��� ���� ����");
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
