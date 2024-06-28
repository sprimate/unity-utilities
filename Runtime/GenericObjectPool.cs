using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class GenericObjectPool<T> where T : Component
{
    ObjectPool<T> m_Pool;

    public ObjectPool<T> Pool
    {
        get
        {
            if (m_Pool == null)
            {
                m_Pool = new ObjectPool<T>(CreateFunction != null ? CreateFunction : CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, true);
            }
            return m_Pool;
        }
    }
    Action<T> OnCreate;
    Func<T> CreateFunction;

    public GenericObjectPool(Action<T> _onCreate)
    {
        OnCreate = _onCreate;
    }

    public GenericObjectPool(Func<T> _createFunction)
    {
        CreateFunction = _createFunction;
    }

    T CreatePooledItem()
    {
        var go = new GameObject();
        var ret = go.AddComponent<T>();
        OnCreate.SafeInvoke(ret);
        return ret;
    }

    // Called when an item is returned to the pool using Release
    void OnReturnedToPool(T system)
    {
        system.gameObject.SetActive(false);
    }

    // Called when an item is taken from the pool using Get
    void OnTakeFromPool(T system)
    {
        system.gameObject.SetActive(true);
    }

    // If the pool capacity is reached then any items returned will be destroyed.
    // We can control what the destroy behavior does, here we destroy the GameObject.
    void OnDestroyPoolObject(T system)
    {
        GameObject.Destroy(system.gameObject);
    }

    public T Get()
    {
        return Pool.Get();
    }

    public void Release(T element)
    {
        Pool.Release(element);
    }

}
