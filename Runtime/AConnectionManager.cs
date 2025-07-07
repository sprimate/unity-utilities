using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AConnectionManager<T1, T2> : AMonoSingleton<T1> where T1 : AConnectionManager<T1, T2> where T2: AConnectionConfiguration
 {
    public bool autoFindConfiguration = true;
    //Whenever a new object is found, use the new configuration for the original instance
    public bool useNewestConfiguration = true;
    public bool shareMyConfiguration = true;
    public T2 configuration;

    public Action<T2> OnConfigurationSwap;
    protected override bool ShouldDestroyOnLoad => false;
    protected override void Awake()
    {
        if (autoFindConfiguration)
        {
            Reset();
        }

        if (hasInstance && instance != this && instance.useNewestConfiguration && shareMyConfiguration)
        { 
            if (instance.configuration != configuration)
            {
                //if destroying newer components but using their configurations, destroy the old configurations
                if (autoFindConfiguration && destroyNewerComponents)
                {
                    var configToRemove = instance.GetComponentInChildren<T2>();
                    Destroy(configToRemove);
                }

                instance.configuration = configuration;
                GenericCoroutineManager.RunAfterFrame(() =>
                {
                    instance.OnConfigurationSwap?.Invoke(configuration);
                }, this);
            }
            if (configuration != null && configuration.transform.IsChildOf(transform))
            {
                if (destroyNewerGameObjects)
                {
                    destroyNewerComponents = true;
                }
                destroyNewerGameObjects = false;
                Debug.Log("WARNING: Cannot destroy GameObject " + gameObject + ". The configuration is also attached to that object.");
            }
        }
        base.Awake();
    }

    protected virtual void Reset()
    {
        if (configuration == null)
        {
            configuration = GetComponentInChildren<T2>();
        }
    }
}

[Serializable]
public abstract class AConnectionConfiguration : MonoBehaviour
{

}