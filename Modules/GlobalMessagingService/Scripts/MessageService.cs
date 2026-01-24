using System;
using System.Linq;
using System.Collections.Generic;
using HitTrax.CoreUtilities;
using static HitTrax.CoreUtilities.SafeFunctions;
using Cysharp.Threading.Tasks;

namespace HitTrax.GlobalMessagingService
{
    public class MessageServices
    {
        public static IMessageService_v1 V1 => Services.Get<IMessageService_v1>();
        //public static IMessageService_v1 V2 => Services.Get<IMessageService_v2>();
    }

    public class MessageService : IMessageService_v1 { }

    public interface IMessageKey<T> { }
    public interface IMessageKey : IMessageKey<Nothing> { }

    public interface IMessageService_v1 : IService
    {
        Nothing AddListener<TKey, TArgs>(Action<TArgs> callback, bool preventDuplicate = false) where TKey : IMessageKey<TArgs> => MessageServiceInternal.AddListener<TKey, TArgs>(callback, preventDuplicate);

        Nothing AddListener<TKey, TArgs>(Action callback, bool preventDuplicate = false) where TKey : IMessageKey<TArgs> => MessageServiceInternal.AddListener<TKey, TArgs>(callback, preventDuplicate);

        Nothing AddListener<TKey>(Action callback, bool preventDuplicate = false) where TKey : IMessageKey<Nothing> => AddListener<TKey, Nothing>(callback, preventDuplicate);

        Nothing AddListener<TKeyAndArgs>(Action<TKeyAndArgs> callback, bool preventDuplicate = false) where TKeyAndArgs : IMessageKey<TKeyAndArgs> => MessageServiceInternal.AddListener<TKeyAndArgs, TKeyAndArgs>(callback, preventDuplicate);

        bool RemoveListener<TKey, TArgs>(Action<TArgs> callback) where TKey : IMessageKey<TArgs> => MessageServiceInternal.RemoveListener<TKey, TArgs>(callback);

        bool RemoveListener<TKey, TArgs>(Action callback) where TKey : IMessageKey<TArgs> => MessageServiceInternal.RemoveListener<TKey, TArgs>(callback);

        bool RemoveListener<TKey>(Action callback) where TKey : IMessageKey<Nothing> => RemoveListener<TKey, Nothing>(callback);

        bool RemoveListener<TKeyAndArgs>(Action<TKeyAndArgs> callback) where TKeyAndArgs : IMessageKey<TKeyAndArgs> => MessageServiceInternal.RemoveListener<TKeyAndArgs, TKeyAndArgs>(callback);
        
        bool RemoveListener(string eventName, Action<object> callback) => MessageServiceInternal.RemoveListener(eventName, callback);

        Nothing AddListenerOnce<TKey, TArgs>(Action<TArgs> callback, bool preventDuplicate = false) where TKey : IMessageKey<TArgs> => MessageServiceInternal.AddListenerOnce<TKey, TArgs>(callback, preventDuplicate);

        Nothing AddListenerOnce<TKey>(Action callback, bool preventDuplicate = false) where TKey : IMessageKey<Nothing> => AddListenerOnce<TKey, Nothing>(_ => callback?.Invoke(), preventDuplicate);

        Nothing AddListenerOnce<TKeyAndArgs>(Action<TKeyAndArgs> callback, bool preventDuplicate = false) where TKeyAndArgs : IMessageKey<TKeyAndArgs> => MessageServiceInternal.AddListenerOnce<TKeyAndArgs, TKeyAndArgs>(callback, preventDuplicate);

        TArgs TryRaise<TEvt, TArgs>(Func<bool> req, TArgs val) where TEvt : IMessageKey<TArgs> => MessageServiceInternal.TryRaise<TEvt, TArgs>(req, val);

        Nothing TryRaise<TEvt>(Func<bool> req) where TEvt : IMessageKey<Nothing> => TryRaise<TEvt, Nothing>(req, default);

        TKeyAndArgs TryRaise<TKeyAndArgs>(Func<bool> req, TKeyAndArgs args) where TKeyAndArgs : IMessageKey<TKeyAndArgs> => MessageServiceInternal.TryRaise<TKeyAndArgs, TKeyAndArgs>(req, args);

        [UnityEngine.HideInCallstack]
        TArgs Raise<TKey, TArgs>(TArgs args) where TKey : IMessageKey<TArgs> => MessageServiceInternal.Raise<TKey, TArgs>(args);

        [UnityEngine.HideInCallstack]
        Nothing Raise<TKey>() where TKey : IMessageKey<Nothing> => Raise<TKey, Nothing>(default);

        [UnityEngine.HideInCallstack]
        TKeyAndArgs Raise<TKeyAndArgs>(TKeyAndArgs args) where TKeyAndArgs : IMessageKey<TKeyAndArgs> => MessageServiceInternal.Raise<TKeyAndArgs, TKeyAndArgs>(args);

        Nothing AddListener(string eventName, Action<object> callback) => MessageServiceInternal.AddListener(eventName, callback);

        Nothing AddListener(string eventName, Action callback) => MessageServiceInternal.AddListener(eventName, callback);

        Nothing AddListener<TArgs>(string eventName, Action<TArgs> callback) => MessageServiceInternal.AddListener(eventName, (obj) => callback?.Invoke((TArgs)obj));
    }

    internal static class MessageServiceInternal
    {
        internal static Dictionary<string, List<Action<object>>> namedEventsDict = new();

        internal static Dictionary<string, List<Action>> parameterlessNamedEventsDict = new();

        internal static bool CanInvoke<K, V>(Dictionary<K, V> dict, K key) => dict != null && dict.ContainsKey(key) && dict[key] != null;

        internal static Nothing AddListener<TKey, TArgs>(Action<TArgs> callback, bool preventDuplicate = false) where TKey : IMessageKey<TArgs> => AddListener(typeof(TKey), callback, preventDuplicate);
        internal static Nothing AddListener<TKey, TArgs>(Action callback, bool preventDuplicate = false) where TKey : IMessageKey<TArgs>
        {
            GenericMessageManager<TKey, TArgs>.parameterlessActionWrappers[callback] = _ => callback?.Invoke();
            return AddListener<TKey, TArgs>(GenericMessageManager<TKey, TArgs>.parameterlessActionWrappers[callback], preventDuplicate);
        }

        internal static bool RemoveListener<TKey, TArgs>(Action<TArgs> callback) where TKey : IMessageKey<TArgs> => RemoveListener(typeof(TKey), callback);
        internal static bool RemoveListener<TKey, TArgs>(Action callback) where TKey : IMessageKey<TArgs>
        {
            if (GenericMessageManager<TKey, TArgs>.parameterlessActionWrappers.TryGetValue(callback, out var wrapper))
            {
                GenericMessageManager<TKey, TArgs>.parameterlessActionWrappers.Remove(callback);
                return RemoveListener<TKey, TArgs>(wrapper);
            }

            return false;
        }

        internal static Nothing AddListenerOnce<TKey, TArgs>(Action<TArgs> callback, bool preventDuplicate = false) where TKey : IMessageKey<TArgs> => AddListenerOnce(typeof(TKey), callback, preventDuplicate);

        [UnityEngine.HideInCallstack]
        internal static TArgs TryRaise<TEvt, TArgs>(Func<bool> req, TArgs val) where TEvt : IMessageKey<TArgs> => req() ? Raise<TEvt, TArgs>(val) : val;

        [UnityEngine.HideInCallstack]
        internal static TArgs Raise<TKey, TArgs>(TArgs args) where TKey : IMessageKey<TArgs> => Raise(typeof(TKey), args);

        internal static Nothing AddListener(string eventName, Action<object> callback)
        {
            namedEventsDict.AddToList(eventName, callback);
            return None;
        }

        internal static Nothing AddListener(string eventName, Action callback)
        {
            parameterlessNamedEventsDict.AddToList(eventName, callback);
            return None;
        }

        internal static bool RemoveListener(string eventName, Action<object> callback) => namedEventsDict.RemoveFromList(eventName, callback);

        private static Nothing AddListener<TKey, TArgs>(TKey key, Action<TArgs> callback, bool preventDuplicate = false) => GenericMessageManager<TKey, TArgs>.AddListener(key, callback, preventDuplicate);

        private static bool RemoveListener<TKey, TArgs>(TKey key, Action<TArgs> callback) => GenericMessageManager<TKey, TArgs>.RemoveListener(key, callback);

        private static Nothing AddListenerOnce<TKey, TArgs>(TKey key, Action<TArgs> callback, bool preventDuplicate = false) => GenericMessageManager<TKey, TArgs>.AddListenerOnce(key, callback, preventDuplicate);
        [UnityEngine.HideInCallstack]
        private static TArgs Raise<TKey, TArgs>(TKey key, TArgs args = default) => GenericMessageManager<TKey, TArgs>.Raise(key, args);

        private static class GenericMessageManager<TKey, TArgs>
        {
            internal static Dictionary<TKey, Action<TArgs>> eventsDict = new Dictionary<TKey, Action<TArgs>>();
            internal static Dictionary<TKey, Action<TArgs>> onceEventsDict = new Dictionary<TKey, Action<TArgs>>();
            internal static Dictionary<Action, Action<TArgs>> parameterlessActionWrappers = new();


            private static bool FailsDuplicateRule(Dictionary<TKey, Action<TArgs>> dict, Action<TArgs> callback, bool preventDuplicate) => preventDuplicate && dict.ContainsValue(callback);

            /// <summary>
            /// Adds the listener.
            /// </summary>
            internal static Nothing AddListener(TKey key, Action<TArgs> callback, bool preventDuplicate = false) => AddListener(eventsDict, key, callback, preventDuplicate);

            /// <summary>
            /// Adds the listener once. Listener is removed once invoked.
            /// </summary>
            internal static Nothing AddListenerOnce(TKey key, System.Action<TArgs> callback, bool preventDuplicate = false) => AddListener(onceEventsDict, key, callback, preventDuplicate);

            internal static Nothing AddListener(Dictionary<TKey, Action<TArgs>> dict, TKey key, System.Action<TArgs> callback, bool preventDuplicate)
            {
                if (FailsDuplicateRule(dict, callback, preventDuplicate))
                {
                    return None;
                }

                if (!dict.ContainsKey(key) && (!preventDuplicate || !dict.ContainsValue(callback)))
                {
                    dict[key] = callback;
                }
                else
                {
                    dict[key] += callback;
                }

                return None;
            }

            internal static bool RemoveListener(TKey key, Action<TArgs> callback)
            {
                var removeA = TryRemoveListener(eventsDict, key, callback);
                var removeB = TryRemoveListener(onceEventsDict, key, callback);
                return removeA || removeB;
            }

            private static bool TryRemoveListener(Dictionary<TKey, Action<TArgs>> dict, TKey key, Action<TArgs> callback)
            {
                if (dict.ContainsKey(key))
                {
                    dict[key] -= callback;
                    return true;
                }

                return false;
            }

            [UnityEngine.HideInCallstack]
            internal static TArgs Raise(TKey key, TArgs args)
            {
                //$"Raising {key} => {args}".Debug();

                static async UniTask RaiseInternal(TKey key, TArgs args)
                {
                    if (!PlayerLoopHelper.IsMainThread)
                    {
                        await UniTask.SwitchToMainThread();                      
                    }

                    TryRaise(eventsDict, key, args);
                    TryRaise(namedEventsDict, key, args);
                    TryRaise(parameterlessNamedEventsDict, key);
                    TryRaise(onceEventsDict, key, args, remove: true);
                }

                RaiseInternal(key, args).Forget();
                return args;
            }

            private static void TryRaise(Dictionary<TKey, Action<TArgs>> dict, TKey key, TArgs args, bool remove = false)
            {
                if (CanInvoke(dict, key))
                {
                    dict[key](args);

                    if (remove)
                    {
                        dict.Remove(key);
                    }
                }
            }

            private static void TryRaise<K>(Dictionary<string, List<Action>> dict, K key)
            {
                var eventName = key.ToString().Split('.').LastOrDefault();

                if (CanInvoke(dict, eventName))
                {
                    foreach (var action in dict[eventName])
                    {
                        if (action != null)
                        {
                            action();
                        }
                    }
                }
            }

            private static void TryRaise<K, A>(Dictionary<string, List<Action<object>>> dict, K key, A args)
            {
                var eventName = key.ToString().Split('.').LastOrDefault();

                if (CanInvoke(dict, eventName))
                {
                    foreach (var action in dict[eventName])
                    {
                        action?.Invoke(args);
                    }
                }
            }
        }
    }
}