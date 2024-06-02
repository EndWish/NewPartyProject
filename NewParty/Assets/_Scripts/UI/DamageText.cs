using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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
        // �̵�
        velocity = new Vector3(velocity.x, velocity.y - 14f * Time.deltaTime, velocity.z);
        transform.position += velocity * Time.deltaTime;

        // ����
        Color color = text.color;
        text.color = new Color(color.r, color.g, color.b, color.a - Time.deltaTime);

        // ������ 0���ϰ� �Ǹ� �����Ѵ�
        if(text.color.a <= 0) {
            Destroy(gameObject);
        }
    }

    public void SetFormat(int dmg, Vector3 color) {
        text.text = dmg.ToString();
        text.color = new Color(color.x, color.y, color.z, text.color.a);
    }
    public void SetFormat(Type type) {
        this.type = type;
        switch (type) {
            case Type.Avoid:
                text.text = "ȸ��~";
                text.color = new Color(0.4f, 0.4f, 0.4f);
                break;

            case Type.Invincibility:
                text.text = "����!";
                text.color = new Color(1f, 1f, 0f);
                break;

            default:
                text.text = "???";
                text.color = Color.white;
                break;
        }
    }

}
