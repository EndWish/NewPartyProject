using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class InfinitePageView<TData> : MonoBehaviour
{
    // ���� ���� //////////////////////////////////////////////////////////////
    public List<TData> Datas;
    public List<PageViewSlot<TData>> PageViewSlots;

    // ���� ���� //////////////////////////////////////////////////////////////
    protected int page = 1;

    // ����Ƽ �Լ� ////////////////////////////////////////////////////////////
    protected virtual void Awake() {
        UpdatePage(1);
    }

    protected virtual void OnEnable() {
        page = Mathf.Min(page, MaxPage);
        if(0 < page)
            UpdatePage(page);
    }

    // �Լ� ///////////////////////////////////////////////////////////////////
    public int MaxPage {
        get { 
            if(0 < PageViewSlots?.Count && Datas != null)
                return (Datas.Count + PageViewSlots.Count - 1) / PageViewSlots.Count;
            return 0;
        }
    }

    public void UpdatePage(int page) {
        if (Datas == null || PageViewSlots?.Count == 0)
            return;

        // ��Ÿ�� �������� �ε��� ������ ���Ѵ�.
        int startIndex = (page - 1) * PageViewSlots.Count;  // inclusive
        int endIndex = Mathf.Min( startIndex + PageViewSlots.Count, Datas.Count);   // exclusive

        // ������ Ȱ��ȭ/��Ȱ��ȭ �ϰ� ������Ʈ�Ѵ�.
        for(int i = 0; i < PageViewSlots.Count; ++i) {
            int index = startIndex + i;

            if(index < endIndex) {  // ��Ī�Ǵ� �����Ͱ� ���� ���
                PageViewSlots[i].gameObject.SetActive(true);
                PageViewSlots[i].SlotUpdate(Datas[index], index);
            } else {    // ���̻� �����Ͱ� ���� ���
                PageViewSlots[i].gameObject.SetActive(false);
            }
        }
    }

    public bool GoNextPage() {
        if(page < MaxPage) {
            UpdatePage(++page);
            return true;
        }
        return false;
    }
    public bool GoPrevPage() {
        if (1 < page) {
            UpdatePage(--page);
            return true;
        }
        return false;
    }
    public bool GoPage(int page) {
        if(1 <= page && page <= MaxPage) {
            this.page = page;
            UpdatePage(page);
            return true;
        }
        return false;
    }

    protected void GoPageWithWheel() {
        if (EventSystem.current.IsPointerOverGameObject()) {
            Vector2 wheelInput2 = Input.mouseScrollDelta;
            if (wheelInput2.y > 0) {    // �� ����
                GoPrevPage();
            } else if (wheelInput2.y < 0) {   // �� �Ʒ���
                GoNextPage();
            }
        }
    }

}
