using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using static HitTrax.CoreUtilities.SafeFunctions;
using System.Text;

namespace HitTrax.CoreUtilities
{
    public static class CollectionUtilities
    {
        private static Random _rng = new Random();
        public static bool ContainsAny<K, V>(this Safe<Dictionary<K, V>> dict, IEnumerable<K> keys) => dict.SelectOut(d => d.ContainsAny(keys), () => false);

        public static bool ContainsAny<K, V>(this Dictionary<K, V> dict, IEnumerable<K> keys) => keys.Any(dict.ContainsKey);

        public static Safe<Dictionary<T, int>> Increment<T>(this Safe<Dictionary<T, int>> optDict, T key, int amount) => SafeFunctions.Select(optDict, dict => Increment(dict, key, amount));

        public static Dictionary<T, int> Increment<T>(this Dictionary<T, int> dict, T key, int amount)
        {
            if (!dict.ContainsKey(key))
            {
                dict.Add(key, amount);
            }
            else
            {
                dict[key] += amount;
            }

            return dict;
        }

        public static Safe<Dictionary<K, List<V>>> AddToList<K, V>(this Dictionary<K, List<V>> dict, K key, V item) => dict.Safe().AddToList(key, item);

        public static Safe<Dictionary<K, List<V>>> AddToList<K, V>(this Safe<Dictionary<K, List<V>>> dict, Safe<K> key, Safe<V> item)
        {
            dict.IfSome(d =>
                key.IfSome(k =>
                    item.IfSome(i =>
                    {
                        if (d.TryGetValue(k, out var list))
                        {
                            list.Add(i);
                        }
                        else
                        {
                            d.Add(k, new List<V>());
                            d[k].Add(i);
                        }
                    })
                )
            );
            return dict;
        }

        public static bool RemoveFromList<K, V>(this Dictionary<K, List<V>> dict, K key, V item) => dict.Safe().RemoveFromList(key, item);

        public static bool RemoveFromList<K, V>(this Safe<Dictionary<K, List<V>>> dict, Safe<K> key, Safe<V> item)
        {
            var removed = false;

            dict.IfSome(d =>
                key.IfSome(k =>
                    item.IfSome(i =>
                    {
                        if (d.TryGetValue(k, out var list))
                        {
                            removed = list.Remove(i);
                        }
                    })
                )
            );

            return removed;
        }

        public static Safe<Dictionary<K, Func<V>>> Set<K, V>(this Safe<Dictionary<K, Func<V>>> optDict, K key, Func<V> func) => optDict.IfSome(dict => dict.Set(key, func));

        public static Safe<Dictionary<K, Func<V>>> Set<K, V>(this Dictionary<K, Func<V>> dict, K key, Func<V> func)
        {
            if (dict == null)
            {
                return None;
            }

            if (dict.ContainsKey(key))
            {
                dict[key] = func;
            }
            else
            {
                dict.Add(key, func);
            }

            return dict.Safe();
        }

        public static Safe<Dictionary<T, int>> Set<T>(this Safe<Dictionary<T, int>> optDict, T key, int value) => SafeFunctions.Select(optDict, dict => Set(dict, key, value));

        public static Safe<Dictionary<T, float>> Set<T>(this Safe<Dictionary<T, float>> optDict, T key, float value) => SafeFunctions.Select(optDict, dict => Set(dict, key, value));

        public static Safe<Dictionary<T, float>> Increment<T>(this Safe<Dictionary<T, float>> optDict, T key, float amount) => SafeFunctions.Select(optDict, dict => Increment(dict, key, amount));

        public static Dictionary<T, float> Increment<T>(this Dictionary<T, float> dict, T key, float amount)
        {
            if (!dict.ContainsKey(key))
            {
                dict.Add(key, amount);
            }
            else
            {
                dict[key] += amount;
            }

            return dict;
        }

        public static Safe<T> MaybeFirst<T>(this IEnumerable<T> items) => items.ToArray().MaybeFirst();
        
        public static Safe<T> MaybeLast<T>(this IEnumerable<T> items) => items.ToArray().MaybeLast();
        
        public static Safe<T> MaybeFirst<T>(this List<T> items) => items.Count == 0 ? None : items[0].Safe();
        
        public static Safe<T> MaybeLast<T>(this List<T> items) => items.Count == 0 ? None : items[items.Count - 1].Safe();
        
        public static Safe<T> MaybeFirst<T>(this T[] items) => items.Length == 0 ? None : items[0].Safe();
        
        public static Safe<T> MaybeLast<T>(this T[] items) => items.Length == 0 ? None : items[items.Length - 1].Safe();
        
        public static Safe<V> TryGet<K, V>(this Safe<Dictionary<K, V>> opt_dict, K key) => opt_dict.SelectOut(dict => dict.TryGet<K, V>(key), () => None);
        
        public static Safe<V> TryGet<K, V>(this IDictionary<K, V> dict, K key) => (key != null && dict.TryGetValue(key, out V val)) ? val.Safe() : None;

        public static Safe<V> TryGet<K, V>(this IDictionary<K, V> dict, Safe<K> key) => key.Select(k => dict.TryGet(k));

        public static Safe<T> TryGet<T>(this T[] items, int i)  => (i < 0 || i >=  items.Length) ? None : items[i].Safe();
        
        public static Safe<T> TryGet<T>(this List<T> items, int i) => (i < 0 || i >= items.Count) ? None : items[i].Safe();

        public static Dictionary<TKey, TValue> Set<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            if (dict.ContainsKey(key))
            {
                dict[key] = value;
            }
            else
            {
                dict.Add(key, value);
            }

            return dict;
        }

        public static Dictionary<TKey, TValue> Set<TKey, TValue>(this Dictionary<TKey, TValue> dict, Safe<TKey> key, Safe<TValue> value)
        {
            if(!key.HasValue)
            {
                "Attempting to set a Key that has a <None> value".LogWarning();
                return dict;
            }

            if (!value.HasValue)
            {
                "Attempting to set a value that has a <None> value".LogWarning();
                return dict;
            }

            return dict.Set(key.UnboxRaw(), value.UnboxRaw());
        }

        public static Dictionary<TKey, TValue> TrySet<TKey, TValue>(this Dictionary<TKey, TValue> dict, Safe<TKey> key, Safe<TValue> value)
        {
            if(key.HasValue && value.HasValue)
            {
                var k = key.UnboxRaw();
                var v = value.UnboxRaw();
                dict.Set(k, v);
            }

            return dict;
        }

        public static Dictionary<K, V> Set<K, V>(this Dictionary<K, V> dict, KeyValuePair<K, V> kvp) => dict.Set(kvp.Key, kvp.Value);

        public static Dictionary<K, V> KVPsToDict<K, V>(this IEnumerable<KeyValuePair<K, V>> items)
        {
            var dict = new Dictionary<K, V>();
            items.ToList().ForEach(item => dict.Set(item.Key, item.Value));
            return dict;
        }

        public static List<T> TryAdd<T>(this List<T> list, Safe<T> maybeItem)
        {
            maybeItem.IfSome(item => list.Add(item));
            return list;
        }

        public static Safe<Dictionary<K, V>> Add<K, V>(Safe<Dictionary<K, V>> dict, IDictionary<K, V> items) => dict.SelectOut(d => d.Add(items), () => new Safe<Dictionary<K, V>>());

        public static Dictionary<K, V> Add<K, V>(this Dictionary<K, V> dict, IDictionary<K, V> items)
        {
            foreach (var item in items)
            {
                dict.Add(item.Key, item.Value);
            }

            return dict;
        }

        public static T AddAndReturnItem<T>(this List<T> list, T item)
        {
            list.Add(item);
            return item;
        }

        public static List<T> AddTo<T>(this List<T> list, T item)
        {
            list.Add(item);
            return list;
        }

        public static List<T> RemoveFromAt<T>(this List<T> list, int index)
        {
            if (index < list.Count)
            {
                list.RemoveAt(index);
            }

            return list;
        }

        public static List<T> AddTo<T>(this List<T> list, IEnumerable<T> newItems)
        {
            foreach (var item in newItems)
            {
                list.AddTo(item);
            }

            return list;
        }

        public static Safe<K> RandomKey<K, V>(this Dictionary<K, V> dict) => dict.ToList().RandomElement().SelectOut(e => e.Key.Safe(), () => None);
        
        public static Safe<V> RandomValue<K, V>(this Dictionary<K, V> dict) => dict.ToList().RandomElement().SelectOut(e => e.Value.Safe(), () => None);

        public static Dictionary<string, V> EnumToDictionary<V>(this IEnumerable<V> items)
        {
            var dict = new Dictionary<string, V>();
            items.ToList().ForEach(item => dict.Set(item.ToString(), item));
            return dict;
        }

        // I'm not crazy about this because it modifies an existing list

        public static List<T> TryAddOnce<T>(this List<T> list, IEnumerable<T> items)
        {
            list.ForEach(item => list.TryAddOnce(item));
            return list;
        }

        public static TList TryAddOnce<TList, T>(this TList list, T item) where TList : IList<T>
        {
            if (!list.Contains(item))
            {
                list.Add(item);
            }

            return list;
        }

        public static int RandomRange(int a, int b) => new Random().Next(a, b);

        public static Safe<T> RandomElement<T>(this IEnumerable<T> e) => e.ToList().RandomElement();

        public static Safe<T> RandomElement<T>(this List<T> list) => (list.Count == 0) ? None : list[RandomRange(0, list.Count)];

        public static bool IsInBounds<T>(this T[] self, int index) => !self.IsOutOfBounds(index);

        public static bool IsInBounds<T>(this T[,] self, int index1, int index2) => !self.IsOutOfBounds(index1, index2);

        public static bool IsOutOfBounds<T>(this T[] self, int index) => index < 0 || index >= self.Length;

        public static bool IsOutOfBounds<T>(this T[,] self, int index0, int index1) => index0 < 0 || index0 >= self.GetLength(0) || index1 < 0 || index1 >= self.GetLength(1);

        public static bool HasItem<TKey>(this Dictionary<TKey, ICollection> self, TKey key) => self.TryGetValue(key, out var col) && col.Count > 0 ? true : false;

        public static T FirstMatch<T>(this IEnumerable<T> items, T search) => items.OrderBy(item => item)
            .FirstOrDefault(item => item.ToString().Contains(search.ToString()));

        public static Safe<T> FirstOrNone<T>(this IEnumerable<Safe<T>> items) => items.ToList().Count > 0 ? items.ToList()[0] : None;

        public static Safe<T> FirstOrNone<T>(this IEnumerable<T> items) => items.ToList().Count > 0 ? items.ToList()[0] : None;

        public static V TryGetOrDefault<K, V>(this Dictionary<K, V> dictionary, K key) => dictionary.ContainsKey(key) ? dictionary[key] : default(V);

        public static string TryGetOrKey(this Dictionary<string, string> dict, string key) => dict.ContainsKey(key) ? dict[key] : key;

        public static V GetOrCreate<K, V>(this Dictionary<K, V> dictionary, K key) where V : new()
            => dictionary.GetOrCreate(key, () => new());

        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TValue> setDefaultValue)
        {
            if (!dict.TryGetValue(key, out var value))
            {
                dict[key] = value = setDefaultValue();
            }

            return value;
        }

        public static int RandIndex<T>(this T[] items) => RandomRange(0, items.Length - 1);

        public static Dictionary<T, V> Copy<T, V>(this Dictionary<T, V> dict)
        {
            var d = new Dictionary<T, V>();

            foreach (var key in dict.Keys) { d.Add(key, dict[key]); }

            return d;
        }

        public static Dictionary<T, K> WhereAsDict<T, K>(this Dictionary<T, K> dict, Func<KeyValuePair<T, K>, bool> predicate)
        {
            var newDict = new Dictionary<T, K>();
            dict.Where(predicate).ToList().ForEach(kvp => newDict.Add(kvp.Key, kvp.Value));
            return newDict;
        }

        // TODO: I believe I prefer my logic in HT Suite, pull that in
        public static Safe<TKey> WeightedRandom<TKey>(this IDictionary<TKey, float> weightedDict)
        {
            var weightSum = 0f;

            foreach (var weight in weightedDict.Values)
            {
                weightSum += weight;
            }

            var randValue = (float)new Random().NextDouble() * weightSum;

            foreach (var kvp in weightedDict)
            {
                if (randValue < kvp.Value)
                {
                    return kvp.Key;
                }

                randValue -= kvp.Value;
            }

            return None;
        }

        public static bool Any<T>(this Safe<List<T>> list, Func<T, bool> predicate) => list.SelectOut(l => l.Any(predicate), () => false);
        
        public static Safe<List<T>> Where<T>(this Safe<List<T>> list, Func<T, bool> predicate) => list.SelectOut(l => l.Where(predicate).ToList(), () => new Safe<List<T>>());

        public static List<T> OnEach<T>(this List<T> list, Action<T> action) => list.OnEach<List<T>, T>(action);

        public static Safe<List<T>> OnEach<T>(this Safe<List<T>> optList, Action<T> action) => optList.OnEach<List<T>, T>(action);

        public static Col OnEach<Col, T>(this Col items, Action<T> action) where Col : IEnumerable<T>
        {
            foreach (var item in items) { action(item); }

            return items;
        }

        public static bool Contains<T>(this Safe<HashSet<T>> optSet, T item) => optSet.SelectOut(set => set.Contains(item), () => false);
        public static Safe<Col> OnEach<Col, T>(this Safe<Col> optItems, Action<T> action) where Col : IEnumerable<T> => optItems.SelectOut(items => items.OnEach(action), () => new Safe<Col>());

        public static Safe<Dictionary<Key, Value>> SetValue<Key, Value>(this Dictionary<Key, Value> dictionary, Key key, Value value) => dictionary.Safe().SetValue(key, value);

        public static Safe<Dictionary<Key, Value>> SetValue<Key, Value>(this Safe<Dictionary<Key, Value>> dictionary, Key key, Value value)
        {
            dictionary
                .IfSome(dict =>
                {
                    if (dict.ContainsKey(key))
                    {
                        dict[key] = value;
                    }
                    else
                    {
                        dict.Add(key, value);
                    }
                });

            return dictionary;
        }

        // TODO: Clean this up and find an appropriate place
        public static T GenerateValueFromWeights<T>(Dictionary<T, int> weightMap, T def)
        {
            var maxScores = new Dictionary<T, int>();
            var cachedMax = 0;

            foreach (var key in weightMap.Keys)
            {
                maxScores.Add(key, weightMap[key] + cachedMax);
                cachedMax += weightMap[key];
            }

            // Generate value
            float val = RandomRange(0, cachedMax);
            var tile = def;
            var selectItem = true;

            foreach (var key in maxScores.Keys)
            {
                if (selectItem)
                {
                    tile = key;
                    selectItem = false;
                }

                if (val >= maxScores[key])
                {
                    tile = key;
                    selectItem = true;
                }
            }

            return tile;
        }
        public static string ToStringFull<T0, T1>(this Dictionary<T0, T1> dict)
        {
            var sb = new StringBuilder();

            if (dict == null)
            {
                return "NULL Dictionary!!";
            }

            sb.AppendLine("Count: " + dict.Count);

            foreach (var item in dict)
            {
                sb.AppendFormat("Key: {0}    Value: {1}\n", item.Key, item.Value);
            }

            return sb.ToString();
        }

        public static T2 Get<T1, T2>(this Dictionary<T1, T2> dict, T1 key)
        {
            if (dict.ContainsKey(key))
            {
                return dict[key];
            }
            return default(T2);
        }
        public static bool Has<T1, T2>(this Dictionary<T1, T2> dict, T1 key)
        {
            return (dict.ContainsKey(key));
        }

        public static Dictionary<TKey, TArgs> ConvertListToDictionary<TKey, TArgs>(this IList<TArgs> list, Func<TArgs, TKey> keyFunc)
        {

            Dictionary<TKey, TArgs> dictionary = new();

            foreach (var item in list)
            {
                var key = keyFunc(item);
                if (!dictionary.ContainsKey(key))
                {
                    dictionary.Add(key, item);
                }
                else
                {
                    $"Duplicate Key found when trying to convert list to dictionary {key}".LogCaution();
                }

            }
            return dictionary;                       
        }

        public static string ToStringExt<T>(this T[] array, string delimiter = ",")
        {
            if (array == null)
            {
                return "NULL";
            }
            else if (array.Length == 0)
            {
                return "EMPTY";
            }
            else
            {
                string s = "";
                for (int i = 0; i < array.Length; i++)
                {
                    s += array[i].ToString();
                    if (i < array.Length - 1)
                    {
                        s += delimiter;
                    }
                }

                return s;
            }
        }

        public static bool TryGetCastedValue<T>(this object[] objectArray, int index, out T ret)
        {
            if (objectArray != null && objectArray.Length > index && objectArray[index] is T val)
            {
                ret = val;
                return true;
            }

            ret = default(T);
            return false;
        }

        public static string ToStringFull<T>(this List<T> list)
        {
            if (list == null)
            {
                return "NULL LIST type: " + typeof(T).ToString();
            }

            var sb = new StringBuilder();
            sb.AppendLine("Count: " + list.Count);

            for (int i = 0; i < list.Count; i++)
            {
                sb.AppendFormat("[{0}]: {1}", i, list[i]);
            }

            return sb.ToString();
        }

        public static string LinesToString<T>(this List<T> list)
        {
            if (list == null)
            {
                return "NULL";
            }

            if (list.Count == 0)
            {
                return "EMPTY";
            }
            var sb = new StringBuilder();
            foreach (var item in list)
            {
                sb.AppendLine(item.ToString());
            }

            return sb.ToString();
        }

        public static T GetCircular<T>(this ICollection<T> collection, ref int i)
        {
            if (collection.Count() == 0)
            {
                throw new Exception("Collection is empty");
            }
            i = i % (collection.Count());
            if (i < 0)
            {
                i += collection.Count();
            }

            if (collection != null && collection.Count() > 0 && i >= 0)
            {
                return collection.ElementAt(i);
            }

            return default(T);
        }

        public static T GetCircular<T>(this ICollection<T> collection, int i)
        {
            return GetCircular(collection, ref i);
        }

        public static T GetNextCircular<T>(this List<T> list, T item)
        {
            int indexOf = list.IndexOf(item);
            if (indexOf >= 0)
            {
                return GetCircular(list, indexOf + 1);
            }
            return default(T);
        }

        public static T GetRandom<T>(this ICollection<T> collection)
        {
            if (collection != null && collection.Count() > 0)
            {
                var randoIndex = _rng.Next(collection.Count());// UnityEngine.Random.Range(0, );
                return collection.ElementAt(randoIndex);
            }

            return default(T);
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = _rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}