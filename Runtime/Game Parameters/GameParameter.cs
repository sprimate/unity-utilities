using System;
using System.Collections;
using System.Collections.Specialized;
using UnityObservables;

public abstract class GameParameter<T> : Observable<T>
{
    public GameParameter(T val) : base(val) { }
    protected SortedList setPreProcessors = new SortedList();
    protected SortedList getPreProcessors = new SortedList();
    public T RawValue => value;
    public override T Value 
    {
        get { return ApplyPreProcessors(base.Value, getPreProcessors); }
    }

    protected override T PreProcessSetValue(T incomingVal)
    {
        return ApplyPreProcessors(incomingVal, setPreProcessors);
    }

    public GameParameterModification<T> AddGetPreProcessor(Func<T, T> prerocessor, int priority = 0)
    {
        return AddPreProcessor(prerocessor, priority, getPreProcessors);

    }
    public GameParameterModification<T> AddSetPreProcessor(Func<T, T> prerocessor, int priority = 0)
    {
        return AddPreProcessor(prerocessor, priority, setPreProcessors);
    }

    GameParameterModification<T> AddPreProcessor(Func<T, T> preprocessor, int priority, SortedList preprocessors)
    {
        return new GameParameterModification<T>(this, preprocessor, priority, preprocessors);
    }

    protected virtual T ApplyPreProcessors(T value, SortedList preprocessors)
    {
        foreach(GameParameterModification<T> modification in preprocessors.Values)
        {
            if (modification?.preprocessor != null)
            {
                value = modification.preprocessor(value);
            }
        }

        return value;
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