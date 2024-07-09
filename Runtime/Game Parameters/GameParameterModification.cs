using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameParameterModification<T>
{
    public GameParameter<T> gameParameter { get; private set; }
    public Func<T, T> preprocessor;
    public SortedList listIBelongTo { get; private set; }

    int? _priority;
    public int priority 
    { 
        get
        {
            return _priority.HasValue ? _priority.Value : default;
        } 
        set
        {
            if (value != _priority)
            {
                Clean();

                _priority = value;

                while(listIBelongTo.ContainsKey(priority))
                {
                    _priority++;
                }

                listIBelongTo.Add(priority, this);
            }
        }
    }
    public GameParameterModification(GameParameter<T> _gameParameter, Func<T, T> _preprocessor, int _priority, SortedList _preprocessorList)
    {
        gameParameter = _gameParameter;
        listIBelongTo = _preprocessorList;
        preprocessor = _preprocessor;

        priority = _priority;//priority must be set last, as the property enforces the priority rules
    }

    public void Clean()
    {
        if (_priority.HasValue && listIBelongTo.ContainsKey(priority))
        {
            listIBelongTo.Remove(priority);
        }
    }
}