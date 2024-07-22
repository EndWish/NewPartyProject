using System;

public interface IDmgAttack
{
    public Action<Ref<float>> OnSetDmg { get; set; }
    public float Dmg { get; set; }
}
