using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Serialization;

/// <summary>
/// This can be used to retrieve instances of singletons that haven't been explicitly defined because C#'s single inheritance structure
/// It's basically a shortcut to stop duplicating "Instance" or "FindObjectOfType" code for each object that inherits from a non-MonoSingleton
/// </summary>
#if UNITY_EDITOR
[UnityEditor.InitializeOnLoadAttribute]
#endif
public static class AMonoSingleton
{
    public static bool applicationExiting;

    static Dictionary<Type, object> instances = new Dictionary<Type, object>();
    public static T GetInstance<T>(bool suppressErrorMessage = false) where T : MonoBehaviour
    {
        var type = typeof(T);

        T thisInstance = null;
        if (instances.ContainsKey(type))
        {
            thisInstance = instances[type] as T;
        }
        // Instance requiered for the first time, we look for it
        if (thisInstance == null)
        {
            thisInstance = GameObject.FindObjectOfType(typeof(T)) as T;
            if (thisInstance != null)
            {
                instances[typeof(T)] = thisInstance;
            }
        }
        if (thisInstance == null && !suppressErrorMessage)
        {
            Debug.LogError("Cannot find an instance of type [" + type + "] in scene.");
        }
        return thisInstance;
    }

#if UNITY_EDITOR
    static AMonoSingleton()
    {
        //If domain not reloaded, statics don't get reset
        UnityEditor.EditorApplication.playModeStateChanged += (state) =>
        {
            if (state == UnityEditor.PlayModeStateChange.EnteredPlayMode || state == UnityEditor.PlayModeStateChange.EnteredEditMode)
            {
                applicationExiting = false;
            }
        };
    }

#endif

}
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
    protected virtual bool ShouldCreateIfNull { get => true; }

    protected virtual bool ShouldDestroyOnLoad { get => !(destroyNewerComponents || destroyNewerGameObjects); }

    protected virtual bool ShouldWarnOfExistingInstance { get => true; }

    //private static T m_Instance = null;

    /// <summary>
    /// Destroy the entire game object the MonoSingleton is attached to
    /// </summary>
    /// 
    [FormerlySerializedAs("destroyDuplicateGameObject")]
    [SerializeField] protected bool destroyNewerGameObjects = false;

    /// <summary>
    /// Destroy only the component of the MonoSingleton
    /// </summary>
    [SerializeField] protected bool destroyNewerComponents = false;


    /// <summary>
    /// This should only be used if you want more tha one instance of this object (NO LONGER A TRUE SINGLETON).
    /// The older object will always be referenced as the "instance" if the older object has "destroyNewerDuplicateGameObjects" checked
    /// This should be false in most cases. If we're thinking about mnaking this true, rethink if this object should be a singleton
    /// </summary>
    [SerializeField] bool neverDestroyThisInstance;

    static HashSet<Type> typesNotToCreate = new HashSet<Type>();

    private static T m_Instance;

    //If it doesn't have an instance, try the same call next frame

    static Action<T> _onInstance;
    public static Action<T> OnInstance
    {
        set
        {
            if (hasInstance)
            {
                value?.Invoke(instance);
                _onInstance = null;
            }
            else
            {
                _onInstance = value;
                GenericCoroutineManager.instance.RunOnNextUpdate(() =>
                {
                    OnInstance = _onInstance;
                }, null);
            }
        }

        get
        {
            return _onInstance;
        }
    }

    public static T instance
    {
        get
        {
            if (AMonoSingleton.applicationExiting || !Application.isPlaying)
            {
                return null;
            }

            if (m_Instance == null)
            {
                m_Instance = AMonoSingleton.GetInstance<T>(true) as T;
            }

            // Object not found, we create a temporary one
            if (m_Instance == null && !typesNotToCreate.Contains(typeof(T)) && !typeof(T).IsAbstract)
            {
                //Debug.Log("WARNING: No instance of " + typeof(T).ToString() + ", a temporary one is created.");

                m_Instance = new GameObject(typeof(T).ToString(), typeof(T)).GetComponent<T>();

                // Problem during the creation, this should not happen
                if (m_Instance == null)
                {
                    Debug.LogError("Problem during the creation of " + typeof(T).ToString());
                }
                else
                {
                    if (!m_Instance.ShouldCreateIfNull)
                    {
                        Debug.LogError("Singleton [" + m_Instance.GetType() + "] could not be found, and is scripted not to be created programatically. Do you need to add an instance to this scene?");
                        typesNotToCreate.Add(m_Instance.GetType());
                        DestroyImmediate(m_Instance.gameObject);
                        m_Instance = null;
                    }
                    {
                        isTemporaryInstance = m_Instance.ShouldDestroyOnLoad;
                        if (isTemporaryInstance)
                        {
                            m_Instance.gameObject.name = "[Temp instance of " + m_Instance.gameObject.name + "]";
                        }
                        else
                        {
                            m_Instance.gameObject.name = "[" + m_Instance + "]";
                        }
                    }
                }
            }

            if (!_isInitialized && m_Instance != null)
            {
                m_Instance.Initialize();
            }
            return m_Instance;
        }
    }

    public static bool hasInstance
    {
        get
        {
            return m_Instance != null;
        }
    }

    public bool isActiveInstance
    {
        get
        {
            return this != null && instance == this;
        }
    }

    public static bool isTemporaryInstance { private set; get; }

    public static bool _isInitialized { private set; get; }

    // If no other monobehaviour request the instance in an awake function
    // executing before this one, no need to search the object.
    protected virtual void Awake()
    {
        if (m_Instance == null)
        {
            m_Instance = this as T;
        }
        else if (m_Instance != this)
        {
            var msg = "";
            if (ShouldDestroyOnLoad && ShouldWarnOfExistingInstance)
            {
                msg = "Another instance of " + GetType() +
                    " already exists (perhaps it persisted from a different scene via " +
                    "DontDestroyOnLoad or HideFlags.DontSave?) - Destroying self.";
            }

            if (m_Instance.ShouldDestroyOnLoad)
            {
                // assign the new scene as the instance
                Debug.Log("Switching to new instance for " + GetType());
                m_Instance = this as T;
                return;
            }

            msg += " Name: " + gameObject.name + " type:  " + GetType();
            gameObject.name += "[MonoSingleton " + GetType() + " Destroyed]";

            UnityEngine.Object context = null;
            if (instance.destroyNewerComponents)
            {
                if (neverDestroyThisInstance)
                {
                    msg += " Not Destroying duplicate object [" + gameObject + "], because it is marked as persistent";
                }
                else
                {
                    msg += " Destroying duplicate component. ID: " + GetInstanceID();
                    DestroyThisComponentImmediately();
                }
            }
            else if (instance.destroyNewerGameObjects)
            {
                if (neverDestroyThisInstance)
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
        if (!_isInitialized)
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

        _isInitialized = true;

        InitSingleton();
    }

    /// <summary>
    /// This function is called when the instance is used the first time
    /// Put all the initializations you need here, as you would do in Awake
    /// </summary>
    public virtual void InitSingleton() { }

    /// Make sure the instance isn't referenced anymore when the user quit, just in case.
    private void OnApplicationQuit()
    {
        m_Instance = null;
        AMonoSingleton.applicationExiting = true;
    }

    private void OnDestroy()
    {
        if (m_Instance == this)
        {
            m_Instance = null;
        }
    }
}