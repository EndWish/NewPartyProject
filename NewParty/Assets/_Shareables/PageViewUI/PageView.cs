using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class PageView<TData> : MonoBehaviour
{
    // 연결 정보 //////////////////////////////////////////////////////////////
    public List<TData> Datas;
    [SerializeField] protected Transform slotsParent;
    public List<PageViewSlot<TData>> PageViewSlots { get; set; } = new List<PageViewSlot<TData>>();

    // 개인 정보 //////////////////////////////////////////////////////////////
    protected int page = 1;

    // 유니티 함수 ////////////////////////////////////////////////////////////
    protected virtual void Awake() {
        PageViewSlots.AddRange(slotsParent.GetComponentsInChildren<PageViewSlot<TData>>());

        UpdatePage(1);
    }

    protected virtual void OnEnable() {
        page = Mathf.Min(page, MaxPage);
        UpdatePage(page);
    }

    // 함수 ///////////////////////////////////////////////////////////////////
    public int MaxPage {
        get { 
            if(0 < PageViewSlots?.Count && Datas != null)
                return Mathf.Max((Datas.Count + PageViewSlots.Count - 1) / PageViewSlots.Count, 1);
            return 1;
        }
    }

    public virtual void UpdatePage(int page) {
        if (Datas == null || PageViewSlots?.Count == 0)
            return;

        // 나타낼 데이터의 인덱스 범위를 구한다.
        int startIndex = (page - 1) * PageViewSlots.Count;  // inclusive
        int endIndex = Mathf.Min( startIndex + PageViewSlots.Count, Datas.Count);   // exclusive

        // 슬롯을 활성화/비활성화 하고 업데이트한다.
        for(int i = 0; i < PageViewSlots.Count; ++i) {
            int index = startIndex + i;

            

            if (0 <= index && index < endIndex) {  // 매칭되는 데이터가 있을 경우
                PageViewSlots[i].gameObject.SetActive(true);
                if(Datas[index] == null) {
                    Debug.Log("Datas[index]가 null 이다.");
                }

                PageViewSlots[i].SlotUpdate(Datas[index], index);
            } else {    // 더이상 데이터가 없는 경우
                PageViewSlots[i].gameObject.SetActive(false);
            }
        }
    }
    public void UpdatePage() {
        page = Mathf.Min(page, MaxPage);
        UpdatePage(page);
    }

    public void GoNextPage() {
        if(page < MaxPage) {
            UpdatePage(++page);
        }
    }
    public void GoPrevPage() {
        if (1 < page) {
            UpdatePage(--page);
        }
    }
    public void GoPage(int page) {
        if(1 <= page && page <= MaxPage) {
            this.page = page;
            UpdatePage(page);
        }
    }

    public void GoPageWithScroll() {
        Vector2 wheelInput = Input.mouseScrollDelta;
        if (wheelInput.y > 0) {    // 휠 위로
            GoPrevPage();
        } else if (wheelInput.y < 0) {   // 휠 아래로
            GoNextPage();
        }
    }

}
