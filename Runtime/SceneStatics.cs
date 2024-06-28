using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneStatics
{
    static bool setSceneUnload;
    static Dictionary<Type, Dictionary<object, object>> staticRefs = new Dictionary<Type, Dictionary<object, object>>();

    public static T Get<T>(object key, object caller, Func<T> defaultValueFunc = null)
    {
        return Get<T>(key, caller.GetType(), defaultValueFunc);
    }

    public static T Get<T>(object key, Type t, Func<T> defaultValueFunc = null)
    {
        SetSceneUnloadCallback();
        Dictionary<object, object> typeDict;
        if (staticRefs.TryGetValue(t, out typeDict))
        {
            object retObj;
            if (typeDict.TryGetValue(key, out retObj))
            {
                if (retObj is T)
                {
                    return (T)retObj;
                }
            }
        }

        var ret = default(T);
        if (defaultValueFunc != null)
        {
            var defaultValue = defaultValueFunc.Invoke();
            if (!defaultValue.Equals(ret))
            {
                Set(key, defaultValue, t);
                return defaultValue;
            }
        }

        return ret;
    }

    public static void Set(object key, object value, object caller)
    {
        Set(key, value, caller.GetType());
    }

    public static void Set(object key, object value, Type t)
    {
        SetSceneUnloadCallback();
        if (!staticRefs.ContainsKey(t))
        {
            staticRefs[t] = new Dictionary<object, object>();
        }

        staticRefs[t][key] = value;
    }

    static void SetSceneUnloadCallback()
    {
        if (!setSceneUnload)
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            setSceneUnload = true;
        }
    }
    static void OnSceneUnloaded(Scene scene)
    {
        staticRefs = new Dictionary<Type, Dictionary<object, object>>();
    }
}
