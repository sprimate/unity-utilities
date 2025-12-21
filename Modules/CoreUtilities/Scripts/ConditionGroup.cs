using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace HitTrax.CoreUtilities
{
    public class ConditionGroup
    {
        private readonly List<Func<UniTask<bool>>> _validators = new();

        public void AddListener(Func<UniTask<bool>> validator)
        {
            _validators.Add(validator);
        }

        public void RemoveListener(Func<UniTask<bool>> validator)
            => _validators.Remove(validator);

        public void RemoveAllListeners()
            => _validators.Clear();

        public async UniTask<bool> EvaluateAsync()
        {
            foreach (var validator in _validators)
            {
                if (!await validator())
                {
                    return false;
                }
            }

            return true;
        }
    }
}