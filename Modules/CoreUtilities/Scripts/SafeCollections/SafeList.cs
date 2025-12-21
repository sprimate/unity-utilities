using System.Collections;
using System.Collections.Generic;
using System;

namespace HitTrax.CoreUtilities
{
    public struct SafeList<T> : IList<T>
    {
        private Safe<List<T>> _list;

        private Safe<List<T>> List
        {
            get
            {
                if (!_list.HasValue)
                {
                    _list = new Safe<List<T>>();
                }

                return _list;
            }
        }

        public SafeList(List<T> list)
        {
            _list = new Safe<List<T>>(list);
        }

        public SafeList(params T[] items)
        {
            _list = new Safe<List<T>>(new List<T>());
            _list.IfSome(list => list.AddRange(items));
        }

        public T this[int index]
        {
            get => List.SelectOut(list => list[index], () => default);
            set => List.IfSome(list => list[index] = value);
        }

        public int Count => List.SelectOut(list => list.Count, () => 0);
        
        public bool IsReadOnly => false;

        public void Add(T item) => List.IfSome(list => list.Add(item));

        public void Clear() => List.IfSome(list => list.Clear());

        public bool Contains(T item) => List.SelectOut((list) => list.Contains(item), () => false);

        public void CopyTo(T[] array, int arrayIndex) => List.IfSome(list => list.CopyTo(array, arrayIndex));

        public IEnumerator<T> GetEnumerator() => List.SelectOut(list => list.GetEnumerator(), () => default);

        public int IndexOf(T item) => List.SelectOut(list => list.IndexOf(item), () => int.MinValue);

        public void Insert(int index, T item) => List.IfSome(list => list.Insert(index, item));

        public bool Remove(T item) => List.SelectOut(list => list.Remove(item), () => false);

        public void RemoveAt(int index) => List.IfSome(list => list.RemoveAt(index));

        public U Select<U>(Func<List<T>, U> ifSome, Func<U> ifNone) => List.SelectOut<List<T>, U>(ifSome, ifNone);

        IEnumerator IEnumerable.GetEnumerator() => List.SelectOut(list => list.GetEnumerator(), default);
    }
}