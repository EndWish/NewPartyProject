using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Transform dragTarget;
    private Vector2 dragOldPos;

    public UnityEvent OnBeginDragEvent;
    public UnityEvent OnDragEvent;
    public UnityEvent OnEndDragEvent;

    public void OnBeginDrag(PointerEventData eventData) {
        dragOldPos = eventData.position;
        OnBeginDragEvent.Invoke();
    }

    public void OnDrag(PointerEventData eventData) {
        Vector2 dragOffset = eventData.position - dragOldPos;
        dragTarget.position += new Vector3(dragOffset.x, dragOffset.y, 0);
        dragOldPos = eventData.position;
        OnDragEvent.Invoke();
    }

    public void OnEndDrag(PointerEventData eventData) {
        OnEndDragEvent.Invoke();
    }
}
