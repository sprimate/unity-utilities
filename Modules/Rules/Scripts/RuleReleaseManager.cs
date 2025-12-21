using HitTrax.CoreUtilities;
using HitTrax.GlobalMessagingService;
using System;
using System.Collections.Generic;

namespace HitTrax.Rules
{
    /// <summary>
    /// The rule release manager is used for managing rules that are listening for events
    /// that we'll want to clean up
    ///
    /// The release manager is called when either a defined release event is invoked,
    /// or a developer expicitly calls release on a Validitor
    ///
    /// When a rule is "released" it clears out its references
    /// </summary>
    /// <typeparam name="TMsgType"></typeparam>
    /// <typeparam name="TMsgArgs"></typeparam>
    internal static class RuleReleaseManager<TMsgType, TMsgArgs> where TMsgType : IMessageKey<TMsgArgs>
    {
        private static IMessageService_v1 MessageService => Services.Get<IMessageService_v1>();

        /// <summary>
        /// Cached rules that are listening for the release event
        /// and requirements needed for release
        /// </summary>

        private static SafeDict<IRule_v1, Func<TMsgArgs, bool>> _rules = new SafeDict<IRule_v1, Func<TMsgArgs, bool>>(new Dictionary<IRule_v1, Func<TMsgArgs, bool>>());

        /// <summary>
        /// Subscribes to a release event without a defined requirement
        /// </summary>
        /// <param name="rule"></param>

        internal static void SubscribeToReleaseMessage(IRule_v1 rule)
        {
            _rules.Set(rule, a => true);
            MessageService.AddListener<TMsgType, TMsgArgs>(OnReleaseMessage, preventDuplicate: true);
        }

        /// <summary>
        /// Subscribes to a release event with arguments and a defined requirement
        /// </summary>
        /// <param name="rule"></param>

        internal static void SubscribeToReleaseMessage(IRule_v1 rule, Func<TMsgArgs, bool> req)
        {
            _rules.Set(rule, req);
            MessageService.AddListener<TMsgType, TMsgArgs>(OnReleaseMessage, preventDuplicate: true);
        }

        /// <summary>
        /// Remove associated rules when its relese event occurs
        /// </summary>
        /// <param name="args"></param>

        internal static void OnReleaseMessage(TMsgArgs args)
        {
            // Designate a list of items to be removed
            List<IRule_v1> remove = new List<IRule_v1>();

            // Release items if they pass requirements and add to the remove list
            foreach (var key in _rules.Keys)
            {
                if (_rules[key](args))
                {
                    key.Release();
                    remove.Add(key);
                }
            }

            // Remove Rules
            foreach (var key in remove)
            {
                _rules.Remove(key);
            }
        }

        internal static void Release(Rule<TMsgType, TMsgArgs> rule)
        {
            // Remove this rule from the manager
            // This is where the rule "clears" itself and gets removed from the RuleManager cache
            RuleManager<TMsgType, TMsgArgs>.Release(rule);

            // Remove this rule from the list of rules that may have associated release events
            _rules.Remove(rule);

            // If there are no rules assocaited with this event type left, then stop listening to the release event
            if (_rules.Count == 0)
            {
                MessageService.RemoveListener<TMsgType, TMsgArgs>(OnReleaseMessage);
            }

        }
    }
}
