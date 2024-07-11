using System;
using UnityEngine.Assertions;

[Serializable]
[Testable]
public partial class IntGameParameter : NumberGameParameter<int>
{
    public IntGameParameter(int val) : base(val) { }
    public IntGameParameter() : this(default) { }//required for inspector
    // Arithmetic operators
    public static int operator +(IntGameParameter a, int b) => a.Value + b;
    public static int operator +(int a, IntGameParameter b) => b + a;
    public static int operator -(IntGameParameter a, int b) => a.Value - b;
    public static int operator -(int a, IntGameParameter b) => a - b.Value;
    public static int operator *(IntGameParameter a, int b) => a.Value * b;
    public static int operator *(int a, IntGameParameter b) => b * a;
    public static int operator /(IntGameParameter a, int b) => a.Value / b;
    public static int operator /(int a, IntGameParameter b) => a / b.Value;

    public static void RunTests()
    {
        IntGameParameter intParam1 = new IntGameParameter(10);
        IntGameParameter intParam2 = new IntGameParameter(20);

        Assert.AreEqual(10, intParam1.Value);
        var x2Id = intParam1.AddGetPreProcessor((val) => val * 2);
        Assert.AreEqual(x2Id.gameParameterPreprocessors.Count, 1);
        Assert.AreEqual(intParam1.RawValue * 2, intParam1.Value);
        var plus5Id = intParam1.AddGetPreProcessor(val => val + 5);
        Assert.AreEqual(intParam1.RawValue * 2 + 5, intParam1.Value);
        x2Id.Clean();
        Assert.AreEqual(intParam1.RawValue + 5, intParam1.Value);
        plus5Id.Clean(); //back to raw 10

        Assert.AreEqual(20, intParam2.Value);
        var div2Id = intParam2.AddSetPreProcessor(val => val / 2);
        intParam2.Value = 20;
        Assert.AreEqual(10, intParam2.Value);
        Assert.AreEqual(intParam2.RawValue, intParam2.Value);
        div2Id.Clean();
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