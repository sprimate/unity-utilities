using System;
using UnityEngine;
using UnityEngine.Assertions;

[Serializable]
[Testable]
public partial class FloatGameParameter : NumberGameParameter<float>
{
    public FloatGameParameter(float val) : base(val) { }
    public FloatGameParameter() : this(default) { }//required for inspector

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
        x2Id.Clean();
        Assert.AreEqual(floatParam1.RawValue + 5, floatParam1.Value);
        plus5Id.Clean(); //back to raw 10

        floatParam2.Value = 20f;
        Assert.AreEqual(20f, floatParam2.Value);
        var div2Id = floatParam2.AddSetPreProcessor(val => val / 2);
        floatParam2.Value = 20f;
        Assert.AreEqual(10f, floatParam2.Value);
        Assert.AreEqual(floatParam2.RawValue, floatParam2.Value);
        div2Id.Clean();
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


        //Test priorities
        FloatGameParameter priorityParamTest = new FloatGameParameter(100f);

        var initialPriorityModification = priorityParamTest.AddGetPreProcessor(val => val * 2f, 10);
        Assert.AreEqual(200f, priorityParamTest.Value);
        var modification = priorityParamTest.AddGetPreProcessor(val => val + 10, 0);
        Assert.AreEqual(210f, priorityParamTest.Value);
        modification.priority = 20;
        Assert.AreEqual(220f, priorityParamTest.Value);
        modification.Clean();
        Assert.AreEqual(200f, priorityParamTest.Value);
        priorityParamTest.AddGetPreProcessor(val => val + 5, int.MaxValue);
        Assert.AreEqual(210f, priorityParamTest.Value);
        var samePriorityModification = priorityParamTest.AddGetPreProcessor(val => val + 2, 10);//same priority as the multiplication means it should happen after
        Assert.IsTrue(samePriorityModification.GetTruePriority() < initialPriorityModification.GetTruePriority());
        Assert.AreEqual(10, samePriorityModification.priority);
        Assert.AreEqual(212f, priorityParamTest.Value);

        var anotherSamePriorityModification = priorityParamTest.AddGetPreProcessor(val => val + 2, 10);//same priority as the multiplication means it should happen after
        Assert.IsTrue(anotherSamePriorityModification.GetTruePriority() < samePriorityModification.GetTruePriority());
        Assert.AreEqual(10, anotherSamePriorityModification.priority);
        Debug.Log(anotherSamePriorityModification.GetTruePriority() + " < " + samePriorityModification.GetTruePriority() + " < " + initialPriorityModification.GetTruePriority());
        Assert.AreEqual(214f, priorityParamTest.Value);

    }
}