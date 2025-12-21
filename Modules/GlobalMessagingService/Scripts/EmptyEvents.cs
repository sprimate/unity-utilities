using HitTrax.CoreUtilities;

namespace HitTrax.GlobalMessagingService
{
    // Empty events can be used as helpers for when you have a type that accepts an event but you want to leave "empty"
    // This is used specifically by the Rule type
    public class EmptyMsg : IMessageKey<Nothing> { }
    public class EmptyMsg<T> : IMessageKey<T> { }
}
