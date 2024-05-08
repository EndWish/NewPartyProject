using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class ExtensionMethod
{
    #region Quaternion
    public static Vector3 ToNormalizedDirection(this Quaternion rotation) {
        return rotation * Vector3.right;
    }
    #endregion

    #region Vector
    // Vector3
    public static Vector2 ToVector2(this Vector3 vector3) {
        return new Vector2(vector3.x, vector3.y);
    }

    // Vector2
    public static Vector3 ToVector3(this Vector2 vector2) {
        return new Vector3(vector2.x, vector2.y, 0f);
    }
    public static Vector3 ToVector3(this Vector2 vector2, float z) {
        return new Vector3(vector2.x, vector2.y, z);
    }

    public static Quaternion ToRotation(this Vector2 vector2) {
        return Quaternion.Euler(0, 0, Mathf.Atan2(vector2.y, vector2.x) * Mathf.Rad2Deg);
    }
    public static float ToDegree(this Vector2 vector2) {
        return Mathf.Atan2(vector2.y, vector2.x) * Mathf.Rad2Deg;
    }
    #endregion

    #region Collection

    #region MinElement
    /// <summary> ���� ���� ���Ҹ� ��ȯ�Ѵ�. ���Ұ� null�� ��� �����Ѵ�. </summary>
    /// <param name="comp"> ���� ������ ���Ŀ� ���� �Լ�(����). IComparable ���� ��ȯ�Ѵ� </param>
    public static T MinElement<T>(this IReadOnlyCollection<T> collection, System.Func<T, IComparable> comp) where T : class {
        T result = null;
        IComparable min = default;
        IComparable compValue = default;

        foreach (T element in collection) {
            if (element == null)
                continue;

            // �񱳴���� ��� result �ʱ�ȭ�Ҷ�
            if (result == null) {
                result = element;
                min = comp(result);
                continue;
            }

            // list[i]�� ���� �� �۴ٸ� ���� �������ش�.
            compValue = comp(element);
            if (compValue.CompareTo(min) < 0) {   // ��ȯ�� : compValue - min
                result = element;
                min = compValue;
            }
        }

        return result;
    }

    /// <summary> ���� ���� ���Ҹ� ��ȯ�Ѵ�. ���Ұ� null�� ��� �����Ѵ�. </summary>
    /// <param name="comp"> ���� ������ ���Ŀ� ���� �Լ�(����). IComparable ���� ��ȯ�Ѵ� </param>
    /// <param name="comp"> ���� ������ ���Ŀ� ���� �Լ�(����). IComparable ���� ��ȯ�Ѵ� </param>
    /// <param name="filter"> true �� ���ҵ鸸 ���ϰ� false�� ���ҵ��� �����Ѵ�. </param>
    public static T MinElement<T>(this IReadOnlyCollection<T> collection, System.Func<T, IComparable> comp, Predicate<T> filter) where T : class {
        T result = null;
        IComparable min = default;
        IComparable compValue = default;

        foreach (T element in collection) {
            if (element == null || !filter(element))
                continue;

            // �񱳴���� ��� result �ʱ�ȭ�Ҷ�
            if (result == null) {
                result = element;
                min = comp(result);
                continue;
            }

            // list[i]�� ���� �� �۴ٸ� ���� �������ش�.
            compValue = comp(element);
            if (compValue.CompareTo(min) < 0) {   // ��ȯ�� : compValue - min
                result = element;
                min = compValue;
            }
        }

        return result;
    }
    #endregion

    #region GetNearest

    /// <summary> point���� ���� ����� ��ü�� ��ȯ�� �ش�. </summary>
    /// <param name="point"> ������ </param>
    public static T GetNearest<T>(this T[] list, Vector3 point) where T : Component {
        T target = list.MinElement(x => {
            return Vector3.SqrMagnitude(x.transform.position - point);
        });
        if (target == null)
            return null;
        else
            return target;
    }

    /// <summary> point���� ���� ����� ��ü�� ��ȯ�� �ش�. </summary>
    /// <param name="point"> ������ </param>
    public static T GetNearest<T>(this T[] list, Vector3 point, Predicate<T> filter) where T : Component {
        T target = list.MinElement(x => {
            if (filter(x))
                return Vector3.SqrMagnitude(x.transform.position - point);
            else
                return float.MaxValue;
        }, filter);

        if (target == null)
            return null;
        else
            return target;
    }

    /// <summary> point���� ���� ����� ��ü�� Transform�� ��ȯ�� �ش�. </summary>
    /// <param name="point"> ������ </param>
    public static Transform GetNearestTr<T>(this T[] list, Vector3 point) where T : Component {
        T target = list.MinElement(x => {
            return Vector3.SqrMagnitude(x.transform.position - point);
        });

        if (target == null)
            return null;
        else
            return target.transform;
    }

    /// <summary> point���� ���� ����� Transform�� ��ȯ�� �ش�. </summary>
    /// <param name="point"> ������ </param>
    public static Transform GetNearestTr(this Transform[] list, Vector3 point) {
        Transform result = list.MinElement(x => {
            return Vector3.SqrMagnitude(x.position - point);
        });

        return result;
    }


    /// <summary> point���� ���� ����� ��ü�� ��ȯ�� �ش�. </summary>
    /// <param name="point"> ������ </param>
    public static T GetNearest<T>(this List<T> list, Vector3 point) where T : Component {
        T target = list.MinElement(x => {
            return Vector3.SqrMagnitude(x.transform.position - point);
        });

        if (target == null)
            return null;
        else
            return target;
    }

    /// <summary> point���� ���� ����� ��ü�� ��ȯ�� �ش�. </summary>
    /// <param name="point"> ������ </param>
    public static T GetNearest<T>(this List<T> list, Vector3 point, Predicate<T> filter) where T : Component {
        T target = list.MinElement(x => {
            if (filter(x))
                return Vector3.SqrMagnitude(x.transform.position - point);
            else
                return float.MaxValue;
        }, filter);

        if (target == null)
            return null;
        else
            return target;
    }

    /// <summary> point���� ���� ����� ��ü�� Transform�� ��ȯ�� �ش�. </summary>
    /// <param name="point"> ������ </param>
    public static Transform GetNearestTr<T>(this List<T> list, Vector3 point) where T : Component {
        T target = list.MinElement(x => {
            return Vector3.SqrMagnitude(x.transform.position - point);
        });

        if (target == null)
            return null;
        else
            return target.transform;
    }

    /// <summary> point���� ���� ����� Transform�� ��ȯ�� �ش�. </summary>
    /// <param name="point"> ������ </param>
    public static Transform GetNearestTr(this List<Transform> list, Vector3 point) {
        Transform result = list.MinElement(x => {
            return Vector3.SqrMagnitude(x.position - point);
        });

        return result;
    }
    #endregion

    #endregion
}
