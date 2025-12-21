//using HitTrax.CoreUtilities;
//using HitTrax.GlobalMessagingService;
//using System;
//using System.Collections.Generic;

//namespace HitTrax.ErrorHandling
//{
//    /// <summary>
//    /// The validate release manager is used for managing validators that are listening for events
//    /// that we'll want to clean up
//    ///
//    /// The release manager is called when either a defined release event is invoked,
//    /// or a developer expicitly calls release on a Validitor
//    ///
//    /// When a validator is "released" it clears out its references
//    /// </summary>
//    /// <typeparam name="EventType"></typeparam>
//    /// <typeparam name="EventArgs"></typeparam>
//    internal static class ValidatorReleaseManager<EventType, EventArgs> where EventType : IMessageKey<EventArgs>
//    {
//        private static IMessageService_v1 MessageService => Services.Get<IMessageService_v1>(); 

//        /// <summary>
//        /// Cached validators that are listening for the release event
//        /// and requirements needed for release
//        /// </summary>

//        private static SafeDict<IValid_v1, Func<EventArgs, bool>> _validators = new SafeDict<IValid_v1, Func<EventArgs, bool>>(new Dictionary<IValid_v1, Func<EventArgs, bool>>());

//        /// <summary>
//        /// Subscribes to a release event without a defined requirement
//        /// </summary>
//        /// <param name="validator"></param>

//        internal static void SubscribeToReleaseEvent(IValid_v1 validator)
//        {        
//            _validators.Set(validator, a => true);
//            MessageService.AddListener<EventType, EventArgs>(OnReleaseEvent, preventDuplicate: true);
//        }

//        /// <summary>
//        /// Subscribes to a release event with arguments and a defined requirement
//        /// </summary>
//        /// <param name="validator"></param>

//        internal static void SubscribeToReleaseEvent(IValid_v1 validator, Func<EventArgs, bool> req)
//        {
//            _validators.Set(validator, req);
//            MessageService.AddListener<EventType, EventArgs>(OnReleaseEvent, preventDuplicate: true);
//        }

//        /// <summary>
//        /// Remove associated validators when its relese event occurs
//        /// </summary>
//        /// <param name="args"></param>

//        internal static void OnReleaseEvent(EventArgs args)
//        {
//            // Designate a list of items to be removed
//            List<IValid_v1> remove = new List<IValid_v1>();

//            // Release items if they pass requirements and add to the remove list
//            foreach (var key in _validators.Keys)
//            {
//                if (_validators[key](args))
//                {
//                    key.Release();
//                    remove.Add(key);                    
//                }
//            }

//            // Remove Validators
//            foreach (var key in remove)
//            {
//                _validators.Remove(key);
//            }
//        }

//        internal static void Release(Validator<EventType, EventArgs> validator)
//        {
//            // Remove this validator from the manager
//            // This is where the validator "clears" itself and gets removed from the ValidateManager cache
//            ValidateManager<EventType, EventArgs>.Release(validator);

//            // Remove this validator from the list of validators that may have associated release events
//            _validators.Remove(validator);

//            // If there are no validators assocaited with this event type left, then stop listening to the release event
//            if (_validators.Count == 0)
//            {
//                MessageService.RemoveListener<EventType, EventArgs>(OnReleaseEvent);
//            }

//        }
//    }
//}
