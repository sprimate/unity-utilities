// DO NOT CHANGE THE NAMESPACE OF THIS CLASS OR YOUR MODULE WILL BREAK 
using System.IO;
using System.Text;
using System;
using UnityEngine;
using HitTrax.CoreUtilities;

namespace HitTrax.UnityUtilities
{
    // DO NOT CHANGE THE NAME OF THIS CLASS OR YOUR MODULE WILL BREAK
    public static class UnityUtilitiesLoader
    {
        // DO NOT CHANGE THE NAME OF THIS FUNCTION OR YOUR MODULE WILL BREAK
        [RuntimeInitializeOnLoadMethod]
        public static void Load()
        {
            Console.SetOut(new ConsoleToDebug());
            Services.RegisterSingleton(new TimerManager());
        }
    }
}