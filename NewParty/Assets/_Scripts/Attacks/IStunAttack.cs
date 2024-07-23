using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStunAttack
{
    public Action<Ref<float>> OnSetStunCha { get; set; }
    public float StunCha { get; set; }
}
