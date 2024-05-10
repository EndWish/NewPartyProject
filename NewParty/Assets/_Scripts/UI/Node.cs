using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Node : MonoBehaviour
{
    [SerializeField] protected NodeInfo nodeInfo;
    [SerializeField] protected List<Node> nextNodes;
    protected Image myImage;

    protected void Awake() {
        myImage = GetComponent<Image>();

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
        if (UserData.Instance.ClearNodes.Contains(nodeInfo.Name)) { // 클리어한 노드일 경우
            myImage.color = Color.green;
        } else {    // 클리어히지 않은 노드일 경우
            myImage.color = Color.gray;
        }
    }

    public bool IsClear() {
        return UserData.Instance.ClearNodes.Contains(nodeInfo.Name);
    }

}
