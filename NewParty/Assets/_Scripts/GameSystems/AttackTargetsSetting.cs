using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AttackTargetsSetting : IEnumerable<Unit>
{
    public List<Unit> TargetList { get; set; }
    public Attack Attack { get; set; }

    public AttackTargetsSetting(Attack attack, List<Unit> targetList) {
        Attack = attack;
        TargetList = targetList;
        for (int i = 0; i < TargetList.Count; i++)
            TargetList[i].OnBecomeAttackTarget?.Invoke(Attack, this);
    }
    public AttackTargetsSetting(Attack attack, params Unit[] units) : this(attack, units.ToList()) {

    }

    public IEnumerator<Unit> GetEnumerator() {
        for(int i = 0; i < TargetList.Count;i++)
            yield return TargetList[i];
    }
    IEnumerator IEnumerable.GetEnumerator() {
        for (int i = 0; i < TargetList.Count; i++)
            yield return TargetList[i];
    }

}
