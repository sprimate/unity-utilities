using System;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

namespace HitTrax.CoreUtilities.Tests
{
    // Singleton test classes and interfaces
    internal interface ISingletonService_v2 : ISingletonService_v1
    {
        public int RandomNum2 { get; set; }
    }

    internal interface ISingletonService_v1 : IService
    {
        public int RandomNum { get; set; }
    }

    internal class TestServiceSingletonWithInterfaces : ISingletonService_v2
    {
        public int RandomNum { get; set; } = -1;

        public int RandomNum2 { get; set; } = -1;
    }

    internal class TestServiceSingleton : IService
    {
        public int randomNum = -1;
    }

    // Instance test classes and interfaces
    internal interface IInstance_v2 : IInstance_v1
    {
        public int RandomNum2 { get; set; }
    }

    internal interface IInstance_v1 : IService
    {
        public int RandomNum { get; set; }
    }

    internal class TestServiceInstanceWithInterfaces : IInstance_v2
    {
        public int RandomNum { get; set; } = -1;

        public int RandomNum2 { get; set; } = -1;
    }

    internal class TestServiceInstance : IService
    {
        public int randomNum = -1;
    }

    public static class ServicesTester
    {
        public static void Run()
        {
            RunBasicSingletonTest();
            RunSingletonWithInterfacesTest();
            RunBasicInstancesTest();
            RunInstancesWithInterfacesTest();
        }

        private static void RunBasicSingletonTest()
        {
            // Make sure all the Gets return null since there's no singleton
            Assert.IsNull(Services.Get<TestServiceSingleton>());
            Assert.IsNull(Services.Get<TestServiceSingleton>(Guid.NewGuid()));
            Assert.IsNull(Services.GetAll<TestServiceSingleton>());

            // Generate a new singleton and register it
            var genericSingleton = new TestServiceSingleton {randomNum = Random.Range(0, 101)};
            Assert.IsTrue(Services.RegisterSingleton(genericSingleton));

            // Confirm all the Gets to make sure they all return the proper object
            Assert.IsNotNull(Services.Get<TestServiceSingleton>());
            Assert.IsNotNull(Services.Get<TestServiceSingleton>(Guid.NewGuid()));
            Assert.IsTrue(Services.GetAll<TestServiceSingleton>().Count > 0);

            // Fail to add the singleton again
            Assert.IsFalse(Services.RegisterSingleton(genericSingleton));

            // Unregister the singleton
            Assert.IsTrue(Services.UnregisterSingleton<TestServiceSingleton>());

            // Make sure all of the Gets return null now
            Assert.IsNull(Services.Get<TestServiceSingleton>());
            Assert.IsNull(Services.Get<TestServiceSingleton>(Guid.NewGuid()));
            Assert.IsNull(Services.GetAll<TestServiceSingleton>());
        }

        private static void RunSingletonWithInterfacesTest()
        {
            // Make sure all the Gets return null since there's no singleton
            Assert.IsNull(Services.Get<TestServiceSingletonWithInterfaces>());
            Assert.IsNull(Services.Get<TestServiceSingletonWithInterfaces>(Guid.NewGuid()));
            Assert.IsNull(Services.GetAll<TestServiceSingletonWithInterfaces>());

            Assert.IsNull(Services.Get<ISingletonService_v1>());
            Assert.IsNull(Services.Get<ISingletonService_v1>(Guid.NewGuid()));
            Assert.IsNull(Services.GetAll<ISingletonService_v1>());

            Assert.IsNull(Services.Get<ISingletonService_v2>());
            Assert.IsNull(Services.Get<ISingletonService_v2>(Guid.NewGuid()));
            Assert.IsNull(Services.GetAll<ISingletonService_v2>());

            // Generate a new singleton and register it
            ISingletonService_v2 interfaceSingleton = new TestServiceSingletonWithInterfaces();
            interfaceSingleton.RandomNum = Random.Range(0, 101);
            interfaceSingleton.RandomNum2 = Random.Range(101, 201);
            var ranNum1Test = interfaceSingleton.RandomNum;
            var ranNum2Test = interfaceSingleton.RandomNum2;
            Assert.IsTrue(Services.RegisterSingleton(interfaceSingleton));

            // Confirm all the Gets to make sure they all return the proper object
            Assert.IsNotNull(Services.Get<TestServiceSingletonWithInterfaces>());
            Assert.IsNotNull(Services.Get<TestServiceSingletonWithInterfaces>(Guid.NewGuid()));
            Assert.IsTrue(Services.GetAll<TestServiceSingletonWithInterfaces>().Count > 0);
            Assert.IsTrue(Services.Get<TestServiceSingletonWithInterfaces>().RandomNum == ranNum1Test);
            Assert.IsTrue(Services.Get<TestServiceSingletonWithInterfaces>().RandomNum2 == ranNum2Test);

            Assert.IsNotNull(Services.Get<ISingletonService_v1>());
            Assert.IsNotNull(Services.Get<ISingletonService_v1>(Guid.NewGuid()));
            Assert.IsTrue(Services.GetAll<ISingletonService_v1>().Count > 0);
            Assert.IsTrue(Services.Get<ISingletonService_v1>().RandomNum == ranNum1Test);

            Assert.IsNotNull(Services.Get<ISingletonService_v2>());
            Assert.IsNotNull(Services.Get<ISingletonService_v2>(Guid.NewGuid()));
            Assert.IsTrue(Services.GetAll<ISingletonService_v2>().Count > 0);
            Assert.IsTrue(Services.Get<ISingletonService_v2>().RandomNum == ranNum1Test);
            Assert.IsTrue(Services.Get<ISingletonService_v2>().RandomNum2 == ranNum2Test);

            // Fail to add the singleton again
            Assert.IsFalse(Services.RegisterSingleton(interfaceSingleton));

            // Fail to add a new instance of the object as a different interface version
            ISingletonService_v1 secondInstanceOfSingleton = new TestServiceSingletonWithInterfaces();
            secondInstanceOfSingleton.RandomNum = Random.Range(201, 301);
            Assert.IsFalse(Services.RegisterSingleton(secondInstanceOfSingleton));

            // Unregister the singleton
            Assert.IsTrue(Services.UnregisterSingleton<ISingletonService_v2>());
        }

        private static void RunBasicInstancesTest()
        {
            // Make sure all the Gets return null since there's no instance
            Assert.IsNull(Services.Get<TestServiceInstance>());
            Assert.IsNull(Services.Get<TestServiceInstance>(Guid.NewGuid()));
            Assert.IsNull(Services.GetAll<TestServiceInstance>());

            // Generate a new instance and register it
            var instance1 = new TestServiceInstance {randomNum = Random.Range(0, 101)};
            var randomNum1Test = instance1.randomNum;
            var instance1Result = Services.RegisterInstance(instance1);
            Assert.IsTrue(instance1Result.success);

            // Confirm all the Gets to make sure they all return the proper object
            Assert.IsNotNull(Services.Get<TestServiceInstance>());
            Assert.IsNotNull(Services.Get<TestServiceInstance>(instance1Result.identifier));
            Assert.IsTrue(Services.GetAll<TestServiceInstance>().Count == 1);
            Assert.IsTrue(Services.Get<TestServiceInstance>().randomNum == randomNum1Test);

            // Fail to add the same instance again
            Assert.IsFalse(Services.RegisterInstance(instance1).success);

            // Add a second instance
            var instance2 = new TestServiceInstance() {randomNum = Random.Range(101, 201)};
            var randomNum2Test = instance2.randomNum;
            var instance2Result = Services.RegisterInstance(instance2);
            Assert.IsTrue(instance2Result.success);

            // Confirm all the Gets to make sure they all return the proper object
            Assert.IsNotNull(Services.Get<TestServiceInstance>());
            Assert.IsNotNull(Services.Get<TestServiceInstance>(instance2Result.identifier));
            Assert.IsTrue(Services.GetAll<TestServiceInstance>().Count == 2);
            Assert.IsTrue(Services.Get<TestServiceInstance>().randomNum == randomNum1Test);
            Assert.IsTrue(Services.Get<TestServiceInstance>(instance2Result.identifier).randomNum == randomNum2Test);

            // Unregister the first instance
            Assert.IsTrue(Services.UnregisterInstance<TestServiceInstance>(instance1Result.identifier));

            // Confirm that the first instance is unregistered
            Assert.IsNull(Services.Get<TestServiceInstance>(instance1Result.identifier));
            Assert.IsNotNull(Services.Get<TestServiceInstance>(instance2Result.identifier));
            Assert.IsTrue(Services.Get<TestServiceInstance>().randomNum == randomNum2Test);
            Assert.IsTrue(Services.GetAll<TestServiceInstance>().Count == 1);

            // Unregister the second instance
            Assert.IsTrue(Services.UnregisterInstance<TestServiceInstance>(instance2Result.identifier));

            // Confirm that the second instance is unregistered
            Assert.IsNull(Services.Get<TestServiceInstance>(instance2Result.identifier));
            Assert.IsNull(Services.Get<TestServiceInstance>());
            Assert.IsNull(Services.GetAll<TestServiceInstance>());

            Assert.IsTrue(Services.RegisterInstance(instance1).success);
            Assert.IsTrue(Services.RegisterInstance(instance2).success);
            Assert.IsTrue(Services.UnregisterAllInstances<TestServiceInstance>());
        }

        private static void RunInstancesWithInterfacesTest()
        {
            // Make sure all the Gets return null since there's no instance
            Assert.IsNull(Services.Get<TestServiceInstanceWithInterfaces>());
            Assert.IsNull(Services.Get<TestServiceInstanceWithInterfaces>(Guid.NewGuid()));
            Assert.IsNull(Services.GetAll<TestServiceInstanceWithInterfaces>());

            Assert.IsNull(Services.Get<IInstance_v1>());
            Assert.IsNull(Services.Get<IInstance_v1>(Guid.NewGuid()));
            Assert.IsNull(Services.GetAll<IInstance_v1>());

            Assert.IsNull(Services.Get<IInstance_v2>());
            Assert.IsNull(Services.Get<IInstance_v2>(Guid.NewGuid()));
            Assert.IsNull(Services.GetAll<IInstance_v2>());

            // Generate a new instance and register it
            IInstance_v2 instance1 = new TestServiceInstanceWithInterfaces {RandomNum = Random.Range(0, 101), RandomNum2 = Random.Range(101, 201)};
            var firstRandomNum1Test = instance1.RandomNum;
            var firstRandomNum2Test = instance1.RandomNum2;
            var instance1Result = Services.RegisterInstance(instance1);
            Assert.IsTrue(instance1Result.success);

            // Confirm all the Gets to make sure they all return the proper object
            Assert.IsNotNull(Services.Get<TestServiceInstanceWithInterfaces>());
            Assert.IsNotNull(Services.Get<TestServiceInstanceWithInterfaces>(instance1Result.identifier));
            Assert.IsTrue(Services.GetAll<TestServiceInstanceWithInterfaces>().Count == 1);
            Assert.IsTrue(Services.Get<TestServiceInstanceWithInterfaces>().RandomNum == firstRandomNum1Test);
            Assert.IsTrue(Services.Get<TestServiceInstanceWithInterfaces>().RandomNum2 == firstRandomNum2Test);

            Assert.IsNotNull(Services.Get<IInstance_v1>());
            Assert.IsNotNull(Services.Get<IInstance_v1>(instance1Result.identifier));
            Assert.IsTrue(Services.GetAll<IInstance_v1>().Count == 1);
            Assert.IsTrue(Services.Get<IInstance_v1>().RandomNum == firstRandomNum1Test);
            Assert.IsTrue(Services.Get<IInstance_v1>(instance1Result.identifier).RandomNum == firstRandomNum1Test);

            Assert.IsNotNull(Services.Get<IInstance_v2>());
            Assert.IsNotNull(Services.Get<IInstance_v2>(instance1Result.identifier));
            Assert.IsTrue(Services.GetAll<IInstance_v2>().Count == 1);
            Assert.IsTrue(Services.Get<IInstance_v2>().RandomNum == firstRandomNum1Test);
            Assert.IsTrue(Services.Get<IInstance_v2>().RandomNum2 == firstRandomNum2Test);

            // Test that changing 1 instance changes all other references in the other dictionary keys (i.e. making sure we're not duplicating the object)
            instance1.RandomNum = 999;
            Assert.IsTrue(Services.Get<IInstance_v2>().RandomNum == 999);
            Assert.IsTrue(Services.Get<IInstance_v1>().RandomNum == 999);
            Assert.IsTrue(Services.Get<TestServiceInstanceWithInterfaces>().RandomNum == 999);

            // Fail to add the same instance again
            Assert.IsFalse(Services.RegisterInstance(instance1).success);

            // Add a second instance of the object as a different interface version
            IInstance_v1 instance2 = new TestServiceInstanceWithInterfaces() {RandomNum = Random.Range(201, 301), RandomNum2 = Random.Range(301, 401)};
            var secondRandomNum1Test = instance2.RandomNum;
            var secondRandomNum2Test = ((TestServiceInstanceWithInterfaces)instance2).RandomNum2;
            var instance2Result = Services.RegisterInstance(instance2);
            Assert.IsTrue(instance2Result.success);

            // Confirm all the Gets to make sure they all return the proper object
            Assert.IsNotNull(Services.Get<TestServiceInstanceWithInterfaces>(instance2Result.identifier));
            Assert.IsTrue(Services.GetAll<TestServiceInstanceWithInterfaces>().Count == 2);
            Assert.IsTrue(Services.Get<TestServiceInstanceWithInterfaces>(instance2Result.identifier).RandomNum == secondRandomNum1Test);
            Assert.IsTrue(Services.Get<TestServiceInstanceWithInterfaces>(instance2Result.identifier).RandomNum2 == secondRandomNum2Test);

            Assert.IsNotNull(Services.Get<IInstance_v1>());
            Assert.IsNotNull(Services.Get<IInstance_v1>(instance2Result.identifier));
            Assert.IsTrue(Services.GetAll<IInstance_v1>().Count == 2);
            Assert.IsTrue(Services.Get<IInstance_v1>(instance2Result.identifier).RandomNum == secondRandomNum1Test);

            Assert.IsNotNull(Services.Get<IInstance_v2>());
            Assert.IsNotNull(Services.Get<IInstance_v2>(instance2Result.identifier));
            Assert.IsTrue(Services.GetAll<IInstance_v2>().Count == 2);
            Assert.IsTrue(Services.Get<IInstance_v2>(instance2Result.identifier).RandomNum == secondRandomNum1Test);
            Assert.IsTrue(Services.Get<IInstance_v2>(instance2Result.identifier).RandomNum2 == secondRandomNum2Test);

            // Unregister the first instance
            Assert.IsTrue(Services.UnregisterInstance<TestServiceInstanceWithInterfaces>(instance1Result.identifier));

            // Confirm that the first instance is unregistered
            Assert.IsTrue(Services.Get<IInstance_v1>().RandomNum == secondRandomNum1Test); // Confirm that we're getting the second instance now as the default
            Assert.IsNull(Services.Get<TestServiceInstanceWithInterfaces>(instance1Result.identifier));
            Assert.IsNotNull(Services.Get<TestServiceInstanceWithInterfaces>(instance2Result.identifier));
            Assert.IsTrue(Services.GetAll<TestServiceInstanceWithInterfaces>().Count == 1);

            // Unregister the second instance
            Assert.IsTrue(Services.UnregisterInstance<TestServiceInstanceWithInterfaces>(instance2Result.identifier));

            // Confirm that the second instance is unregistered
            Assert.IsNull(Services.Get<TestServiceInstanceWithInterfaces>(instance2Result.identifier));
            Assert.IsNull(Services.Get<TestServiceInstanceWithInterfaces>());
            Assert.IsNull(Services.GetAll<TestServiceInstanceWithInterfaces>());

            Assert.IsTrue(Services.RegisterInstance(instance1).success);
            Assert.IsTrue(Services.RegisterInstance(instance2).success);
            Assert.IsTrue(Services.UnregisterAllInstances<IInstance_v2>());
        }
    }
}