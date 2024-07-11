using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PrioritizedPreProcessors<T> : IEnumerable<GameParameterModification<T>>
{
    public SortedList<int, LinkedList<GameParameterModification<T>>> priorityStructure;
    public Dictionary<GameParameterModification<T>, LinkedListNode<GameParameterModification<T>>> linkedLIstNodeReferenceDict = new Dictionary<GameParameterModification<T>, LinkedListNode<GameParameterModification<T>>>();
    public int Count => linkedLIstNodeReferenceDict.Count;

    public PrioritizedPreProcessors()
    {
        IComparer<int> descendingComparer = Comparer<int>.Create((x, y) => y.CompareTo(x));
        priorityStructure = new SortedList<int, LinkedList<GameParameterModification<T>>>(descendingComparer);
    }

    public float GetPriority(GameParameterModification<T> modification)
    {
        if (priorityStructure.TryGetValue(modification.priority, out var list))
        {
            var node = linkedLIstNodeReferenceDict[modification];
            float incrementValue = 0.0001f;
            float priority = modification.priority;
            while(node.Previous != null)
            {
                node = node.Previous;
                priority -= incrementValue;
            }

            return priority;
        }

        return modification.priority;
    }

    public void Insert(GameParameterModification<T> modification)
    {
        if (!priorityStructure.TryGetValue(modification.priority, out var nodes))
        {
            nodes = new LinkedList<GameParameterModification<T>>();
            priorityStructure.Add(modification.priority, nodes);
        }

        linkedLIstNodeReferenceDict[modification] = nodes.AddLast(modification);
    }

    public void Remove(GameParameterModification<T> modification)
    {
        if (linkedLIstNodeReferenceDict.TryGetValue(modification, out var node))
        {
            var list = node.List;
            linkedLIstNodeReferenceDict.Remove(modification);
            list.Remove(node);
            if (list.Count == 0)
            {
                if (modification.HasValidPriority && priorityStructure.TryGetValue(modification.priority, out var priorityStructureList) && list == priorityStructureList)//in case with no desync, this fast check should work
                {
                    priorityStructure.Remove(modification.priority);
                }
                else
                {
                    priorityStructure.RemoveAt(priorityStructure.IndexOfValue(node.List));
                }
            }
        }
    }

    public IEnumerator<GameParameterModification<T>> GetEnumerator()
    {
        foreach (var kvp in priorityStructure)
        {
            foreach (var modification in kvp.Value)
            {
                yield return modification;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}