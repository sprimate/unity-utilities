#if UNITY_EDITOR
using UnityEditor;

namespace HitTrax.UnityUtilities
{
    public static class EditorUtilities
    {
        public static void AddSpaces(int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                EditorGUILayout.Space();
            }
        }
    }
}
#endif