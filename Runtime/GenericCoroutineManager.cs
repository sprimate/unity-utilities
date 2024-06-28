using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class GenericCoroutineManager : MonoSingleton<GenericCoroutineManager>
{
    Thread mainThread;

    Dictionary<float, WaitForSeconds> waitForSecondsCache = new Dictionary<float, WaitForSeconds>();
    Dictionary<float, WaitForSecondsRealtime> waitForSecondsRealtimeCache = new Dictionary<float, WaitForSecondsRealtime>();
    public WaitForEndOfFrame waitForEndOfFrame { get; private set; } = new WaitForEndOfFrame();
    public WaitForFixedUpdate waitForFixedUpdate { get; private set; } = new WaitForFixedUpdate();
    Dictionary<Action, Action> nullificationActions = new Dictionary<Action, Action>();

    Action runOnLateUpdate;
    public static void RunOnLateUpdate(Action a, object objectToCheckNullification, bool removeFromAction = false)
    {
        if (removeFromAction)
        {
            if (instance.nullificationActions.ContainsKey(a))
            {
                instance.runOnLateUpdate -= instance.nullificationActions[a];
                instance.nullificationActions.Remove(a);
            }
        }
        else
        {
            instance.nullificationActions[a] = GetObjectNullificationWrapper(a, objectToCheckNullification, () => { RunOnLateUpdate(a, objectToCheckNullification, true); });
            instance.runOnLateUpdate += instance.nullificationActions[a];
        }
    }
    Action runOnUpdate;
    public static void RunOnUpdate(Action a, object objectToCheckNullification, bool removeFromAction = false)
    {
        if (removeFromAction)
        {
            if (instance.nullificationActions.ContainsKey(a))
            {
                instance.runOnUpdate -= instance.nullificationActions[a];
                instance.nullificationActions.Remove(a);
            }
        }
        else
        {
            instance.nullificationActions[a] = GetObjectNullificationWrapper(a, objectToCheckNullification, () => { RunOnUpdate(a, objectToCheckNullification, true); });
            instance.runOnUpdate += instance.nullificationActions[a];
        }
    }
    Action runOnFixedUpdate;
    public static void RunOnFixedUpdate(Action a, object objectToCheckNullification, bool removeFromAction = false)
    {
        if (removeFromAction)
        {
            if (instance.nullificationActions.ContainsKey(a))
            {
                instance.runOnFixedUpdate -= instance.nullificationActions[a];
                instance.nullificationActions.Remove(a);
            }
        }
        else
        {
            instance.nullificationActions[a] = GetObjectNullificationWrapper(a, objectToCheckNullification, () => { RunOnFixedUpdate(a, objectToCheckNullification, true); });
            instance.runOnFixedUpdate += instance.nullificationActions[a];
        }
    }

    Action runOnLateFixedUpdate;
    public static void RunOnLateFixedUpdate(Action a, object objectToCheckNullification, bool removeFromAction = false)
    {
        if (removeFromAction)
        {
            if (instance.nullificationActions.ContainsKey(a))
            {
                instance.runOnLateFixedUpdate -= instance.nullificationActions[a];
                instance.nullificationActions.Remove(a);
            }
        }
        else
        {
            instance.nullificationActions[a] = GetObjectNullificationWrapper(a, objectToCheckNullification, () => { RunOnLateFixedUpdate(a, objectToCheckNullification, true); });
            instance.runOnLateFixedUpdate += instance.nullificationActions[a];
        }
    }
    static Action runOnNextUpdate;
    public void RunOnNextUpdate(Action a, object objectToCheckNullification, bool removeFromAction = false)
    {
        if (removeFromAction)
        {
            if (nullificationActions.ContainsKey(a))
            {
                runOnNextUpdate -= nullificationActions[a];
                nullificationActions.Remove(a);
            }
        }
        else
        {
            nullificationActions[a] = GetObjectNullificationWrapper(a, objectToCheckNullification, null);
            runOnNextUpdate += nullificationActions[a];
        }
    }

    static Action runOnNextLateUpdate;
    public void RunOnNextLateUpdate(Action a, object objectToCheckNullification, bool removeFromAction = false)
    {
        if (removeFromAction)
        {
            if (nullificationActions.ContainsKey(a))
            {
                runOnNextLateUpdate -= nullificationActions[a];
                nullificationActions.Remove(a);
            }
        }
        else
        {
            nullificationActions[a] = GetObjectNullificationWrapper(a, objectToCheckNullification, null);
            runOnNextLateUpdate += nullificationActions[a];
        }
    }

    static Action runOnNextFixedUpdate;
    public void RunOnNextFixedUpdate(Action a, object objectToCheckNullification, bool removeFromAction = false)
    {
        if (removeFromAction)
        {
            if (nullificationActions.ContainsKey(a))
            {
                runOnNextFixedUpdate -= nullificationActions[a];
                nullificationActions.Remove(a);
            }
        }
        else
        {
            nullificationActions[a] = GetObjectNullificationWrapper(a, objectToCheckNullification, null);
            runOnNextFixedUpdate += nullificationActions[a];
        }
    }

    static Action runOnNextLateFixedUpdate;
    public void RunOnNextLateFixedUpdate(Action a, object objectToCheckNullification, bool removeFromAction = false)
    {
        if (removeFromAction)
        {
            if (nullificationActions.ContainsKey(a))
            {
                runOnNextLateFixedUpdate -= nullificationActions[a];
                nullificationActions.Remove(a);
            }
        }
        else
        {
            nullificationActions[a] = GetObjectNullificationWrapper(a, objectToCheckNullification, null);
            runOnNextLateFixedUpdate += nullificationActions[a];
        }
    }
    public static WaitForEndOfFrame WaitForEndOfFrame()
    {
        return instance.waitForEndOfFrame;
    }
    public static WaitForSeconds WaitForSeconds(float seconds)
    {
        if (!instance.waitForSecondsCache.ContainsKey(seconds))
        {
            instance.waitForSecondsCache[seconds] = new WaitForSeconds(seconds);
        }

        return instance.waitForSecondsCache[seconds];
    }
    public static WaitForSecondsRealtime WaitForSecondsRealtime(float seconds)
    {
        if (!instance.waitForSecondsRealtimeCache.ContainsKey(seconds))
        {
            instance.waitForSecondsRealtimeCache[seconds] = new WaitForSecondsRealtime(seconds);
        }

        return instance.waitForSecondsRealtimeCache[seconds];
    }

    public static IEnumerator RunInSeconds(float numSeconds, Action action, object cancelOnObjectNullification)
    {
        action = GetObjectNullificationWrapper(action, cancelOnObjectNullification, null);
        if (instance.mainThread != Thread.CurrentThread)
        {
            runOnNextUpdate += () => { RunInSeconds(numSeconds, (action), null); };
            return null;
        }

        var ret = instance.RunInSecondsCoroutine(numSeconds, action);
        instance.StartCoroutine(ret);
        return ret;
    }

    public static IEnumerator RunInSecondsRealtime(float numSeconds, Action action, object objectToCheckNullification)
    {
        action = GetObjectNullificationWrapper(action, objectToCheckNullification, null);

        if (instance.mainThread != Thread.CurrentThread)
        {
            runOnNextUpdate += () => { RunInSecondsRealtime(numSeconds, action, null); };
            return null;
        }
        var ret = instance.RunInSecondsRealtimeCoroutine(numSeconds, action);
        instance.StartCoroutine(ret);
        return ret;
    }

    public static Action RunAfterFixedFrames(int numFrames, Action action, object objectToCheckNullification)
    {
        action = GetObjectNullificationWrapper(action, objectToCheckNullification, null);
        if (instance.mainThread != Thread.CurrentThread)
        {
            runOnNextUpdate += () => { RunInFixedFrames(numFrames, action); };
            return null;
        }

        bool shouldCancel = false;
        Action cancel = () =>
        {
            shouldCancel = true;
        };

        Action eachFixed = null;
        eachFixed = () =>
        {
            if (numFrames <= 0)
            {
                action.SafeInvoke();
            }
            else if (!shouldCancel)
            {
                RunAfterFixedFrames(numFrames - 1, action, null);
            }

            instance.runOnLateFixedUpdate -= eachFixed;
        };

        instance.runOnLateFixedUpdate += eachFixed;
        var ret = RunInFixedFrames(numFrames, action);
        return cancel;
    }
    static Action GetObjectNullificationWrapper(Action actionToWrap, object objectToCheckNullification, Action onNull)
    {
        if (objectToCheckNullification == null)
        {
            return actionToWrap;
        }

        return () =>
        {
            try
            {
                bool isNull = false;
                if (objectToCheckNullification is UnityEngine.Object)
                {
                    isNull = !(objectToCheckNullification as UnityEngine.Object);
                }
                else
                {
                    isNull = objectToCheckNullification == null;
                }

                if (!isNull)
                {
                    actionToWrap.SafeInvoke();
                }
                else
                {
                    onNull.SafeInvoke();
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Exception running null-wrapped action. objectToCheckNullification [" + objectToCheckNullification + "]: " + e);
            }
        };
    }

    static IEnumerator RunInFixedFrames(int numFrames, Action action)
    {
        if (numFrames <= 0)
        {
            numFrames = 1;
        }

        while (numFrames-- > 0)
        {
            yield return instance.waitForFixedUpdate;
        }

        action?.Invoke();
    }


    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);//.DontDestroyOnLoad();
        mainThread = Thread.CurrentThread;
        StartCoroutine(LateFixedUpdate());
    }

    public static IEnumerator RunInFrames(int numFrames, Action action, object objectToCheckNullification)
    {
        action = GetObjectNullificationWrapper(action, objectToCheckNullification, null);

        if (instance.mainThread != Thread.CurrentThread)
        {
            runOnNextLateUpdate += () => { RunInFrames(numFrames, action, null); };
            return null;
        }

        var coroutine = instance.RunInFramesCoroutine(numFrames, action);
        instance.StartCoroutine(coroutine);
        return coroutine;
    }


    public static IEnumerator RunAfterFrame(Action action, object objectToCheckNullification)
    {
        return ExecuteAfterFrame(action, objectToCheckNullification);
    }

    public static IEnumerator ExecuteAfterFrame(Action action, object objectToCheckNullification)
    {
        action = GetObjectNullificationWrapper(action, objectToCheckNullification, null);
        if (instance.mainThread != Thread.CurrentThread)
        {
            runOnNextLateUpdate += () => { ExecuteAfterFrame(action, null); };
            return null;
        }
        var ret = instance.ExecuteAfterFrameCoroutine(action);
        instance.StartCoroutine(ret);
        return ret;
    }

    public static IEnumerator ExecuteAfter(Func<bool> condition, Action action, object objectToCheckNullification)
    {
        action = GetObjectNullificationWrapper(action, objectToCheckNullification, null);
        if (instance.mainThread != Thread.CurrentThread)
        {
            return null;
        }
        var ret = instance.ExecuteAfterCoroutine(condition, action);
        instance.StartCoroutine(ret);
        return ret;
    }

    IEnumerator EmptyCoroutine()
    {
        yield return null;
    }
    //  Dictionary<Func<bool>, WaitUntil> waitUntilCache = new Dictionary<Func<bool>, WaitUntil>(); 
    IEnumerator ExecuteAfterCoroutine(Func<bool> condition, Action after)
    {
        if (condition())
        {
            after.SafeInvoke();
            yield break;
        }
        yield return new WaitUntil(condition);
        after.SafeInvoke();
    }

    public static IEnumerator DoAfter(Func<bool> condition, Action after, object objectToCheckNullification, Action onTimeout = null, float timeout = 5f)
    {
        after = GetObjectNullificationWrapper(after, objectToCheckNullification, null);
        var ret = instance.DoAfterCoroutine(condition, after, onTimeout, timeout);
        instance.StartCoroutine(ret);
        return ret;
    }

    IEnumerator DoAfterCoroutine(Func<bool> condition, Action after, Action onTimeOut, float timeout)
    {
        float startTime = Time.realtimeSinceStartup;
        while (!condition() && Time.realtimeSinceStartup < startTime + timeout)
        {
            yield return null;
        }

        if (!condition())
        {
            if (onTimeOut != null)
            {
                onTimeOut?.Invoke();
            }
            else
            {
                UnityEngine.Debug.LogError("Timeout in DoAfterCoroutine after " + timeout + " seconds");
            }
        }
        else
        {
            after?.Invoke();
        }
    }

    public static IEnumerator RunAtEndOfFrame(Action action, object objectToCheckNullification)
    {
        return ExecuteAfterFrame(action, objectToCheckNullification);
    }

    public static IEnumerator ExecuteAtEndOfFrame(Action action, object objectToCheckNullification)
    {
        return ExecuteAfterFrame(action, objectToCheckNullification);
    }

    IEnumerator RunInFramesCoroutine(int numFrames, Action action)
    {
        for (int i = 0; i < numFrames; i++)
        {
            yield return null;
        }
        action.SafeInvoke();
    }

    IEnumerator RunInSecondsCoroutine(float seconds, Action action)
    {
        if (seconds > 0)
        {
            yield return WaitForSeconds(seconds);
        }

        action.SafeInvoke();
    }

    IEnumerator RunInSecondsRealtimeCoroutine(float seconds, Action action)
    {
        yield return WaitForSecondsRealtime(seconds);
        action.SafeInvoke();
    }
    IEnumerator ExecuteAfterFrameCoroutine(Action action)
    {
        yield return waitForEndOfFrame;
        action.SafeInvoke();
    }

    void Update()
    {
        if (runOnUpdate != null)
            runOnUpdate.SafeInvoke();

        if (runOnNextUpdate != null)
        {
            runOnNextUpdate.Invoke();
            runOnNextUpdate = null;
        }
    }

    private void FixedUpdate()
    {
        runOnFixedUpdate.SafeInvoke();
        if (runOnNextFixedUpdate != null)
        {
            runOnNextFixedUpdate.Invoke();
            runOnNextFixedUpdate = null;
        }
    }

    IEnumerator LateFixedUpdate()
    {
        while (true)
        {
            yield return waitForFixedUpdate;
            runOnLateFixedUpdate.SafeInvoke();
            if (runOnNextLateUpdate != null)
            {
                runOnNextLateFixedUpdate.SafeInvoke();
                runOnNextLateFixedUpdate = null;
            }
        }
    }

    void LateUpdate()
    {
        if (runOnLateUpdate != null)
        {
            runOnLateUpdate.Invoke();
        }

        if (runOnNextLateUpdate != null)
        {
            runOnNextLateUpdate.Invoke();
            runOnNextLateUpdate = null;
        }
    }
}