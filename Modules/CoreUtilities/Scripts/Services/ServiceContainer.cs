using System;
using System.Collections.Generic;
using System.Linq;

namespace HitTrax.CoreUtilities
{
    public struct ServiceContainer
    {
        private object _singleton;
        private Dictionary<Guid, object> _instances;

        internal bool IsSingleton { get; }
        internal Type CoreType { get; }

        public ServiceContainer(Type coreType, bool singleton)
        {
            _singleton = null;
            _instances = null;
            IsSingleton = singleton;
            CoreType = coreType;
        }

        internal void RegisterSingleton(object service)
        {
            _singleton = service;
        }

        internal Guid RegisterInstance(object service)
        {
            _instances ??= new Dictionary<Guid, object>();

            if (_instances.ContainsValue(service))
            {
                return _instances.FirstOrDefault(x => x.Value == service).Key;
            }

            var guid = Guid.NewGuid();
            _instances.Add(guid, service);
            return guid;
        }

        internal void UnregisterInstance(Guid guid)
        {
            if (_instances != null && _instances.ContainsKey(guid))
            {
                _instances.Remove(guid);
            }
        }

        internal void UnregisterAllInstances()
        {
            _instances?.Clear();
        }

        internal object Get()
        {
            if (IsSingleton)
            {
                return _singleton;
            }

            if (_instances != null && _instances.Count > 0)
            {
                return _instances.FirstOrDefault().Value;
            }

            return null;
        }

        internal object GetById(Guid identifier)
        {
            if (IsSingleton)
            {
                return _singleton;
            }

            if (_instances != null && _instances.TryGetValue(identifier, out var id))
            {
                return id;
            }

            return null;
        }

        internal List<object> GetAll()
        {
            if (IsSingleton)
            {
                return new List<object> {_singleton};
            }

            return _instances != null ? _instances.Values.ToList() : new List<object>();
        }

        internal List<T> GetAll<T>() where T : class
        {
            if (IsSingleton)
            {
                return new List<T> {_singleton as T};
            }

            if (_instances != null)
            {
                var list = new List<T>();

                foreach (var instance in _instances.Values)
                {
                    if (instance is T t)
                    {
                        list.Add(t);
                    }
                }

                return list;
            }

            return new List<T>();
        }
    }
}