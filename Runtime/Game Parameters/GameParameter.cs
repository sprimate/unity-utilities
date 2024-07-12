using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using UnityObservables;

public abstract class GameParameter<T> : Observable<T>
{
    public GameParameter(T val) : base(val) { }

    protected PrioritizedPreProcessors<T> setPreProcessors = new PrioritizedPreProcessors<T>();
    protected PrioritizedPreProcessors<T> getPreProcessors = new PrioritizedPreProcessors<T>();
    public T RawValue => value;
    public override T Value 
    {
        get { return ApplyPreProcessors(base.Value, getPreProcessors); }
    }

    protected override T PreProcessSetValue(T incomingVal)
    {
        return ApplyPreProcessors(incomingVal, setPreProcessors);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="prerocessor"></param>
    /// <param name="priority">Higher Priority PreProcessors are processed first</param>
    /// <returns></returns>
    public GameParameterModification<T> AddGetPreProcessor(Func<(T val, object context), T> prerocessor, int priority = 0)
    {
        var valueBeforeNewPreprocessor = Value;
        var ret = AddPreProcessor(prerocessor, priority, getPreProcessors);
        ProcessEvents(valueBeforeNewPreprocessor, false);
        return ret;
    }

    public GameParameterModification<T> AddGetPreProcessor(Func<T, T> prerocessor, int priority = 0)
    {
        return AddGetPreProcessor((data) =>
        {
            return prerocessor.Invoke(data.val); 
        }, priority);
    }

    public GameParameterModification<T> AddSetPreProcessor(Func<(T val, object context), T> prerocessor, int priority = 0)
    {
        return AddPreProcessor(prerocessor, priority, setPreProcessors);
    }

    public GameParameterModification<T> AddSetPreProcessor(Func<T, T> prerocessor, int priority = 0)
    {
        return AddSetPreProcessor((data) =>
        {
            return prerocessor.Invoke(data.val);
        }, priority);
    }

    public void Clean(GameParameterModification<T> modification)
    {
        var valBefore = Value;
        modification.gameParameterPreprocessors.Remove(modification);
        modification.ResetPriority();
        ProcessEvents(valBefore, false);
    }

    GameParameterModification<T> AddPreProcessor(Func<(T val, object context), T> preprocessor, int priority, PrioritizedPreProcessors<T> preprocessors)
    {
        return new GameParameterModification<T>(this, preprocessor, priority, preprocessors);
    }

    protected virtual T ApplyPreProcessors(T value, PrioritizedPreProcessors<T> preprocessors)
    {
        return ApplyPreProcessors(value, null, preprocessors);
    }

    protected virtual T ApplyPreProcessors(T value, object context, PrioritizedPreProcessors<T> preprocessors)
    {
        foreach (GameParameterModification<T> modification in preprocessors)
        {
            if (modification?.preprocessor != null)
            {
                value = modification.preprocessor((value, context));
            }
        }

        return value;
    }

    public void SetValueWithContext(T value, object context, bool _forceSendEvents = false)
    {
        base.SetValue(ApplyPreProcessors(value, context, setPreProcessors), _forceSendEvents);
    }

    public override void SetValue(T value, bool _forceSendEvents = false)
    {
        base.SetValue(ApplyPreProcessors(value, setPreProcessors), _forceSendEvents);
    }
}

public abstract class NumberGameParameter<T> : GameParameter<T> where T : struct, IComparable, IComparable<T>, IConvertible, IEquatable<T>, IFormattable
{
    public NumberGameParameter(T val) : base(val) { }

    // Comparison operators between NumberGameParameter<T, TDerived> and NumberGameParameter<T, TDerived>
    public static bool operator <(NumberGameParameter<T> a, NumberGameParameter<T> b) => a.Value.CompareTo(b.Value) < 0;
    public static bool operator >(NumberGameParameter<T> a, NumberGameParameter<T> b) => a.Value.CompareTo(b.Value) > 0;
    public static bool operator <=(NumberGameParameter<T> a, NumberGameParameter<T> b) => a.Value.CompareTo(b.Value) <= 0;
    public static bool operator >=(NumberGameParameter<T> a, NumberGameParameter<T> b) => a.Value.CompareTo(b.Value) >= 0;

    // Comparison operators between NumberGameParameter<T, TDerived> and T
    public static bool operator <(NumberGameParameter<T> a, T b) => a.Value.CompareTo(b) < 0;
    public static bool operator <(T a, NumberGameParameter<T> b) => a.CompareTo(b.Value) < 0;
    public static bool operator >(NumberGameParameter<T> a, T b) => a.Value.CompareTo(b) > 0;
    public static bool operator >(T a, NumberGameParameter<T> b) => a.CompareTo(b.Value) > 0;
    public static bool operator <=(NumberGameParameter<T> a, T b) => a.Value.CompareTo(b) <= 0;
    public static bool operator <=(T a, NumberGameParameter<T> b) =>a.CompareTo(b.Value) <= 0;
    public static bool operator >=(NumberGameParameter<T> a, T b) => a.Value.CompareTo(b) >= 0;
    public static bool operator >=(T a, NumberGameParameter<T> b) => a.CompareTo(b.Value) >= 0;
}