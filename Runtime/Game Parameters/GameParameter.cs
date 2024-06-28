using System;
using System.Collections;
using System.Collections.Specialized;
using UnityObservables;

public abstract class GameParameter<T> : Observable<T>
{
    protected OrderedDictionary setPreProcessors = new OrderedDictionary();
    protected OrderedDictionary getPreProcessors = new OrderedDictionary();
    public T RawValue => value;
    public override T Value 
    {
        get { return ApplyPreProcessors(base.Value, ref getPreProcessors); }
    }

    protected override T PreProcessSetValue(T incomingVal)
    {
        return ApplyPreProcessors(incomingVal, ref setPreProcessors);
    }

    public Guid AddGetPreProcessor(Func<T, T> prerocessor)
    {
        return AddPreProcessor(prerocessor, ref getPreProcessors);

    }
    public Guid AddSetPreProcessor(Func<T, T> prerocessor)
    {
        return AddPreProcessor(prerocessor, ref setPreProcessors);
    }

    Guid AddPreProcessor(Func<T, T> preprocessor, ref OrderedDictionary preprocessors)
    {
        var ret = Guid.NewGuid();
        preprocessors[ret] = preprocessor;
        return ret;
    }

    public void RemoveSetPreProcessor(Guid guid)
    {
        setPreProcessors.Remove(guid);
    }

    public void RemoveGetPreProcessor(Guid guid)
    {
        getPreProcessors.Remove(guid);
    }

    protected virtual T ApplyPreProcessors(T value, ref OrderedDictionary preprocessors)
    {
        foreach(DictionaryEntry de in preprocessors)
        {
            Func<T, T> preprocessor = de.Value as Func<T, T>;
            if (preprocessor != null)
            {
                value = preprocessor(value);
            }
        }

        return value;
    }

    public override void SetValue(T value, bool _forceSendEvents = false)
    {
        base.SetValue(ApplyPreProcessors(value, ref setPreProcessors), _forceSendEvents);
    }

}

public abstract class NumberGameParameter<T> : GameParameter<T> where T : struct, IComparable, IComparable<T>, IConvertible, IEquatable<T>, IFormattable
{
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