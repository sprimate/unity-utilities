using UnityEngine;
using HitTrax.CoreUtilities;
using HitTrax.GlobalMessagingService;

namespace HitTrax.ErrorHandling
{
    public class MsgBallCollidedTest : IMessageKey<GameObject> { }

    public class CollisionCheck : MonoBehaviour
    {
        IMessageService_v1 Messager => Services.Get<IMessageService_v1>();        

        private void OnTriggerEnter(Collider other)
        {
            Messager.Raise<MsgBallCollidedTest, GameObject>(other.gameObject);
        }
    }
}
