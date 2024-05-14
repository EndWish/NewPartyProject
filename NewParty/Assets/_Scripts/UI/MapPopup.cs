using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPopup : MonoBehaviour
{
    [SerializeField] private Transform nodes;

    public bool IsMovingNodeToCenter { get; set; } = false;
    public Node SelectedNode { get; set; } = null;

    private void Update()
    {
        MoveNodeToCenter();
    }

    private void MoveNodeToCenter() {
        if (IsMovingNodeToCenter && nodes != null) {
            nodes.localPosition = Vector3.Lerp(Vector3.zero, nodes.localPosition, Mathf.Pow(0.001f, Time.deltaTime));
            if((Vector3.zero - nodes.localPosition).sqrMagnitude <= 25f * 25f) {
                IsMovingNodeToCenter = false;
            }
        }
    }
    
    public void Toggle() {
        if (gameObject.activeSelf)
            gameObject.SetActive(false);
        else
            gameObject.SetActive(true);
    }

    public void OnClickEnterBtn() {
        if(SelectedNode != null && SelectedNode is DungeonNode) {
            GameManager.Instance.SetDungeonInfo((DungeonNodeInfo)SelectedNode.NodeInfo);
            gameObject.SetActive(false);
        }
    }

}
