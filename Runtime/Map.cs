
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Potentially has some issues when both T1 and T2 are the same type.
/// </summary>
/// <typeparam name="T1"></typeparam>
/// <typeparam name="T2"></typeparam>
public class Map<T1, T2> : IEnumerable<KeyValuePair<T1, T2>>
{
    public Dictionary<T1, T2> ForwardDict = new Dictionary<T1, T2>();
    public Dictionary<T2, T1> ReverseDict = new Dictionary<T2, T1>();

    public Map()
    {
        this.Forward = new Indexer<T1, T2>(ForwardDict);
        this.Reverse = new Indexer<T2, T1>(ReverseDict);
    }

    public class Indexer<T3, T4>
    {
        private Dictionary<T3, T4> _dictionary;
        public Indexer(Dictionary<T3, T4> dictionary)
        {
            _dictionary = dictionary;
        }
        public T4 this[T3 index]
        {
            get
            {
                try { return _dictionary[index]; }
                catch (KeyNotFoundException ke)
                {
                   /* //Don't throw errors if we haven't spawned everything yet
                    if (Time.frameCount <= NetworkedGraphicalEffectManager.spawnedFrame)
                    {
                        return default(T4);
                    }
                    */
                    /*Debug.LogError(Time.frameCount + ".) KeyNotFoundExecption. Key: " + index);
                    Debug.LogError("Are you sure you're requesting the instantiated graphical effect, or are you accidentally requesting the prefab??");
                    
                    throw ke;
                    */
                    return default(T4);
                }
            }
            set { _dictionary[index] = value; }
        }
    }

    public void AddOrReplace(T1 t1, T2 t2)
    {
        ForwardDict[t1] = t2;
        ReverseDict[t2] = t1;
    }

    public void Add(T1 t1, T2 t2)
    {
        ForwardDict.Add(t1, t2);
        ReverseDict.Add(t2, t1);
    }

    public void Remove(T1 t1)
    {
        var t2 = ForwardDict[t1];
        ForwardDict.Remove(t1);
        ReverseDict.Remove(t2);
    }

    public void Remove(T2 t2)
    {
        var t1 = ReverseDict[t2];
        ForwardDict.Remove(t1);
        ReverseDict.Remove(t2);
    }

    public void Clear()
    {
        ForwardDict.Clear();
        ReverseDict.Clear();
    }

    public bool Contains(T1 t1)
    {
        return ForwardDict.ContainsKey(t1);
    }

    public bool Contains(T2 t2)
    {
        return ReverseDict.ContainsKey(t2);
    }

    public T2 Get(T1 t1)
    {
        return ForwardDict[t1];
    }

    public T1 Get(T2 t2)
    {
        return ReverseDict[t2];
    }

    public bool TryGetValue2(T1 t1, out T2 t2)//Unambiguous call when using same type
    {
        if (TryGetValue(t1, out t2))
        {
            return true;
        }
        else
        {
            var success = TryGetValue((T2)(object) t1, out var toCastToT2);
            if (success)
            {
                t2 = (T2)(object) toCastToT2;
                return true;
            }

            return false;
        }
    }

    public bool TryGetValue(T1 t1, out T2 t2)
    {
        return ForwardDict.TryGetValue(t1, out t2);
    }

    public bool TryGetValue(T2 t2, out T1 t1)
    {
        return ReverseDict.TryGetValue(t2, out t1);
    }

    public IEnumerator<KeyValuePair<T1, T2>> GetEnumerator()
    {
        return ForwardDict.GetEnumerator();
    }

    // Must also implement IEnumerable.GetEnumerator, but implement as a private method.
    private IEnumerator GetEnumerator1()
    {
        return this.GetEnumerator();
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator1();
    }

    public T1 this[T2 t2]
    {
        get { return ReverseDict[t2]; }
        set { AddOrReplace(value, t2); }
    }

    public T2 this[T1 t1]
    {
        get { return ForwardDict[t1]; }
        set { AddOrReplace(t1, value); }
    }
    public Indexer<T1, T2> Forward { get; private set; }
    public Indexer<T2, T1> Reverse { get; private set; }

    public int Count { get => ForwardDict.Count; }
}