using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum TokenType
{
    None, Atk, Skill, Barrier, Num,
}

public enum TokenDeleteHistory
{
    None, Use, Discrad, Overflow,
}

public class Token : MonoBehaviourPun, IPointerClickHandler
{
    static public string GetTokenPrefabPath() {
        return "Prefabs/Token/Token";
    }

    // 연결 정보 //////////////////////////////////////////////////////////////
    protected RectTransform myRectTransform;
    [SerializeField] protected Transform imagesParent;
    [SerializeField] protected Image iconImage;
    [SerializeField] protected Image backgroundImage;
    [SerializeField] private GameObject plusIcon;

    [SerializeField] protected TokenSharedData sharedData;

    // 개인 정보 //////////////////////////////////////////////////////////////
    private TokenType type = TokenType.None;
    private Unit owner = null;
    
    private bool isSelected = false;

    // 유니티 함수 //////////////////////////////////////////////////////////////
    private void Awake() {
        myRectTransform = GetComponent<RectTransform>();
        plusIcon.SetActive(false);
    }

    private void Update() {
        // 선택에 따라 토큰 위치바뀌도록 한다.
        float imgHeight = myRectTransform.sizeDelta.y;

        Vector3 targetPos = Vector3.zero;
        if (IsSelected) targetPos = new Vector3(0, -imgHeight / 2f, 0);
        imagesParent.localPosition = Vector3.MoveTowards(imagesParent.localPosition, targetPos, imgHeight * 2f * Time.deltaTime);
    }

    private void OnDestroy() {
        if(Owner != null) {
            Owner.CoOnEndMyTurn -= OnEndOwnerTurn;
        }
    }

    // 함수 /////////////////////////////////////////////////////////////////////
    
    public Sprite IconSp {
        get { return sharedData.GetTokenIconSprite(Type); }
    }
    public Sprite BackgroundSp {
        get { return sharedData.GetTokenBackgroundSprites(Type); }
    }

    [PunRPC]
    private void TypeRPC(TokenType type) {
        this.type = type;
        iconImage.sprite = IconSp;
        backgroundImage.sprite = BackgroundSp;
    }
    public TokenType Type {
        get { return type; }
        set { photonView.RPC("TypeRPC", RpcTarget.All, value); }
    }

    [PunRPC]
    private void IsSelectedRPC(bool isSelected) {
        this.isSelected = isSelected;
    }
    public bool IsSelected {
        get { return isSelected; }
        set { photonView.RPC("IsSelectedRPC", RpcTarget.All, value); }
    }

    [PunRPC]
    private void OwnerRPC(int viewId) {
        Unit prevOwner = owner;
        owner = PhotonView.Find(viewId).GetComponent<Unit>();

        if (prevOwner != null) {
            prevOwner.CoOnEndMyTurn -= OnEndOwnerTurn;
        }

        if (owner != null) {
            plusIcon.SetActive(true);
            owner.CoOnEndMyTurn += OnEndOwnerTurn;
        }
        else {
            plusIcon.SetActive(false);
        }

    }
    public Unit Owner {
        get { return owner; }
        set { 
            photonView.RPC("OwnerRPC", RpcTarget.All, value?.photonView.ViewID ?? -1);
        }
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (Owner.IsMine() && Owner.HasTurn() && !BattleSelectable.IsRunning) {
            IsSelected = !IsSelected;
        }
    }

    public IEnumerator OnEndOwnerTurn() {
        SetActivePlusIcon(false);
        IsSelected = false;
        yield break;
    }

    [PunRPC]
    private void SetActivePlusIconRPC(bool active) {
        plusIcon.SetActive(active);
    }
    public void SetActivePlusIcon(bool active) {
        photonView.RPC("SetActivePlusIconRPC", RpcTarget.All, active);
    }

    // 삭제
    [PunRPC]
    private void DestroyRPC() {
        Object.Destroy(gameObject);
    }
    public void Destroy() {
        photonView.RPC("DestroyRPC", RpcTarget.AllBuffered);
    }

}
