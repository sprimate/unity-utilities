#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace HitTrax.UnityUtilities
{
    // These scripts were used to testing the player prefs utilities and ensure it was working as expected
    public class PlayerPrefsTest : MonoBehaviour { }

    [CustomEditor(typeof(PlayerPrefsTest))]
    public class PlayerPrefsTestEditor : Editor {

        static string _prefIntTest = "PrefIntTest";
        static string _prefFloatTest = "PrefFloatTest";
        static string _prefVectorTest = "PrefVectorTest";

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Add Random Int"))
            {
                PlayerPrefsUtilities.AddToPrefList(_prefIntTest, MathUtilities.RandVal(0, 100));
            }

            if (GUILayout.Button("Add Random Float"))
            {
                PlayerPrefsUtilities.AddToPrefList(_prefFloatTest, MathUtilities.RandVal(0f, 100f));
            }

            if (GUILayout.Button("Add Random Vector"))
            {
                int x = MathUtilities.RandVal(0, 100);
                int y = MathUtilities.RandVal(0, 100);
                int z = MathUtilities.RandVal(0, 100);

                PlayerPrefsUtilities.AddToPrefList(_prefVectorTest, new Vector3(x, y, z));
            }

            if (GUILayout.Button("Clear All"))
            {
                PlayerPrefsUtilities.ClearList(_prefIntTest);
                PlayerPrefsUtilities.ClearList(_prefFloatTest);
                PlayerPrefsUtilities.ClearList(_prefVectorTest);
            }

            int index = 0;
            GUILayout.Label("=== Int List === ");
            foreach (var val in PlayerPrefsUtilities.GetIntPrefsList(_prefIntTest))
            {
                GUILayout.Label(index.ToString() + " " + val.ToString());
                index++;    
            }

            index = 0;
            GUILayout.Label("=== Float List === ");
            foreach (var val in PlayerPrefsUtilities.GetFloatPrefsList(_prefFloatTest))
            {
                GUILayout.Label(index.ToString() + " " + val.ToString());
                index++;
            }

            index = 0;
            GUILayout.Label("=== Vector List === ");
            foreach (var val in PlayerPrefsUtilities.GetVector3PrefsList(_prefVectorTest))
            {
                GUILayout.Label(index.ToString() + " " + val.ToString());
                index++;
            }
        }

    }
}
#endif