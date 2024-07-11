using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameParameterModification
{
    protected double? _priority;
    public abstract float priority { get; set; }
    public abstract void Clean();

}

public class GameParameterModification<T> : GameParameterModification
{
    public GameParameter<T> gameParameter { get; private set; }
    public Func<T, T> preprocessor;
    public SortedList<double, GameParameterModification<T>> gameParameterPreprocessors { get; protected set; }
    public override float priority
    {
        get
        {
            float ret = _priority.HasValue ? (float)_priority.Value : default;
            return float.IsPositiveInfinity(ret) ? float.MaxValue : (float.IsNegativeInfinity(ret) ? float.MinValue : ret);
        }
        set
        {
            if (value != _priority)
            {
                var initialPriority = _priority;
                Clean();
                _priority = value;


                while (_priority.HasValue && gameParameterPreprocessors.ContainsKey(_priority.Value))
                {
                    _priority -= 0.01;
                }

                if (_priority.HasValue)
                {
                    gameParameterPreprocessors.Add(_priority.Value, this);
                }
            }
        }
    }

    public GameParameterModification(GameParameter<T> _gameParameter, Func<T, T> _preprocessor, float _priority, SortedList<double, GameParameterModification<T>> _preprocessorList)
    {
        gameParameter = _gameParameter;
        gameParameterPreprocessors = _preprocessorList;
        preprocessor = _preprocessor;

        priority = _priority;//priority must be set last, as its property enforces the priority rules
    }

    public override void Clean()
    {
        if (_priority.HasValue && gameParameterPreprocessors.ContainsKey(priority))
        {
            gameParameterPreprocessors.Remove(priority);
        }

        _priority = null;
    }
}