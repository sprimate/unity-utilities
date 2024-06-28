using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public static class ActionExtension
{
    static bool shouldDebug = false;

    //Provide better stack trace visiblity in builds, at the expense of execution speed 
    static void InvokeSafelyInternal(Delegate[] delegateList, params object[] parameters)
    {
        foreach (var d in delegateList)
        {
            try
            {
                if (shouldDebug)
                {
                    Debug.Log("Running Delegate: " + d.Target + "|" + d.Method);
                }

                d.DynamicInvoke(parameters);
            }
            catch (Exception e)
            {
                Debug.LogError("Exception in Action Invocation: " + e);
            }
        }
    }

    static void InvokeSafelyInternal(Delegate d, params object[] parameters)
    {
        //Only do the more expensive execution in Editor and Debug builds
        if (Application.isEditor || Debug.isDebugBuild)
        {
            if (d != null)
            {
                InvokeSafelyInternal(d.GetInvocationList(), parameters);
            }
        }
        else
        {
            try
            {
                d?.DynamicInvoke(parameters);
            }
            catch (Exception e)
            {
                Debug.LogError("Exception in Delegate Invocation: " + e);
            }
        }
    }

    public static void SafeInvoke(this Action a)
    {
        InvokeSafelyInternal(a);
    }

    public static void SafeInvoke<T>(this Action<T> a, T ret)
    {

        InvokeSafelyInternal(a, ret);
    }

    public static void SafeInvoke(this UnityAction a)
    {
        InvokeSafelyInternal(a);
    }
    public static void SafeInvoke<T>(this UnityAction<T> a, T ret)
    {

        InvokeSafelyInternal(a, ret);
    }

    public static void SafeInvoke<T1, T2>(this Action<T1, T2> a, T1 ret, T2 ret2)
    {
        InvokeSafelyInternal(a, ret, ret2);
    }

    public static void SafeInvoke<T1, T2, T3>(this Action<T1, T2, T3> a, T1 ret, T2 ret2, T3 ret3)
    {

        InvokeSafelyInternal(a, ret, ret2, ret3);
    }

    public static void SafeInvoke<T1, T2>(this UnityAction<T1, T2> a, T1 ret, T2 ret2)
    {

        InvokeSafelyInternal(a, ret, ret2);
    }

    public static void SafeInvoke(this UnityEvent e)
    {
        if (e != null)
        {
            Action actionWrapper = e.Invoke;//isn't technically of type delegate
            InvokeSafelyInternal(actionWrapper);
        }
    }

    public static UnityAction ToUnityAction(this Action a)
    {
        return () =>
        {
            a.SafeInvoke();
        };
    }
    public static Action ToAction(this UnityAction a)
    {
        return () =>
        {
            a.SafeInvoke();
        };
    }

    public static Action<T> ToAction<T>(this UnityAction<T> a)
    {
        return (t) =>
        {
            a.SafeInvoke(t);
        };
    }

    public static UnityEvent ToUnityEvent(this Action e)
    {
        UnityEvent ret = new UnityEvent();
        ret.AddListener(e.ToUnityAction());
        return ret;
    }

    public static void SafeAddListener(this UnityEvent e, UnityAction action)
    {
        if (e == null)
        {
            e = new UnityEvent();
        }

        e.AddListener(action);
    }

    /// <summary>
    /// e1 and e2 are the same thing.
    /// The parameter passed when using the extension method is the same as the second parameter, but as a reference
    /// </summary>

    public static void AddListenerOnce(this UnityEvent e1, ref UnityEvent e2, UnityAction action)
    {
        AddOnce(ref e2, action);
    }

    public static void AddOnce(ref UnityEvent a, UnityAction actionToRunOnce)
    {
        bool shouldRun = true;
        UnityAction toAdd = () =>
        {
            if (shouldRun)
            {
                shouldRun = false;
                actionToRunOnce();
            }
        };

        a.AddListener(toAdd);
    }

    public static void AddOnce(this Action a, ref Action b, Action actionToRunOnce)
    {
        AddOnce(ref b, actionToRunOnce);
    }

    public static void AddOnce(ref Action a, Action actionToRunOnce)
    {
        bool shouldRun = true;
        Action toAdd = () =>
        {
            if (shouldRun)
            {
                shouldRun = false;
                actionToRunOnce();
            }
        };

        a += toAdd;
    }

    public static void AddOnce<T>(this Action<T> a, ref Action<T> b, Action<T> actionToRunOnce)
    {
        AddOnce(ref b, actionToRunOnce);
    }

    public static void AddOnce<T>(ref Action<T> a, Action<T> actionToRunOnce)
    {
        bool shouldRun = true;
        Action<T> toAdd = (val) =>
        {
            if (shouldRun)
            {
                shouldRun = false;
                actionToRunOnce(val);
            }
        };

        a += toAdd;
    }

    public static void AddOnce<T1, T2>(this Action<T1, T2> a, ref Action<T1, T2> b, Action<T1, T2> actionToRunOnce)
    {
        AddOnce(ref b, actionToRunOnce);
    }

    public static void AddOnce<T1, T2>(ref Action<T1, T2> a, Action<T1, T2> actionToRunOnce)
    {
        bool shouldRun = true;
        Action<T1, T2> toAdd = (val1, val2) =>
        {
            if (shouldRun)
            {
                shouldRun = false;
                actionToRunOnce(val1, val2);
            }
        };

        a += toAdd;
    }
}