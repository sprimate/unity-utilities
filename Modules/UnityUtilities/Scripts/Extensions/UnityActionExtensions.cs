namespace HitTrax.UnityUtilities
{
    using System;
    using UnityEngine.Events;

    public static class UnityActionExtensions
    {
        public static UnityAction ToUnityAction(this Action a)
        {
            return () =>
            {
                a?.Invoke();
            };
        }
        public static Action ToAction(this UnityAction a)
        {
            return () =>
            {
                a?.Invoke();
            };
        }

        public static Action<T> ToAction<T>(this UnityAction<T> a)
        {
            return (t) =>
            {
                a?.Invoke(t);
            };
        }

        public static UnityEvent ToUnityEvent(this Action e)
        {
            UnityEvent ret = new UnityEvent();
            ret.AddListener(e.ToUnityAction());
            return ret;
        }

        /// <summary>
        /// thisEvent and eventToModify are the same. Since Actions are delegates (value types), you need to pass a ref for this to work
        /// "thisEvent" is jus there so it shows up via intellisense as an extension method.
        /// The parameter passed when using the extension method is the same as the second parameter, but as a reference
        /// </summary>

        public static void AddListenerOnce(this UnityEvent thisEvent, ref UnityEvent eventToModify, UnityAction actionToRunOnce)
        {
            AddOnce(ref eventToModify, actionToRunOnce);
        }

        public static void AddOnce(ref UnityEvent a, UnityAction actionToRunOnce)
        {
            bool shouldRun = true;
            UnityAction toAdd = () =>
            {
                if (shouldRun)
                {
                    shouldRun = false;
                    actionToRunOnce();
                }
            };

            a.AddListener(toAdd);
        }

        public static void ReplaceListeners(this UnityEvent evt, UnityAction action)
        {
            evt.RemoveAllListeners();
            evt.AddListener(action);
        }

        public static void ReplaceListeners<T>(this UnityEvent<T> evt, UnityAction<T> action)
        {
            evt.RemoveAllListeners();
            evt.AddListener(action);
        }
    }
}
