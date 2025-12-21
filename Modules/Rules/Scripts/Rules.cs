using System;
using HitTrax.CoreUtilities;
using HitTrax.ErrorHandling;
using HitTrax.GlobalMessagingService;

namespace HitTrax.Rules
{
    /// <summary>
    /// When the developer builds a Rule with LogFailAsWarning, LogFailAsError, etc
    /// The instance of the rule will store that information with this
    /// </summary>
    internal enum ErrorLogType
    {
        None, Info, Warning, Error
    }

    /// <summary>
    /// For storing the rule, this will give the developer argumentless Release and Validate methods
    /// </summary>

    public interface IRule_v1
    {
        /// <summary>
        /// Remove the rule from the manager
        /// </summary>
        void Release();

        /// <summary>
        /// Run based how it was defined by the builder. Run the rule and return the result (true,false) and the rule.
        /// </summary>

        (bool result, IRule_v1 rule) Run();
    }

    /// <summary>
    /// For storing the rule, this will give the developer a rule that accepts an argument
    /// </summary>
    /// <typeparam name="TMsgArgs"></typeparam>

    public interface IRule_v1<TMsgArgs>
    {
        /// <summary>
        /// Run based how it was defined by the builder. Run the rule and return the result (true,false) and the rule.
        /// </summary>


        (bool result, IRule_v1<TMsgArgs> rule) Run(TMsgArgs args);

        /// <summary>
        /// Run the rule and define success and fail results
        /// onFail recieves both the arguments and the error (LogInfo)
        /// </summary>


        (bool result, IRule_v1<TMsgArgs> rule) Run(Action<TMsgArgs> onSuccess, Action<TMsgArgs, LogInfo> onFail, TMsgArgs args);
    }

    /// <summary>
    /// This is for getting the rule and defining both the MsgType and MsgArgs
    /// It's here just in case, but at the moment it doesn't appear to have a clear use case
    /// </summary>
    /// <typeparam name="TMsgType"></typeparam>
    /// <typeparam name="TMsgArgs"></typeparam>

    public interface IRule_v1<TMsgType, TMsgArgs> : IRule_v1, IRule_v1<TMsgArgs> where TMsgType : IMessageKey<TMsgArgs>
    {
        //(bool result, IValidator_v1<TMsgType, TMsgArgs> rule) Run(Action<TMsgArgs> onSuccess, Action<TMsgArgs, LogInfo> onFail, TMsgArgs args);
    }


    /// <summary>
    /// The rule is the main data type that gets constructed and stores the data about the rule
    /// Constructors and most methods are internal as they are handled by the "builder"
    /// Thus, preventing the developer from mutating defined rule
    /// Though the "summaries" refer to what the developer can do with them, in reality they are almost all wrapped by the builder function
    /// The few "public" members such as Validate and Release are handled by the interfaces
    /// </summary>
    /// <typeparam name="TMsgType"></typeparam>
    /// <typeparam name="TMsgArgs"></typeparam>

    public class Rule<TMsgType, TMsgArgs> : IRule_v1<TMsgType, TMsgArgs> where TMsgType : IMessageKey<TMsgArgs>
    {
        private IMessageService_v1 _messageService = Services.Get<IMessageService_v1>();
        private LogInfo _errorInfo;
        private Action<TMsgArgs> _onSuccess;
        private Action<TMsgArgs, LogInfo> _onFail;
        private Action _onReleaseAction;
        private Func<TMsgArgs, bool> _isValid;
        private Func<TMsgArgs, string> _runtimeErrorInfo;
        private Func<TMsgArgs, string> _successLogString;
        private Func<TMsgArgs, LogInfo> _successLogLogInfo;
        private Func<TMsgArgs, bool> _logSuccessReq;
        private Func<TMsgArgs, bool> _logFailReq;
        private Safe<string> _logCategories;

        private ErrorLogType _errorLogType = ErrorLogType.None;
        private bool _logOnce = false;
        private bool _executeOne = false;

        // Prevent other modules from trying to construct a rule without going through the "builder"
        internal Rule()
        {
        }

        /// <summary>
        /// Construct a rule and an associated error
        /// </summary>
        /// <param name="errorInfo"></param>

        internal Rule(LogInfo errorInfo)
        {
            _errorInfo = errorInfo;
        }

        /// <summary>
        /// Remove the rule from the managers
        /// </summary>

        public void Release()
        {           
            _onReleaseAction?.Invoke();
            RuleReleaseManager<TMsgType, TMsgArgs>.Release(this);
        }


        /// <summary>
        /// Run rule without arguments
        /// If success and fail functions are defined by the builder they will be invoked
        /// </summary>
        /// <returns>Tuple with a result and the rule</returns>

        public (bool, IRule_v1) Run() => ValidateAndRun(_onSuccess, _onFail, default);

        /// <summary>
        /// Run rule with arguments
        /// If success and fail functions are defined by the builder they will be invoked
        /// </summary>
        /// <returns>Tuple with a result and the rule</returns>

        public (bool, IRule_v1<TMsgArgs>) Run(TMsgArgs args) => ValidateAndRun(_onSuccess, _onFail, args);

        /// <summary>
        /// Run rule without and allows the developer to define success and fail functions
        /// This will override success and fail functions that may have been defined by the builder
        /// </summary>
        /// <returns>Tuple with a result and the rule</returns>

        public (bool, IRule_v1<TMsgArgs>) Run(Action<TMsgArgs> onSuccess, Action<TMsgArgs, LogInfo> onFail, TMsgArgs args)
        {
            if (_executeOne)
            {
                // Since we're only validating once, and we've now validated,
                // Remove the associated event listener
                _messageService.RemoveListener<TMsgType, TMsgArgs>(OnMessageTriggered);
            }
            return ValidateAndRun(onSuccess, onFail, args);
        }

        /// <summary>
        /// Helper method for validating and then running either success or fail based on validation
        /// </summary>      

        private (bool, Rule<TMsgType, TMsgArgs>) ValidateAndRun(Action<TMsgArgs> onSuccess, Action<TMsgArgs, LogInfo> onFail, TMsgArgs args)
         => IsValid(args) ? DoSuccess(onSuccess, args) : DoFail(onFail, args);


        /// <summary>
        /// A simple helper to deremine if we're valid
        /// If there is no defined validation predicate, then we are valid
        /// otherwise, run the function and return the result
        /// </summary>

        private bool IsValid(TMsgArgs args) => _isValid == null || _isValid(args);

        /// <summary>
        /// Constructor with no error log
        /// </summary>        

        internal Rule(bool addListener)
        {
            if (addListener)
            {
                _messageService.AddListener<TMsgType, TMsgArgs>(OnMessageTriggered);
            }
        }


        /// <summary>
        /// Construct a rule with an error info and decide whether or not this should listen for an event
        /// </summary>        

        internal Rule(LogInfo errorInfo, bool addListener)
        {
            _errorInfo = errorInfo;
            if (addListener)
            {
                _messageService.AddListener<TMsgType, TMsgArgs>(OnMessageTriggered);
            }
        }

        /// <summary>
        /// This cleanup is called when the object has been "released"
        /// </summary>

        internal void Clear()
        {
            _onSuccess = null;
            _onFail = null;
            _isValid = null;
            _runtimeErrorInfo = null;
            _successLogString = null;
            _successLogLogInfo = null;
            _logSuccessReq = null;
            _logFailReq = null;
            _onReleaseAction = null;
            _messageService.RemoveListener<TMsgType, TMsgArgs>(OnMessageTriggered);
        }

        /// <summary>
        /// Predefine the predicate used for determining whether or not we passed validation with arguments
        /// </summary>


        internal Rule<TMsgType, TMsgArgs> SetPredicate(Func<TMsgArgs, bool> isValid)
        {
            _isValid = isValid;
            return this;
        }

        /// <summary>
        /// Predefine the predicate used for determining whether or not we passed validation without arguments
        /// </summary>

        internal Rule<TMsgType, TMsgArgs> SetPredicate(Func<bool> isValid)
        {
            _isValid = a => isValid();
            return this;
        }

        /// <summary>
        /// Perform this action each time validation is checked and passed, with arguments
        /// </summary>

        internal Rule<TMsgType, TMsgArgs> SetOnSuccess(Action<TMsgArgs> onSuccess)
        {
            _onSuccess = onSuccess;
            return this;
        }

        /// <summary>
        /// Perform this action each time validation is checked and passed, with arguments
        /// </summary>

        internal Rule<TMsgType, TMsgArgs> AddOnSuccess(Action<TMsgArgs> onSuccess)
        {
            _onSuccess += onSuccess;
            return this;
        }

        /// <summary>
        /// Perform this action each time validation is checked and passed, without arguments
        /// </summary>

        internal Rule<TMsgType, TMsgArgs> SetOnSuccess(Action onSuccess)
        {
            _onSuccess = _ => onSuccess();
            return this;
        }

        /// <summary>
        /// Perform this action each time validation is checked and passed, without arguments
        /// </summary>

        internal Rule<TMsgType, TMsgArgs> AddOnSuccess(Action onSuccess)
        {
            _onSuccess += _ => onSuccess();
            return this;
        }

        /// <summary>
        /// Perform this action each time validation is checked and failed, with arguments
        /// Receive the arguments and the error log
        /// </summary>

        internal Rule<TMsgType, TMsgArgs> SetOnFail(Action<TMsgArgs, LogInfo> onFail)
        {
            _onFail = onFail;
            return this;
        }

        /// <summary>
        /// Perform this action each time validation is checked and failed, without arguments
        /// Receive the error log
        /// </summary>

        internal Rule<TMsgType, TMsgArgs> SetOnFail(Action onFail)
        {
            _onFail = (_, _) => onFail();
            return this;
        }

        /// <summary>
        /// Perform this action each time validation is checked and failed, without arguments
        /// </summary>

        internal Rule<TMsgType, TMsgArgs> SetOnFail(Action<LogInfo> onFail)
        {
            _onFail = (arg, errorInfo) => onFail(errorInfo);
            return this;
        }

        /// <summary>
        /// Set a log upon successful validation, with arguments
        /// </summary>

        internal Rule<TMsgType, TMsgArgs> SetSuccessLog(Func<TMsgArgs, string> message)
        {
            _successLogString = message;
            return this;
        }

        /// <summary>
        /// Set a log upon successful validation, with arguments
        /// </summary>

        internal Rule<TMsgType, TMsgArgs> SetSuccessLog(Func<TMsgArgs, LogInfo> message)
        {
            _successLogLogInfo = message;
            return this;
        }

        internal Rule<TMsgType, TMsgArgs> SetLogCategories(string categories)
        {
            _logCategories = categories;
            return this;
        }

        // internal Rule<TMsgType, TMsgArgs> AddLogCategory(string category)
        // {
        //     if (!_logCategories.HasValue)
        //     {
        //         _logCategories = category;
        //     }
        //     else
        //     {
        //         _logCategories = _logCategories.UnboxRaw() + "," + category;
        //     }

        //     return this;
        // }

        internal Rule<TMsgType, TMsgArgs> SetOnReleaseAction(Action action)
        {
            _onReleaseAction = action;
            return this;
        }

        /// <summary>
        /// Sets a predicate for determining whether or not you want any logs active for this rule
        /// </summary>

        internal Rule<TMsgType, TMsgArgs> SetLogAnyRequirement(Func<TMsgArgs, bool> req)
        {
            SetLogSuccessRequirement(req);
            SetLogFailRequirement(req);
            return this;
        }

        /// <summary>
        /// Sets a predicate for determining whether or not you want success logs active for this rule
        /// </summary>

        internal Rule<TMsgType, TMsgArgs> SetLogSuccessRequirement(Func<TMsgArgs, bool> req)
        {
            _logSuccessReq = req;
            return this;
        }

        /// <summary>
        /// Sets a predicate for determining whether or not you want error logs active for this rule
        /// </summary>

        internal Rule<TMsgType, TMsgArgs> SetLogFailRequirement(Func<TMsgArgs, bool> req)
        {
            _logFailReq = req;
            return this;
        }

        /// <summary>
        /// Explicitly tell the rule not to log
        /// </summary>        

        internal Rule<TMsgType, TMsgArgs> DontLog()
        {
            _errorLogType = ErrorLogType.None;
            return this;
        }

        /// <summary>
        /// Tell the rule to only log the first time it validates
        /// </summary>

        internal Rule<TMsgType, TMsgArgs> LogOnce()
        {
            _logOnce = true;
            return this;
        }

        /// <summary>
        /// Tell the rule to only run once when an associated event is raised
        /// </summary>

        internal Rule<TMsgType, TMsgArgs> RunOnce()
        {
            _executeOne = true;
            return this;
        }

        /// <summary>
        /// Tell the rule to log every error as Info
        /// </summary>
        /// <returns></returns>

        internal Rule<TMsgType, TMsgArgs> LogFailAsInfo()
        {
            _errorLogType = ErrorLogType.Info;
            return this;
        }

        /// <summary>
        /// Tell the rule to log every error as Info.
        /// Allows the developer to defer additional information to be logged at runtime.
        /// </summary>

        internal Rule<TMsgType, TMsgArgs> LogFailAsInfo(Func<TMsgArgs, string> runtimeInfo)
        {
            _runtimeErrorInfo = runtimeInfo;
            return LogFailAsInfo();
        }

        /// <summary>
        /// Tell the rule to log every error as a Warning
        /// </summary>
        /// <returns></returns>

        internal Rule<TMsgType, TMsgArgs> LogAsWarning()
        {
            _errorLogType = ErrorLogType.Warning;
            return this;
        }

        /// <summary>
        /// Tell the rule to log every error as a Warning.
        /// Allows the developer to defer additional information to be logged at runtime.
        /// </summary>

        internal Rule<TMsgType, TMsgArgs> LogAsWarning(Func<TMsgArgs, string> runtimeInfo)
        {
            _runtimeErrorInfo = runtimeInfo;
            return LogAsWarning();
        }

        /// <summary>
        /// Log a failed validation as error as an error explicilty.
        /// Though this is default, this method is called by the builder to ensure the behavior.
        /// </summary>
        /// <returns></returns>

        internal Rule<TMsgType, TMsgArgs> LogFailAsError()
        {
            _errorLogType = ErrorLogType.Error;
            return this;
        }

        /// <summary>
        /// Tell the rule to log every error as an Error.
        /// Allows the developer to defer additional information to be logged at runtime.
        /// </summary>

        internal Rule<TMsgType, TMsgArgs> LogFairAsError(Func<TMsgArgs, string> additionalInfo)
        {
            _runtimeErrorInfo = additionalInfo;
            return LogFailAsError();
        }

        /// <summary>
        /// For Rules that have a defined Message upon their creation, handle the event when it's triggered
        /// </summary>

        private void OnMessageTriggered(TMsgArgs args)
        {
            Run(_onSuccess, _onFail, args);
        }

        /// <summary>
        /// Handles all success related work such as invoking the action and logging
        /// </summary>       
        /// <returns>Returns the result true (for success) and this rule</returns>

        private (bool, Rule<TMsgType, TMsgArgs>) DoSuccess(Action<TMsgArgs> onSuccess, TMsgArgs args)
        {            
            try
            {
                onSuccess?.Invoke(args);

                if (_logCategories.HasValue)
                {
                    this.Log(GetSuccessInfo(args), _logCategories.UnboxRaw(), req: () => CanLogSuccess(args));
                }
                else
                {
                   this.LogFrom(GetSuccessInfo(args), req: () => CanLogSuccess(args)); 
                }                
            }
            catch(Exception e)
            {
                e.LogException();
            }
            
            return (true, this);
        }

        /// <summary>
        /// Determines whether or not error logging requirements are met
        /// </summary>

        private bool MeetsErrorLogRequirements(TMsgArgs args) => _logFailReq == null || _logFailReq.Invoke(args);

        /// <summary>
        /// Determines whether or not success logging requirements are met
        /// </summary>

        private bool MeetsSuccessLogRequirements(TMsgArgs args) => _logSuccessReq == null || _logSuccessReq(args);

        /// <summary>
        /// Helper determining whether or not we should log success by checking if the success log if null
        /// </summary>

        private bool CanLogSuccess(TMsgArgs args) =>
            (_successLogString != null || _successLogLogInfo != null)
            && MeetsSuccessLogRequirements(args);


        /// <summary>
        /// Handles everything related to failure
        /// </summary>      
        /// <returns>Returns the result false (for failure) and this Validator</returns>

        private (bool, Rule<TMsgType, TMsgArgs>) DoFail(Action<TMsgArgs, LogInfo> onFail, TMsgArgs args)
        {
            // Create a new struct and attach additional info
            var error = _errorInfo.SetAdditionalInfo(GetRuntimeErrorInfo(args));

            // Log and raise errors as needed
            if (_logCategories.HasValue)
            {
                DoErrorLog(error.ToString(), _logCategories.UnboxRaw(), req: () => MeetsErrorLogRequirements(args));         
            }
            else
            {
                DoErrorLog(error.ToString(), req: () => MeetsErrorLogRequirements(args));
            }

            // When the logger raises an error it raises a generic ErrorMsgType that stores this error info
            Services.Get<IErrorLogService_v1>().RaiseError(error);

            // Stop logging from now on if log once is true
            if (_logOnce)
            {
                _errorLogType = ErrorLogType.None;
            }

            try
            {
                // Invoke the onFail function if its defined
                onFail?.Invoke(args, _errorInfo);
            }
            catch(Exception e)
            {
                e.LogException();
            }
            // return
            return (false, this);
        }

        /// <summary>
        /// If additional runtime info is defined then grab it, otherwise return an empty string
        /// </summary>

        private string GetRuntimeErrorInfo(TMsgArgs args) => _runtimeErrorInfo == null ? "" : _runtimeErrorInfo(args);

        /// <summary>
        /// If additional success info is defined then grab it, otherwise return an empty string
        /// </summary>

        private string GetSuccessInfo(TMsgArgs args)
        {
            if (_successLogLogInfo != null)
            {
                return _successLogLogInfo(args).ToString();
            }
            else if (_successLogString != null)
            {
                return _successLogString(args);
            }

            return "";
        }

        /// <summary>
        /// Determine how to log the error and then log it.
        /// Ignore it if its type None
        /// </summary>

        private Rule<TMsgType, TMsgArgs> DoErrorLog(string message, Func<bool> req)
            => _errorLogType == ErrorLogType.Info ? this.LogFrom(message, req) :
               _errorLogType == ErrorLogType.Warning ? this.LogWarning(message, req) :
               _errorLogType == ErrorLogType.Error ? this.LogError(message, req) :
                this;

        private Rule<TMsgType, TMsgArgs> DoErrorLog(string message, string categories, Func<bool> req)
            => _errorLogType == ErrorLogType.Info ? this.Log(message, categories, req) :
               _errorLogType == ErrorLogType.Warning ? this.LogWarning(message, req) :
               _errorLogType == ErrorLogType.Error ? this.LogError(message, req) :
                this;

        /// <summary>
        /// Used by the builder to expose builder functions to the developer
        /// </summary>

        internal IRuleBuilderObject_v1<TMsgType, TMsgArgs> ToBuilderV1() => new RuleBuilder<TMsgType, TMsgArgs>(this);

    }
}
