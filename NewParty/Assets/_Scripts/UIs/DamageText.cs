using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class DamageText : MonoBehaviour
{
    public enum Type {
        None, Avoid, Invincibility
    }

    private TextMeshPro text;

    private Type type;
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

    public void SetFormat(float dmg, Vector3 color, int criStack) {
        StringBuilder sb = new StringBuilder();

        sb.Append(100 <= dmg ? (int)dmg : (Mathf.Round(dmg * 10) / 10f));
        if (0 < criStack)
            sb.AppendFormat("<color=#F11414><size=2>(치명타x{0})</size></color>", criStack);

        text.text = sb.ToString();
        text.color = new Color(color.x, color.y, color.z, text.color.a);

    }
    public void SetFormat(Type type) {
        this.type = type;
        switch (type) {
            case Type.Avoid:
                text.text = "회피~";
                text.color = new Color(0.4f, 0.4f, 0.4f);
                break;

            case Type.Invincibility:
                text.text = "무적!";
                text.color = new Color(1f, 1f, 0f);
                break;

            default:
                text.text = "???";
                text.color = Color.white;
                break;
        }
    }

}
