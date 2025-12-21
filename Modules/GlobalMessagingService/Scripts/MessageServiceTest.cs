using UnityEngine;
using System.Collections;
using HitTrax.CoreUtilities;

namespace HitTrax.GlobalMessagingService.Tests
{
    public struct PitchData
    {
        public int speed;
        public int angle;
    }

    public class MessageA : IMessageKey { }

    public class MessageB : IMessageKey<int> { }

    public class MessageC : IMessageKey<PitchData> { }

    public class MessageServiceTest : MonoBehaviour
    {
        private IMessageService_v1 _messaging = Services.Get<IMessageService_v1>();

        private void Start()
        {
            Debug.Log("Add Listeners");

            //_messaging.AddListener<MessageA>(OnMessageA);
            //_messaging.AddListener<MessageB, int>(OnMessageB);
            //_messaging.AddListener<MessageC, PitchData>(OnMessageC);
            //_messaging.AddListener("MessageC", OnAnnonymousMessage);
            //_messaging.AddListener("MessageC", OnAnnonymousMessageEmpty);

            _messaging.AddListener<MessageC, PitchData>(OnMessageC, preventDuplicate: false);
            _messaging.AddListener<MessageC, PitchData>(OnMessageC, preventDuplicate: false);
            _messaging.AddListener<MessageC, PitchData>(OnMessageC, preventDuplicate: false);
            _messaging.AddListener<MessageC, PitchData>(OnMessageC, preventDuplicate: false);

            StartCoroutine(DelayedRaise());
        }

        private IEnumerator DelayedRaise()
        {
            Debug.Log("Wait");
            yield return new WaitForSeconds(2);
            Debug.Log("Riase");
            _messaging.Raise<MessageA>();
            _messaging.Raise<MessageB, int>(5);
            _messaging.Raise<MessageC, PitchData>(new PitchData { angle = 3, speed = 5 });
        }

        private void OnMessageA()
        {
            Debug.Log("MESSAGE A");
        }

        private void OnMessageB(int val)
        {
            Debug.Log($"MESSAGE B {val}");
        }

        private void OnMessageC(PitchData data)
        {
            Debug.Log($"MESSAGE C - Speed: {data.speed}");
        }

        private void OnAnnonymousMessage(object obj)
        {
            PitchData data = (PitchData)(obj);
            Debug.Log($"Annonymous MESSAGE C - Speed: {data.speed}");
        }

        private void OnAnnonymousMessageEmpty()
        {
            Debug.Log("Annonymous MESSAGE Empty");
        }
    }
}
