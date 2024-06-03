using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestSkillManager : MonoBehaviour
{
    [SerializeField] private GameObject skillBtnParent;
    [SerializeField] private GameObject outOfStackPopup;
    [SerializeField] private GameObject notMyPartyPopup;

    private void Update() {
        Party party = BattleManager.Instance.Parties[(int)TeamType.Player][0];

        if (party.photonView.IsMine) {
            notMyPartyPopup.SetActive(false);

            if (0 < party.RemainRestSkill) {
                skillBtnParent.SetActive(true);
                outOfStackPopup.SetActive(false);
            } else {
                skillBtnParent.SetActive(false);
                outOfStackPopup.SetActive(true);
            }
        } else {
            notMyPartyPopup.SetActive(true);
        }

        // ���� ���� �ٸ� ��Ƽ�� �޽� ��ų Ƚ���� ���Ҵٸ� �� ��Ƽ�� 0��° ��Ƽ�� �ǵ��� �Ѵ�

    }

}
