using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Frame
{
    public Frame(int val)
    {
        value = val;
    }
    const float referenceFrameRate = 60;
    public int value;
    public float Seconds => ToSeconds();
    public float ToSeconds()
    {
        return (float)value / referenceFrameRate;
    }

    public static Frame FromSeconds(float seconds)
    {
        return new Frame(Mathf.CeilToInt(seconds * referenceFrameRate));
    }

    // change explicit to implicit depending on what you need
    public static implicit operator Frame(int v)
    {
        return new Frame(v);
    }

    // change explicit to implicit depending on what you need
    public static implicit operator int(Frame f)
    {
        return f.value;
    }

    public override string ToString()
    {
        return value.ToString();
    }
}

[Serializable]
public struct FrameRange
{
    public Frame min;
    public Frame max;

    public FrameRange(Frame _min, Frame _max)
    {
        min = _min;
        max = _max;
    }

    // change explicit to implicit depending on what you need
    public static explicit operator FrameRange(Vector2Int v)
    {
        return new FrameRange(v.x, v.y);
    }

    // change explicit to implicit depending on what you need
    public static explicit operator Vector2Int(FrameRange f)
    {
        return new Vector2Int(f.min, f.max);
    }
}