using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    private void Update() {
        if (Input.GetKeyDown(KeyCode.A)) {
            Unit newUnit = Resources.Load<Unit>("Prefabs/Units/" + UnitType.Garuda.ToString());
            Instantiate(newUnit, Vector3.zero, Quaternion.identity, null);
        }
    }

}
