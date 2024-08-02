using System;
using UnityEngine;
#if UNITY_EDITOR
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
#endif

namespace OdinExtensions
{
    /// <summary>
    ///     Allows for foldout element to remain and put any inline properties next to it
    /// </summary>
    public class InlinePartialAttribute : Attribute
    {
        public readonly bool ExpandToVertical;

        /// <summary>
        ///     Allows for foldout element to remain and put any inline properties next to it
        /// </summary>
        /// <param name="expandToVertical">Should all items be put in horizontal or vertical orientation</param>
        public InlinePartialAttribute(bool expandToVertical = false)
        {
            ExpandToVertical = expandToVertical;
        }
    }
    
    /// <summary>
    ///     Should given field that is showed as inline, be also duplicated into foldout menu
    /// </summary>
    public class ShowInFoldoutEditorAttribute : Attribute
    {
        /// <summary>
        ///    Should given field that is showed as inline, be also duplicated into foldout menu
        /// </summary>
        public ShowInFoldoutEditorAttribute()
        {
        }
    }

#if UNITY_EDITOR
    [DrawerPriority(0, 95)]
    public class InlinePartialAttributeDrawer : OdinAttributeDrawer<InlinePartialAttribute>
    {
        private bool _expanded;

        protected override void DrawPropertyLayout(GUIContent label)
        {
            EditorGUILayout.BeginHorizontal();
            
            _expanded = SirenixEditorGUI.Foldout(_expanded, label);
            
            if (Attribute.ExpandToVertical)
            {
                EditorGUILayout.BeginVertical();
            }

            foreach (var child in Property.Children)
            {
                if (child.GetAttribute<InlinePropertyAttribute>() != null)
                {
                    child.Draw();
                }
            }

            if (Attribute.ExpandToVertical)
            {
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndHorizontal();
            if (_expanded)
            {
                EditorGUI.indentLevel++;
                foreach (var child in Property.Children)
                {
                    if (child.GetAttribute<InlinePropertyAttribute>() == null || child.GetAttribute<ShowInFoldoutEditorAttribute>() != null)
                    {
                        child.Draw();
                    }
                }

                EditorGUI.indentLevel--;
            }
        }
    }
#endif
}