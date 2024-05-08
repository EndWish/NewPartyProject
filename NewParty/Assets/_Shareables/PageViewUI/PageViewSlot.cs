using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PageViewSlot<TData> : MonoBehaviour
{
    public int dataIndex { get; protected set; } = -1;
    public TData Data { get; protected set; }

    public virtual void SlotUpdate(TData data, int index) {
        dataIndex = index;
        this.Data = (TData)data;
    }
}
