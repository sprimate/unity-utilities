// DO NOT CHANGE THE NAMESPACE OF THIS CLASS OR YOUR MODULE WILL BREAK 
using HitTrax.CoreUtilities;
using UnityEngine;

namespace HitTrax.GlobalMessagingService
{
    // DO NOT CHANGE THE NAME OF THIS CLASS OR YOUR MODULE WILL BREAK
    public static class GlobalMessagingServiceLoader
    {
        // DO NOT CHANGE THE NAME OF THIS FUNCTION OR YOUR MODULE WILL BREAK
        [RuntimeInitializeOnLoadMethod]
        public static void Load()
        {
            Services.RegisterSingleton(new MessageService());
        }
    }
}