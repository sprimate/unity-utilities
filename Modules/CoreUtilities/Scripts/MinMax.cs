using System;
using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace HitTrax.CoreUtilities
{
    [Serializable]
    public class MinMax
    {
#if ODIN_INSPECTOR
        [HorizontalGroup]
#endif
        public float min;
#if ODIN_INSPECTOR
        [HorizontalGroup]
#endif
        public float max;

        public float GetRandom() => UnityEngine.Random.Range(min, max);
    }
}
