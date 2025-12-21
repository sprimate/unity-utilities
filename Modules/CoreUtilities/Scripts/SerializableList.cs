using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace HitTrax.CoreUtilities
{
    [Serializable]
    public class SerializableNode<T>
    {
        [SerializeReference] public T value;
        [SerializeReference] public SerializableNode<T> next;

        public SerializableNode()
        {

        }

        public SerializableNode(T value)
        {
            this.value = value;
        }
    }

    /// <summary>
    /// A LinkedList (doesn't use any backing arrays) that is deserialized properly when used with custom types 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class SerializableList<T> : IList<T>, IReadOnlyList<T>
    {
        [SerializeReference] private SerializableNode<T> _head;

        public int Count //TODO - Cache this so we're not recalculating it every time when nothing was added or removed
        {
            get
            {
                int count = 0;
                var current = _head;
                while (current != null)
                {
                    count++;
                    current = current.next;
                }
                return count;
            }
        }


        public static implicit operator List<T>(SerializableList<T> serializableList)
        {
            return serializableList.ToList();
        }

        public static implicit operator SerializableList<T>(List<T> list)
        {
            var newList = new SerializableList<T>();

            foreach (var item in list)
            {
                newList.Add(item);
            }

            return newList;
        }

        public T this[int index]
        {
            get
            {
                return GetNode(index).value;
            }
            set
            {
                GetNode(index).value = value;
            }
        }

        public bool IsReadOnly => false;

        public void Add(T item)
        {
            if (_head == null)
            {
                _head = new SerializableNode<T>(item);
                return;
            }

            var current = _head;
            while (current.next != null)
            {
                current = current.next;
            }

            current.next = new SerializableNode<T>(item);
        }

        public void Clear()
        {
            ForEachNode(node => node.next = null);
            _head = null;
        }

        public bool Contains(T item)
        {
            return this.Any(x => EqualityComparer<T>.Default.Equals(x, item));
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            foreach (var item in this)
            {
                array[arrayIndex++] = item;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            var current = _head;
            while (current != null)
            {
                yield return current.value;
                current = current.next;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(T item)
        {
            int i = 0;
            foreach (var val in this)
            {
                if (EqualityComparer<T>.Default.Equals(val, item))
                {
                    return i;
                }
                i++;
            }
            return -1;
        }

        public void Insert(int index, T item)
        {
            if (index < 0 || index > Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            var node = new SerializableNode<T>(item);

            if (index == 0)
            {
                node.next = _head;
                _head = node;
                return;
            }

            var prev = GetNode(index - 1);
            node.next = prev.next;
            prev.next = node;
        }

        public bool Remove(T item)
        {
            SerializableNode<T> previous = null;
            var current = _head;

            while (current != null)
            {
                if (EqualityComparer<T>.Default.Equals(current.value, item))
                {
                    RemoveNode(previous, current);
                    return true;
                }

                previous = current;
                current = current.next;
            }

            return false;
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            SerializableNode<T> previous = null;
            var current = _head;

            for (int i = 0; i < index; i++)
            {
                previous = current;
                current = current.next;
            }

            RemoveNode(previous, current);
        }

        private void RemoveNode(SerializableNode<T> previous, SerializableNode<T> node)
        {
            if (previous != null)
            {
                previous.next = node.next;
            }
            else
            {
                _head = node.next;
            }

            node.next = null;
        }

        private SerializableNode<T> GetNode(int index)
        {
            var current = _head;
            for (int i = 0; i < index; i++)
            {
                current = current.next;
                if (current == null)
                {
                    throw new ArgumentOutOfRangeException();
                }
            }

            return current;
        }

        private void ForEachNode(Action<SerializableNode<T>> action)
        {
            var current = _head;
            while (current != null)
            {
                var next = current.next;
                action?.Invoke(current);
                current = next;
            }
        }

        public List<T> ToList()
        {
            var list = new List<T>();
            foreach (var item in this)
            {
                list.Add(item);
            }
            
            return list;
        }        
    }
}
