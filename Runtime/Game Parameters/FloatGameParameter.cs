using System;
using UnityEngine.Assertions;

[Serializable]
[Testable]
public partial class FloatGameParameter : NumberGameParameter<float>
{
    public FloatGameParameter(float val) : base(val) { }

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
        FloatGameParameter floatParam1 = new FloatGameParameter(10f);
        FloatGameParameter floatParam2 = new FloatGameParameter(20f);

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