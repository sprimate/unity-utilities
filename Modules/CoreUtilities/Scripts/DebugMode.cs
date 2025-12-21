using System;

namespace HitTrax.CoreUtilities
{
    public static class DebugMode
    {
        public static Action<bool> onDebugModeChanged;

        public static bool IsActive { get; private set; }

        public static void Set(bool toDebug)
        {
            if (IsActive != toDebug)
            {
                IsActive = toDebug;
                onDebugModeChanged?.Invoke(IsActive);
            }
        }
    }
}
