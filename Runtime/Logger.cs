using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;

namespace HitTrax.CoreUtilities
{
	public static class Logger
    {
        private static HashSet<object> _logOnceKeys = new();
        private static Stopwatch _traceTimeStamp = Stopwatch.StartNew();
        private static int _traceFrameCount ;

        public static void Init()
        {
            static async UniTask CacheFrameCountLoop()
            {
                if (!PlayerLoopHelper.IsMainThread)
                {
                    await UniTask.SwitchToMainThread();
                }

                while (Application.isPlaying)
                {
                    await UniTask.Yield(PlayerLoopTiming.PreUpdate);
                    _traceFrameCount = Time.frameCount;
                }
            }

            _traceFrameCount = -1;
            CacheFrameCountLoop().Forget();
            Console.SetOut(new ConsoleToDebug());
            Console.SetError(new ConsoleToDebug(true));
        }


        [HideInCallstack]
        public static void Log(this string message, UnityEngine.Object context = null) => UnityEngine.Debug.Log(message);

        [HideInCallstack]
        public static void LogError(this string message, UnityEngine.Object context = null) => UnityEngine.Debug.LogError(message, context);

        [HideInCallstack]
        public static void LogWarning(this string message, UnityEngine.Object context = null) => UnityEngine.Debug.LogWarning(message, context);

        [HideInCallstack]
        public static void LogException(this Exception ex, UnityEngine.Object context = null) => UnityEngine.Debug.LogException(ex, context);

        [HideInCallstack]
        public static void Log(this string message, bool shouldDisplay, UnityEngine.Object context = null)
        {
            if (shouldDisplay)
            {
                message.Log(context);
            }
        }

        [HideInCallstack]
        public static void LogError(this string message, bool shouldDisplay, UnityEngine.Object context = null)
        {
            if (shouldDisplay)
            {
                message.LogError(context);
            }
        }

        [HideInCallstack]
        public static void LogWarning(this string message, bool shouldDisplay, UnityEngine.Object context = null)
        {
            if (shouldDisplay)
            {
                message.LogWarning(context);
            }
        }

        [HideInCallstack]
        public static void LogInfo(this string message, bool shouldDisplay = true, UnityEngine.Object context = null)
        {
            if (shouldDisplay)
            {
                var prefix = Application.isEditor ? $"<color=green>INFO: </color>" : "INFO: ";
                $"{prefix}{message}".Log(context);
            }
        }

        [HideInCallstack]
        public static void LogCaution(this string message, bool shouldDisplay = true, UnityEngine.Object context = null)
        {
            if (shouldDisplay)
            {
                var prefix = Application.isEditor ? $"<color=yellow>CAUTION: </color>" : "CAUTION: ";
                $"{prefix}{message}".Log(context);
            }
        }

        [HideInCallstack]
        public static void LogDebug(this string message, bool shouldDisplay, UnityEngine.Object context = null)
        {
            if (shouldDisplay) //TODO - For Debug, We should probably have a global super-debug mode where we log all debugs.
            {
                var prefix = Application.isEditor ? $"<color=cyan>DEBUG: </color>" : "DEBUG: ";
                $"{prefix}{message}".Log(context);
            }
        }

        
//#if UNITY_EDITOR
        [HideInCallstack]
        public static void LogTrace(this string message, bool shouldDisplay = true, UnityEngine.Object context = null)
        {
            if (shouldDisplay)
            {
                var timingString = $"({_traceFrameCount}f-{_traceTimeStamp.Elapsed.TotalSeconds.ToString("F3")}s)";
                var timing = Application.isEditor ? $"<color=yellow>{timingString}</color>" : timingString;
                var prefix = Application.isEditor ? $"<color=magenta>TRACE</color>" : "TRACE";
                $"{prefix} {timing}: {message}".Log(context);
            }
        }
//#endif

        [HideInCallstack]
        public static void Log(this string message, Func<bool> req, UnityEngine.Object context = null)
        {
            if (req != null && req())
            {
                message.Log(context);
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
        public static T Log<T>(this T source, string message, Func<bool> req)
        {
            message.Log(req);
            return source;
        }

        [HideInCallstack]
        public static T Log<T>(this T source, string message)
        {
            message.Log(source as UnityEngine.Object);
            return source;
        }

        [HideInCallstack]
        public static T Log<T>(this T source, string message, UnityEngine.Object context)
        {
            message.Log(context);
            return source;
        }

        [HideInCallstack]
        public static void LogWarning(string message, Func<bool> req)
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
        public static T LogWarning<T>(this T source, string message, UnityEngine.Object context)
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
        public static T LogError<T>(this T source, string message, UnityEngine.Object context)
        {
            message.LogError(context);
            return source;
        }

        [HideInCallstack]
        public static void LogOnce(string message, object key, UnityEngine.Object context = null)
        {
            if (!_logOnceKeys.Contains(key))
            {
                message.Log(context);
                _logOnceKeys.Add(key);
            }
        }

        [HideInCallstack]
        public static void Info(this string message, bool shouldDisplay = true, UnityEngine.Object context = null)
            => LogInfo(message, shouldDisplay, context);

        [HideInCallstack]
        //"ShouldDisplay" is mandatory here, unlike the other levels. Since it's 'Debug', We want to make sure you're in "debug" mode before showing
        public static void Debug(this string message, bool shouldDisplay = true, UnityEngine.Object context = null)
            => LogDebug(message, shouldDisplay, context);

        /*
#if UNITY_EDITOR
        //(Priyal) This obsolete tag makes it more obvious in an IDE if you forgot to delete a Trace before committing
        //It also makes it easier to find your relevant logs at a glance in a long script
        [HideInCallstack, Obsolete("This should be used as a personal log function for developing and debugging, and should not be committed.")]
        */
        [HideInCallstack]
        public static void Trace(this string message, bool shouldDisplay = true, UnityEngine.Object context = null)
            => LogTrace(message, shouldDisplay, context);
//#endif

        [HideInCallstack]
        public static void Caution(this string message, bool shouldDisplay = true, UnityEngine.Object context = null)
            => LogCaution(message, shouldDisplay, context);

        [HideInCallstack]
        public static void Error(this string message, bool shouldDisplay = true, UnityEngine.Object context = null)
            => LogError(message, shouldDisplay, context);
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