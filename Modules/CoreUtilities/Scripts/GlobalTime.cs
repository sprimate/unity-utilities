using Cysharp.Threading.Tasks;
using System.Diagnostics;
using UnityEngine;

namespace HitTrax.CoreUtilities
{
    public static class GlobalTime
    {
        private static Stopwatch _sw;
        private static Stopwatch Sw {
            get {
                if (_sw == null)
                {
                    Init();
                }

                return _sw;
            }
        }
        public static long Ms => Sw.ElapsedMilliseconds;
        public static double Secs => Sw.Elapsed.TotalSeconds;

        public static int Frame { get; private set; } = -1;
        public static void Init()
        {
            _sw = Stopwatch.StartNew();
            static async UniTask CacheFrameCountLoop()
            {
                while (Application.isPlaying)
                {
                    Frame = Time.frameCount;
                    await UniTask.Yield(PlayerLoopTiming.PreUpdate);
                }
            }

            CacheFrameCountLoop().Forget();
        }
    }
}