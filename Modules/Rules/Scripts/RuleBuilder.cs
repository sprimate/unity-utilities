using System;
using System.Collections.Generic;
using HitTrax.CoreUtilities;
using HitTrax.ErrorHandling;
using HitTrax.GlobalMessagingService;

namespace HitTrax.Rules
{

    /// <summary>
    /// This is the entry point for building a Rule
    /// Besides "Create" there will not be any direct access to this builder
    /// All other build functions will go through the generic interface
    /// Pass in Log Info when creating a Rule that is specifically checking for "Error" when fail
    /// If you are simply treating the Rule as a true vs false check, you may decide to ignore the error log
    /// </summary>

    public interface IBuildRuleService_v1 : IService
    {
        /// <summary>
        /// Create a Rule that allows the developer to define a GMS message for when it should be validated
        /// </summary>
        /// <returns>Returns a builder to allow the developer to continue to construct the validator</returns>

        public IRuleBuilderObject_v1<TMsgType, TMsgArgs> Create<TMsgType, TMsgArgs>() where TMsgType : IMessageKey<TMsgArgs>
          => RuleManager<TMsgType, TMsgArgs>.Create(addListener: true);
    
        /// <summary>
        /// Create a Rule that can accept arguments upon validation
        /// </summary>       
        /// <returns>Returns a builder to allow the developer to continue to construct the validator</returns>

        public IRuleBuilderObject_v1<EmptyMsg<TArg>, TArg> Create<TArg>()
            => RuleManager<EmptyMsg<TArg>, TArg>.Create(addListener: false);

        /// <summary>
        /// Create a Rule that is not intended to by used with a generic TMsgType or Argument type
        /// </summary>
        /// <returns>Returns a builder to allow the developer to continue to construct the validator</returns>
        public IRuleBuilderObject_v1<EmptyMsg, Nothing> Create()
            => RuleManager<EmptyMsg, Nothing>.Create(addListener: false);

        /// <summary>
        /// Create a Rule that allows the developer to define a GMS message for when it should be validated
        /// </summary>
        /// <returns>Returns a builder to allow the developer to continue to construct the validator</returns>

        public IRuleBuilderObject_v1<TMsgType, TMsgArgs> Create<TMsgType, TMsgArgs>(LogInfo errorInfo) where TMsgType : IMessageKey<TMsgArgs>
            => RuleManager<TMsgType, TMsgArgs>.Create(errorInfo, addListener: true);

        /// <summary>
        /// Create a Rule that can accept arguments upon validation
        /// </summary>       
        /// <returns>Returns a builder to allow the developer to continue to construct the validator</returns>

        public IRuleBuilderObject_v1<EmptyMsg<TArg>, TArg> Create<TArg>(LogInfo errorInfo)
            => RuleManager<EmptyMsg<TArg>, TArg>.Create(errorInfo, addListener: false);

        /// <summary>
        /// Create a Rule that is not intended to by used with a generic TMsgType or Argument type
        /// </summary>
        /// <returns>Returns a builder to allow the developer to continue to construct the validator</returns>
        public IRuleBuilderObject_v1<EmptyMsg, Nothing> Create(LogInfo errorInfo)
            => RuleManager<EmptyMsg, Nothing>.Create(errorInfo, addListener: false);

        public IRule_v1 CreateSuccessRule<TMsgType, TMsgArgs>(Action<TMsgArgs> action, Func<TMsgArgs, bool> predicate = null) where TMsgType : IMessageKey<TMsgArgs>
            => Create<TMsgType, TMsgArgs>().SetOnSuccess(action).SetPredicate(val => predicate == null ? true : predicate(val)).Rule;

        public IRule_v1 CreateSuccessRule<TMsgType, TMsgArgs>(Action action, Func<TMsgArgs, bool> predicate = null) where TMsgType : IMessageKey<TMsgArgs>
            => Create<TMsgType, TMsgArgs>().SetOnSuccess(action).SetPredicate(val => predicate == null ? true : predicate(val)).Rule;
            
    }

    public static class RuleBuilders
    {
        public static IBuildRuleService_v1 V1 => Services.Get<IBuildRuleService_v1>();
    }

    internal class BuildRuleService : IBuildRuleService_v1 { }

    /// <summary>
    /// The validate manager manages state, adding and releasing Rules generally called
    /// by the BuildRule and ReleaseRuleManager
    /// </summary>
    /// <typeparam name="TMsgType"></typeparam>
    /// <typeparam name="TMsgArgs"></typeparam>

    internal static class RuleManager<TMsgType, TMsgArgs> where TMsgType : IMessageKey<TMsgArgs>
    {
        /// <summary>
        /// The collection of Rules created
        /// </summary>
        /// 
        private static HashSet<Rule<TMsgType, TMsgArgs>> _rules = new();

        /// <summary>
        /// Called by the builder to cache a newly created Rule that does not require an error log
        /// </summary>
        /// <param name="errorInfo"></param>
        /// <param name="addListener"></param>
        /// <returns></returns>
        internal static IRuleBuilderObject_v1<TMsgType, TMsgArgs> Create(bool addListener)
        {
            var rule = new Rule<TMsgType, TMsgArgs>(addListener);
            _rules.Add(rule);
            return rule.ToBuilderV1();
        }

        /// <summary>
        /// Called by the builder to cache a newly created Rule
        /// </summary>
        /// <param name="errorInfo"></param>
        /// <returns></returns>

        internal static IRuleBuilderObject_v1<TMsgType, TMsgArgs> Create(LogInfo errorInfo, bool addListener)
        {
            var rule = new Rule<TMsgType, TMsgArgs>(errorInfo, addListener);
            _rules.Add(rule);
            return rule.ToBuilderV1();
        }

        /// <summary>
        /// Called exclusively by the RuleReleaseManager
        /// Nulls out the Rule and removes it from the cache
        /// </summary>
        /// <param name="rule"></param>

        internal static void Release(Rule<TMsgType, TMsgArgs> rule)
        {
            rule.Clear();
            _rules.Remove(rule);
        }
    }

    /// <summary>
    /// This interface layer is used to give the user access to "builder" functionality without direct access
    /// to the Rule object itself
    ///
    /// There are functions that allow you to accept arguments or not accept arguments
    /// For complete clarity, those are the arguments recived upon execution whether that's through GMS
    /// or, plugged into a Rule function
    /// </summary>

    public interface IRuleBuilderObject_v1<TMsgType, TMsgArgs> where TMsgType : IMessageKey<TMsgArgs>
    {
        /// <summary>
        /// When the developer is done building, they can return out the validator if they want to store it
        /// This is also used by the builder
        ///         

        public Rule<TMsgType, TMsgArgs> Rule
        {
            get;
        }

        /// <summary>
        /// Define the validator's predicate (with arguments)
        /// </summary>
        /// <param name="isValid"></param>

        public IRuleBuilderObject_v1<TMsgType, TMsgArgs> SetPredicate(Func<TMsgArgs, bool> isValid)
            => Rule.SetPredicate(isValid).ToBuilderV1();

        /// <summary>
        /// Define the rule's predicate (without arguments)
        /// </summary>
        /// <param name="isValid"></param>

        public IRuleBuilderObject_v1<TMsgType, TMsgArgs> SetPredicate(Func<bool> isValid)
            => Rule.SetPredicate(isValid).ToBuilderV1();

        /// <summary>
        /// Define success action with arguments.
        /// Invoked upon successful validation
        /// </summary>
        /// <param name="onSuccess"></param>
        /// <returns></returns>

        public IRuleBuilderObject_v1<TMsgType, TMsgArgs> SetOnSuccess(Action<TMsgArgs> onSuccess)
            => Rule.SetOnSuccess(onSuccess).ToBuilderV1();

        /// <summary>
        /// Define success action with arguments.
        /// Invoked upon successful validation
        /// </summary>
        /// <param name="onSuccess"></param>
        /// <returns></returns>

        public IRuleBuilderObject_v1<TMsgType, TMsgArgs> AddOnSuccess(Action<TMsgArgs> onSuccess)
            => Rule.AddOnSuccess(onSuccess).ToBuilderV1();

        /// <summary>
        /// Define success action without arguments.
        /// Invoked upon successful validation
        /// </summary>
        /// <param name="onSuccess"></param>
        /// <returns></returns>
        public IRuleBuilderObject_v1<TMsgType, TMsgArgs> SetOnSuccess(Action onSuccess)
            => Rule.SetOnSuccess(onSuccess).ToBuilderV1();

        /// <summary>
        /// Define success action without arguments.
        /// Invoked upon successful validation
        /// </summary>
        /// <param name="onSuccess"></param>
        /// <returns></returns>
        public IRuleBuilderObject_v1<TMsgType, TMsgArgs> AddOnSuccess(Action onSuccess)
            => Rule.AddOnSuccess(onSuccess).ToBuilderV1();

        /// <summary>
        /// Define fail action without arguments
        /// Invoked upon fail validation
        /// </summary>
        /// <param name="onFail"></param>
        /// <returns></returns>

        public IRuleBuilderObject_v1<TMsgType, TMsgArgs> SetOnFail(Action onFail)
            => Rule.SetOnFail(onFail).ToBuilderV1();


        /// <summary>
        /// Define fail action with arguments (Message Arguments and Error Info (LogInfo)).
        /// Invoked upon validation failure
        /// </summary>
        /// <param name="onFail"></param>
        /// <returns></returns>
        public IRuleBuilderObject_v1<TMsgType, TMsgArgs> SetOnFail(Action<TMsgArgs, LogInfo> onFail)
            => Rule.SetOnFail(onFail).ToBuilderV1();

        /// <summary>
        /// Explicitly tell the rule to not log
        /// </summary>        
        public IRuleBuilderObject_v1<TMsgType, TMsgArgs> DontLog() => Rule.DontLog().ToBuilderV1();

        /// <summary>
        /// Define a function (with arguments) for whether or not the rule being constructed should log success & fail.
        /// </summary>
        /// <param name="requirement"></param>

        public IRuleBuilderObject_v1<TMsgType, TMsgArgs> LogIf(Func<TMsgArgs, bool> requirement)
            => Rule.SetLogAnyRequirement(requirement).ToBuilderV1();

        /// <summary>
        /// Define a function (without arguments) for whether or not the rule being constructed should log success & fail.
        /// </summary>
        /// <param name="requirement"></param>

        public IRuleBuilderObject_v1<TMsgType, TMsgArgs> LogIf(Func<bool> requirement)
            => Rule.SetLogAnyRequirement(_ => requirement()).ToBuilderV1();

        /// <summary>
        /// Define a function (with arguments) for whether or not the rule being constructed should log success.
        /// </summary>
        /// <param name="requirement"></param>

        public IRuleBuilderObject_v1<TMsgType, TMsgArgs> LogSuccessIf(Func<TMsgArgs, bool> requirement)
            => Rule.SetLogSuccessRequirement(requirement).ToBuilderV1();

        /// <summary>
        /// Define a function (without arguments) for whether or not the rule being constructed should log success.
        /// </summary>
        /// <param name="requirement"></param>

        public IRuleBuilderObject_v1<TMsgType, TMsgArgs> LogSuccessIf(Func<bool> onLogRequestReq)
            => Rule.SetLogSuccessRequirement(_ => onLogRequestReq()).ToBuilderV1();

        /// <summary>
        /// Define a function (with arguments) for whether or not the rule being constructed should log failure.
        /// </summary>
        /// <param name="requirement"></param>

        public IRuleBuilderObject_v1<TMsgType, TMsgArgs> LogFailIf(Func<TMsgArgs, bool> requirement)
            => Rule.SetLogFailRequirement(requirement).ToBuilderV1();

        /// <summary>
        /// Define a function (without arguments) for whether or not the rule being constructed should log failure.
        /// </summary>
        /// <param name="requirement"></param>

        public IRuleBuilderObject_v1<TMsgType, TMsgArgs> LogFailIf(Func<bool> requirement)
           => Rule.SetLogFailRequirement(_ => requirement()).ToBuilderV1();

        /// <summary>
        /// Define a success message with arguments
        /// </summary>

        public IRuleBuilderObject_v1<TMsgType, TMsgArgs> SetSuccessLog(Func<TMsgArgs, string> message)
            => Rule.SetSuccessLog(message).ToBuilderV1();

        /// <summary>
        /// Define a success message without arguments
        /// </summary>

        public IRuleBuilderObject_v1<TMsgType, TMsgArgs> SetSuccessLog(Func<string> message)
            => Rule.SetSuccessLog(_ => message()).ToBuilderV1();

        /// <summary>
        /// Define comma separated categories for rules for easier filtering of displaying logs
        /// </summary>
        /// <param name="categories"></param>
        /// <returns></returns>

        public IRuleBuilderObject_v1<TMsgType, TMsgArgs> SetLogCategories(string categories)
            => Rule.SetLogCategories(categories).ToBuilderV1();

        /// <summary>
        /// Append new category. 
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>

        // (Anthony) The problem with this, is that the way things are currently built
        // There's a chance of the same log being printed twice, which will get confusing
        // This can be resolved, but not looking to resolve that right now
        
        // public IRuleBuilderObject_v1<TMsgType, TMsgArgs> AddLogCategory(string category)
        //     => Rule.AddLogCategory(category).ToBuilderV1();

        /// <summary>
        /// Tell the rule to log all errors as "Info"
        /// </summary>

        public IRuleBuilderObject_v1<TMsgType, TMsgArgs> LogFailAsInfo()
            => Rule.LogFailAsInfo().ToBuilderV1();

        /// <summary>
        /// Tell the rule to log all failed validations as "Info"
        /// Define additional runtime info but don't accept arguments
        /// </summary>
        /// <param name="runtimeInfo"></param>
        /// <returns></returns>

        public IRuleBuilderObject_v1<TMsgType, TMsgArgs> LogFailAsInfo(Func<string> runtimeInfo)
            => Rule.LogFailAsInfo(_ => runtimeInfo()).ToBuilderV1();

        /// <summary>
        /// Tell the rule to log all failed validations as "Info"
        /// Define additional runtime info and accept arguments
        /// </summary>
        /// <param name="runtimeInfo"></param>
        /// <returns></returns>

        public IRuleBuilderObject_v1<TMsgType, TMsgArgs> LogFailAsInfo(Func<TMsgArgs, string> runtimeInfo)
            => Rule.LogFailAsInfo(runtimeInfo).ToBuilderV1();

        /// <summary>
        /// Tell the rule to log all failed validations as "Warnings"
        /// </summary>
        public IRuleBuilderObject_v1<TMsgType, TMsgArgs> LogFailAsWarning()
            => Rule.LogAsWarning().ToBuilderV1();

        /// <summary>
        /// Tell the rule to log all failed validations as "Warnings"
        /// Define additional runtime info but don't accept arguments
        /// </summary>
        /// <param name="runtimeInfo"></param>
        /// <returns></returns>
        public IRuleBuilderObject_v1<TMsgType, TMsgArgs> LogFailAsWarning(Func<string> runtimeInfo)
            => Rule.LogAsWarning(_ => runtimeInfo()).ToBuilderV1();

        /// <summary>
        /// Tell the rule to log all failed validations as "Warning"
        /// Define additional runtime info and accept arguments
        /// </summary>
        /// <param name="runtimeInfo"></param>
        /// <returns></returns>

        public IRuleBuilderObject_v1<TMsgType, TMsgArgs> LogFailAsWarning(Func<TMsgArgs, string> runtimeInfo)
            => Rule.LogAsWarning(runtimeInfo).ToBuilderV1();

        /// <summary>
        /// Tell the rule to log all failed validations as "Errors"
        /// </summary>

        public IRuleBuilderObject_v1<TMsgType, TMsgArgs> LogFailAsError()
            => Rule.LogFailAsError().ToBuilderV1();

        /// <summary>
        /// Tell the rule to log all failed validations as "Errors"
        /// Define additional runtime info but don't accept arguments
        /// </summary>
        /// <param name="runtimeInfo"></param>
        /// <returns></returns>

        public IRuleBuilderObject_v1<TMsgType, TMsgArgs> LogFailAsError(Func<string> runtimeInfo)
            => Rule.LogFairAsError(_ => runtimeInfo()).ToBuilderV1();

        /// <summary>
        /// Tell the rule to log all failed validations as "Errors"
        /// Define additional runtime info and accept arguments
        /// </summary>
        /// <param name="runtimeInfo"></param>
        /// <returns></returns>
        public IRuleBuilderObject_v1<TMsgType, TMsgArgs> LogFailAsError(Func<TMsgArgs, string> runtimeInfo)
            => Rule.LogFairAsError(runtimeInfo).ToBuilderV1();

        /// <summary>
        /// Tell the rule to only perform its validation log once.
        /// The rule will still validate but will stop logging
        /// </summary>

        public IRuleBuilderObject_v1<TMsgType, TMsgArgs> LogOnce()
            => Rule.LogOnce().ToBuilderV1();

        /// <summary>
        /// Tell the rule to only validate one time when its assocaited message is raised
        /// It does not prevent validation explicitly called by the developer
        /// Therefore, the object is not automatically released after validation
        /// </summary>

        public IRuleBuilderObject_v1<TMsgType, TMsgArgs> RunOnce()
            => Rule.RunOnce().ToBuilderV1();

        /// <summary>
        /// Define a release message.
        /// When the message is triggered:
        /// 1. The associated rule clears out its data
        /// 2. Unsubscribes from messages
        /// 3. Gets removed from the RuleManager Cahce
        /// 4. Gets removed from the RuleReleaseManager cache
        /// </summary>

        public IRuleBuilderObject_v1<TMsgType, TMsgArgs> ReleaseWhen<TReleaseMsg, TReleaseMsgArgs>() where TReleaseMsg : IMessageKey<TReleaseMsgArgs>
        {
            RuleReleaseManager<TReleaseMsg, TReleaseMsgArgs>.SubscribeToReleaseMessage(Rule);
            return Rule.ToBuilderV1();
        }

        /// <summary>
        /// Define an argumentless release message but accept a requirement (req).
        /// If the message is raised and the required predicate passes, then release the rule
        /// </summary>

        public IRuleBuilderObject_v1<TMsgType, TMsgArgs> ReleaseWhen<TReleaseMsg, TReleaseMsgArgs>(Func<bool> req) where TReleaseMsg : IMessageKey<TReleaseMsgArgs>
        {
            RuleReleaseManager<TReleaseMsg, TReleaseMsgArgs>.SubscribeToReleaseMessage(Rule, _ => req());
            return Rule.ToBuilderV1();
        }

        /// <summary>
        /// Define a release message with arguments but accept a requirement (req).
        /// If the message is raised and the required predicate passes, then release the rule
        /// </summary>


        public IRuleBuilderObject_v1<TMsgType, TMsgArgs> ReleaseWhen<TReleaseMsg, TReleaseMsgArgs>(Func<TReleaseMsgArgs, bool> req) where TReleaseMsg : IMessageKey<TReleaseMsgArgs>
        {
            RuleReleaseManager<TReleaseMsg, TReleaseMsgArgs>.SubscribeToReleaseMessage(Rule, req);
            return Rule.ToBuilderV1();
        }

        /// <summary>
        /// Define an action for when the callback is completed
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>

        public IRuleBuilderObject_v1<TMsgType, TMsgArgs> SetOnReleaseAction(Action action)
        {
            Rule.SetOnReleaseAction(action);
            return Rule.ToBuilderV1();
        }
    }

    /// <summary>
    /// This is required to return the builder interface from the Rule as needed
    /// </summary>
    /// <typeparam name="TMsgType"></typeparam>
    /// <typeparam name="TMsgArgs"></typeparam>
    public struct RuleBuilder<TMsgType, TMsgArgs> : IRuleBuilderObject_v1<TMsgType, TMsgArgs> where TMsgType : IMessageKey<TMsgArgs>
    {
        /// <summary>
        /// The rule that the builder will be affecting and that the developer can extract after building
        /// </summary>
        public Rule<TMsgType, TMsgArgs> Rule { get; private set; }

        /// <summary>
        /// Construct the builder and define the new rule that the builder initially constructs
        /// </summary>
        /// <param name="rule"></param>

        internal RuleBuilder(Rule<TMsgType, TMsgArgs> rule)
        {
            Rule = rule;
        }
    }

}
