using UnityEngine;

// DO NOT CHANGE THE NAMESPACE OF THIS CLASS OR YOUR MODULE WILL BREAK 
namespace HitTrax.CoreUtilities
{
    // DO NOT CHANGE THE NAME OF THIS CLASS OR YOUR MODULE WILL BREAK
    public static class CoreUtilitiesLoader
    {
        // DO NOT CHANGE THE NAME OF THIS FUNCTION OR YOUR MODULE WILL BREAK
        [RuntimeInitializeOnLoadMethod]
        public static void Load()
        {
            Logger.Init();
            GlobalTime.Init();
        }
    }
}