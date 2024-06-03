using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;

public class TestFuncIEnumerator : MonoBehaviour
{
    static int count = 0;

    public Func<IEnumerator> OnEvent;

    private void Start() {
        OnEvent += Co1;
        OnEvent += Co1;
        OnEvent += Co2;
        OnEvent += Co1;
        OnEvent += Co2;
        OnEvent += Co1;
        OnEvent += Co1;
        OnEvent += Co2;
        OnEvent += Co2;
    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.Space)) {

            //OnEvent.Invoke();

           StartCoroutine(CoFunc(OnEvent));
        }
    }

    public IEnumerator Co1() {
        Debug.Log((count++) + " Co1 수행중...");
        yield return new WaitForSeconds(0.5f);
    }

    public IEnumerator Co2() {
        Debug.Log((count++) + " Co2 수행중...");
        yield return new WaitForSeconds(0.5f);
    }

    public IEnumerator CoFunc(Func<IEnumerator> func) {
        foreach (Func<IEnumerator> del in func.GetInvocationList()) {
            yield return StartCoroutine(del.Invoke());
        }
    }


}

