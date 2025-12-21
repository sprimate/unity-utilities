using System.Collections;
using System.Collections.Generic;
using static HitTrax.CoreUtilities.SafeFunctions;

namespace HitTrax.CoreUtilities
{

    /// <summary>
    /// Potentially has some issues when both T1 and T2 are the same type.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    public class Map<T1, T2> : IEnumerable<KeyValuePair<T1, T2>>
    {
        public Dictionary<T1, T2> forwardDict = new Dictionary<T1, T2>();
        public Dictionary<T2, T1> reverseDict = new Dictionary<T2, T1>();

        public Map()
        {
            this.Forward = new Indexer<T1, T2>(forwardDict);
            this.Reverse = new Indexer<T2, T1>(reverseDict);
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
                    try
                    {
                        return _dictionary[index];
                    }
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
                set
                {
                    _dictionary[index] = value;
                }
            }
        }

        public void AddOrReplace(T1 t1, T2 t2)
        {
            forwardDict[t1] = t2;
            reverseDict[t2] = t1;
        }

        public void Add(T1 t1, T2 t2)
        {
            forwardDict.Add(t1, t2);
            reverseDict.Add(t2, t1);
        }

        public void Remove(T1 t1)
        {
            var t2 = forwardDict[t1];
            forwardDict.Remove(t1);
            reverseDict.Remove(t2);
        }

        public void Remove(T2 t2)
        {
            var t1 = reverseDict[t2];
            forwardDict.Remove(t1);
            reverseDict.Remove(t2);
        }

        public void Clear()
        {
            forwardDict.Clear();
            reverseDict.Clear();
        }

        public bool Contains(T1 t1)
        {
            return forwardDict.ContainsKey(t1);
        }

        public bool Contains(T2 t2)
        {
            return reverseDict.ContainsKey(t2);
        }

        public T2 Get(T1 t1)
        {
            return forwardDict[t1];
        }

        public T1 Get(T2 t2)
        {
            return reverseDict[t2];
        }

        public Safe<T2> TryGet(T1 t1)
        {
            return forwardDict.TryGetValue(t1, out var t2) ? t2 : None;
        }

        public Safe<T1> TryGet(T2 t2)
        {
            return reverseDict.TryGetValue(t2, out var t1) ? t1 : None;
        }

        public bool TryGetValue1(T2 t2, out T1 t1)//Unambiguous call when using same type
        {
            if (TryGetValue(t2, out t1))
            {
                return true;
            }
            else
            {
                var success = TryGetValue((T1)(object)t1, out var toCastToT1);
                if (success)
                {
                    t1 = (T1)(object)toCastToT1;
                    return true;
                }

                return false;
            }
        }

        public bool TryGetValue2(T1 t1, out T2 t2)//Unambiguous call when using same type
        {
            if (TryGetValue(t1, out t2))
            {
                return true;
            }
            else
            {
                var success = TryGetValue((T2)(object)t1, out var toCastToT2);
                if (success)
                {
                    t2 = (T2)(object)toCastToT2;
                    return true;
                }

                return false;
            }
        }

        public bool TryGetValue(T1 t1, out T2 t2)
        {
            return forwardDict.TryGetValue(t1, out t2);
        }

        public bool TryGetValue(T2 t2, out T1 t1)
        {
            return reverseDict.TryGetValue(t2, out t1);
        }

        public bool TryRemove(T1 t1)
        {
            if (Contains(t1))
            {
                Remove(t1);
                return true;
            }

            return false;
        }

        public bool TryRemove(T2 t2)
        {
            if (Contains(t2))
            {
                Remove(t2);
                return true;
            }

            return false;
        }

        public IEnumerator<KeyValuePair<T1, T2>> GetEnumerator()
        {
            return forwardDict.GetEnumerator();
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
            get
            {
                return reverseDict[t2];
            }
            set
            {
                AddOrReplace(value, t2);
            }
        }

        public T2 this[T1 t1]
        {
            get
            {
                return forwardDict[t1];
            }
            set
            {
                AddOrReplace(t1, value);
            }
        }
        public Indexer<T1, T2> Forward
        {
            get; private set;
        }
        public Indexer<T2, T1> Reverse
        {
            get; private set;
        }

        public int Count
        {
            get => forwardDict.Count;
        }
    }

    public static class MapExtensions
    {
        public static Safe<T2> TryGet<T1, T2>(this Map<T1, T2> map, Safe<T1> safeT1) => safeT1.Select(t1 => map.TryGet(t1));
        public static Safe<T1> TryGet<T1, T2>(this Map<T1, T2> map, Safe<T2> safeT2) => safeT2.Select(t2 => map.TryGet(t2));

        // For ambiguous TryGets
        public static Safe<T1> TryGet1<T1, T2>(this Map<T1, T2> map, T2 t2) => map.reverseDict.TryGet(t2);
        
        // For ambiguous TryGets
        public static Safe<T2> TryGet2<T1, T2>(this Map<T1, T2> map, T1 t1) => map.forwardDict.TryGet(t1);

        public static void AddOrReplace<T1, T2>(this Map<T1, T2> map, Safe<T1> safeT1, Safe<T2> safeT2)
            => safeT1.IfSome(t1 => safeT2.IfSome(t2 => map.AddOrReplace(t1, t2)));

        public static bool TryRemove<T1, T2>(this Map<T1, T2> map, Safe<T1> safeT1)
            => safeT1.SelectOut(t1 => map.TryRemove(t1), () => false);

        public static bool TryRemove<T1, T2>(this Map<T1, T2> map, Safe<T2> safeT2)
            => safeT2.SelectOut(t2 => map.TryRemove(t2), () => false);

        public static Map<T1, T2> Copy<T1, T2>(this Map<T1, T2> source)
        {
            var newMap = new Map<T1, T2>();
            foreach (var pair in source)
            {
                newMap.Add(pair.Key, pair.Value);
            }

            return newMap;
        }

    }
}