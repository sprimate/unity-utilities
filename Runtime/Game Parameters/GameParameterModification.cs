using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameParameterModification
{
    protected float? _priority;
    public abstract float priority { get; set; }
    public abstract void Clean();

}

public class GameParameterModification<T> : GameParameterModification
{
    public GameParameter<T> gameParameter { get; private set; }
    public Func<T, T> preprocessor;
    public SortedList<float, GameParameterModification<T>> gameParameterPreprocessors { get; protected set; }
    public override float priority
    {
        get
        {
            return _priority.HasValue ? _priority.Value : default;
        }
        set
        {
            if (value != _priority)
            {
                var initialPriority = _priority;
                Clean();
                _priority = value;

                bool goingDown = true;
                bool firstTry = true;
                while (gameParameterPreprocessors.ContainsKey(priority))
                {
                    if (priority == float.MinValue)
                    {
                        if (!firstTry)
                        {
                            Debug.LogError("Cannot reduce priority any lower, we are already at the lowest. Please adjust other priorities using MinValue");
                            _priority = initialPriority;
                            break;
                        } 
                        else
                        {
                            goingDown = false;
                        }
                    }

                    _priority += (goingDown ? -.01f : .01f);
                    firstTry = false;
                }

                gameParameterPreprocessors.Add(priority, this);
            }
        }
    }

    public GameParameterModification(GameParameter<T> _gameParameter, Func<T, T> _preprocessor, float _priority, SortedList<float, GameParameterModification<T>> _preprocessorList)
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