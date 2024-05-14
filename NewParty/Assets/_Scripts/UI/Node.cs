using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Node : MonoBehaviour
{
    [SerializeField] protected Image iconImage;
    [SerializeField] protected GameObject selectIcon;
    [SerializeField] protected MapPopup mapPopup;
    [SerializeField] protected NodeInfo nodeInfo;
    [SerializeField] protected List<Node> nextNodes;
    protected Image myImage;

    protected void Awake() {
        myImage = GetComponent<Image>();

        selectIcon.SetActive(false);
        iconImage.sprite = nodeInfo.Icon;

        if (!IsClear())
            gameObject.SetActive(false);
    }

    protected void Start() {
        HashSet<NodeName> ClearNodes = UserData.Instance.ClearNodes;

        if (IsClear()) {
            foreach (var node in nextNodes) {
                if (!node.gameObject.activeSelf) {
                    node.gameObject.SetActive(true);
                }
            }
        }
    }

    protected void OnEnable() {
        if (UserData.Instance.ClearNodes.Contains(nodeInfo.Name)) { // Ŭ������ ����� ���
            myImage.color = Color.green;
        } else {    // Ŭ�������� ���� ����� ���
            myImage.color = Color.gray;
        }
    }

    public virtual NodeInfo NodeInfo { get { return nodeInfo; } }

    public bool IsClear() {
        return UserData.Instance.ClearNodes.Contains(nodeInfo.Name);
    }

    public void OnSelected() {
        if(mapPopup.SelectedNode != null) {
            mapPopup.SelectedNode.selectIcon.SetActive(false);
        }
        mapPopup.SelectedNode = this;
        selectIcon.SetActive(true);
    }

}
