using HitTrax.CoreUtilities;
using HitTrax.GlobalMessagingService;
using static HitTrax.CoreUtilities.SafeFunctions;

using System;
using System.Collections.Generic;


// DO NOT CHANGE THE NAMESPACE OF THIS CLASS OR YOUR MODULE WILL BREAK 
namespace HitTrax.ErrorHandling
{
    /// <summary>
    /// A generic event that gets raised when RaiseError is called. The argument will contain the LogInfo (Error)
    /// </summary>
    public class MsgError : IMessageKey<LogInfo> { }

    public interface IErrorLogService_v1 : IService
    {
        /// <summary>
        /// Create a log type or get it if it's already been created
        /// </summary>
        
        LogInfo Error(int code, string key, string message)
            => LoggerInternal.Error(code, key, message);

        /// <summary>
        /// Raise an error with the MsgError datatype
        /// </summary>
        /// <param name="error"></param>
        /// <param name="additionalInfo"></param>
        /// <returns></returns>

        LogInfo RaiseError(LogInfo error, string additionalInfo = "")
           => error.RaiseError(additionalInfo);

        /// <summary>
        /// Raise an error with a developer created ErrorMsg type
        /// </summary>
        /// <typeparam name="ErrorMsgType"></typeparam>
        /// <param name="error"></param>
        /// <param name="additionalInfo"></param>
        /// <returns></returns>

        LogInfo RaiseError<ErrorMsgType>(LogInfo error, string additionalInfo = "") where ErrorMsgType : MsgError
            => error.RaiseError(additionalInfo);

    }

    public class ErrorLogService : IErrorLogService_v1 { }

    // This info is intended to be primarily be used for "errors"
    // But cit an contain information for Warning and Info Log
    public struct LogInfo
    {
        /// <summary>
        /// Generally used for the error code
        /// </summary>
        internal Safe<int> ErrorCode { get; private set; }

        /// <summary>
        /// A unique key
        /// </summary>

        internal string Key { get; private set; }

        /// <summary>
        /// Description, generally used for logging 
        /// </summary>
        internal string Description { get; private set; }

        /// <summary>
        /// This string generally used when a developer adds additional log info at runtime
        /// </summary>

        internal string AdditionalInfo { get; private set; }

        /// <summary>
        /// Assocaited Exception
        /// </summary>

        internal Exception Exception { get; private set; }

        public LogInfo(string key, string message)
        {
            ErrorCode = int.MinValue;
            Key = key;
            Description = message;
            AdditionalInfo = "";
            Exception = null;
        }

        internal LogInfo(int errorCode, string key, string message)
        {
            ErrorCode = errorCode;
            Key = key;
            Description = message;
            AdditionalInfo = "";
            Exception = null;
        }

        public LogInfo SetAdditionalInfo(string info)
        {
            AdditionalInfo = info;
            return this;
        }

        /// <summary>
        /// Allows the developer to store exception if one is associated with the log
        /// </summary>

        internal LogInfo SetException(Exception exception)
        {
            Exception = exception;
            return this;
        }

        /// <summary>
        /// If there's an exception, return it's message, otherwise return a blank string
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        internal string ExceptionMessage(Exception exception) => Exception == null ? "" : Exception.Message;

        /// <summary>
        /// Can use "negative" values to define this log as "success"
        /// </summary>
        internal bool Success => !ErrorCode.HasValue;

        /// <summary>
        /// Can use "positive" values to define this log as "fail"
        /// </summary>
        internal bool Fail => ErrorCode.HasValue;

        public override string ToString() {
            string errorCode = ErrorCode.HasValue ? $"Error Code: {ErrorCode.UnboxRaw()} - " : "";
            string key = string.IsNullOrEmpty(Key) ? "" : $"{Key} - ";
            return $"{errorCode}{key}{Description} {AdditionalInfo} {ExceptionMessage(Exception)}";
        }
    }

    // DO NOT CHANGE THE NAME OF THIS CLASS OR YOUR MODULE WILL BREAK
    public static class ErrorHandlingLoader
    {
        // DO NOT CHANGE THE NAME OF THIS FUNCTION OR YOUR MODULE WILL BREAK
        public static void Load()
        {
            // This is where you can initialize your module
            Services.RegisterSingleton(new ErrorLogService());            
        }
    }

    // Defined error codes for this Service
    internal static class ErrorCreationErrors
    {        
        internal static LogInfo ErrorCodeMismatch => new LogInfo(10000, "Duplicate Log Codes", "Duplicate Log Codes are not allowed.");
        internal static LogInfo ErrorKeyMismatch => new LogInfo(10001, "Duplicate Log Key", "Duplicate Log keys are not allowed.");
    }

    public static class LoggerInternal
    {    
        internal static Dictionary<int, LogInfo> _logDict = new();

        /// <summary>
        /// This either creates an error or gets the error already created and returns that error out
        /// If the error error already exists (matches code or key) an error will be logged accordingly
        /// </summary>
        
        internal static LogInfo Error(int errorCode, string key, string message)
            => GetOrAddError(new LogInfo(errorCode, key, message))
                .SelectOut(e => e, () => _logDict[errorCode]);

        // Raise Generic Errors
        internal static LogInfo RaiseError(this LogInfo error, string additionalInfo = "")
             => Services
                    .Get<IMessageService_v1>()
                    .Raise<MsgError, LogInfo>(error.SetAdditionalInfo(additionalInfo));

        // Raise an Error with a defined MsgType

        internal static LogInfo RaiseError<ErrorMsgType>(this LogInfo error, string additionalInfo = "") where ErrorMsgType : MsgError
            => Services
                    .Get<IMessageService_v1>()
                    .Raise<ErrorMsgType, LogInfo>(error.SetAdditionalInfo(additionalInfo));

        internal static Safe<LogInfo> GetOrAddError(LogInfo error)
            => error.ErrorCode.HasValue ?
                _logDict
                    .TryGet(error.ErrorCode)
                    .Select(
                        ifSome: source => CheckMismatch(error, source),
                        ifNone: () => _logDict.TrySet(error.ErrorCode, error)[error.ErrorCode.UnboxRaw()]
                    ) : None;


        /// <summary>
        /// Check to see if a developer is trying to get an error (LogInfo)
        /// If getting one that is already defined check to see if there is a Code or Key mismatch
        /// If either are a mismatch, raise an Error
        /// This will allow the software to continue as normal but alert the developer of a duplication found
        /// </summary>
        /// <param name="newError"></param>
        /// <param name="sourceError"></param>
        /// <returns></returns>
        
        internal static LogInfo CheckMismatch(LogInfo newError, LogInfo sourceError)
        {
            if (newError.ErrorCode.Equals(sourceError.ErrorCode) && newError.Key != sourceError.Key)
            {
                // Error Key Mismatch
                RaiseError(ErrorCreationErrors.ErrorCodeMismatch, $"{newError}");
                
            }
            else if (newError.Key == sourceError.Key && !newError.Equals(sourceError.ErrorCode))
            {
                // Error Code Missmatch
                RaiseError(ErrorCreationErrors.ErrorKeyMismatch, $"{newError}");
            }

            return newError;
        }

    }

}