using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum TokenType
{
    None, Atk, Skill, Shield, Num,
}

public class Token : MonoBehaviour, IPointerClickHandler
{
    // 연결 정보 //////////////////////////////////////////////////////////////
    private RectTransform myRectTransform;

    [SerializeField] private Transform imagesParent;

    [SerializeField] private Image iconImage;
    [SerializeField, EnumNamedArray(typeof(TokenType))] private List<Sprite> tokenIconSprites;

    [SerializeField] private Image backgroundImage;
    [SerializeField, EnumNamedArray(typeof(TokenType))] private List<Sprite> tokenBackgroundSprites;

    private Unit owner = null;
    [SerializeField] private GameObject newAdditionIcon;

    // 개인 정보 //////////////////////////////////////////////////////////////
    private TokenType type = TokenType.None;
    public bool IsSelected { get; set; } = false;

    // 유니티 함수 //////////////////////////////////////////////////////////////
    private void Awake() {
        myRectTransform = GetComponent<RectTransform>();
        newAdditionIcon.SetActive(false);
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

    public TokenType Type {
        get { return type; }
        set { 
            type = value;
            iconImage.sprite = tokenIconSprites[(int)type];
            backgroundImage.sprite = tokenBackgroundSprites[(int)type];
        }
    }

    public Unit Owner {
        get { return owner; }
        set {
            Unit prevOwner = owner;
            owner = value;
            if (owner != null) {
                newAdditionIcon.SetActive(true);
                owner.CoOnEndMyTurn += OnEndOwnerTurn;
            } else {
                newAdditionIcon.SetActive(false);
                if(prevOwner != null) {
                    prevOwner.CoOnEndMyTurn -= OnEndOwnerTurn;
                }
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (Owner.IsMine() && Owner.HasTurn() && !BattleSelectable.IsRunning) {
            IsSelected = !IsSelected;
        }
    }

    public IEnumerator OnEndOwnerTurn(Unit owner) {
        newAdditionIcon.SetActive(false);
        IsSelected = false;
        yield break;
    }

}
