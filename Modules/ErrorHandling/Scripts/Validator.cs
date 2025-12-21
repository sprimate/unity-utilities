//using HitTrax.CoreUtilities;
//using HitTrax.GlobalMessagingService;
//using System;

//namespace HitTrax.ErrorHandling
//{
//    /// <summary>
//    /// When the developer builds a Validator with LogAsWarning, LogAsError, etc
//    /// The instance of the validator will store that information with this
//    /// </summary>
//    internal enum LogType
//    {
//        None, Info, Warning, Error
//    }

//    /// <summary>
//    /// For storing the validator, this will give the developer argumentless Release and Validate methods
//    /// </summary>

//    public interface IValid_v1
//    {
//        /// <summary>
//        /// Remove the validator from the manager
//        /// </summary>
//        void Release();

//        /// <summary>
//        /// Run validation based how it was defined by the builder. Run the validation and return the result (true,false) and the validator.
//        /// </summary>
        
//        (bool result, IValid_v1 validator) Validate();
//    }

//    /// <summary>
//    /// For storing the validator, this will give the developer a validator that accepts an argument
//    /// </summary>
//    /// <typeparam name="EventArgs"></typeparam>

//    public interface IValid_v1<EventArgs> 
//    {
//        /// <summary>
//        /// Run validation based how it was defined by the builder. Run the validation and return the result (true,false) and the validator.
//        /// </summary>
        

//        (bool result, IValid_v1<EventArgs> validator) Validate(EventArgs args);

//        /// <summary>
//        /// Run the validator and define success and fail results
//        /// onFail recieves both the arguments and the error (LogInfo)
//        /// </summary>
       

//        (bool result, IValid_v1<EventArgs> validator) Validate(Action<EventArgs> onSuccess, Action<EventArgs, LogInfo> onFail, EventArgs args);
//    }

//    /// <summary>
//    /// This is for getting the validator and defining both the EventType and EventArgs
//    /// It's here just in case, but at the moment it doesn't appear to have a clear use case
//    /// </summary>
//    /// <typeparam name="EventType"></typeparam>
//    /// <typeparam name="EventArgs"></typeparam>

//    public interface IValid_v1<EventType, EventArgs> : IValid_v1, IValid_v1<EventArgs> where EventType : IMessageKey<EventArgs>
//    {
//        //(bool result, IValidator_v1<EventType, EventArgs> validator) Validate(Action<EventArgs> onSuccess, Action<EventArgs, LogInfo> onFail, EventArgs args);
//    }


//    /// <summary>
//    /// The validator is the main data type that gets constructed and stores the data about the validation
//    /// Constructors and most methods are internal as they are handled by the "builder"
//    /// Thus, preventing the developer from mutating defined validator
//    /// Though the "summaries" refer to what the developer can do with them, in reality they are almost all wrapped by the builder function
//    /// The few "public" members such as Validate and Release are handled by the interfaces
//    /// </summary>
//    /// <typeparam name="EventType"></typeparam>
//    /// <typeparam name="EventArgs"></typeparam>

//    public class Validator<EventType, EventArgs> : IValid_v1<EventType, EventArgs> where EventType : IMessageKey<EventArgs>  
//    {
//        private IMessageService_v1 _messageService = Services.Get<IMessageService_v1>();
//        private LogInfo _errorInfo;
//        private Action<EventArgs> _onSuccess;
//        private Action<EventArgs, LogInfo> _onFail;
//        private Func<EventArgs, bool> _isValidWithArgs;
//        private Func<EventArgs, string> _runtimeErrorInfo;
//        private Func<EventArgs, string> _successLogString;
//        private Func<EventArgs, LogInfo> _successLogLogInfo;
//        private Func<EventArgs, bool> _logSuccessReq;
//        private Func<EventArgs, bool> _logFailReq;

//        private LogType _logType = LogType.Error;
//        private bool _logOnce = false;
//        private bool _validateOnce = false;

//        // Prevent other modules from trying to construct a validator without going through the "builder"
//        internal Validator()
//        {
//        }

//        /// <summary>
//        /// Construct a validator and define its error
//        /// </summary>
//        /// <param name="errorInfo"></param>

//        internal Validator(LogInfo errorInfo)
//        {
//            _errorInfo = errorInfo;            
//        }

//        /// <summary>
//        /// Remove the validator from the managers
//        /// </summary>

//        public void Release()
//        {
//            ValidatorReleaseManager<EventType, EventArgs>.Release(this);
//        }


//        /// <summary>
//        /// Run validate without arguments
//        /// If success and fail functions are defined by the builder they will be invoked
//        /// </summary>
//        /// <returns>Tuple with a result and the validator</returns>

//        public (bool, IValid_v1) Validate() => ValidatePrivate(_onSuccess, _onFail, default);

//        /// <summary>
//        /// Run validate with arguments
//        /// If success and fail functions are defined by the builder they will be invoked
//        /// </summary>
//        /// <returns>Tuple with a result and the validator</returns>

//        public (bool, IValid_v1<EventArgs>) Validate(EventArgs args) => ValidatePrivate(_onSuccess, _onFail, args);

//        /// <summary>
//        /// Run validate without and allows the developer to define success and fail functions
//        /// This will override success and fail functions that may have been defined by the builder
//        /// </summary>
//        /// <returns>Tuple with a result and the validator</returns>

//        public (bool,IValid_v1<EventArgs>) Validate(Action<EventArgs> onSuccess, Action<EventArgs, LogInfo> onFail, EventArgs args)
//        {            
//            if (_validateOnce)
//            {
//                // Since we're only validating once, and we've now validated,
//                // Remove the associated event listener
//                _messageService.RemoveListener<EventType, EventArgs>(OnEventTriggered);
//            }
//            return ValidatePrivate(onSuccess, onFail, args);
//        }

//        /// <summary>
//        /// Private helper method for validating and directing to either success or fail
//        /// </summary>      

//        private (bool, Validator<EventType, EventArgs>) ValidatePrivate(Action<EventArgs> onSuccess, Action<EventArgs, LogInfo> onFail, EventArgs args)
//         => IsValid(args) ? DoSuccess(onSuccess, args) : DoFail(onFail, args);


//        /// <summary>
//        /// A simple helper to deremine if we're valid
//        /// If there is no defined validation predicate, then we are valid
//        /// otherwise, run the function and return the result
//        /// </summary>
        
//        private bool IsValid(EventArgs args) => _isValidWithArgs == null || _isValidWithArgs(args);

//        /// <summary>
//        /// Constructor with no error log
//        /// </summary>        

//        internal Validator(bool addListener)
//        {
//            if (addListener)
//            {
//                _messageService.AddListener<EventType, EventArgs>(OnEventTriggered);
//            }
//        }


//        /// <summary>
//        /// Construct a validator with an error info and decide whether or not this should listen for an event
//        /// </summary>        

//        internal Validator(LogInfo errorInfo, bool addListener)
//        {            
//            _errorInfo = errorInfo;
//            if (addListener)
//            {
//                _messageService.AddListener<EventType, EventArgs>(OnEventTriggered);
//            }            
//        }

//        /// <summary>
//        /// This cleanup is called when the object has been "released"
//        /// </summary>

//        internal void Clear()
//        {
//            _onSuccess = null;
//            _onFail = null;
//            _isValidWithArgs = null;
//            _runtimeErrorInfo = null;
//            _successLogString = null;
//            _successLogLogInfo = null;
//            _logSuccessReq = null;
//            _logFailReq = null;
//            _messageService.RemoveListener<EventType, EventArgs>(OnEventTriggered);
//        }

//        /// <summary>
//        /// Predefine the predicate used for determining whether or not we passed validation with arguments
//        /// </summary>
        
    
//        internal Validator<EventType, EventArgs> SetPredicate(Func<EventArgs, bool> isValid)
//        {
//            _isValidWithArgs = isValid;
//            return this;
//        }

//        /// <summary>
//        /// Predefine the predicate used for determining whether or not we passed validation without arguments
//        /// </summary>
        
//        internal Validator<EventType, EventArgs> SetPredicate(Func<bool> isValid)
//        {
//            _isValidWithArgs = a => isValid();
//            return this;
//        }

//        /// <summary>
//        /// Perform this action each time validation is checked and passed, with arguments
//        /// </summary>

//        internal Validator<EventType, EventArgs> SetOnSuccess(Action<EventArgs> onSuccess)
//        {
//            _onSuccess = onSuccess;
//            return this;
//        }

//        /// <summary>
//        /// Perform this action each time validation is checked and passed, without arguments
//        /// </summary>

//        internal Validator<EventType, EventArgs> SetOnSuccess(Action onSuccess)
//        {
//            _onSuccess = arg => onSuccess();
//            return this;
//        }

//        /// <summary>
//        /// Perform this action each time validation is checked and failed, with arguments
//        /// Receive the arguments and the error log
//        /// </summary>

//        internal Validator<EventType, EventArgs> SetOnFail(Action<EventArgs, LogInfo> onFail)
//        {
//            _onFail = onFail;
//            return this;
//        }

//        /// <summary>
//        /// Perform this action each time validation is checked and failed, without arguments
//        /// Receive the arguments and the error log
//        /// </summary>

//        internal Validator<EventType, EventArgs> SetOnFail(Action<LogInfo> onFail)
//        {
//            _onFail = (arg, errorInfo) => onFail(errorInfo); 
//            return this;
//        }

        
//        /// <summary>
//        /// Set a log upon successful validation, with arguments
//        /// </summary>

//        internal Validator<EventType, EventArgs> SetSuccessLog(Func<EventArgs, string> message)
//        {
//            _successLogString = message;
//            return this;
//        }

//        /// <summary>
//        /// Set a log upon successful validation, with arguments
//        /// </summary>

//        internal Validator<EventType, EventArgs> SetSuccessLog(Func<EventArgs, LogInfo> message)
//        {
//            _successLogLogInfo = message;
//            return this;
//        }

//        /// <summary>
//        /// Sets a predicate for determining whether or not you want any logs active for this validator
//        /// </summary>

//        internal Validator<EventType, EventArgs> SetLogAnyRequirement(Func<EventArgs, bool> req)
//        {
//            SetLogSuccessRequirement(req);
//            SetLogFailRequirement(req);
//            return this;
//        }

//        /// <summary>
//        /// Sets a predicate for determining whether or not you want success logs active for this validator
//        /// </summary>

//        internal Validator<EventType, EventArgs> SetLogSuccessRequirement(Func<EventArgs, bool> req)
//        {
//            _logSuccessReq = req;
//            return this;
//        }

//        /// <summary>
//        /// Sets a predicate for determining whether or not you want error logs active for this validator
//        /// </summary>

//        internal Validator<EventType, EventArgs> SetLogFailRequirement(Func<EventArgs, bool> req)
//        {
//            _logFailReq = req;
//            return this;
//        }

//        /// <summary>
//        /// Explicitly tell the validator not to log
//        /// </summary>        

//        internal Validator<EventType, EventArgs> DontLog()
//        {
//            _logType = LogType.None;
//            return this;
//        }

//        /// <summary>
//        /// Tell the validator to only log the first time it validates
//        /// </summary>

//        internal Validator<EventType, EventArgs> LogOnce()
//        {
//            _logOnce = true;
//            return this;
//        }

//        /// <summary>
//        /// Tell the validator to only validate once when an associated event is raised
//        /// </summary>
        
//        internal Validator<EventType, EventArgs> ValidateOnce()
//        {
//            _validateOnce = true;
//            return this;
//        }

//        /// <summary>
//        /// Tell the validator to log every error as Info
//        /// </summary>
//        /// <returns></returns>

//        internal Validator<EventType, EventArgs> LogFailAsInfo()
//        {
//            _logType = LogType.Info;
//            return this;
//        }

//        /// <summary>
//        /// Tell the validator to log every error as Info.
//        /// Allows the developer to defer additional information to be logged at runtime.
//        /// </summary>
        
//        internal Validator<EventType, EventArgs> LogFailAsInfo(Func<EventArgs, string> runtimeInfo)
//        {            
//            _runtimeErrorInfo = runtimeInfo;
//            return LogFailAsInfo();
//        }

//        /// <summary>
//        /// Tell the validator to log every error as a Warning
//        /// </summary>
//        /// <returns></returns>

//        internal Validator<EventType, EventArgs> LogAsWarning()
//        {
//            _logType = LogType.Warning;
//            return this;
//        }

//        /// <summary>
//        /// Tell the validator to log every error as a Warning.
//        /// Allows the developer to defer additional information to be logged at runtime.
//        /// </summary>

//        internal Validator<EventType, EventArgs> LogAsWarning(Func<EventArgs, string> runtimeInfo)
//        {
//            _runtimeErrorInfo = runtimeInfo;
//            return LogAsWarning();
//        }

//        /// <summary>
//        /// Log an error as an error explicilty.
//        /// Though this is default, this method is called by the builder to ensure the behavior.
//        /// </summary>
//        /// <returns></returns>

//        internal Validator<EventType, EventArgs> LogAsError()
//        {
//            _logType = LogType.Error;
//            return this;
//        }

//        /// <summary>
//        /// Tell the validator to log every error as an Error.
//        /// Allows the developer to defer additional information to be logged at runtime.
//        /// </summary>

//        internal Validator<EventType, EventArgs> LogAsError(Func<EventArgs, string> additionalInfo)
//        {
//            _runtimeErrorInfo = additionalInfo;
//            return LogAsError();
//        }

//        /// <summary>
//        /// For Validators that have a defined Event upon their creation, handle the event when it's triggered
//        /// </summary>
       
//        private void OnEventTriggered(EventArgs args)
//        {
//            Validate(_onSuccess, _onFail, args);
//        }

//        /// <summary>
//        /// Handles all success related work such as invoking the action and logging
//        /// </summary>       
//        /// <returns>Returns the result true (for success) and this validator</returns>

//        private (bool,Validator<EventType, EventArgs>) DoSuccess(Action<EventArgs> onSuccess, EventArgs args)
//        {
//            onSuccess?.Invoke(args);
//            Debugger.Log(GetSuccessInfo(args), req: () => CanLogSuccess(args));
//            return (true,this);
//        }

//        /// <summary>
//        /// Determines whether or not error logging requirements are met
//        /// </summary>
       
//        private bool MeetsErrorLogRequirements(EventArgs args) => _logFailReq == null || _logFailReq.Invoke(args);

//        /// <summary>
//        /// Determines whether or not success logging requirements are met
//        /// </summary>

//        private bool MeetsSuccessLogRequirements(EventArgs args) => _logSuccessReq == null || _logSuccessReq(args);

//        /// <summary>
//        /// Helper determining whether or not we should log success by checkin if the success log if null
//        /// </summary>
      
//        private bool CanLogSuccess(EventArgs args) =>
//            (_successLogString != null || _successLogLogInfo != null)
//            && MeetsSuccessLogRequirements(args);
        

//        /// <summary>
//        /// Handles everything related to failure
//        /// </summary>      
//        /// <returns>Returns the result false (for failure) and this Validator</returns>

//        private (bool, Validator<EventType, EventArgs>) DoFail(Action<EventArgs, LogInfo> onFail, EventArgs args)
//        {
//            // Create a new struct and attach additional info
//            var error = _errorInfo.SetAdditionalInfo(GetRuntimeErrorInfo(args));

//            // Log and raise errors as needed
//            DoErrorLog(error.ToString(), req: () => MeetsErrorLogRequirements(args));

//            // When the logger raises an error it raises a generic ErrorEventType that stores this error info
//            LoggerInternal.RaiseError(error);

//            // Stop logging from now on if log once is true
//            if (_logOnce)
//            {
//                _logType = LogType.None;
//            }

//            // Invoke the onFail function if its defined
//            onFail?.Invoke(args, _errorInfo);

//            // return
//            return (false, this);
//        }

//        /// <summary>
//        /// If additional runtime info is defined then grab it, otherwise return an empty string
//        /// </summary>
        
//        private string GetRuntimeErrorInfo(EventArgs args) => _runtimeErrorInfo == null ? "" : _runtimeErrorInfo(args);

//        /// <summary>
//        /// If additional success info is defined then grab it, otherwise return an empty string
//        /// </summary>
        
//        private string GetSuccessInfo(EventArgs args)
//        {
//            if(_successLogLogInfo != null)
//            {
//                return _successLogLogInfo(args).ToString();
//            }
//            else if(_successLogLogInfo != null)
//            {
//                return _successLogString(args);
//            }

//            return "";
//        }


//        /// <summary>
//        /// Determine how to log the error and then log it.
//        /// Ignore it if its type None
//        /// </summary>
        
//        private Validator<EventType, EventArgs> DoErrorLog(string message, Func<bool> req)
//            => _logType == LogType.Info ? this.Log(message, req) :
//               _logType == LogType.Warning ? this.LogWarning(message, req) :
//               _logType == LogType.Error ? this.LogError(message, req) :
//                this;

//        /// <summary>
//        /// Used by the builder to expose builder functions to the developer
//        /// </summary>
        
//        internal IValidateBuilderObject_v1<EventType, EventArgs> AsBuilderV1() => new ValidateBuilder<EventType, EventArgs>(this);        
//    }
//}
