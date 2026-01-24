#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using UnityEngine;

namespace HitTrax.UnityUtilities
{
    public class SignalRaiser : MonoBehaviour
    {
        [SerializeReference]
        private ISerializableSignal _eventInstance;

#if ODIN_INSPECTOR
        [ShowIf("@UnityEngine.Application.isPlaying")]
        [Button]//raise the event directly from an inspector on click
#endif
        public void Raise()
        {
            if (_eventInstance != null)
            {
                _eventInstance.Raise();
            }
            else
            {
                Debug.LogError(nameof(_eventInstance) + " is unset");
            }
        }
    }
}