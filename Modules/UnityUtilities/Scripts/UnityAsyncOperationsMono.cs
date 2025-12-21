using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using HitTrax.CoreUtilities;
using HitTrax.GlobalMessagingService;
using System.Threading;
using Cysharp.Threading.Tasks;
using System.Linq;

namespace HitTrax.UnityUtilities
{
    public struct MsgUpdate : IMessageKey<float> { }
    public struct MsgFixedUpdate : IMessageKey<float> { }
    public struct MsgLateUpdate : IMessageKey<float> { }
    public struct MsgLateFixedUpdate : IMessageKey<float> { }

    internal class UnityAsyncOperationsMono : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
        private void Start()
        {
            StartCoroutine(LateFixedUpdate());
        }

        private void OnGUI()
        {
            UnityAsyncOperations.OnGUI();
        }

        // Update is called once per frame
        private void Update()
        {
            UnityAsyncOperations.OnUpdate(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            UnityAsyncOperations.OnFixedUpdate(Time.fixedDeltaTime);
        }

        private IEnumerator LateFixedUpdate()
        {
            while (this != null)
            {
                yield return UnityAsyncOperations.WaitForFixedUpdate;
                UnityAsyncOperations.OnLateFixedUpdate(Time.fixedDeltaTime);
            }
        }

        private void LateUpdate()
        {
            UnityAsyncOperations.OnLateUpdate(Time.deltaTime);
        }

        internal void CoroutineStarter(IEnumerator routine)
        {
            StartCoroutine(routine);
        }

        private void OnApplicationQuit()
        {
            UnityAsyncOperations.OnApplicationQuit();
        }
    }

    public static class UnityAsyncOperations
    {
        private const string GLOBAL = "global";
        private static UnityAsyncOperationsMono _unityAsyncComponent;

        private static Dictionary<object, List<Action<float>>> _updateListeners;
        private static Dictionary<object, List<Action<float>>> _fixedUpdateListeners;
        private static Dictionary<object, List<Action<float>>> _lateUpdateListeners;
        private static Dictionary<object, List<Action<float>>> _lateFixedUpdateListeners;
        private static Dictionary<object, Action> _onGuiListeners;
        private static Dictionary<object, Action> _onApplicationQuitListeners;

        public static WaitForEndOfFrame WaitForEndOfFrame { get; private set; } = new WaitForEndOfFrame();
        public static WaitForFixedUpdate WaitForFixedUpdate { get; private set; } = new WaitForFixedUpdate();

        private static async UniTask<UnityAsyncOperationsMono> Init()
        {
            if (!PlayerLoopHelper.IsMainThread)
            {
                await UniTask.SwitchToMainThread();
            }

            if (_unityAsyncComponent == null)
            {
                _unityAsyncComponent = new GameObject(nameof(UnityAsyncOperationsMono)).AddComponent<UnityAsyncOperationsMono>();
            }

            return _unityAsyncComponent;
        }

        //The coroutine will be started immediately if you call this from the main thread.
        public static void StartCoroutine(IEnumerator routine, Action onCoroutineStarted = null)
        {
            static async UniTask StartCoroutineOnMainThread(IEnumerator routine)
            {
                (await Init()).CoroutineStarter(routine);
            }

            StartCoroutineOnMainThread(routine).AsCallback(onCoroutineStarted);
        }

        public static void AddOnApplicationQuitListener(Action action)
        {
            AddOnApplicationQuitListener(GLOBAL, action);
        }

        public static void AddOnApplicationQuitListener(object context, Action listener)
        {
            AddListener(ref _onApplicationQuitListeners, context, listener);
        }

        public static void AddOnGuiListener(Action listener)
        {
            AddOnGuiListener(GLOBAL, listener);
        }

        public static void AddOnGuiListener(object context, Action listener)
        {
            AddListener(ref _onGuiListeners, context, listener);
        }

        public static void AddUpdateListener(Action<float> listener)
        {
            AddUpdateListener(GLOBAL, listener);
        }

        public static void AddUpdateListener(object context, Action<float> listener)
        {
            AddListener(ref _updateListeners, context, listener);
        }

        public static void AddFixedUpdateListener(Action<float> listener)
        {
            AddFixedUpdateListener(GLOBAL, listener);
        }

        public static void AddFixedUpdateListener(object context, Action<float> listener)
        {
            AddListener(ref _fixedUpdateListeners, context, listener);
        }

        public static void AddLateUpdateListener(Action<float> listener)
        {
            AddLateUpdateListener(GLOBAL, listener);
        }

        public static void AddLateUpdateListener(object context, Action<float> listener)
        {
            AddListener(ref _lateUpdateListeners, context, listener);
        }

        public static void AddLateFixedUpdateListener(object context, Action<float> listener)
        {
            AddListener(ref _lateFixedUpdateListeners, context, listener);
        }

        public static void RemoveOnApplicationQuitListener(Action action)
        {
            RemoveOnApplicationQuitListener(GLOBAL, action);
        }

        public static void RemoveOnApplicationQuitListener(object context, Action action)
        {
            if (_onApplicationQuitListeners.ContainsKey(context))
            {
                _onApplicationQuitListeners[context] -= action;
            }
        }

        public static void RemoveOnGuiListener(Action listener)
        {
            RemoveListener(ref _onGuiListeners, GLOBAL, listener);
        }

        public static void RemoveUpdateListener(Action<float> listener)
        {
            RemoveListener(ref _updateListeners, GLOBAL, listener);
        }

        public static void RemoveFixedUpdateListener(Action<float> listener)
        {
            RemoveListener(ref _fixedUpdateListeners, GLOBAL, listener);
        }

        public static void RemoveLateUpdateListener(Action<float> listener)
        {
            RemoveListener(ref _lateUpdateListeners, GLOBAL, listener);
        }

        public static void RemoveLateFixedUpdateListener(Action<float> listener)
        {
            RemoveListener(ref _lateFixedUpdateListeners, GLOBAL, listener);
        }

        public static void RemoveOnGuiListener(object context, Action listener)
        {
            RemoveListener(ref _onGuiListeners, context, listener);
        }

        public static void RemoveOnGuiListeners(object context)
        {
            RemoveListeners(ref _onGuiListeners, context);
        }

        public static void RemoveUpdateListener(object context, Action<float> listener)
        {
            RemoveListener(ref _updateListeners, context, listener);
        }

        public static void RemoveUpdateListeners(object context)
        {
            RemoveListeners(ref _updateListeners, context);
        }

        public static void RemoveFixedUpdateListener(object context, Action<float> listener)
        {
            RemoveListener(ref _fixedUpdateListeners, context, listener);
        }

        public static void RemoveFixedUpdateListeners(object context)
        {
            RemoveListeners(ref _fixedUpdateListeners, context);
        }

        public static void RemoveLateUpdateListener(object context, Action<float> listener)
        {
            RemoveListener(ref _lateUpdateListeners, context, listener);
        }

        public static void RemoveLateUpdateListeners(object context)
        {
            RemoveListeners(ref _lateUpdateListeners, context);
        }

        public static void RemoveLateFixedUpdateListener(object context, Action<float> listener)
        {
            RemoveListener(ref _lateFixedUpdateListeners, context, listener);
        }

        public static void RemoveLateFixedUpdateListeners(object context)
        {
            RemoveListeners(ref _lateFixedUpdateListeners, context);
        }

        public static void RemoveAllListeners(string context)
        {
            RemoveUpdateListeners(context);
            RemoveFixedUpdateListeners(context);
            RemoveLateUpdateListeners(context);
            RemoveLateFixedUpdateListeners(context);
        }

        internal static void RemoveListener(ref Dictionary<object, List<Action<float>>> listeners, object context, Action<float> listener)
        {
            listeners.IfSome(dictionary =>
            {
                if (dictionary.TryGetValue(context, out var list))
                {
                    list.Remove(listener);
                }
            });
        }

        internal static void RemoveListener(ref Dictionary<object, Action> listeners, object context, Action listener)
        {
            listeners.IfSome(dictionary =>
            {
                if (dictionary.ContainsKey(context))
                {
                    dictionary[context] -= listener;
                }
            });
        }

        internal static void RemoveListeners(ref Dictionary<object, List<Action<float>>> listeners, object context)
        {
            listeners.Remove(context);
        }

        internal static void RemoveListeners(ref Dictionary<object, Action> listeners, object context)
        {
            listeners.Remove(context);
        }

        public static void AddListener(ref Dictionary<object, List<Action<float>>> listeners, object context, Action<float> listener)
        {
            Init().Forget();
            if (listeners == null) { listeners = new Dictionary<object, List<Action<float>>>(); }
            if (!listeners.ContainsKey(context)) { listeners.Add(context, new List<Action<float>>()); }
            if (listener == null) { return; }
            listeners[context].Add(listener);
        }

        public static void AddListener(ref Dictionary<object, Action> listeners, object context, Action listener)
        {
            Init().Forget();
            if (listeners == null)
            {
                listeners = new();
            }

            if (listeners.ContainsKey(context))
            {
                listeners[context] += listener;
            }
            else
            {
                listeners[context] = listener;
            }
        }

        internal static void OnApplicationQuit()
        {
            foreach (var (context, action) in _onApplicationQuitListeners)
            {
                action?.Invoke();
            }
        }
        internal static void OnGUI()
        {
            DoUpdate(_onGuiListeners);
        }

        internal static void OnUpdate(float delta)
        {
            DoUpdate(_updateListeners, delta);
            MessageServices.V1.Raise<MsgUpdate, float>(delta);
        }

        internal static void OnFixedUpdate(float delta)
        {
            DoUpdate(_fixedUpdateListeners, delta);
            MessageServices.V1.Raise<MsgFixedUpdate, float>(delta);
        }

        internal static void OnLateUpdate(float delta)
        {
            DoUpdate(_lateUpdateListeners, delta);
            MessageServices.V1.Raise<MsgLateUpdate, float>(delta);
        }

        internal static void OnLateFixedUpdate(float delta)
        {
            DoUpdate(_lateFixedUpdateListeners, delta);
            MessageServices.V1.Raise<MsgLateFixedUpdate, float>(delta);
        }

        private static void DoUpdate(Safe<Dictionary<object, List<Action<float>>>> listeners, float delta)
        {
            listeners
                .IfSome(dictionary =>
                {
                    foreach (var key in dictionary.Keys.ToArray())//TODO - could cache these keys to avoid the copy each frame, if we use this a ton and it becomes a problem
                    {
                        foreach(var action in dictionary[key].ToArray()) 
                        {
                            action?.Invoke(delta);
                        }                        
                    }
                });
        }

        private static void DoUpdate(Safe<Dictionary<object, Action>> listeners)
        {
            listeners
                .IfSome(dictionary =>
                {
                    foreach (var val in dictionary.Values.ToArray())
                    {
                        val?.Invoke();
                    }
                });
        }

        public static CancellationTokenSource RunInSeconds(float numSeconds, Action action)
        {
            async UniTask WaitForSecondsInternal(float numSeconds, Action action, CancellationTokenSource canceler)
            {
                await UniTask.WaitForSeconds(numSeconds, delayTiming: PlayerLoopTiming.Update, cancellationToken: canceler.Token);
                if (!canceler.IsCancellationRequested)
                {
                    action?.Invoke();
                }
            }

            var canceler = new CancellationTokenSource();
            WaitForSecondsInternal(numSeconds, action, canceler).Forget();
            return canceler;
        }

        public static CancellationTokenSource RunInSecondsRealtime(float numSeconds, Action action)
        {
            async UniTask WaitForSecondsRealtimeInternal(float numSeconds, Action action, CancellationTokenSource canceler)
            {
                await UniTask.WaitForSeconds(numSeconds, ignoreTimeScale: true, cancellationToken: canceler.Token);
                if (!canceler.IsCancellationRequested)
                {
                    action?.Invoke();
                }
            }

            var canceler = new CancellationTokenSource();
            WaitForSecondsRealtimeInternal(numSeconds, action, canceler).Forget();
            return canceler;
        }

        public static CancellationTokenSource RunAfterFixedFrames(int numFrames, Action action)
        {
            static async UniTask RunAfterFixedFramesInternal(int numFrames, Action action, CancellationTokenSource canceler)
            {
                for (int i = 0; i < numFrames; i++)
                {
                    if (canceler.IsCancellationRequested)
                    {
                        return;
                    }

                    await UniTask.WaitForFixedUpdate(canceler.Token);
                }

                if (!canceler.IsCancellationRequested)
                {
                    action?.Invoke();
                }
            }

            var canceler = new CancellationTokenSource();
            RunAfterFixedFramesInternal(numFrames, action, canceler).Forget();
            return canceler;
        }

        public static CancellationTokenSource RunInFrames(int numFrames, Action action)
        {
            static async UniTask RunInFramesInternal(int numFrames, Action action, CancellationTokenSource canceler)
            {
                await UniTask.DelayFrame(numFrames, cancellationToken: canceler.Token);
                if (!canceler.IsCancellationRequested)
                {
                    action?.Invoke();
                }
            }

            var canceler = new CancellationTokenSource();
            RunInFramesInternal(numFrames, action, canceler).Forget();
            return canceler;
        }

        public static CancellationTokenSource RunAtEndOfFrame(Action action)
        {
            return RunAfterFrame(action);
        }

        public static CancellationTokenSource RunAfterFrame(Action action)
        {
            static async UniTask RunAfterFrameInternal(Action action, CancellationTokenSource canceler)
            {
                await UniTask.WaitForEndOfFrame(canceler.Token);
                if (!canceler.IsCancellationRequested)
                {
                    action?.Invoke();
                }
            }

            var canceler = new CancellationTokenSource();
            RunAfterFrameInternal(action, canceler).Forget();
            return canceler;
        }

        public static CancellationTokenSource RunAfter(Func<bool> condition, Action action, PlayerLoopTiming timing = PlayerLoopTiming.Update)
        {
            static async UniTask RunAfterInternal(Func<bool> condition, Action action, PlayerLoopTiming timing, CancellationTokenSource canceler)
            {
                await UniTask.WaitUntil(condition, timing, cancellationToken: canceler.Token);
                if (!canceler.IsCancellationRequested)
                {
                    action?.Invoke();
                }
            }

            var canceler = new CancellationTokenSource();
            RunAfterInternal(condition, action, timing, canceler).Forget();
            return canceler;
        }

        public static void RunOnMainThread(Action action)
        {
            static async UniTask RunOnMainThreadInternal(Action action)
            {
                if (!PlayerLoopHelper.IsMainThread)
                {
                    await UniTask.SwitchToMainThread();
                }

                action?.Invoke();
            }

            RunOnMainThreadInternal(action).Forget();
        }
    }
}