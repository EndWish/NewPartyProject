using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    private TextMeshPro text;

    private Vector3 velocity;

    private void Awake() {
        text = GetComponent<TextMeshPro>();
    }

    private void Start() {
        velocity = new Vector3(UnityEngine.Random.Range(-2f, 2f), UnityEngine.Random.Range(5f, 8f), 0);

        Color color = text.color;
        text.color = new Color(color.r, color.g, color.b, 1.5f);
    }

    private void Update() {
        // 이동
        velocity = new Vector3(velocity.x, velocity.y - 14f * Time.deltaTime, velocity.z);
        transform.position += velocity * Time.deltaTime;

        // 투명도
        Color color = text.color;
        text.color = new Color(color.r, color.g, color.b, color.a - Time.deltaTime);

        // 투명도가 0이하가 되면 삭제한다
        if(text.color.a <= 0) {
            Destroy(gameObject);
        }
    }

    public void SetFormat(int dmg, Vector3 color) {
        text.text = dmg.ToString();
        text.color = new Color(color.x, color.y, color.z, text.color.a);
    }

}
