using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameParameterModification
{
    protected int? _priority;
    public abstract int priority { get; set; }
    public abstract double GetTruePriority();
    public bool HasValidPriority => _priority.HasValue;
    public abstract void Clean();
}

public class GameParameterModification<T> : GameParameterModification
{
    public GameParameter<T> gameParameter { get; private set; }
    public Func<T, T> preprocessor;
    public PrioritizedPreProcessors<T> gameParameterPreprocessors { get; protected set; }

    public override int priority
    {
        get
        {
            return _priority.HasValue ? _priority.Value : int.MinValue;
        }
        set
        {
            if (value != _priority)
            {
                var initialPriority = _priority;
                Clean();
                _priority = value;

                gameParameterPreprocessors.Insert(this);
            }
        }
    }

    public GameParameterModification(GameParameter<T> _gameParameter, Func<T, T> _preprocessor, int _priority, PrioritizedPreProcessors<T> _preprocessorList)
    {
        gameParameter = _gameParameter;
        gameParameterPreprocessors = _preprocessorList;
        preprocessor = _preprocessor;

        priority = _priority;//priority must be set last, as its property enforces the priority rules
    }

    /// <summary>
    /// True priority means there are no collisions
    /// </summary>
    /// <returns></returns>
    public override double GetTruePriority() => gameParameterPreprocessors.GetPriority(this);

    public override void Clean()
    {
        gameParameterPreprocessors.Remove(this);
        _priority = null;
    }
}