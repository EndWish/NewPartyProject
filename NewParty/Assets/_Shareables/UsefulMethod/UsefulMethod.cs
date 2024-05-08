using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UsefulMethod
{
    static public bool ProbabilityPerTime(float probability, float sec) {
        // x = 1회당 성공확률
        // x = 1 - (1 - probability)^(t/sec);
        float x = 1f - Mathf.Pow(1f - probability, Time.deltaTime / sec);
        return Random.Range(0f, 1f) <= x;
    }

    static public float MultiplyPerTime(float percent, float sec) {
        return Mathf.Pow(percent, Time.deltaTime / sec);
    }

    static public Vector3 GetWorldPositionAtMousePosition() {
        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        pos.z = 0;
        return pos;
    }

}
