using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameParameterModification
{
    protected int? _priority;
    public SortedList gameParameterPreprocessors { get; protected set; }

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

                while (gameParameterPreprocessors.ContainsKey(priority))
                {
                    _priority++;
                }

                gameParameterPreprocessors.Add(priority, this);
            }
        }
    }
    public virtual void Clean()
    {
        if (_priority.HasValue && gameParameterPreprocessors.ContainsKey(priority))
        {
            gameParameterPreprocessors.Remove(priority);
        }

        _priority = null;
    }
}

public class GameParameterModification<T> : GameParameterModification
{
    public GameParameter<T> gameParameter { get; private set; }
    public Func<T, T> preprocessor;

    public GameParameterModification(GameParameter<T> _gameParameter, Func<T, T> _preprocessor, int _priority, SortedList _preprocessorList)
    {
        gameParameter = _gameParameter;
        gameParameterPreprocessors = _preprocessorList;
        preprocessor = _preprocessor;

        priority = _priority;//priority must be set last, as the property enforces the priority rules
    }
}