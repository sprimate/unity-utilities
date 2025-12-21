using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class TestRunner
{
#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Run All Tests")]
#endif
    public static async Task RunAllTests() //ToDo, allow this to handle editor coroutines, to test timing based things
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
                    var method = type.GetMethod("RunTests", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

                    // If the method exists, invoke it
                    if (method != null)
                    {
                        typesToTest.Add((type, method));
                    }
                    else
                    {
                        Debug.LogWarning($"<color=red>{type.Name}</color> does not have a static [RunTests] method");
                    }
                }
            }
        }

        Debug.Log("---Starting tests on " + typesToTest.Count + " types---");
        foreach (var toTest in typesToTest)
        {
            try
            {
#if UNITY_EDITOR
                var returnValue = toTest.method.Invoke(null, null);
                try
                {
                    if (returnValue is Task t)// is System.Runtime.CompilerServices.INotifyCompletion)//either it's null, or it's the awaiter, or it'll have been synchronously in the invoke
                    {
                        await t;
                    }
                    else if (returnValue is UniTask ut)
                    {
                        await ut;
                    }

                    //todo - do this with reflection on all things with a GetAwaiter function
                }

                catch { /*Debug.LogError(e);*/ }//doesn't matter, we just won't await. It's already running
#else
                toTest.method.Invoke(null, null);
#endif
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

    private static bool IsAwaitable(object obj)
    {
        if (obj == null)
        {
            return false;
        }
        var type = obj.GetType();
        return type.GetMethod("GetAwaiter", BindingFlags.Public | BindingFlags.Instance) != null;
    }

#if UNITY_EDITOR
public class UnsafeReferenceFinder
{
    /*
    [UnityEditor.MenuItem("Tools/Find Unsafe Assembly References")]
    public static void FindUnsafeRefs()
    {
        var scriptAssemblyPath = System.IO.Path.Combine(UnityEngine.Application.dataPath, "../Library/ScriptAssemblies");
        foreach (var file in System.IO.Directory.GetFiles(scriptAssemblyPath, "*.dll"))
        {
            try
            {
                var resolver = new Mono.Cecil.DefaultAssemblyResolver();
                resolver.AddSearchDirectory(scriptAssemblyPath);
                var readerParams = new Mono.Cecil.ReaderParameters { AssemblyResolver = resolver };

                var asm = Mono.Cecil.AssemblyDefinition.ReadAssembly(file, readerParams);
                foreach (var reference in asm.MainModule.AssemblyReferences)
                {
                    if (reference.Name == "System.Runtime.CompilerServices.Unsafe") 
                    {
                        Debug.Log($"'{System.IO.Path.GetFileName(file)}' references System.Runtime.CompilerServices.Unsafe");
                    }
                }
            }
            catch
            {
                Debug.LogWarning($"Could not read {file}");
            }
        }
    }
    */
}
#endif

}