using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class PageView<TData> : MonoBehaviour
{
    // ���� ���� //////////////////////////////////////////////////////////////
    public List<TData> Datas;
    [SerializeField] protected Transform slotsParent;
    public List<PageViewSlot<TData>> PageViewSlots { get; set; } = new List<PageViewSlot<TData>>();

    // ���� ���� //////////////////////////////////////////////////////////////
    protected int page = 1;

    // ����Ƽ �Լ� ////////////////////////////////////////////////////////////
    protected virtual void Awake() {
        PageViewSlots.AddRange(slotsParent.GetComponentsInChildren<PageViewSlot<TData>>());

        UpdatePage(1);
    }

    protected virtual void OnEnable() {
        page = Mathf.Min(page, MaxPage);
        UpdatePage(page);
    }

    // �Լ� ///////////////////////////////////////////////////////////////////
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

        // ��Ÿ�� �������� �ε��� ������ ���Ѵ�.
        int startIndex = (page - 1) * PageViewSlots.Count;  // inclusive
        int endIndex = Mathf.Min( startIndex + PageViewSlots.Count, Datas.Count);   // exclusive

        // ������ Ȱ��ȭ/��Ȱ��ȭ �ϰ� ������Ʈ�Ѵ�.
        for(int i = 0; i < PageViewSlots.Count; ++i) {
            int index = startIndex + i;

            

            if (0 <= index && index < endIndex) {  // ��Ī�Ǵ� �����Ͱ� ���� ���
                PageViewSlots[i].gameObject.SetActive(true);
                if(Datas[index] == null) {
                    Debug.Log("Datas[index]�� null �̴�.");
                }

                PageViewSlots[i].SlotUpdate(Datas[index], index);
            } else {    // ���̻� �����Ͱ� ���� ���
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
        if (wheelInput.y > 0) {    // �� ����
            GoPrevPage();
        } else if (wheelInput.y < 0) {   // �� �Ʒ���
            GoNextPage();
        }
    }

}
