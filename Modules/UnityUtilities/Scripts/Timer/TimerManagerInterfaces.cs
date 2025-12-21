using System;
using HitTrax.CoreUtilities;

namespace HitTrax.UnityUtilities
{
    public interface ITimerManager_v1 : IService
    {
        ITimer_v1 CreateTimer(Action action, float time) => TimerManagerInternal.CreateTimer(action, time);
        ITimer_v1 CreateTimer(float time) => TimerManagerInternal.CreateTimer(null, time);
        void Kill(ITimer_v1 timer) => TimerManagerInternal.Kill(timer);
    } 

    internal class TimerManager : ITimerManager_v1 { }
}