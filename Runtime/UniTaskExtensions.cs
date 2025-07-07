using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class UniTaskCoroutineRunner : AMonoSingleton<UniTaskCoroutineRunner> { }

public static class UniTaskExtensions
{
    //alias methods to match the same verbage as our coroutine helpers
    public static UniTask ToUniTask(this Task task) => task.AsUniTask();
    public static UniTask<T> ToUniTask<T>(this Task<T> task) => task.AsUniTask();

    /// <summary>
    /// Converts an IEnumerator coroutine to a UniTask without direct cancellation support (but still canceled if the coroutineRunner is destroyed.
    /// </summary>
    public static UniTask ToUniTask(this IEnumerator enumerator, MonoBehaviour coroutineRunner = null) => ToCancelableUniTask(enumerator, coroutineRunner).task;

    /// <summary>
    /// Converts an IEnumerator coroutine to a UniTask with cancellation support.
    /// Returns a (UniTask, Action) tuple where the caller can cancel the task.
    /// </summary>
    public static (UniTask task, Action Cancel) ToCancelableUniTask(this IEnumerator enumerator, MonoBehaviour coroutineRunner = null)
    {
        if (coroutineRunner == null)
        {
            coroutineRunner = UniTaskCoroutineRunner.instance;
        }

        var source = AutoResetUniTaskCompletionSource.Create();
        Coroutine outerCoroutine = null;
        Coroutine innerCoroutine = null;
        bool completed = false;
        bool canceled = false;
        IEnumerator Core(IEnumerator inner, AutoResetUniTaskCompletionSource source)
        {
            innerCoroutine = coroutineRunner.StartCoroutine(inner);
            yield return innerCoroutine;
            innerCoroutine = outerCoroutine = null;
            completed = true;
            source.TrySetResult();
        }

        //outerCoroutine is responsible for keeping track of the completion of the ACTUAL coroutine you want to run
        outerCoroutine = coroutineRunner.StartCoroutine(Core(enumerator, source));

        // Attach cancellation to Unity’s DestroyCancellationToken
        Action cancelAction = () =>
        {
            if (!completed && !canceled)
            {
                if (innerCoroutine != null)
                {
                    coroutineRunner.StopCoroutine(innerCoroutine);
                    innerCoroutine = null;
                }

                if (outerCoroutine != null)
                {
                    coroutineRunner.StopCoroutine(outerCoroutine);
                    outerCoroutine = null;
                }

                canceled = source.TrySetCanceled();
            }
            else if (canceled)
            {
                //Debug.LogError("Can't cancel this coroutine: This cancelation has already been processed");
            }
            else if (completed)
            {
                //Debug.Log("Already Completed: cant cancel");
            }
        };

        //If the coroutine Runner is destroyed, assume the object was canceled
        coroutineRunner.destroyCancellationToken.Register(cancelAction);
        return (source.Task, cancelAction);
    }
    public static UniTask AsUniTask(this IEnumerator enumerator) => enumerator.ToUniTask();
    public static (UniTask task, Action cancel) AsCancelableUniTask(this IEnumerator enumerator, MonoBehaviour coroutineRunner = null) => enumerator.ToCancelableUniTask(coroutineRunner);
    public static void AsCallback<T>(this UniTask<T> task, Action<T> callback) => task.ContinueWith(callback);
    public static void AsCallback(this UniTask task, Action callback) => task.ContinueWith(callback);
}