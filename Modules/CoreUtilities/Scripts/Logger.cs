using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace HitTrax.CoreUtilities
{
    public static class Logger
    {
        private const string CAT_SEPARATOR = ",";
        private static HashSet<object> _logOnceKeys = new();
        private static Dictionary<string, List<string>> _logCacheByCategory = new();
        private static List<string> _globalLogCache = new();

        public static void Init()
        {
            Console.SetOut(new ConsoleToDebug());
            Console.SetError(new ConsoleToDebug(true));
        }

        private static string ReplaceUnusedColors(this string message)
        {
            // Replace colors that are not displaying in app
            message = message.Replace("<color=magenta>", "<color=purple>");
            message = message.Replace("<color=cyan>", "<color=blue>");
            return message;
        }

        private static void AddToGlobalLogCache(string message)
        {            
            _globalLogCache.Add(message.ReplaceUnusedColors());
        }

        private static void AddToLogCache(string message, string cat)
        {
            if (!_logCacheByCategory.ContainsKey(cat))
                {
                    _logCacheByCategory.Add(cat, new List<string>());
                }

                _logCacheByCategory[cat].Add(message.ReplaceUnusedColors());
        }

        public static void ClearLogCache()
        {
            _logCacheByCategory.Clear();
            _globalLogCache.Clear();
        }

        [HideInCallstack]
        public static string Log(this string message, object context = null)
        {
            AddToGlobalLogCache(message);
            UnityEngine.Debug.Log(message, context as UnityEngine.Object);
            return message;
        }

        [HideInCallstack]
        public static string Log(this string message, string categories, object context = null)
        {
            foreach (var cat in categories.Split(CAT_SEPARATOR))
            {
                AddToLogCache(message, cat);
            }

            Log(message, context);
            return message;
        }

        [HideInCallstack]
        public static string LogError(this string message, object context = null)
        {
            AddToGlobalLogCache($"Error: {message}");
            UnityEngine.Debug.LogError(message, context as UnityEngine.Object);
            return message;
        }

        [HideInCallstack]
        public static string LogWarning(this string message, object context = null)
        {
            if (!DebugMode.IsActive)
            {
                return message;
            }

            AddToGlobalLogCache($"Warning: {message}");
            UnityEngine.Debug.LogWarning(message, context as UnityEngine.Object);
            return message;
        }

        [HideInCallstack]
        public static Exception LogException(this Exception ex, object context = null)
        {
            AddToGlobalLogCache($"Exception: {ex.ToString()}");
            UnityEngine.Debug.LogException(ex, context as UnityEngine.Object);
            return ex;
        }

        [HideInCallstack]
        public static string Log(this string message, bool shouldDisplay, object context = null)
        {
            if (!DebugMode.IsActive)
            {
                return message;
            }

            if (shouldDisplay)
            {
                return message.Log(context);
            }

            return message;
        }

        [HideInCallstack]
        public static void LogError(this string message, bool shouldDisplay, object context = null)
        {
            if (shouldDisplay)
            {
                AddToGlobalLogCache($"Error: {message}");
                message.LogError(context);
            }
        }

        [HideInCallstack]
        public static void LogWarning(this string message, bool shouldDisplay, object context = null)
        {
            if (shouldDisplay)
            {
                AddToGlobalLogCache($"Warning: {message}");
                message.LogWarning(context);
            }
        }

        [HideInCallstack]
        public static void LogInfo(this string message, bool shouldDisplay = true, object context = null)
        {
            if (shouldDisplay)
            {
                var prefix = Application.isEditor ? $"<color=green>INFO: </color>" : "INFO: ";
                $"{prefix}{message}".Log(context);
            }
        }

        [HideInCallstack]
        public static void LogCaution(this string message, bool shouldDisplay = true, object context = null)
        {
            if (shouldDisplay)
            {
                var prefix = Application.isEditor ? $"<color=yellow>CAUTION: </color>" : "CAUTION: ";
                $"{prefix}{message}".Log(context);
            }
        }

        [HideInCallstack]
        public static void LogDebug(this string message, bool shouldDisplay, object context = null)
        {
            if (shouldDisplay || DebugMode.IsActive)
            {
                var prefix = Application.isEditor ? $"<color=cyan>DEBUG: </color>" : "DEBUG: ";
                $"{prefix}{message}".Log(context);
            }
        }

#if UNITY_EDITOR
        [HideInCallstack]
        public static void LogTrace(this string message, bool shouldDisplay = true, object context = null)
        {
            if (shouldDisplay)
            {
                var timingString = $"({GlobalTime.Frame}f-{GlobalTime.Secs.ToString("F3")}s)";
                var timing = Application.isEditor ? $"<color=yellow>{timingString}</color>" : timingString;
                var prefix = Application.isEditor ? $"<color=magenta>TRACE</color>" : "TRACE:";
                $"{prefix} {timing}: {message}".Log(context);
            }
        }
#endif

        [HideInCallstack]
        public static void Log(this string message, Func<bool> req, object context = null)
        {
            if (!DebugMode.IsActive)
            {
                return;
            }

            if (req?.Invoke() == true)
            {
                message.Log(context);
            }
        }

        [HideInCallstack]
        public static void Log(this string message, Func<bool> req, string categories, object context = null)
        {
            if(req == null || req())
            {
                message.Log(categories, context);
            }
        }

        [HideInCallstack]
        public static void LogOnce(string message, object key, object context = null)
        {
            if (!_logOnceKeys.Contains(key))
            {
                message.Log(context);
                AddToGlobalLogCache(message);
                _logOnceKeys.Add(key);
            }
        }

        [HideInCallstack]
        public static T LogThis<T>(this T self)
        {
            self.ToString().Log();
            return self;
        }

        [HideInCallstack]
        public static T LogThis<T>(this T self, Func<T, string> message)
        {
            message(self).Log();
            return self;
        }

        [HideInCallstack]
        public static void Log<T>(this T item, Func<bool> req)
        {
            if (req?.Invoke() == true)
            {
                item.ToString().Log();
            }
        }

        [HideInCallstack]
        public static T Log<T>(this T source, string message, string categories, Func<bool> req)
        {
            if (req?.Invoke() == true)
            {
                foreach (var cat in categories.Split(CAT_SEPARATOR))
                {
                    Log(message, cat);
                }
            }

            return source;
        }

        [HideInCallstack]
        public static T LogFrom<T>(this T source, string message, Func<bool> req)
        {
            message.Log(req);
            return source;
        }

        [HideInCallstack]
        public static T LogFrom<T>(this T source, string message)
        {
            message.Log(source);
            return source;
        }

        [HideInCallstack]
        public static T LogFrom<T>(this T source, string message, object context)
        {
            message.Log(context);
            return source;
        }

        [HideInCallstack]
        public static void LogWarning(this string message, Func<bool> req)
        {
            if (req?.Invoke() == true)
            {
                message.LogWarning();
            }
        }

        [HideInCallstack]
        public static T LogWarning<T>(this T source, string message, Func<bool> req)
        {
            LogWarning(message, req);
            return source;
        }

        [HideInCallstack]
        public static T LogWarning<T>(this T source, string message)
        {
            message.LogWarning();
            return source;
        }

        [HideInCallstack]
        public static T LogWarning<T>(this T source, string message, object context)
        {
            message.LogWarning(context);
            return source;
        }

        [HideInCallstack]
        public static void LogError(this string message, Func<bool> req)
        {
            if (req?.Invoke() == true)
            {
                message.LogError();
            }
        }

        [HideInCallstack]
        public static T LogError<T>(this T source, string message, Func<bool> req)
        {
            message.LogError(req);
            return source;
        }

        [HideInCallstack]
        public static T LogError<T>(this T source, string message)
        {
            message.LogError();
            return source;
        }

        [HideInCallstack]
        public static T LogError<T>(this T source, string message, object context)
        {
            message.LogError(context);
            return source;
        }

        [HideInCallstack]
        public static void Info(this string message, bool shouldDisplay = true, object context = null)
            => LogInfo(message, shouldDisplay, context);

        [HideInCallstack]
        //"ShouldDisplay" is mandatory here, unlike the other levels. Since it's 'Debug', We want to make sure you're in "debug" mode before showing
        public static void Debug(this string message, bool shouldDisplay = true, object context = null)
            => LogDebug(message, shouldDisplay, context);

#if UNITY_EDITOR
        //(Priyal) This obsolete tag makes it more obvious in an IDE if you forgot to delete a Trace before merging to stable
        //It also makes it easier to find your relevant logs at a glance in a long script
        [HideInCallstack, Obsolete("This should be used as a personal log function for developing and debugging, and should not be committed.", false)]
        public static void Trace(this string message, bool shouldDisplay, object context = null)
            => LogTrace(message, shouldDisplay, context);

        //(Priyal) This obsolete tag makes it more obvious in an IDE if you forgot to delete a Trace before merging to stable
        //It also makes it easier to find your relevant logs at a glance in a long script
        [HideInCallstack, Obsolete("This should be used as a personal log function for developing and debugging, and should not be committed.", false)]
        public static void Trace(this string message, object context = null)
            => message.Trace(true, context);
#endif

        [HideInCallstack]
        public static void Caution(this string message, bool shouldDisplay = true, object context = null)
            => LogCaution(message, shouldDisplay, context);

        [HideInCallstack]
        public static void Error(this string message, bool shouldDisplay = true, object context = null)
            => LogError(message, shouldDisplay, context);

        [HideInCallstack]
        public static void LogGui(this string message, bool shouldDisplay = true, object context = null)
            => LogGui(message, shouldDisplay, context);

        public static List<string> GetGlobalLogCache()
            => _globalLogCache;

        public static List<string> GetGlobalLogCache(IEnumerable<string> exclude)
        {
            // (Anthony) I'd like to do this without creating a new list each time
            List<string> results = new List<string>();
            foreach (var item in _globalLogCache.ToList())//make a copy for thread safety
            {
                if (!exclude.Contains(item))
                {
                    results.Add(item);
                }
            }

            return results;
        }

        public static List<string> GetLogCache(this string categories)
            => GetLogCache(categories.Split(CAT_SEPARATOR));

        public static List<string> GetLogCache(this IEnumerable<string> categories)
        {
            // UnityEngine.Debug.Log($"GetLogCache Cats: {categories.Count()}");
            List<string> results = new();
            foreach (var cat in categories)
            {
                //UnityEngine.Debug.Log($"GetLogCache Cat: {cat}");
                _logCacheByCategory
                        .TryGet(cat)
                        .IfSome(list =>
                        {
                            //UnityEngine.Debug.Log($"GetLogCache List: {list.Count}");
                            results.AddRange(list);
                        })
                        .IfNone(() =>
                        {
                            //UnityEngine.Debug.Log($"GetLogCache List not found {cat}");
                        });
                        ;
            }
            return results;
        }
        
        public static List<string> GetLogCache(this IEnumerable<string> categories, IEnumerable<string> exclude)
        {
            List<string> results = new();
            foreach (var cat in categories)
            {
                if (!exclude.Contains(cat))
                {
                    _logCacheByCategory
                        .TryGet(cat)
                        .IfSome(list =>
                        {                           
                            results.AddRange(list);
                        })                        
                    ;
                }                
            }

            return results;
        }

    }

    public class ConsoleToDebug : TextWriter
    {
        private readonly bool _isError;

        public ConsoleToDebug(bool isError = false)
        {
            _isError = isError;
        }

        public override void WriteLine(string message)
        {
            try
            {
                if (!Application.isPlaying)
                { return; }

                string prefix = _isError ? "<color=red>Console.Error: </color>" : "<color=grey>Console.WriteLine: </color>";
                string stack = Application.isEditor || _isError ? $"\n\n{Environment.StackTrace}" : string.Empty;

                var msg = $"{prefix}{message}{stack}";
                if (_isError)
                {
                    msg.Error();
                }
                else
                {
                    msg.Log();
                }
            }
            catch { }
        }

        public override Encoding Encoding => Encoding.UTF8;
    }
}