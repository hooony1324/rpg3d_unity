using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    private static bool isQuitting = false;
    public static T Instance
    {
        get
        {
            if (instance == null && !isQuitting)
                instance = FindFirstObjectByType<T>(FindObjectsInactive.Include) ?? new GameObject(typeof(T).Name).AddComponent<T>();
            return instance;
        }

    }

    protected virtual void OnApplicationQuit() => isQuitting = true;
}