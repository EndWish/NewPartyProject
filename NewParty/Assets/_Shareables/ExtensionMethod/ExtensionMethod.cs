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
    /// <summary> 가장 작은 원소를 반환한다. 원소가 null일 경우 무시한다. </summary>
    /// <param name="comp"> 비교할 변수나 수식에 대한 함수(람다). IComparable 값을 반환한다 </param>
    public static T MinElement<T>(this IReadOnlyCollection<T> collection, System.Func<T, IComparable> comp) where T : class {
        T result = null;
        IComparable min = default;
        IComparable compValue = default;

        foreach (T element in collection) {
            if (element == null)
                continue;

            // 비교대상이 없어서 result 초기화할때
            if (result == null) {
                result = element;
                min = comp(result);
                continue;
            }

            // list[i]의 값이 더 작다면 값을 갱신해준다.
            compValue = comp(element);
            if (compValue.CompareTo(min) < 0) {   // 반환값 : compValue - min
                result = element;
                min = compValue;
            }
        }

        return result;
    }

    /// <summary> 가장 작은 원소를 반환한다. 원소가 null일 경우 무시한다. </summary>
    /// <param name="comp"> 비교할 변수나 수식에 대한 함수(람다). IComparable 값을 반환한다 </param>
    /// <param name="comp"> 비교할 변수나 수식에 대한 함수(람다). IComparable 값을 반환한다 </param>
    /// <param name="filter"> true 인 원소들만 비교하고 false인 원소들은 무시한다. </param>
    public static T MinElement<T>(this IReadOnlyCollection<T> collection, System.Func<T, IComparable> comp, Predicate<T> filter) where T : class {
        T result = null;
        IComparable min = default;
        IComparable compValue = default;

        foreach (T element in collection) {
            if (element == null || !filter(element))
                continue;

            // 비교대상이 없어서 result 초기화할때
            if (result == null) {
                result = element;
                min = comp(result);
                continue;
            }

            // list[i]의 값이 더 작다면 값을 갱신해준다.
            compValue = comp(element);
            if (compValue.CompareTo(min) < 0) {   // 반환값 : compValue - min
                result = element;
                min = compValue;
            }
        }

        return result;
    }
    #endregion

    #region GetNearest

    /// <summary> point에서 가장 가까운 객체를 반환해 준다. </summary>
    /// <param name="point"> 기준점 </param>
    public static T GetNearest<T>(this T[] list, Vector3 point) where T : Component {
        T target = list.MinElement(x => {
            return Vector3.SqrMagnitude(x.transform.position - point);
        });
        if (target == null)
            return null;
        else
            return target;
    }

    /// <summary> point에서 가장 가까운 객체를 반환해 준다. </summary>
    /// <param name="point"> 기준점 </param>
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

    /// <summary> point에서 가장 가까운 객체의 Transform을 반환해 준다. </summary>
    /// <param name="point"> 기준점 </param>
    public static Transform GetNearestTr<T>(this T[] list, Vector3 point) where T : Component {
        T target = list.MinElement(x => {
            return Vector3.SqrMagnitude(x.transform.position - point);
        });

        if (target == null)
            return null;
        else
            return target.transform;
    }

    /// <summary> point에서 가장 가까운 Transform을 반환해 준다. </summary>
    /// <param name="point"> 기준점 </param>
    public static Transform GetNearestTr(this Transform[] list, Vector3 point) {
        Transform result = list.MinElement(x => {
            return Vector3.SqrMagnitude(x.position - point);
        });

        return result;
    }


    /// <summary> point에서 가장 가까운 객체를 반환해 준다. </summary>
    /// <param name="point"> 기준점 </param>
    public static T GetNearest<T>(this List<T> list, Vector3 point) where T : Component {
        T target = list.MinElement(x => {
            return Vector3.SqrMagnitude(x.transform.position - point);
        });

        if (target == null)
            return null;
        else
            return target;
    }

    /// <summary> point에서 가장 가까운 객체를 반환해 준다. </summary>
    /// <param name="point"> 기준점 </param>
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

    /// <summary> point에서 가장 가까운 객체의 Transform을 반환해 준다. </summary>
    /// <param name="point"> 기준점 </param>
    public static Transform GetNearestTr<T>(this List<T> list, Vector3 point) where T : Component {
        T target = list.MinElement(x => {
            return Vector3.SqrMagnitude(x.transform.position - point);
        });

        if (target == null)
            return null;
        else
            return target.transform;
    }

    /// <summary> point에서 가장 가까운 Transform을 반환해 준다. </summary>
    /// <param name="point"> 기준점 </param>
    public static Transform GetNearestTr(this List<Transform> list, Vector3 point) {
        Transform result = list.MinElement(x => {
            return Vector3.SqrMagnitude(x.position - point);
        });

        return result;
    }
    #endregion

    #endregion
}
