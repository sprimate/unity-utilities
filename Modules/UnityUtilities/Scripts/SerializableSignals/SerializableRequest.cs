using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace HitTrax.UnityUtilities
{
    public enum RequestStatus { NotStarted, Active, Completed, TimedOut, Canceled }

    /// <summary>
    /// This class should be extended when you want to fire an event, but you want to know when it's completed
    /// "Requests" are considered "Completed" when somebody calls the "Complete" function.
    /// Listeners and callers can get callbacks when the request is completed.
    /// </summary>
    /// <typeparam name="TEventSelf"></typeparam>
    public abstract class ARequest<TEventSelf> : ASignal<TEventSelf>, IRequest where TEventSelf : ARequest<TEventSelf>
    {
        private Action<RequestStatus> _onComplete;
        protected UniTaskCompletionSource _completionSource;
        protected CancellationTokenSource _cts;
        private bool IsDone => Status == RequestStatus.Completed || Status == RequestStatus.Canceled || Status == RequestStatus.TimedOut;

        public RequestStatus Status { get; private set; }

        public void OnCompleted(Action<RequestStatus> onDone)
        {
            if (IsDone)
            {
                onDone?.Invoke(Status);
            }
            else
            {
                _onComplete += onDone;
            }
        }

        public virtual void Complete()
        {
            if (IsDone)
            {
                Debug.LogError($"Calling {nameof(Complete)}() on an already-finished request");
            }
            else if (Status == RequestStatus.NotStarted)
            {
                //Shouldn't get here
                Debug.LogError($"Calling {nameof(Complete)}() on a not-started request");
            }
            else if (Status == RequestStatus.Active)
            {
                _completionSource?.TrySetResult();
            }
            else
            {
                Debug.LogError($"Unhandled Status [{Status}]");
            }
        }

        public virtual bool TryCancel()
        {
            if (IsDone || Status == RequestStatus.NotStarted || Status != RequestStatus.Active)
            {
                return false;
            }

            _cts.Cancel();
            return true;
        }

        public virtual void Cancel()
        {
            if (TryCancel())
            {
                return;
            }


            if (IsDone)
            {
                Debug.LogError($"Calling {nameof(Cancel)}() on an already-finished request");
            }
            else if (Status == RequestStatus.NotStarted)
            {
                //Shouldn't get here
                Debug.LogError($"Calling {nameof(Cancel)}() on a not-started request");
            }
            else
            {
                Debug.LogError($"Unhandled Status [{Status}]");
            }
        }

        public virtual async UniTask<RequestStatus> RaiseAsync(float? timeout = 2f)
        {
            _completionSource = new UniTaskCompletionSource();
            _cts = new CancellationTokenSource();
            Raise();

            try
            {
                if (timeout.HasValue)
                {
                    var timeoutTask = UniTask.Delay(Mathf.RoundToInt(timeout.Value * 1000f), cancellationToken: _cts.Token);
                    var completedTask = await UniTask.WhenAny(_completionSource.Task, timeoutTask);

                    if (_completionSource.Task.Status.IsCompleted())
                    {
                        Status = RequestStatus.Completed;
                    }
                    else
                    {
                        Status = RequestStatus.TimedOut;
                        Debug.Log("WARNING: " + this + " Timed out after " + timeout.Value + " seconds");
                    }
                }
                else
                {
                    await _completionSource.Task.AttachExternalCancellation(_cts.Token);
                    Status = RequestStatus.Completed;
                }
            }
            catch (OperationCanceledException)
            {
                Status = RequestStatus.Canceled;
            }

            _onComplete?.Invoke(Status);
            _onComplete = null;
            _completionSource = null;
            _cts = null;
            return Status;
        }

        public void Raise(Action<RequestStatus> onDone, float? timeout = 2f)
        {
            OnCompleted(onDone);
            Raise(timeout);
        }

        public override void Raise() => Raise(2f);

        public void Raise(float? timeout = 2f)
        {
            //Always RaiseAsync, in case a different listener wants a completion callback
            if (_completionSource == null)
            {
                RaiseAsync(timeout).Forget();
            }
            else
            {
                Status = RequestStatus.Active;
                base.Raise();
            }
        }
    }

    public abstract class ASerializableRequest<TEventSelf> : ARequest<TEventSelf>, ISerializableRequest where TEventSelf : ARequest<TEventSelf>, new()
    {
        public ISerializableRequest Clone() => (ISerializableRequest)MemberwiseClone();
    }

    /// <summary>
    /// This class should be extended when you want to fire an event, but you want to know when it's completed
    /// "Requests" are considered "Completed" when somebody calls the "Complete" function.
    /// Listeners and callers can get callbacks when the request is completed.
    /// This class also has a "TResult" generic, so callers must call you call "Complete", you must pass in the result of the request
    /// </summary>
    /// <typeparam name="TResult">This is the type of the result your request requires</typeparam>
    public abstract class ARequest<TEventSelf, TResult> : ARequest<TEventSelf>, IRequest<TResult> where TEventSelf : ARequest<TEventSelf>
    {
        private TResult _result;
        public void OnCompleted(Action<RequestStatus, TResult> onDone)
        {
            base.OnCompleted(status =>
            {
                onDone?.Invoke(status, _result);
            });
        }

        public bool TryGetResult(out TResult outResult)
        {
            outResult = _result;
            return Status == RequestStatus.Completed;
        }

        // Hidden from usage; causes compile-time error
        //If you override/hide the error for some reason, it should still work
        [Obsolete("This request expects a result. Please use Complete(TResult) instead", true)]
        public new void Complete()
        {
            base.Complete();
        }

        public void Complete(TResult result)
        {
            _result = result;
            base.Complete();
        }

        public void Raise(Action<RequestStatus, TResult> onCompleted, float? timeout = 2f)
        {
            OnCompleted(onCompleted);
            base.Raise(timeout);
        }

        public virtual new async UniTask<(RequestStatus status, TResult result)> RaiseAsync(float? timeout = 2f)
        {
            await base.RaiseAsync(timeout);
            return (Status, _result);
        }
    }

    public abstract class ASerializableRequest<TEventSelf, TResult> : ARequest<TEventSelf, TResult>, ISerializableRequest<TResult> where TEventSelf : ARequest<TEventSelf>, new()
    {
        public ISerializableRequest Clone() => (ISerializableRequest)MemberwiseClone();
    }

    public interface IRequest : ISignal
    {
        void Complete();
        void Cancel();
        void Raise(Action<RequestStatus> onComplete, float? timeout = 2f);
    }

    public interface IRequest<TRet> : IRequest
    {
        void Complete(TRet result);
        void Raise(Action<RequestStatus, TRet> onCompleted, float? timeout = 2f);
    }

    public interface ISerializableRequest<TRet> : IRequest<TRet> { }
    public interface ISerializableRequest : IRequest
    {
        ISerializableRequest Clone();
        TRet Clone<TRet>() where TRet : ISerializableRequest => (TRet)Clone();
    }
}