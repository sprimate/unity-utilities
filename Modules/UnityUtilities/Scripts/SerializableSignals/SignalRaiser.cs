using Sirenix.OdinInspector;
using UnityEngine;

namespace HitTrax.UnityUtilities
{
    public class SignalRaiser : MonoBehaviour
    {
        [SerializeReference]
        private ISerializableSignal _eventInstance;

        [ShowIf("@UnityEngine.Application.isPlaying")]
        [Button]//raise the event directly from an inspector on click
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