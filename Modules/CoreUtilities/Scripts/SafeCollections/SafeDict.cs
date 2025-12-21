using System.Collections;
using System.Collections.Generic;
using static HitTrax.CoreUtilities.SafeFunctions;

namespace HitTrax.CoreUtilities
{
    public class SafeDict<K, V> : IDictionary<K, V>
    {
        private Safe<Dictionary<K, V>> _dictionary;

        private Safe<Dictionary<K, V>> Dictionary
        {
            get
            {
                if (!_dictionary.HasValue)
                {
                    _dictionary = new Safe<Dictionary<K, V>>();
                }

                return _dictionary;
            }
        }

        public V this[K key]
        {
            get => Dictionary.SelectOut(d => d[key], () => default);
            set => this.Set(key, value);
        }

        public Safe<V> Value(K key) => TryGetValue(key, out var value) ? value : None;

        public SafeDict<K, V> Set(K key, V value)
        {
            Dictionary.IfSome(d =>
            {
                if (d.ContainsKey(key))
                {
                    d[key] = value;
                }
                else
                {
                    d.Add(key, value);
                }
            });

            return this;
        }

        public SafeDict<K, V> TrySet(K key, Safe<V> value)
            => value.SelectOut(val => Set(key, val), () => this);

        public SafeDict()
        {
            _dictionary = new Safe<Dictionary<K, V>>();
        }

        public SafeDict(Dictionary<K, V> dict)
        {
            _dictionary = dict;
        }

        public ICollection<K> Keys => Dictionary.SelectOut(d => d.Keys, () => default);

        public ICollection<V> Values => Dictionary.SelectOut(d => d.Values, () => default);

        public int Count => Dictionary.SelectOut(d => d.Count, () => 0);

        public bool IsReadOnly => false;

        public void Add(K key, V value) => Dictionary.IfSome(d => d.Add(key, value));

        public void Add(KeyValuePair<K, V> item) => this.Add(item.Key, item.Value);

        public void Clear() => Dictionary.IfSome(d => d.Clear());

        public bool Contains(KeyValuePair<K, V> item) => Dictionary.SelectOut(d => ((IDictionary)d).Contains(item), () => false);

        public bool ContainsKey(K key) => Dictionary.SelectOut(d => d.ContainsKey(key), () => false);

        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex) => Dictionary.IfSome(d => ((IDictionary)d).CopyTo(array, arrayIndex));

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator() => Dictionary.SelectOut(d => d.GetEnumerator(), () => default);

        public bool Remove(K key) => Dictionary.SelectOut(d => d.Remove(key), () => false);

        public bool Remove(KeyValuePair<K, V> item) =>
            Dictionary.SelectOut(d =>
                {
                    var hasKvp = d.ContainsKey(item.Key) && d.ContainsValue(item.Value);
                    ((IDictionary)d).Remove(item);
                    return hasKvp;
                },
                () => false);

        public bool TryGetValue(K key, out V value)
        {
            V returnValue = default;
            var hasValue = false;

            Dictionary.IfSome(d =>
                {
                    if (d.TryGetValue(key, out var val))
                    {
                        returnValue = val;
                        hasValue = true;
                    }
                }
            );

            value = returnValue;
            return hasValue;
        }

        IEnumerator IEnumerable.GetEnumerator() => Dictionary.SelectOut(d => d.GetEnumerator(), () => default);
    }

    public static class SafeDictionaryExtensions
    {
        public static SafeDict<K, HashSet<V>> Add<K, V>(this SafeDict<K, HashSet<V>> safeDict, K key, V value)
        {
            if (safeDict.TryGetValue(key, out HashSet<V> result))
            {
                result.Add(value);
            }
            else
            {
                safeDict.Add(key, new HashSet<V>() { value });
            }

            return safeDict;
        }

        public static SafeDict<K, HashSet<V>> TryAdd<K, V>(this SafeDict<K, HashSet<V>> safeDict, K key, Safe<V> value)
        {
            value.IfSome(val => safeDict.Add(key, val));
            return safeDict;
        }
    }
}