using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;

namespace UnityEngine
{
    public static class GameObjectExt
    {
        public static void RemoveFromDontDestroyOnLoad(this GameObject go)
        {
            GameObject newGO = new GameObject();
            go.transform.SetParent(newGO.transform); // NO longer DontDestroyOnLoad();
            newGO.name = "ShouldDestroyOnLoad";
        }

        public static void DontDestroyOnLoad(this GameObject g)
        {
            g.transform.SetParent(null);
            UnityEngine.Object.DontDestroyOnLoad(g);
        }

        public static string GetHierarchyPath(this GameObject o)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.Append("/" + o.name);
            if (o.transform.parent != null)
            {
                Transform p = o.transform.parent;
                while (p != null)
                {
                    builder.Insert(0, "/" + p.name);
                    p = p.parent;
                }
            }
            return builder.ToString();
        }


        private static Dictionary<string, GameObject> _cache = new Dictionary<string, GameObject>();
        private static bool _sceneChangeSubscribed;
        public static GameObject Find(string name, bool debugLogError = true, Action<string> OnError = null)
        {
            if (!_sceneChangeSubscribed)
            {
                _sceneChangeSubscribed = true;
                SceneManager.sceneLoaded += FlushCache;
            }

            if (_cache.ContainsKey(name) && _cache[name] != null)
            {
                return _cache[name];
            }

            GameObject found = GameObject.Find(name);
            if (found == null)
            {
                if (debugLogError)
                {
                    Debug.LogErrorFormat("Could not find GameObject named '{0}'", name);
                }

                if (OnError != null)
                {
                    OnError.Invoke(name);
                }
            }
            else
            {
                if (!_cache.ContainsKey(name))
                {
                    _cache.Add(name, found);
                }

                _cache[name] = found;
            }

            return found;
        }

        private static void FlushCache(Scene arg0, LoadSceneMode arg1)
        {
            _cache = new Dictionary<string, GameObject>();
        }

        public static Transform FindTransform(
            string name, bool debugLogError = true, Action<string> OnError = null)
        {
            GameObject found = Find(name, debugLogError, OnError);
            if (found)
            {
                return found.transform;
            }
            else
            {
                return null;
            }
        }

        public static T FindOrCreateObjectOfType<T>() where T : MonoBehaviour
        {
            T t = GameObject.FindFirstObjectByType<T>();
            if (t == null)
            {
                var go = new GameObject("[" + typeof(T).ToString() + "]");
                t = go.AddComponent<T>();
            }

            return t;
        }

        /// <summary>
        /// Makes a copy of the Vector3 with changed x/y/z values, keeping all undefined values as they were before. Can be
        /// called with named parameters like vector.Change3(x: 5, z: 10), for example, only changing the x and z components.
        /// </summary>
        /// <param name="vector">The Vector3 to be copied with changed values.</param>
        /// <param name="x">If this is not null, the x component is set to this value.</param>
        /// <param name="y">If this is not null, the y component is set to this value.</param>
        /// <param name="z">If this is not null, the z component is set to this value.</param>
        /// <returns>A copy of the Vector3 with changed values.</returns>
        public static Vector3 Copy(this Vector3 vector, float? x = null, float? y = null, float? z = null)
        {
            if (x.HasValue)
            {
                vector.x = x.Value;
            }
            if (y.HasValue)
            {
                vector.y = y.Value;
            }
            if (z.HasValue)
            {
                vector.z = z.Value;
            }

            return vector;
        }

        /// <summary>
        /// Set a transform to a parent, but you can keep position, scale, or rotation the same
        /// </summary>  
        public static void SetParentPreseveTransform(this Transform transform, Transform parent, bool preservePosition = true, bool preserveRotation = true, bool preserveScale = true)
        {
            Vector3 beforePos = transform.position;
            Vector3 beforeEuler = transform.eulerAngles;
            Vector3 beforeScale = transform.lossyScale;
            transform.SetParent(parent);
            if (preservePosition)
            {
                transform.position = beforePos;
            }

            if (preserveRotation)
            {
                transform.eulerAngles = beforeEuler;
            }

            if (preserveScale)
            {
                transform.SetLossyScale(beforeScale);
            }
        }

        public static void SetLossyScale(this Transform transform, Vector3 lossyScale)
        {
            lossyScale = transform.lossyScale.Copy(lossyScale.x, lossyScale.y, lossyScale.z);

            transform.localScale = Vector3.one;
            transform.localScale = new Vector3(lossyScale.x / transform.lossyScale.x,
                                               lossyScale.y / transform.lossyScale.y,
                                               lossyScale.z / transform.lossyScale.z);
        }

        public static string GetFullPath(this Transform transform, bool includeSceneName = true)
        {
            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }
            if (includeSceneName)
            {
                return transform.gameObject.scene.name + "/" + path;
            }
            else
            {
                return path;
            }
        }

        public static T AddOrGetComponent<T>(this GameObject gameObject) where T : Component
        {
            T t = gameObject.GetComponent<T>();
            if (t == null)
            {
                t = gameObject.AddComponent<T>();
            }

            if (t == null)
            {
                Debug.LogError("Unable to add component of type " + typeof(T) + " to [" + gameObject + "]");
            }
            return t;
        }

        public static bool HasComponent<T>(this GameObject gameObject) where T : Component
        {
            return gameObject.GetComponent<T>() != null;
        }

        public static bool TryGetComponent<T>(this GameObject gameObject, out T component) where T : Component
        {
            component = gameObject.GetComponent<T>();
            return component != null;
        }

        public static T GetOrAddComponent<T>(this MonoBehaviour go) where T : Component
        {
            return go.gameObject.AddOrGetComponent<T>();
        }

        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            return go.AddOrGetComponent<T>();
        }

        public static T AddOrGetComponent<T>(this Transform transform) where T : Component
        {
            return transform.gameObject.AddOrGetComponent<T>();
        }

        public static T GetOrAddComponent<T>(this Transform transform) where T : Component
        {
            return transform.gameObject.AddOrGetComponent<T>();
        }

        public static void SetXSizeDelta(this RectTransform t, float x)
        {
            t.sizeDelta = new Vector2(x, t.sizeDelta.y);
        }
        public static void SetYSizeDelta(this RectTransform t, float y)
        {
            t.sizeDelta = new Vector2(t.sizeDelta.x, y);
        }

        public static Vector2 GetSizeDelta(this RectTransform t)
        {
            return t.sizeDelta;
        }

        public static RectTransform SetSizeDelta(this RectTransform t, Vector2 size)
        {
            t.SetXSizeDelta(size.x);
            t.SetYSizeDelta(size.y);
            return t;
        }

        public static Transform SetXLocalScale(this Transform t, float x)
        {
            Vector3 newScale = new Vector3(x, t.localScale.y, t.localScale.z);
            t.localScale = newScale;
            return t;

        }

        public static Transform SetYLocalScale(this Transform t, float y)
        {
            Vector3 newScale = new Vector3(t.localScale.x, y, t.localScale.z);
            t.localScale = newScale;
            return t;

        }

        public static Transform SetZLocalScale(this Transform t, float z)
        {
            Vector3 newScale = new Vector3(t.localScale.x, t.localScale.y, z);
            t.localScale = newScale;
            return t;

        }

        public static Transform SetXPosition(this Transform t, float x)
        {
            Vector3 newPosition = new Vector3(x, t.position.y, t.position.z);
            t.position = newPosition;
            return t;

        }

        public static Transform SetYPosition(this Transform t, float y)
        {
            Vector3 newPosition = new Vector3(t.position.x, y, t.position.z);
            t.position = newPosition;
            return t;

        }

        public static Transform SetPositionZ(this Transform t, float z)
        {
            Vector3 newPosition = new Vector3(t.position.x, t.position.y, z);
            t.position = newPosition;
            return t;
        }

        public static Transform SetLocalPositionX(this Transform t, float x)
        {
            Vector3 newPosition = new Vector3(x, t.localPosition.y, t.localPosition.z);
            t.localPosition = newPosition;
            return t;
        }

        public static Transform SetLocalPositionY(this Transform t, float y)
        {
            Vector3 newPosition = new Vector3(t.localPosition.x, y, t.localPosition.z);
            t.localPosition = newPosition;
            return t;
        }

        public static Transform SetLocalPositionZ(this Transform t, float z)
        {
            Vector3 newPosition = new Vector3(t.localPosition.x, t.localPosition.y, z);
            t.localPosition = newPosition;
            return t;
        }

        public static Transform SetEulerAnglesX(this Transform t, float x) => t.SetRotationX(x);
        public static Transform SetEulerAnglesY(this Transform t, float y) => t.SetRotationY(y);
        public static Transform SetEulerAnglesZ(this Transform t, float z) => t.SetRotationZ(z);

        public static Transform SetRotationX(this Transform t, float x)
        {
            Vector3 newEuler = new Vector3(x, t.eulerAngles.y, t.eulerAngles.z);
            t.eulerAngles = (newEuler);
            return t;
        }

        public static Transform SetRotationY(this Transform t, float y)
        {
            Vector3 newEuler = new Vector3(t.eulerAngles.x, y, t.eulerAngles.z);
            t.eulerAngles = newEuler;
            return t;

        }

        public static Transform SetRotationZ(this Transform t, float z)
        {
            Vector3 newEuler = new Vector3(t.eulerAngles.x, t.eulerAngles.y, z);
            t.eulerAngles = (newEuler);
            return t;

        }

        public static Transform SetLocalRotationX(this Transform t, float x)
        {
            Vector3 newEuler = new Vector3(x, t.localEulerAngles.y, t.localEulerAngles.z);
            t.localEulerAngles = (newEuler);
            return t;

        }

        public static Transform SetLocalRotationY(this Transform t, float y)
        {
            Vector3 newEuler = new Vector3(t.localEulerAngles.x, y, t.localEulerAngles.z);
            t.localEulerAngles = newEuler;
            return t;

        }

        public static Transform SetLocalRotationZ(this Transform t, float z)
        {
            Vector3 newEuler = new Vector3(t.localEulerAngles.x, t.localEulerAngles.y, z);
            t.localEulerAngles = (newEuler);
            return t;

        }

        public static Transform SetLocalScaleX(this Transform t, float x)
        {
            Vector3 newScale = new Vector3(x, t.localScale.y, t.localScale.z);
            t.localScale = (newScale);
            return t;

        }

        public static Transform SetLocalScaleY(this Transform t, float y)
        {
            Vector3 newScale = new Vector3(t.localScale.x, y, t.localScale.z);
            t.localScale = newScale;
            return t;

        }

        public static Transform SetLocalScaleZ(this Transform t, float z)
        {
            Vector3 newScale = new Vector3(t.localScale.x, t.localScale.y, z);
            t.localScale = (newScale);
            return t;
        }

        public static Transform SetLossyScaleX(this Transform t, float x)
        {
            t.SetLossyScale(new Vector3(x, t.lossyScale.y, t.lossyScale.z));
            return t;
        }
        public static Transform SetLossyScaleY(this Transform t, float y)
        {
            t.SetLossyScale(new Vector3(t.lossyScale.x, y, t.lossyScale.z));
            return t;
        }
        public static Transform SetLossyScaleZ(this Transform t, float z)
        {
            t.SetLossyScale(new Vector3(t.lossyScale.x, t.lossyScale.y, z));
            return t;
        }
        public static void SetDefaultValues(this Transform t)
        {
            Normalize(t);
        }

        public static void SetParentAndNormalize(this Transform t, Transform parent, bool updatePosition = true, bool updateRotation = true, bool updateScale = true)
        {
            t.SetParent(parent);
            t.Normalize(updatePosition, updateRotation, updateScale);
        }

        public static void Normalize(this Transform t, bool updatePosition = true, bool updateRotation = true, bool updateScale = true)
        {
            if (updateRotation)
            {
                t.localEulerAngles = Vector3.zero;
            }

            if (updatePosition)
            {
                t.localPosition = Vector3.zero;
            }

            if (updateScale)
            {
                t.localScale = Vector3.one;
            }
        }

        /// <summary>
        /// Sets the active state of all immediate children within the 
        /// specified game object.
        /// </summary>
        /// <param name="gameObject">Target object</param>
        /// <param name="active">Active state</param>
        public static void SetChildrenActive(this GameObject gameObject, bool active)
        {
            foreach (Transform child in gameObject.transform)
            {
                child.gameObject.SetActive(active);
            }
        }
    }

    public static class GameObjectLayerExt
    {
        static Dictionary<GameObject, int> _gameObjectDefaultLayers = new Dictionary<GameObject, int>();
        public static GameObject SetLayerRecursive(this GameObject gameObject, int layer)
        {
            if (gameObject == null)
            {
                return null;
            }
            gameObject.layer = layer;
            foreach (Transform t in gameObject.transform)
            {
                t.gameObject.SetLayerRecursive(layer);
            }

            return gameObject;
        }

        public static GameObject SetLayerRecursive(this GameObject gameObject, string layerName, params string[] ignoredGoNames)
        {
            return SetLayerRecursive(gameObject, layerName, ignoredGoNames != null ? new List<string>(ignoredGoNames) : null);
        }

        public static GameObject SetLayerRecursive(this GameObject gameObject, string layerName, List<string> ignoredGoNames = null)
        {
            if (gameObject == null)
            {
                return null;
            }

            int layer = LayerMask.NameToLayer(layerName);

            if (ignoredGoNames == null || !ignoredGoNames.Contains(gameObject.name))
            {
                if (!_gameObjectDefaultLayers.ContainsKey(gameObject))
                {
                    _gameObjectDefaultLayers.Add(gameObject, gameObject.layer);
                }
                gameObject.layer = layer;
            }

            foreach (Transform t in gameObject.transform)
            {
                t.gameObject.SetLayerRecursive(layerName, ignoredGoNames);
            }

            return gameObject;
        }

        public static void RestoreOriginalLayer(this GameObject gameObject)
        {
            if (_gameObjectDefaultLayers.ContainsKey(gameObject))
            {
                gameObject.layer = _gameObjectDefaultLayers[gameObject];
            }
        }

        public static void RestoreOriginalLayersRecursive(this GameObject gameObject)
        {
            gameObject.RestoreOriginalLayer();
            foreach (Transform t in gameObject.transform)
            {
                t.gameObject.RestoreOriginalLayersRecursive();
            }
        }
    }
}