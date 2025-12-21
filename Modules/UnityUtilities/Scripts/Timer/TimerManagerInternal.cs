using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using HitTrax.CoreUtilities;

namespace HitTrax.UnityUtilities
{
    public interface ITimer_v1 : IProgressPercent
    {
        bool Loop { get; }
        bool Paused { get; }
        bool Completed { get; }
        float TimeRemaining { get; }
        float TotalTime { get; }
        ITimer_v1 Reset();
        ITimer_v1 Pause();
        ITimer_v1 Restart();
        ITimer_v1 Start();
        ITimer_v1 SetPauseState(bool isPaused);
        ITimer_v1 TogglePauseState();
        ITimer_v1 SetIsLoop(bool loop);
        UniTask AsUniTask();

    }
    /// <summary>
    /// Timer Data is managed by the Timed Events class
    /// </summary>
    /// <remarks>   
    /// (Anthony) On choice of data type:
    /// I'm generally inclined to use a struct for its immutability but the timer, almost by definition
    /// is a stateful type. Since it is stateful, and therefore mutable, I believe a class is best
    /// </remarks>
    internal class Timer : ITimer_v1
    {
        public bool Completed { get; private set; }
        public bool Loop { get; private set; }
        public bool Paused { get; private set; }
        private float? _timeRemaining;
        public float TimeRemaining {
            get => _timeRemaining.HasValue ? _timeRemaining.Value : float.PositiveInfinity;
            private set => _timeRemaining = value; }
        public float TotalTime { get; private set; }        
        public float ProgressPercent
        {
            get
            {
                return TotalTime == 0 ? 1 : 1 - (TimeRemaining / TotalTime);
            }
        }

        private UniTaskCompletionSource _uniTaskCompletion;
        private Action _onComplete;

        ~Timer() { }

        internal Timer(Action onComplete, float time) : this(time)
        {
            _onComplete = onComplete;
        }

        internal Timer(float time)
        {
            TotalTime = time;
            Paused = true;
        }

        public ITimer_v1 Reset()
        {
            _timeRemaining = null;
            Paused = false;
            return this;
        }

        public ITimer_v1 Restart()
        {
            Reset();
            TimeRemaining = TotalTime;
            return this;
        }

        public ITimer_v1 SetIsLoop(bool loop)
        {
            Loop = loop;
            return this;
        }

        public ITimer_v1 Pause()
        {
            if (_timeRemaining != null)
            {
                Paused = true;
            }

            return this;
        }

        /// <summary>
        /// Start or Resume
        /// </summary>
        /// <returns></returns>
        public ITimer_v1 Start()
        {
            if (!_timeRemaining.HasValue) //If timer hasn't yet been started, Start from beginning
            {
                return Restart();
            }

            Paused = false; //otherwise, resume
            return this;
        }


        public ITimer_v1 SetPauseState(bool isPaused)
        {
            Paused = isPaused;
            return this;
        }

        public ITimer_v1 TogglePauseState()
        {
            Paused = !Paused;
            return this;
        }

        public UniTask AsUniTask()
        {
            if (_uniTaskCompletion == null)
            {
                _uniTaskCompletion = new();
            }

            return _uniTaskCompletion.Task;
        }

        internal void DecrementTimeRemaining(float amount)
        {
            TimeRemaining -= amount;
        }

        internal void SetCompleted()
        {
            Paused = Completed = true;
            _onComplete?.Invoke(); // Invoke action
            _uniTaskCompletion?.TrySetResult();//notify awaiters
            _uniTaskCompletion = null; //and clear out this completionSource, which can only be set once
        }
    }

    /// <summary>
    /// Timed Events is used to reduce the necessity for managing coroutines and update functions when dealing with timed behaviors. 
    /// The general intention is to have a manager that can allow the developer to very simply trigger an action when a specificed amount of time has passed
    /// </summary>
    internal static class TimerManagerInternal
    {
        internal static int Count => _timers.Count;
        private static List<Timer> _timers = new();
        private static List<Timer> _removeTimers = new();
        private static bool _initialized = false;

        private static void TryInit()
        {
            if (_initialized)
            {
                return;
            }

            _timers = new List<Timer>();
            _removeTimers = new List<Timer>();
            UnityAsyncOperations.AddUpdateListener(OnUpdate);
            _initialized = true;
        }

        private static void OnUpdate(float deltaTime)
        {
            // Update annonymous events
            for (int i = 0; i < _timers.Count; i++)
            {
                UpdateEvent(_timers[i], deltaTime);
            }

            foreach (var eventInfo in _removeTimers)
            {
                _timers.Remove(eventInfo);
            }

            _removeTimers.Clear();
        }

        private static void UpdateEvent(Timer timer, float deltaTime)
        {
            if (!timer.Paused)
            {
                timer.DecrementTimeRemaining(deltaTime);
            }

            if (timer.TimeRemaining < 0)
            {
                timer.SetCompleted();

                if (timer.Loop)
                {
                    // Loop if needed
                    timer.Restart();
                }
                else
                {
                    // Set event to be removed
                    _removeTimers.Add(timer);
                }
            }
        }

        internal static ITimer_v1 CreateTimer(Action onComplete, float time)
        {
            TryInit();
            var timedEvent = new Timer(onComplete, time);
            _timers.Add(timedEvent);
            return timedEvent;
        }

        internal static void Kill(ITimer_v1 timer)
        {
            if (timer == null || _timers == null || timer is not Timer t)
            {
                return;
            }

            _timers.Remove(t);
        }
    }
}