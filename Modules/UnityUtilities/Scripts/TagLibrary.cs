using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using HitTrax.CoreUtilities;

namespace HitTrax.UnityUtilities
{
    internal class TagLibrary : ScriptableObject
    {
        internal const string ASSET_PATH = "Assets/Editor/TagLibrary.asset";

        [SerializeField, Delayed, OnCollectionChanged(nameof(Sort)), OnValueChanged(nameof(Sort), includeChildren: true)]
        private List<string> _validTags = new();

        private static TagLibrary _instance;
        internal static TagLibrary Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

#if UNITY_EDITOR
                _instance = UnityEditor.AssetDatabase.LoadAssetAtPath<TagLibrary>(ASSET_PATH);

                if (_instance == null)
                {
                    _instance = CreateInstance<TagLibrary>();

                    string folder = System.IO.Path.GetDirectoryName(ASSET_PATH);
                    if (!UnityEditor.AssetDatabase.IsValidFolder(folder))
                    {
                        System.IO.Directory.CreateDirectory(folder);
                        UnityEditor.AssetDatabase.Refresh();
                    }

                    UnityEditor.AssetDatabase.CreateAsset(_instance, ASSET_PATH);
                    UnityEditor.AssetDatabase.SaveAssets();
                    UnityEditor.AssetDatabase.Refresh();
                }
#else
                _instance = CreateInstance<TagLibrary>();
#endif

                return _instance;
            }
        }


        private void Sort() => _validTags.Sort(StringComparer.OrdinalIgnoreCase);
        internal static IEnumerable<string> GetValidTags() => Instance._validTags.AsReadOnly();
        internal static bool ContainsTag(string tag) => Instance._validTags.Contains(tag);

        //Add a tag and ensure in alphabetical order to catch similar-but-technically-different tags
        internal static bool AddTag(string tag)
        {
            if(string.IsNullOrWhiteSpace(tag) || Instance._validTags.Contains(tag))
            {
                return false;
            }

            Instance._validTags.TryAddOnce(tag);
            Instance.Sort();

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(TagLibrary.Instance);
            UnityEditor.AssetDatabase.SaveAssets();
#endif

            return true;
        }
    }
}