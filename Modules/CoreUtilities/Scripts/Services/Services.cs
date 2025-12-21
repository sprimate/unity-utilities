using System;
using System.Collections.Generic;
using UnityEngine;

namespace HitTrax.CoreUtilities
{
    public static class Services
    {
        private static readonly Dictionary<Type, ServiceContainer> _services = new();

        /// <summary>
        /// Register an object as a singleton service. This allows other objects to access your service as the root type
        /// or any of its derivative interface types.
        /// </summary>
        /// <param name="service">The object to register as a singleton service.</param>
        /// <returns>Returns true if the service was successfully registered as a singleton. Returns false if the service is already registered
        /// or if the object given was null. Note: A service already being registered could mean the same instance of the object is
        /// already registered OR another instance of the object.</returns>
        public static bool RegisterSingleton<T>(T service) where T : IService
        {
            if (service == null)
            {
                Debug.LogError("Cannot add null object as service");
                return false;
            }

            var type = service.GetType();

            if (_services.ContainsKey(type))
            {
                Debug.LogError($"Service already registered");
                return false;
            }

            var container = new ServiceContainer(type, true);
            container.RegisterSingleton(service);

            var allTypes = GetAllInterfacesAndCoreType(type);

            for (var i = 0; i < allTypes.Count; ++i)
            {
                _services.Add(allTypes[i], container);
            }

            return true;
        }

        /// <summary>
        /// Unregisters the object as a singleton service. This will not destroy the object but will simply free up the type to be registered
        /// as either a singleton or instance service in the future.
        /// </summary>
        /// <returns>Returns true if the service was successfully removed OR if there was no service to remove to begin with.
        /// Returns false if the service to be unregistered exists but is NOT a singleton (i.e. it's registered as an instance).</returns>
        public static bool UnregisterSingleton<T>() where T : IService
        {
            var type = typeof(T);

            if (_services.TryGetValue(type, out var container))
            {
                if (!container.IsSingleton)
                {
                    Debug.LogError("Cannot unregister a service that is flagged as an instance");
                    return false;
                }

                RemoveAllInterfacesAndCoreType(container);
            }

            return true;
        }

        /// <summary>
        /// Registers the object as an instance service. Instance services can have multiple instances of the same type and
        /// can return the entire list or a singular object if needed. To keep track of the instance, a unique identifier is given out for
        /// the registering instance to keep track of when it wants to unregister the instance in the future.
        /// </summary>
        /// <param name="service">The object to register as an instance service.</param>
        /// <returns>Returns a tuple that contains a boolean and a guid. The boolean returns true if the service has been successfully registered and
        /// added to the instance list for that service. Returns false if the service already exists as a singleton service (you cannot mix instance
        /// and singleton services together). Also returns false if the given instance is already registered or if the given object is null. The unique
        /// identifier is assigned to the specific instance and can be used to unregister this instance from the list of active instances under this service.
        /// If the result is false the guid returned will match Guid.Empty.</returns>
        public static (bool success, Guid identifier) RegisterInstance<T>(T service) where T : IService
        {
            if (service == null)
            {
                Debug.LogError("Cannot add null object as service");
                return (false, Guid.Empty);
            }

            var type = service.GetType();
            var resultGuid = Guid.Empty;

            if (_services.TryGetValue(type, out var container))
            {
                if (container.IsSingleton)
                {
                    Debug.LogError("Cannot register as an instance service because this service already exists as a singleton service");
                    return (false, Guid.Empty);
                }

                if (container.GetAll().Contains(service))
                {
                    Debug.LogError("Service already registered");
                    return (false, Guid.Empty);
                }

                resultGuid = container.RegisterInstance(service);
                return (true, resultGuid);
            }

            container = new ServiceContainer(type, false);
            var allTypes = GetAllInterfacesAndCoreType(type);
            resultGuid = container.RegisterInstance(service);

            for (var i = 0; i < allTypes.Count; ++i)
            {
                _services.Add(allTypes[i], container);
            }

            return (true, resultGuid);
        }

        /// <summary>
        /// Unregisters the object from the instance service. If no instances are left, the service will be removed from the list of services.
        /// This will not destroy the object but will simply free up the type to be registered as a singleton or instance service in the future.
        /// </summary>
        /// <param name="id">The unique identifier used to find the instance.</param>
        /// <returns>Returns true if the instance was successfully unregistered or if there was never any instance to unregister to begin with.
        /// Returns false if the service already exists as a singleton service (you cannot mix instance and singleton services together).
        /// Also returns false if the given unique identifier is empty.</returns>
        public static bool UnregisterInstance<T>(Guid id) where T : IService
        {
            if (id == Guid.Empty)
            {
                Debug.LogError("The provided identifier is empty");
                return false;
            }

            var type = typeof(T);

            if (_services.TryGetValue(type, out var container))
            {
                if (container.IsSingleton)
                {
                    Debug.LogError("Cannot unregister a service that is flagged as a singleton");
                    return false;
                }

                container.UnregisterInstance(id);

                if (container.Get() == null)
                {
                    RemoveAllInterfacesAndCoreType(container);
                }
            }

            return true;
        }

        /// <summary>
        /// Unregisters all instances of a type from the service. This will not destroy the object but will simply free up the type to be registered
        /// in the future as either a singleton service or an instance service.
        /// </summary>
        /// <returns>Returns true if the service has been successfully unregistered or if there was nothing to unregister to begin with.
        /// Returns false if the given type is already registered as a singleton service.</returns>
        public static bool UnregisterAllInstances<T>() where T : IService
        {
            var type = typeof(T);

            if (_services.TryGetValue(type, out var container))
            {
                if (container.IsSingleton)
                {
                    Debug.LogError("Cannot unregister a service that is flagged as a singleton");
                    return false;
                }

                container.UnregisterAllInstances();
                RemoveAllInterfacesAndCoreType(container);
            }

            return true;
        }

        /// <summary>
        /// Gets the reference to the service associated with the given type.
        /// </summary>
        /// <returns>Returns the singleton object if it exists and the service is a singleton, or returns the first instance of the instance service list
        /// if it exists. Returns null if nothing can be found.</returns>
        public static T Get<T>() where T : class
        {
            var type = typeof(T);

            if (_services.TryGetValue(type, out var container))
            {
                return container.Get() as T;
            }

            return null;
        }

        /// <summary>
        /// Gets the reference to the service associated with the given type and unique identifier. If the service is a singleton service, it will just return
        /// the singleton object.
        /// </summary>
        /// <param name="identifier">The unique identifier used to find the desired instance object.</param>
        /// <returns>Returns the singleton object if it exists and the service is a singleton, or returns the desired instance of the instance service list
        /// if it exists. Returns null if nothing can be found.</returns>
        public static T Get<T>(Guid identifier) where T : class
        {
            if (identifier == Guid.Empty)
            {
                Debug.LogError("The provided identifier is empty");
                return null;
            }

            var type = typeof(T);

            if (_services.TryGetValue(type, out var container))
            {
                return container.GetById(identifier) as T;
            }

            return null;
        }

        /// <summary>
        /// Gets the reference to all instances of the service associated with the given type, or gets the only reference to the singleton object if
        /// the service is flagged as a singleton service.
        /// </summary>
        /// <returns>Returns a list of all instances of the given service, or a list with a single item in it that is a reference to the singleton
        /// object if it is flagged as a singleton service. Returns null if nothing can be found.</returns>
        public static List<T> GetAll<T>() where T : class
        {
            var type = typeof(T);

            if (_services.TryGetValue(type, out var container))
            {
                return container.GetAll<T>();
            }

            return null;
        }

        private static List<Type> GetAllInterfacesAndCoreType(Type desiredType)
        {
            if (desiredType == null)
            {
                return new List<Type>();
            }

            var serviceType = typeof(IService);
            var allTypes = new List<Type> {desiredType};
            var interfaces = desiredType.GetInterfaces();

            for (var i = 0; i < interfaces.Length; ++i)
            {
                if (serviceType.IsAssignableFrom(interfaces[i]) && interfaces[i] != serviceType)
                {
                    allTypes.Add(interfaces[i]);
                }
            }

            return allTypes;
        }

        private static void RemoveAllInterfacesAndCoreType(ServiceContainer container)
        {
            var allTypes = GetAllInterfacesAndCoreType(container.CoreType);

            for (var i = 0; i < allTypes.Count; ++i)
            {
                _services.Remove(allTypes[i]);
            }
        }
    }
}