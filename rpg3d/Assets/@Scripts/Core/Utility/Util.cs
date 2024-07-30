using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Util : MonoBehaviour
{
    public static T GetOrAddComponent<T>(GameObject go) where T : UnityEngine.Component
    {
        T component = go.GetComponent<T>();
        if (component == null)
            component = go.AddComponent<T>();
        return component;
    }

    public static T FindAncestor<T>(GameObject go) where T : UnityEngine.Object
    {
        Transform t = go.transform;
        while (t != null)
        {
            T component = t.GetComponent<T>();
            if (component != null)
                return component;
            t = t.parent;
        }
        return null;
    }

    public static GameObject FindChild(GameObject go, string name = null, bool recursive = false)
    {
        Transform transform = FindChild<Transform>(go, name, recursive);
        if (transform == null)
            return null;

        return transform.gameObject;
    }

    public static T FindChild<T>(GameObject go, string name = null, bool recursive = false) where T : UnityEngine.Object
    {
        if (go == null)
            return null;

        if (recursive == false)
        {
            for (int i = 0; i < go.transform.childCount; i++)
            {
                Transform transform = go.transform.GetChild(i);
                if (string.IsNullOrEmpty(name) || transform.name == name)
                {
                    T component = transform.GetComponent<T>();
                    if (component != null)
                        return component;
                }
            }
        }
        else
        {
            foreach (T component in go.GetComponentsInChildren<T>())
            {
                if (string.IsNullOrEmpty(name) || component.name == name)
                    return component;
            }
        }

        return null;
    }

    public static Color HexToColor(string color)
    {
        Color parsedColor;

        if (color.Contains("#") == false)
            ColorUtility.TryParseHtmlString("#" + color, out parsedColor);
        else
            ColorUtility.TryParseHtmlString(color, out parsedColor);

        return parsedColor;
    }

    public static readonly Dictionary<Type, Array> enumDict = new Dictionary<Type, Array>();
    public static T GetRandomEnumValue<T>() where T : struct, Enum
    {
        Type type = typeof(T);

        if (!enumDict.ContainsKey(type))
            enumDict[type] = Enum.GetValues(type);

        Array values = enumDict[type];

        int index = UnityEngine.Random.Range(0, values.Length);
        return (T)values.GetValue(index);
    }
    public static T ParseEnum<T>(string value)
    {
        return (T)Enum.Parse(typeof(T), value, true);
    }

    #region Size

    public static long OneGB = 1000000000;
    public static long OneMB = 1000000;
    public static long OneKB = 1000;

    /// <summary> 바이트 <paramref name="byteSize"/> 사이즈에 맞게끔 적절한 단위 <see cref="ESizeUnits"/> 타입을 가져온다 </summary>
    public static ESizeUnits GetProperByteUnit(long byteSize)
    {
        if (byteSize >= OneGB)
            return ESizeUnits.GB;
        else if (byteSize >= OneMB)
            return ESizeUnits.MB;
        else if (byteSize >= OneKB)
            return ESizeUnits.KB;
        return ESizeUnits.Byte;
    }

    /// <summary> 바이트를 <paramref name="byteSize"/> <paramref name="unit"/> 단위에 맞게 숫자를 변환한다 </summary>
    public static long ConvertByteByUnit(long byteSize, ESizeUnits unit)
    {
        return (long)((byteSize / (double)System.Math.Pow(1024, (long)unit)));
    }

    /// <summary> 바이트를 <paramref name="byteSize"/> 단위와 함께 출력이 가능한 문자열 형태로 변환한다 </summary>
    public static string GetConvertedByteString(long byteSize, ESizeUnits unit, bool appendUnit = true)
    {
        string unitStr = appendUnit ? unit.ToString() : string.Empty;
        return $"{ConvertByteByUnit(byteSize, unit).ToString("0.00")}{unitStr}";
    }

    #endregion
}
public enum ESizeUnits
{
    Byte,
    KB,
    MB,
    GB
}