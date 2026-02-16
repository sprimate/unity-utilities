using HitTrax.CoreUtilities;
using HitTrax.GlobalMessagingService;
using System;

namespace HitTrax.UnityUtilities
{
    /// <summary>
    /// Provides a standard for types that will tie their own data to a message as its parameter.
    /// </summary>
    public abstract class ASignal<TSelf> : IMessageKey<TSelf>, ISignal where TSelf : ASignal<TSelf>
    {
        public virtual void Raise() => SendMessage();
        protected virtual void SendMessage()
        {
            Services.Get<IMessageService_v1>().Raise(this as TSelf);
        }    
    }

    public abstract class ASerializableSignal<TSelf> : ASignal<TSelf>, ISerializableSignal where TSelf : ASerializableSignal<TSelf>, new() { }

    public interface ISignal// : IMessageKey
    {
        void Raise();
    }

    //Allows for any ISerializableSignal to be accessible from the inspector without needing to supply the specific type in script from ASerializableSignal<SomeType>
    public interface ISerializableSignal : ISignal { }
}