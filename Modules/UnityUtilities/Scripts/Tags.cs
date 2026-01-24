using UnityEngine;
using UnityEngine.SceneManagement;
using HitTrax.CoreUtilities;
using static HitTrax.CoreUtilities.SafeFunctions;
using System.Collections.Generic;
using System.Linq;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace HitTrax.UnityUtilities
{
    /// <summary>
    /// A Monobehaviour that allows the developer to define a GameObject
    /// as having multiple tags by string.
    /// This way, we can retreive the same object by different categories
    /// instead of just the limitation of one unity tag.
    /// </summary>

    public class Tags : MonoBehaviour
#if ODIN_INSPECTOR
    , ISelfValidator
#endif
    {
#if ODIN_INSPECTOR
        [ValueDropdown(nameof(GetValidTags))]
#else
        [SerializeField]
#endif
        public List<string> tags = new();

        void OnEnable()
        {
            TagManager.Add(this);
        }

        void OnDisable()
        {
            TagManager.Remove(this);
        }

        public bool IsInTagList(string tag) => tags.Contains(tag);

        ///////VALIDATIONS/////////
        private IEnumerable<string> GetValidTags() => TagLibrary.GetValidTags();

#if ODIN_INSPECTOR
        public void Validate(SelfValidationResult result)
        {
            var validTags = GetValidTags();
            var sizeBefore = tags.Count;
            tags = tags.Distinct().ToList();//no duplicates
            if (sizeBefore < tags.Count)
            {
                Debug.Log($"Removed {tags.Count - sizeBefore} duplicates", this);
            }

            foreach (var invalidTag in tags.Where(tag => !validTags.Contains(tag)))
            {
                result.AddWarning($"Potential Invalid Tag: [{invalidTag}]");
            }
        }
#endif

#if UNITY_EDITOR
#if ODIN_INSPECTOR
        [SerializeField, CustomValueDrawer(nameof(CheckSubmitNewTag))]
#else
        [SerializeField]
#endif
        private string _newTag;

        private string CheckSubmitNewTag(string value, GUIContent label)
        {
            if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Return)
            {
                AddNewTag();
            }

            return UnityEditor.EditorGUI.TextField(UnityEditor.EditorGUILayout.GetControlRect(), label, value);
        }

#if ODIN_INSPECTOR
        [Button("Add", Icon = SdfIconType.Plus, Style = ButtonStyle.FoldoutButton, Expanded = true)]
#endif
        private void AddNewTag()
        {
            if (string.IsNullOrWhiteSpace(_newTag))
            {
                return;
            }

            if (TagLibrary.ContainsTag(_newTag))
            {
                tags.TryAddOnce(_newTag);
                _newTag = string.Empty;
            }
            else
            {
                if (UnityEditor.EditorUtility.DisplayDialog(
                    title: "Confirm Tag Addition",
                    message: $"The tag [{_newTag}] isn't in the TagLibrary. Would you like to add it?",
                    ok: "Yes",
                    cancel: "Cancel")
                )
                {
                    TagLibrary.AddTag(_newTag);
                    tags.TryAddOnce(_newTag);
                    _newTag = string.Empty;
                }
            }
        }

#if ODIN_INSPECTOR
        [Button, PropertyOrder(-100)]
#endif
        private void ViewTagLibrary()
        {
            UnityEditor.Selection.activeObject = TagLibrary.Instance;
            UnityEditor.EditorGUIUtility.PingObject(TagLibrary.Instance);
        }
#endif
    }

    public static class TagManager
    {
        private static Dictionary<string, HashSet<Tags>> _taggedObjectsByTag = new();

        /// <summary>
        /// Check to see if an object is tagged with either Unity's built in system or with the Tags Monobehavior.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>

        public static bool IsTagged(this GameObject gameObject)
            => gameObject.GetTags().Length > 0;

        /// <summary>
        /// /// Check to see if an object is tagged with either Unity's built in system or with the Tags Monobehavior.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="component"></param>
        /// <returns></returns>

        public static bool IsTagged<T>(this T component) where T : Component
            => component.GetTags().Length > 0;

        /// <summary>
        /// Is this object of the tag type?
        /// </summary>
        /// <remarks>
        /// This extends off of any component as a convenience.
        /// </remarks>

        public static bool IsTag<T>(this T component, string tag) where T : Component
            => component.tag == tag || component.IsTagInCache(tag);

        /// <summary>
        /// Is this object of the tag type?
        /// </summary>       

        public static bool IsTag(this GameObject gameObject, string tag)
            => gameObject.tag == tag || gameObject.transform.IsTagInCache(tag);

        /// <summary>
        /// Does this object have any of the following tags?
        /// </summary>
        /// <remarks>
        /// This extends off of any component as a convenience.
        /// </remarks>       


        public static bool HasAnyTag<T>(this T component, params string[] tags) where T : Component
            => tags.Any(tag => component.IsTag(tag));

        /// <summary>
        /// Does this object have any of the following tags?
        /// </summary>
        /// <remarks>
        /// This extends off of any component as a convenience.
        /// </remarks>       

        public static bool HasAnyTag<T>(this T component, IEnumerable<string> tags) where T : Component
            => tags.Any(tag => component.IsTag(tag));

        /// <summary>
        /// Does this object have any of the following tags?
        /// </summary>

        public static bool HasAnyTag(this GameObject gameObject, params string[] tags)
            => gameObject.transform.HasAnyTag(tags);

        /// <summary>
        /// Does this object have any of the following tags?
        /// </summary>

        public static bool HasAnyTag(this GameObject gameObject, IEnumerable<string> tags)
            => gameObject.transform.HasAnyTag(tags);

        /// <summary>
        /// Does this object have all of the following tags?
        /// </summary>

        public static bool HasAllTags<T>(this T component, params string[] tags) where T : Component
            => tags.All(tag => component.IsTag(tag));

        /// <summary>
        /// Does this object have all of the following tags?
        /// </summary>

        public static bool HasAllTags<T>(this T component, IEnumerable<string> tags) where T : Component
            => tags.All(tag => component.IsTag(tag));

        /// <summary>
        /// Does this object have all of the following tags?
        /// </summary>

        public static bool HasAllTags(this GameObject gameObject, params string[] tags)
            => tags.All(tag => gameObject.IsTag(tag));

        /// <summary>
        /// Does this object have all of the following tags?
        /// </summary>


        public static bool HasAllTags<T>(this GameObject gameObject, IEnumerable<string> tags) where T : Component
           => tags.All(tag => gameObject.IsTag(tag));

        /// <summary>
        /// Find the first family member (child, parent, sibling) that has all tags
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        /// <remarks>
        /// There is currently no logic behind which family member returns first, so this may need to be reworked to return the closes relative first.
        /// </remarks>

        public static Safe<GameObject> FirstFamilyMemberWithAllTags(this Safe<GameObject> gameObject, params string[] tags)
            => gameObject.Select(go => go.FirstFamilyMemberWithAllTags(tags));

        /// <summary>
        /// Find the first family member (child, parent, sibling) that has all tags
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        /// <remarks>
        /// There is currently no logic behind which family member returns first, so this may need to be reworked to return the closes relative first.        
        /// </remarks>

        public static Safe<GameObject> FirstFamilyMemberWithAllTags(this GameObject gameObject, params string[] tags)
            => gameObject
                    .AllFamilyMemberWithAllTags(tags)
                    .Select(tags => tags.First().Safe().Select(tag => tag.gameObject))
                    ;


        /// <summary>
        /// Get all family members of all of the selected tags
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tags"></param>
        /// <returns></returns>

        public static Safe<IEnumerable<Tags>> AllFamilyMemberWithAllTags(this Safe<GameObject> gameObject, params string[] tags)
            => gameObject.Select(go => go.AllFamilyMemberWithAllTags(tags));

        /// <summary>
        /// Get all family members of all of the selected tags
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tags"></param>
        /// <returns></returns>

        public static Safe<IEnumerable<Tags>> AllFamilyMemberWithAllTags(this GameObject gameObject, params string[] tags)
           => gameObject
                   .transform
                   .AllInTheFamily<Tags>()
                   .Where(tag => tag.HasAllTags(tags))
                   .Safe()
                   ;



        /// <summary>
        /// Get all objects of the tag type
        /// </summary>

        public static GameObject[] GetObjectsByTag(this string tag)
            => _taggedObjectsByTag
                .TryGet(tag)
                .SelectOut(ToGameObjects, () => new GameObject[0]);

        public static GameObject[] GetObjectsByTag(this string tag, Scene requiredScene)
            => tag
                .GetObjectsByTag()
                .Where(obj => obj.scene.name == requiredScene.name)
                .ToArray();

        public static Safe<GameObject> GetFirstObjectByTag(this Safe<string> tag, Safe<Scene> requiredScene = default)
            => tag.Select(t => t.GetFirstObjectByTag(requiredScene));

        public static Safe<GameObject> GetFirstObjectByTag(this string tag, Safe<Scene> requiredScene = default)
        {
            var objs = requiredScene.SelectOut(scene => tag.GetObjectsByTag(scene), () => tag.GetObjectsByTag());
            if (objs.Length == 0)
            {
                $"Attempting get tag {tag} but it's not found".LogWarning();
                return None;
            }

            return objs[0];
        }

        public static Safe<Vector3> GetPositionByTag(this string tag)
            => tag.GetFirstObjectByTag()
                  .Select(go => go.transform.position);

        /// <summary>
        /// Get all of the object's tags, including Unity's built in tag feature.
        /// </summary>

        public static string[] GetTags<T>(this T component) where T : Component
        {
            const string untagged = "Untagged";
            Tags tagComponent = component.GetComponent<Tags>();

            if (tagComponent == null)
            {
                return (component.tag != untagged) ? new string[] { component.tag } : new string[0];
            }
            else
            {
                return (component.tag != untagged) ? tagComponent.tags.Append(component.tag).ToArray() : tagComponent.tags.ToArray();
            }
        }

        /// <summary>
        /// Get all of the object's tags, including Unity's built in tag feature.
        /// </summary>

        public static string[] GetTags(this GameObject gameObject)
           => gameObject.transform.GetTags();

        internal static void Add(Tags taggedObject)
        {
            foreach (var tag in taggedObject.tags)
            {
                Add(tag, taggedObject);
            }
        }

        internal static void Remove(Tags taggedObject)
        {
            foreach (var tag in taggedObject.tags)
            {
                Remove(tag, taggedObject);
            }
        }

        private static void Add(string key, Tags taggedObject)
        {
            if (!_taggedObjectsByTag.ContainsKey(key))
            {
                _taggedObjectsByTag.Add(key, new HashSet<Tags>());
            }

            _taggedObjectsByTag[key].Add(taggedObject);
        }

        private static bool Remove(string key, Tags taggedObject)
        {
            if (_taggedObjectsByTag.TryGetValue(key, out var result))
            {
                return result.Remove(taggedObject);
            }

            return false;
        }

        private static GameObject[] ToGameObjects(HashSet<Tags> tags)
            => tags.Select(t => t.gameObject)
                   .ToArray();

        private static bool IsTagInCache<T>(this T component, string tag) where T : Component
            => component.GetTags().Contains(tag);

        public static Safe<GameObject> SetPositionByTag(this Safe<GameObject> go, string tag)
            => go.Select(g => g.SetPositionByTag(tag));

        public static GameObject SetPositionByTag(this GameObject go, string tag)
        {
            go.transform.SetPos(tag.GetPositionByTag());
            return go;
        }
    }
}
