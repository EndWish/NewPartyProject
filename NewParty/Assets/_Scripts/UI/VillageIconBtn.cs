using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class VillageIconBtn : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] protected SpriteRenderer outline;
    public UnityEvent OnClick;

    public void OnPointerClick(PointerEventData eventData) {
        OnClick.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        outline.color = new Color(1, 145f/255f, 0, 1);
    }

    public void OnPointerExit(PointerEventData eventData) {
        outline.color = Color.white;
    }
    
}
