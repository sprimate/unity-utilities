using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class TestRunner
{
#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Run All Tests")]
#endif
    public static void RunAllTests() //ToDo, allow this to handle editor coroutines, to test timing based things
    {
        // Get all assemblies
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        List<(Type type, MethodInfo method)> typesToTest = new List<(Type, MethodInfo)>();

        // Loop through each assembly
        foreach (var assembly in assemblies)
        {
            // Get all types in the assembly
            var types = assembly.GetTypes();
            // Loop through each type
            foreach (var type in types)
            {
                // Check if the type has the Testable attribute
                if (type.GetCustomAttributes(typeof(TestableAttribute), false).Length > 0)
                {
                    // Find the RunTests method
                    var method = type.GetMethod("RunTests", BindingFlags.Static | BindingFlags.Public);

                    // If the method exists, invoke it
                    if (method != null)
                    {
                        typesToTest.Add((type, method));
                    }
                    else
                    {
                        Debug.LogWarning($"<color=red>{type.Name}</color> does not have a static RunTests method");
                    }
                }
            }
        }

        Debug.Log("---Starting tests on " + typesToTest.Count + " types---");
        foreach (var toTest in typesToTest)
        {
            try
            {
                toTest.method.Invoke(null, null);
                Debug.Log($"<color=cyan>{toTest.type.Name}</color>: <color=green>Completed</color>");

            }
            catch (Exception e)
            {
                Debug.LogException(e);
                Debug.Log($"<color=cyan>{toTest.type.Name}</color>: <color=red>Failed</color>");
            }
        }

        Debug.Log("<color=magenta>All tests completed.</color>");
    }
}