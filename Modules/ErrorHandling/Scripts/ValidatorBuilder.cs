//using System;
//using System.Collections.Generic;
//using HitTrax.CoreUtilities;
//using HitTrax.GlobalMessagingService;

//namespace HitTrax.ErrorHandling
//{
   
//    /// <summary>
//    /// This is the entry point for building a Validator
//    /// Besides "Create" there will not be any direct access to this builder
//    /// All other build functions will go through the generic interface
//    /// Pass in Log Info when creating a Validator that is specifically checking for "Error" when fail
//    /// If you are simply treating the validator as a true vs false check, you may decide to ignore the error log
//    /// </summary>

//    public interface IBuildValidService_v1 : IService
//    {

//        /// <summary>
//        /// Create a validator that allows the developer to define a GMS event for when it should be validated
//        /// </summary>
//        /// <returns>Returns a builder to allow the developer to continue to construct the validtor</returns>

//        public IValidateBuilderObject_v1<EventType, EventArgs> Create<EventType, EventArgs>() where EventType : IMessageKey<EventArgs>
//          => ValidateManager<EventType, EventArgs>.Create(addListener: true);

//        /// <summary>
//        /// Create a validator that can accept arguments upon validation
//        /// </summary>       
//        /// <returns>Returns a builder to allow the developer to continue to construct the validtor</returns>

//        public IValidateBuilderObject_v1<EmptyEvent<TArg>, TArg> Create<TArg>()
//            => ValidateManager<EmptyEvent<TArg>, TArg>.Create(addListener: false);

//        /// <summary>
//        /// Create a validator that is not intended to by used with a generic EventType or Argument type
//        /// </summary>
//        /// <returns>Returns a builder to allow the developer to continue to construct the validtor</returns>
//        public IValidateBuilderObject_v1<EmptyEvent, Nothing> Create()
//            => ValidateManager<EmptyEvent, Nothing>.Create(addListener: false);

//        /// <summary>
//        /// Create a validator that allows the developer to define a GMS event for when it should be validated
//        /// </summary>
//        /// <returns>Returns a builder to allow the developer to continue to construct the validtor</returns>

//        public IValidateBuilderObject_v1<EventType, EventArgs> Create<EventType, EventArgs>(LogInfo errorInfo) where EventType : IMessageKey<EventArgs>
//            => ValidateManager<EventType, EventArgs>.Create(errorInfo, addListener: true);

//        /// <summary>
//        /// Create a validator that can accept arguments upon validation
//        /// </summary>       
//        /// <returns>Returns a builder to allow the developer to continue to construct the validtor</returns>

//        public IValidateBuilderObject_v1<EmptyEvent<TArg>, TArg> Create<TArg>(LogInfo errorInfo)
//            => ValidateManager<EmptyEvent<TArg>, TArg>.Create(errorInfo, addListener: false);
        
//        /// <summary>
//        /// Create a validator that is not intended to by used with a generic EventType or Argument type
//        /// </summary>
//        /// <returns>Returns a builder to allow the developer to continue to construct the validtor</returns>
//        public IValidateBuilderObject_v1<EmptyEvent, Nothing> Create(LogInfo errorInfo)
//            => ValidateManager<EmptyEvent, Nothing>.Create(errorInfo, addListener: false);
//    }

//    internal class BuildValidator : IBuildValidService_v1 { }

//    /// <summary>
//    /// The validate manager manages state, adding and releasing validators generally called
//    /// by the BuildValidator and ReleaseValidatorManager
//    /// </summary>
//    /// <typeparam name="EventType"></typeparam>
//    /// <typeparam name="EventArgs"></typeparam>

//    internal static class ValidateManager<EventType, EventArgs> where EventType : IMessageKey<EventArgs>
//    {
//        /// <summary>
//        /// The collection of validators created
//        /// </summary>
//        /// 
//        private static HashSet<Validator<EventType, EventArgs>> _validators = new();

//        /// <summary>
//        /// Called by the builder to cache a newly created Validator that does not require an error log
//        /// </summary>
//        /// <param name="errorInfo"></param>
//        /// <param name="addListener"></param>
//        /// <returns></returns>
//        internal static IValidateBuilderObject_v1<EventType, EventArgs> Create(bool addListener)
//        {
//            var validator = new Validator<EventType, EventArgs>(addListener);
//            _validators.Add(validator);
//            return validator.AsBuilderV1();
//        }

//        /// <summary>
//        /// Called by the builder to cache a newly created Validator
//        /// </summary>
//        /// <param name="errorInfo"></param>
//        /// <returns></returns>

//        internal static IValidateBuilderObject_v1<EventType, EventArgs> Create(LogInfo errorInfo, bool addListener)
//        {
//            var validator = new Validator<EventType, EventArgs>(errorInfo, addListener);
//            _validators.Add(validator);
//            return validator.AsBuilderV1();
//        }

//        /// <summary>
//        /// Called exclusively by the ValidatorReleaseManager
//        /// Nulls out the validator and removes it from the cache
//        /// </summary>
//        /// <param name="validator"></param>

//        internal static void Release(Validator<EventType, EventArgs> validator)
//        {
//            validator.Clear();
//            _validators.Remove(validator);
//        }
//    }

//    /// <summary>
//    /// This interface layer is used to give the user access to "builder" functionality without direct access
//    /// to the Validate object itself
//    ///
//    /// There are functions that allow you to accept arguments or not accept arguments
//    /// For complete clarity, those are the arguments recived upon validation whether that's through GMS
//    /// or, plugged into a Validate function
//    /// </summary>
    
//    public interface IValidateBuilderObject_v1<EventType, EventArgs> where EventType : IMessageKey<EventArgs>
//    {
//        /// <summary>
//        /// When the developer is done building, they can return out the validator if they want to store it
//        /// This is also used by the builder
//        ///         

//        public Validator<EventType, EventArgs> Validator
//        {
//            get;
//        }

//        /// <summary>
//        /// Define the validator's predicate (with arguments)
//        /// </summary>
//        /// <param name="isValid"></param>
        
//        public IValidateBuilderObject_v1<EventType, EventArgs> SetPredicate(Func<EventArgs, bool> isValid)
//            => Validator.SetPredicate(isValid).AsBuilderV1();

//        /// <summary>
//        /// Define the validator's predicate (without arguments)
//        /// </summary>
//        /// <param name="isValid"></param>

//        public IValidateBuilderObject_v1<EventType, EventArgs> SetPredicate(Func<bool> isValid)
//            => Validator.SetPredicate(isValid).AsBuilderV1();

//        /// <summary>
//        /// Define success action with arguments.
//        /// Invoked upon successful validation
//        /// </summary>
//        /// <param name="onSuccess"></param>
//        /// <returns></returns>

//        public IValidateBuilderObject_v1<EventType, EventArgs> SetOnSuccess(Action<EventArgs> onSuccess)
//            => Validator.SetOnSuccess(onSuccess).AsBuilderV1();

//        /// <summary>
//        /// Define success action without arguments.
//        /// Invoked upon successful validation
//        /// </summary>
//        /// <param name="onSuccess"></param>
//        /// <returns></returns>
//        public IValidateBuilderObject_v1<EventType, EventArgs> SetOnSuccess(Action onSuccess)
//            => Validator.SetOnSuccess(onSuccess).AsBuilderV1();

//        /// <summary>
//        /// Define fail action with arguments (Event Arguments and Error Info (LogInfo)).
//        /// Invoked upon validation failure
//        /// </summary>
//        /// <param name="onFail"></param>
//        /// <returns></returns>
//        public IValidateBuilderObject_v1<EventType, EventArgs> SetOnFail(Action<EventArgs, LogInfo> onFail)
//            => Validator.SetOnFail(onFail).AsBuilderV1();

//        /// <summary>
//        /// Explicitly tell the validator to not log
//        /// </summary>        
//        public IValidateBuilderObject_v1<EventType, EventArgs> DontLog() => Validator.DontLog().AsBuilderV1();

//        /// <summary>
//        /// Define a function (with arguments) for whether or not the validator being constructed should log success & fail.
//        /// </summary>
//        /// <param name="requirement"></param>
        
//        public IValidateBuilderObject_v1<EventType, EventArgs> LogIf(Func<EventArgs, bool> requirement)
//            => Validator.SetLogAnyRequirement(requirement).AsBuilderV1();

//        /// <summary>
//        /// Define a function (without arguments) for whether or not the validator being constructed should log success & fail.
//        /// </summary>
//        /// <param name="requirement"></param>

//        public IValidateBuilderObject_v1<EventType, EventArgs> LogIf(Func<bool> requirement)
//            => Validator.SetLogAnyRequirement(_ => requirement()).AsBuilderV1();

//        /// <summary>
//        /// Define a function (with arguments) for whether or not the validator being constructed should log success.
//        /// </summary>
//        /// <param name="requirement"></param>

//        public IValidateBuilderObject_v1<EventType, EventArgs> LogSuccessIf(Func<EventArgs, bool> requirement)
//            => Validator.SetLogSuccessRequirement(requirement).AsBuilderV1();

//        /// <summary>
//        /// Define a function (without arguments) for whether or not the validator being constructed should log success.
//        /// </summary>
//        /// <param name="requirement"></param>

//        public IValidateBuilderObject_v1<EventType, EventArgs> LogSuccessIf(Func<bool> onLogRequestReq)
//            => Validator.SetLogSuccessRequirement(_ => onLogRequestReq()).AsBuilderV1();

//        /// <summary>
//        /// Define a function (with arguments) for whether or not the validator being constructed should log failure.
//        /// </summary>
//        /// <param name="requirement"></param>

//        public IValidateBuilderObject_v1<EventType, EventArgs> LogFailIf(Func<EventArgs, bool> requirement)
//            => Validator.SetLogFailRequirement(requirement).AsBuilderV1();

//        /// <summary>
//        /// Define a function (without arguments) for whether or not the validator being constructed should log failure.
//        /// </summary>
//        /// <param name="requirement"></param>

//        public IValidateBuilderObject_v1<EventType, EventArgs> LogFailIf(Func<bool> requirement)
//           => Validator.SetLogFailRequirement(_ => requirement()).AsBuilderV1();

//        /// <summary>
//        /// Define a success message with arguments
//        /// </summary>
       

//        public IValidateBuilderObject_v1<EventType, EventArgs> SetSuccessLog(Func<EventArgs, string> message)
//            => Validator.SetSuccessLog(message).AsBuilderV1();

//        /// <summary>
//        /// Define a success message without arguments
//        /// </summary>

//        public IValidateBuilderObject_v1<EventType, EventArgs> SetSuccessLog(Func<string> message)
//            => Validator.SetSuccessLog(_ => message()).AsBuilderV1();

//        /// <summary>
//        /// Tell the validator to log all errors as "Info"
//        /// </summary>

//        public IValidateBuilderObject_v1<EventType, EventArgs> LogFailAsInfo()
//            => Validator.LogFailAsInfo().AsBuilderV1();

//        /// <summary>
//        /// Tell the validator to log all failed validations as "Info"
//        /// Define additional runtime info but don't accept arguments
//        /// </summary>
//        /// <param name="runtimeInfo"></param>
//        /// <returns></returns>

//        public IValidateBuilderObject_v1<EventType, EventArgs> LogFailAsInfo(Func<string> runtimeInfo)
//            => Validator.LogFailAsInfo(_ => runtimeInfo()).AsBuilderV1();

//        /// <summary>
//        /// Tell the validator to log all failed validations as "Info"
//        /// Define additional runtime info and accept arguments
//        /// </summary>
//        /// <param name="runtimeInfo"></param>
//        /// <returns></returns>

//        public IValidateBuilderObject_v1<EventType, EventArgs> LogFailAsInfo(Func<EventArgs, string> runtimeInfo)
//            => Validator.LogFailAsInfo(runtimeInfo).AsBuilderV1();

//        /// <summary>
//        /// Tell the validator to log all failed validations as "Warnings"
//        /// </summary>
//        public IValidateBuilderObject_v1<EventType, EventArgs> LogAsWarning()
//            => Validator.LogAsWarning().AsBuilderV1();

//        /// <summary>
//        /// Tell the validator to log all failed validations as "Warnings"
//        /// Define additional runtime info but don't accept arguments
//        /// </summary>
//        /// <param name="runtimeInfo"></param>
//        /// <returns></returns>
//        public IValidateBuilderObject_v1<EventType, EventArgs> LogAsWarning(Func<string> runtimeInfo)
//            => Validator.LogAsWarning(_ => runtimeInfo()).AsBuilderV1();

//        /// <summary>
//        /// Tell the validator to log all failed validations as "Warning"
//        /// Define additional runtime info and accept arguments
//        /// </summary>
//        /// <param name="runtimeInfo"></param>
//        /// <returns></returns>

//        public IValidateBuilderObject_v1<EventType, EventArgs> LogAsWarning(Func<EventArgs, string> runtimeInfo)
//            => Validator.LogAsWarning(runtimeInfo).AsBuilderV1();

//        /// <summary>
//        /// Tell the validator to log all failed validations as "Errors"
//        /// </summary>

//        public IValidateBuilderObject_v1<EventType, EventArgs> LogAsError()
//            => Validator.LogAsError().AsBuilderV1();

//        /// <summary>
//        /// Tell the validator to log all failed validations as "Errors"
//        /// Define additional runtime info but don't accept arguments
//        /// </summary>
//        /// <param name="runtimeInfo"></param>
//        /// <returns></returns>

//        public IValidateBuilderObject_v1<EventType, EventArgs> LogAsError(Func<string> runtimeInfo)
//            => Validator.LogAsError(_ => runtimeInfo()).AsBuilderV1();

//        /// <summary>
//        /// Tell the validator to log all failed validations as "Errors"
//        /// Define additional runtime info and accept arguments
//        /// </summary>
//        /// <param name="runtimeInfo"></param>
//        /// <returns></returns>
//        public IValidateBuilderObject_v1<EventType, EventArgs> LogAsError(Func<EventArgs, string> runtimeInfo)
//            => Validator.LogAsError(runtimeInfo).AsBuilderV1();

//        /// <summary>
//        /// Tell the validator to only perform its validation log once.
//        /// The validator will still validate but will stop logging
//        /// </summary>
        
//        public IValidateBuilderObject_v1<EventType, EventArgs> LogOnce()
//            => Validator.LogOnce().AsBuilderV1();

//        /// <summary>
//        /// Tell the validator to only validate one time when its assocaited event is raised
//        /// It does not prevent validation explicitly called by the developer
//        /// Therefore, the object is not automatically released after validation
//        /// </summary>
        
//        public IValidateBuilderObject_v1<EventType, EventArgs> ValidateOnce()
//            => Validator.ValidateOnce().AsBuilderV1();

//        /// <summary>
//        /// Define a release event.
//        /// When the event is triggered:
//        /// 1. The associated validator clears out its data
//        /// 2. Unsubscribes from events
//        /// 3. Gets removed from the ValidtorManager Cahce
//        /// 4. Gets removed from the ValidatorReleaseManager cache
//        /// </summary>
        
//        public IValidateBuilderObject_v1<EventType, EventArgs> ReleaseEvent<ReleaseEvent, ReleaseEventArgs>() where ReleaseEvent : IMessageKey<ReleaseEventArgs>
//        {
//            ValidatorReleaseManager<ReleaseEvent, ReleaseEventArgs>.SubscribeToReleaseEvent(Validator);
//            return Validator.AsBuilderV1();
//        }

//        /// <summary>
//        /// Define an argumentless release event but accept a requirement (req).
//        /// If the event is raised and the required predicate passes, then release the validator
//        /// </summary>
//        /// <typeparam name="ReleaseEvent"></typeparam>
//        /// <typeparam name="ReleaseEventArgs"></typeparam>
//        /// <param name="req"></param>
//        /// <returns></returns>

//        public IValidateBuilderObject_v1<EventType, EventArgs> ReleaseEvent<ReleaseEvent, ReleaseEventArgs>(Func<bool> req) where ReleaseEvent : IMessageKey<ReleaseEventArgs>
//        {
//            ValidatorReleaseManager<ReleaseEvent, ReleaseEventArgs>.SubscribeToReleaseEvent(Validator, _ => req());
//            return Validator.AsBuilderV1();
//        }

//        /// <summary>
//        /// Define a release event with arguments but accept a requirement (req).
//        /// If the event is raised and the required predicate passes, then release the validator
//        /// </summary>
//        /// <typeparam name="ReleaseEvent"></typeparam>
//        /// <typeparam name="ReleaseEventArgs"></typeparam>
//        /// <param name="req"></param>
//        /// <returns></returns>

//        public IValidateBuilderObject_v1<EventType, EventArgs> ReleaseEvent<ReleaseEvent, ReleaseEventArgs>(Func<ReleaseEventArgs, bool> req) where ReleaseEvent : IMessageKey<ReleaseEventArgs>
//        {
//            ValidatorReleaseManager<ReleaseEvent, ReleaseEventArgs>.SubscribeToReleaseEvent(Validator, req);
//            return Validator.AsBuilderV1();
//        }
//    }

//    /// <summary>
//    /// This is required to return the builder interface from the Validator as needed
//    /// </summary>
//    /// <typeparam name="EventType"></typeparam>
//    /// <typeparam name="EventArgs"></typeparam>
//    public struct ValidateBuilder<EventType, EventArgs> : IValidateBuilderObject_v1<EventType, EventArgs> where EventType : IMessageKey<EventArgs>
//    {
//        /// <summary>
//        /// The validator that the builder will be affecting and that the developer can extract after building
//        /// </summary>
//        public Validator<EventType, EventArgs> Validator { get; private set; }

//        /// <summary>
//        /// Construct the builder and define the new validator that the builder initially constructs
//        /// </summary>
//        /// <param name="validator"></param>

//        internal ValidateBuilder(Validator<EventType, EventArgs> validator)
//        {
//            Validator = validator;
//        }

//    }

//}
