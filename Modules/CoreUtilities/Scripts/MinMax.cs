using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HitTrax.CoreUtilities
{
    [Serializable]
    public class MinMax
    {
        [HorizontalGroup]
        public float min;
        [HorizontalGroup]
        public float max;

        public float GetRandom() => UnityEngine.Random.Range(min, max);
    }
}
