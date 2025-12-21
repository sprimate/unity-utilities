using UnityEngine;
using HitTrax.GlobalMessagingService;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using HitTrax.CoreUtilities;
using System;

namespace HitTrax.UnityUtilities
{
    public class GameObjectSwitchOnMessage : SerializedMonoBehaviour
    {
        [Serializable]
        public class MessageGoPair
        {
            public string message;
            public GameObject gameObject;
        }

        static private IMessageService_v1 _messageService;

        [SerializeReference] private SerializableList<MessageGoPair> _objects;

        Dictionary<string, Action<object>> _events = new();

        private void Start()
        {
            // If message service is null, it likely means that OnEnable was not able to get it since it has not yet been registers
            // while that probably should not happen, logic is to handle that case

            if(_messageService == null)
            {
                _messageService = Services.Get<IMessageService_v1>();
                Register(_objects);
            }
        }

        private void OnEnable()
        {
            _messageService = Services.Get<IMessageService_v1>();
            Register(_objects);
        }

        private void OnDisable()
        {
            Unregister(_objects);
        }

        private void Register(IEnumerable<MessageGoPair> objs)
        {
            if (_messageService == null)
            {
                // Where should we keep a gorup of commonly used errors like this.
                //CoreUtilities.Logger.LogError("Service not found error. Message service not found while attempting to add a listener. Exiting.");
                return;
            }

            foreach (var obj in objs)
            {
                _events.Add(obj.message, _ => HandleEvent(obj.message));
                _messageService.AddListener(obj.message, _events[obj.message]);
            }
        }

        private void Unregister(IEnumerable<MessageGoPair> objs)
        {
            if (_messageService == null)
            {
                // Where should we keep a gorup of commonly used errors like this.
                CoreUtilities.Logger.LogError("Service not found error. Message service not found while attempting to add a listener. Exiting.");
                return;
            }

            foreach (var obj in objs)
            {
                _messageService.RemoveListener(obj.message, _events[obj.message]);
            }

            _events.Clear();
        }

        private void HandleEvent(string messageName)
        {
            // Cache for objects that should be activated or deactivated
            List<GameObject> toDeactivate = new List<GameObject>();
            List<GameObject> toActivate = new List<GameObject>();

            // Cache objects that should be activated, deactivate others
            foreach (var pair in _objects)
            {
                if(pair.message == messageName)
                {
                    toActivate.Add(pair.gameObject);
                }
                else
                {
                    toDeactivate.Add(pair.gameObject);
                }
            }

            // Deactivate objects but only if there is at least one object to activate
            if (toActivate.Count > 0)
            {
                foreach(var obj in toDeactivate)
                {
                    obj.SetActive(false);
                }
            }

            // Ensure objects that should be active are in fact activated

            // The reason to activate the objects on a second pass is to prevent an object from being deactivated,
            // when it should be active because it is assigned to multiple messages

            foreach(var obj in toActivate)
            {
                obj.gameObject.SetActive(true);
            }
        }
    }
}