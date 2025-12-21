using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using HitTrax.UnityUtilities;

/// <summary>
/// Mono singleton Class. Extend this class to make singleton component.
/// Example: 
/// <code>
/// public class Foo : MonoSingleton<Foo>
/// </code>. To get the instance of Foo class, use <code>Foo.instance</code>
/// Override <code>Init()</code> method instead of using <code>Awake()</code>
/// from this class.
/// </summary>
public abstract class AMonoSingleton<T> : MonoBehaviour where T : AMonoSingleton<T>
{
    public static bool HasInstance
    {
        get
        {
            return Instance != null;
        }
    }

    public bool IsActiveInstance
    {
        get
        {
            return this != null && Instance == this;
        }
    }

    public static bool IsDynamicInstance
    {
        private set; get;
    }

    public static bool IsInitialized
    {
        private set; get;
    }

    protected virtual bool ShouldCreateIfNull => true;

    protected virtual bool ShouldDestroyOnLoad => true;

    protected virtual bool ShouldWarnOfExistingInstance => true;


    private static Action<T> _onInstance_Internal;
    public static Action<T> OnInstance
    {
        set
        {
            if (HasInstance)
            {
                value?.Invoke(Instance);
                _onInstance_Internal = null;
            }
            else
            {
                _onInstance_Internal = value;
                UnityAsyncOperations.RunAfterFrame(() =>
                {
                    _onInstance_Internal = OnInstance;
                });
            }
        }

        get
        {
            return OnInstance;
        }
    }

    public static T Instance
    {
        get
        {
            if (AMonoSingletonInternal.ApplicationExiting || !Application.isPlaying)
            {
                return null;
            }

            if (_instance_m == null)
            {
                _instance_m = AMonoSingletonInternal.GetInstance<T>(true) as T;
            }

            // Object not found, we create a dynamic one
            if (_instance_m == null && !_typesNotToCreate.Contains(typeof(T)) && !typeof(T).IsAbstract)
            {
                _instance_m = new GameObject(typeof(T).ToString(), typeof(T)).GetComponent<T>();

                // Problem during the creation, this should not happen
                if (_instance_m == null)
                {
                    Debug.LogError("Problem during the creation of " + typeof(T).ToString());
                }
                else
                {
                    if (!_instance_m.ShouldCreateIfNull)
                    {
                        Debug.LogError("Singleton [" + _instance_m.GetType() + "] could not be found, and is scripted not to be created programatically. Do you need to add an instance to this scene?");
                        _typesNotToCreate.Add(_instance_m.GetType());
                        DestroyImmediate(_instance_m.gameObject);
                        _instance_m = null;
                    }
                    {
                        IsDynamicInstance = _instance_m.ShouldDestroyOnLoad;
                        if (IsDynamicInstance)
                        {
                            _instance_m.gameObject.name = $"<Lazy> [{_instance_m.gameObject.name}]";
                        }
                        else
                        {
                            _instance_m.gameObject.name = $"[{_instance_m}]";
                        }
                    }
                }
            }

            if (!IsInitialized && _instance_m != null)
            {
                _instance_m.Initialize();
            }

            return _instance_m;
        }
    }

    private static T _instance_m;

    private static HashSet<Type> _typesNotToCreate = new HashSet<Type>();


    /// <summary>
    /// Destroy the entire game object the MonoSingleton is attached to
    /// </summary>
    [SerializeField] protected bool _destroyNewerGameObjects = false;

    /// <summary>
    /// Destroy only the component of the MonoSingleton
    /// </summary>
    [SerializeField] protected bool _destroyNewerComponents = false;


    /// <summary>
    /// This should only be used if you want more tha one instance of this object (NO LONGER A TRUE SINGLETON).
    /// The older object will always be referenced as the "instance" if the older object has "destroyNewerDuplicateGameObjects" checked
    /// This should be false in most cases. If we're thinking about mnaking this true, rethink if this object should be a singleton
    /// </summary>
    [SerializeField] private bool _neverDestroyThisInstance;



    // If no other monobehaviour request the instance in an awake function
    // executing before this one, no need to search the object.
    protected virtual void Awake()
    {
        if (Instance == null)
        {
            _instance_m = this as T;
        }
        else if (Instance != this)
        {
            Type type = GetType();
            var msg = "";
            if (ShouldDestroyOnLoad && ShouldWarnOfExistingInstance)
            {
                msg = "Another instance of " + type +
                    " already exists (perhaps it persisted from a different scene via " +
                    "DontDestroyOnLoad or HideFlags.DontSave?) - Destroying self.";
            }

            /*if (Instance.ShouldDestroyOnLoad)
            {
                // assign the new scene as the instance
                Debug.Log($"Switching to new instance for {type}");
                _instance_m = this as T;
                return;
            }*/

            msg += $" Name: {gameObject.name} type: {type}";
            gameObject.name += $"[MonoSingleton {type} Destroyed]";

            UnityEngine.Object context = null;
            if (Instance._destroyNewerComponents)
            {
                if (_neverDestroyThisInstance)
                {
                    msg += " Not Destroying duplicate object [" + gameObject + "], because it is marked as persistent";
                }
                else
                {
                    msg += " Destroying duplicate component. ID: " + GetInstanceID();
                    DestroyThisComponentImmediately();
                }
            }
            else if (Instance._destroyNewerGameObjects)
            {
                if (_neverDestroyThisInstance)
                {
                    msg += " Not Destroying duplicate object [" + gameObject + "], because it is marked as persistent";
                }
                else
                {
                    msg += " Destroying duplicate gameObject id: " + gameObject.GetInstanceID();
                    DestroyImmediate(gameObject);
                }
            }
            else
            {
                msg += " Destroying duplicate instance id: " + this.GetInstanceID();
                context = gameObject;
                DestroyThisComponentImmediately();
            }

            if (ShouldWarnOfExistingInstance)
            {
                Debug.Log(msg, context);
            }

            return;
        }
        if (!IsInitialized)
        {
            Initialize();
        }
    }

    protected virtual void DestroyThisComponentImmediately()
    {
        DestroyImmediate(this);
    }

    private void Initialize()
    {
        if (!ShouldDestroyOnLoad)
        {
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }

        IsInitialized = true;
    }

    /// Make sure the instance isn't referenced anymore when the user quit, just in case.
    private void OnApplicationQuit()
    {
        _instance_m = null;
        AMonoSingletonInternal.ApplicationExiting = true;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            _instance_m = null;
        }
    }
}

/// <summary>
/// This can be used to retrieve instances of singletons that haven't been explicitly defined because C#'s single inheritance structure
/// It's basically a shortcut to stop duplicating "Instance" or "FindObjectOfType" code for each object that inherits from a non-MonoSingleton
/// </summary>
#if UNITY_EDITOR
[UnityEditor.InitializeOnLoadAttribute]
#endif
internal static class AMonoSingletonInternal
{
    internal static bool ApplicationExiting;
    private static Dictionary<Type, object> _instances = new Dictionary<Type, object>();
    internal static T GetInstance<T>(bool suppressErrorMessage = false) where T : MonoBehaviour
    {
        var type = typeof(T);

        T thisInstance = null;
        if (_instances.ContainsKey(type))
        {
            thisInstance = _instances[type] as T;
        }
        // Instance requiered for the first time, we look for it
        if (thisInstance == null)
        {
            thisInstance = GameObject.FindFirstObjectByType(typeof(T)) as T;
            if (thisInstance != null)
            {
                _instances[typeof(T)] = thisInstance;
            }
        }
        if (thisInstance == null && !suppressErrorMessage)
        {
            Debug.LogError("Cannot find an instance of type [" + type + "] in scene.");
        }

        return thisInstance;
    }

#if UNITY_EDITOR
    static AMonoSingletonInternal()
    {
        //If domain not reloaded, statics don't get reset
        UnityEditor.EditorApplication.playModeStateChanged += (state) =>
        {
            if (state == UnityEditor.PlayModeStateChange.EnteredPlayMode || state == UnityEditor.PlayModeStateChange.EnteredEditMode)
            {
                ApplicationExiting = false;
            }
        };
    }

#endif

}