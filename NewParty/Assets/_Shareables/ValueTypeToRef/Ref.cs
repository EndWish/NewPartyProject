using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class Ref<T> where T : struct
{
    public T Value;

    public Ref() {}
    public Ref(T value) : this() { this.Value = value; }
    
}
