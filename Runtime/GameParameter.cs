using System;
using System.Collections;
using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.Assertions;
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

[Serializable]
[Testable]
public partial class IntGameParameter : NumberGameParameter<int>{
    // Arithmetic operators
    public static int operator +(IntGameParameter a, int b)=> a.Value + b;
    public static int operator +(int a, IntGameParameter b) => b + a;
    public static int operator -(IntGameParameter a, int b) => a.Value - b;
    public static int operator -(int a, IntGameParameter b) => a - b.Value;
    public static int operator *(IntGameParameter a, int b) => a.Value * b;
    public static int operator *(int a, IntGameParameter b) => b * a;
    public static int operator /(IntGameParameter a, int b) => a.Value / b;
    public static int operator /(int a, IntGameParameter b) => a / b.Value;

    public static void RunTests()
    {
        IntGameParameter intParam1 = new IntGameParameter();
        IntGameParameter intParam2 = new IntGameParameter();
        intParam1.SetValue(10);

        Assert.AreEqual(10, intParam1.Value);
        var x2Id = intParam1.AddGetPreProcessor((val) => val * 2);
        Assert.AreEqual(intParam1.RawValue * 2, intParam1.Value);
        var plus5Id = intParam1.AddGetPreProcessor(val => val + 5);
        Assert.AreEqual(intParam1.RawValue * 2 + 5, intParam1.Value);
        intParam1.RemoveGetPreProcessor(x2Id);
        Assert.AreEqual(intParam1.RawValue + 5, intParam1.Value);
        intParam1.RemoveGetPreProcessor(plus5Id); //back to raw 10

        intParam2.Value = 20;
        Assert.AreEqual(20, intParam2.Value);
        var div2Id = intParam2.AddSetPreProcessor(val => val / 2);
        intParam2.Value = 20;
        Assert.AreEqual(10, intParam2.Value);
        Assert.AreEqual(intParam2.RawValue, intParam2.Value);
        intParam2.RemoveSetPreProcessor(div2Id);
        Assert.AreEqual(10, intParam2.Value);
        intParam2.Value = 20; //back to raw 20
        Assert.AreEqual(20, intParam2.Value);


        Assert.IsTrue(intParam1 < intParam2);
        Assert.IsTrue(intParam2 > intParam1);
        Assert.IsTrue(intParam1 <= intParam2);
        Assert.IsTrue(intParam2 >= intParam1);
        Assert.IsTrue(intParam1 <= 10);
        Assert.IsTrue(10 >= intParam1);
        Assert.IsTrue(intParam2 >= 20);
        Assert.IsTrue(20 <= intParam2);

        Assert.AreEqual(30, intParam1 + 20);
        Assert.AreEqual(30, 20 + intParam1);
        Assert.AreEqual(-10, intParam1 - 20);
        Assert.AreEqual(10, 20 - intParam1);
        Assert.AreEqual(200, intParam1 * 20);
        Assert.AreEqual(200, 20 * intParam1);

        Assert.AreEqual(0.5f, (float)intParam1 / 20.0f);
        Assert.AreEqual(2.0f, 20.0f / (float)intParam1);
    }
}

[Serializable]
[Testable]
public partial class FloatGameParameter : NumberGameParameter<float>
{
    // Arithmetic operators
    public static float operator +(FloatGameParameter a, float b) => a.Value + b;
    public static float operator +(float a, FloatGameParameter b) => b + a;
    public static float operator -(FloatGameParameter a, float b) => a.Value - b;
    public static float operator -(float a, FloatGameParameter b) => a - b.Value;
    public static float operator *(FloatGameParameter a, float b) => a.Value * b;
    public static float operator *(float a, FloatGameParameter b) => b * a;
    public static float operator /(FloatGameParameter a, float b) => a.Value / b;
    public static float operator /(float a, FloatGameParameter b) => a / b.Value;

    public static void RunTests()
    {
        FloatGameParameter floatParam1 = new FloatGameParameter();
        FloatGameParameter floatParam2 = new FloatGameParameter();
        floatParam1.SetValue(10f);

        Assert.AreEqual(10f, floatParam1.Value);
        var x2Id = floatParam1.AddGetPreProcessor((val) => val * 2);
        Assert.AreEqual(floatParam1.RawValue * 2, floatParam1.Value);
        var plus5Id = floatParam1.AddGetPreProcessor(val => val + 5);
        Assert.AreEqual(floatParam1.RawValue * 2 + 5, floatParam1.Value);
        floatParam1.RemoveGetPreProcessor(x2Id);
        Assert.AreEqual(floatParam1.RawValue + 5, floatParam1.Value);
        floatParam1.RemoveGetPreProcessor(plus5Id); //back to raw 10

        floatParam2.Value = 20f;
        Assert.AreEqual(20f, floatParam2.Value);
        var div2Id = floatParam2.AddSetPreProcessor(val => val / 2);
        floatParam2.Value = 20f;
        Assert.AreEqual(10f, floatParam2.Value);
        Assert.AreEqual(floatParam2.RawValue, floatParam2.Value);
        floatParam2.RemoveSetPreProcessor(div2Id);
        Assert.AreEqual(10f, floatParam2.Value);
        floatParam2.Value = 20f; //back to raw 20
        Assert.AreEqual(20f, floatParam2.Value);

        Assert.IsTrue(floatParam1 < floatParam2);
        Assert.IsTrue(floatParam2 > floatParam1);
        Assert.IsTrue(floatParam1 <= floatParam2);
        Assert.IsTrue(floatParam2 >= floatParam1);
        Assert.IsTrue(floatParam1 <= 10f);
        Assert.IsTrue(10f >= floatParam1);
        Assert.IsTrue(floatParam2 >= 20f);
        Assert.IsTrue(20f <= floatParam2);

        Assert.AreEqual(30f, floatParam1 + 20f);
        Assert.AreEqual(30f, 20f + floatParam1);
        Assert.AreEqual(-10f, floatParam1 - 20f);
        Assert.AreEqual(10f, 20f - floatParam1);
        Assert.AreEqual(200f, floatParam1 * 20f);
        Assert.AreEqual(200f, 20f * floatParam1);

        Assert.AreEqual(0.5f, floatParam1 / 20f);
        Assert.AreEqual(2.0f, 20f / floatParam1);
    }

}