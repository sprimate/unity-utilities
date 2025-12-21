
using HitTrax.CoreUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static HitTrax.CoreUtilities.SafeFunctions;

namespace HitTrax.UnityUtilities
{
    public static class UnityComponentUtilities
    {

        public static Safe<T> MaybeInChild<T>(this Safe<GameObject> parent, bool inclusive = false) where T : Component
            => parent.Select(p => p.MaybeInChild<T>());

        public static Safe<T> MaybeInChild<T>(this GameObject parent, bool inclusive = false) where T : Component
            => parent != null ? parent.transform.MaybeInChild<T>(inclusive) : None;

        public static Safe<R> MaybeInChild<T, R>(this Safe<T> obj, bool inclusive = false) where R : Component where T : Component
           => SafeFunctions.Select(obj, c => c.MaybeInChild<R>(false));

        public static Safe<T> MaybeInChild<T>(this Component obj, bool inclusive = false)
        {
            if (inclusive && obj.TryGetComponent(out T comp))
            {
                return comp;
            }

            var child = obj.GetComponentInChildren<T>();
            return (child != null) ? child : None;
        }

        public static T GetComponentInChildrenOnly<T>(this Component c) where T : Component
        {
            foreach (Transform child in c.transform)
            {
                var component = child.GetComponentInChildren<T>(); // Recursive
                if (component != null)
                {
                    return component;
                }
            }
            return null;
        }

        public static Safe<T> MaybeComponent<T>(this Safe<GameObject> gameObject) where T : Component
            => gameObject.Select(go => go.MaybeComponent<T>());

        public static Safe<T> MaybeComponent<T>(this Safe<Component> comp) where T : Component
            => comp.Select(c => c.MaybeComponent<T>());
        
        public static Safe<T> MaybeComponent<T>(this GameObject gameObject) where T : Component
            => gameObject.transform.MaybeComponent<T>();

        public static Safe<T> MaybeComponent<T>(this Component obj, Func<T, bool> predicate)
            => obj.MaybeComponent<T>()
                  .Select<T, T>(item => (predicate(item)) ? item : None);
        public static Safe<T> MaybeComponent<T>(this Component obj)
            => obj.TryGetComponent<T>(out T comp) ? comp : None;

        public static Safe<R> MaybeComponent<T, R>(this Safe<T> obj) where T : Component
            => SafeFunctions.Select(obj, o => o.MaybeComponent<R>());

        public static Safe<R> MaybeComponent<T, R>(this T obj) where T : Component
            => obj.IfComponent<T, R, Safe<R>>(item => item, () => None);
        public static bool HasComponent<C>(this Component comp) => comp.GetComponent<C>() != null;

        public static TRet IfComponent<Orig, Check, TRet>(this Orig obj, Func<Check, TRet> ifTrue, Func<TRet> ifFalse) where Orig : Component
            => obj.TryGetComponent(out Check chk) ? ifTrue(chk) : ifFalse();


        public static Safe<GameObject> MaybeGameObject<T>(this Safe<T> component) where T : Component
            => component.Select(c => c.gameObject);
        
        public static T AddOrGetComponent<T>(this MonoBehaviour monoBehaviour) where T : Component
        {
            T component = monoBehaviour.GetComponent<T>();
            return (component == null) ? monoBehaviour.gameObject.AddComponent<T>() : component;
        }

        // Basically Add or Get
        public static T AddOrGetComponent<T>(this Transform transform) where T : Component
            => transform.MaybeComponent<T>()
                        .Unbox(ifNone: () => transform.gameObject.AddComponent<T>());

        static public Safe<Transform> MaybeParent(this Safe<Component> comp)
            => SafeFunctions.Select(comp, c => c.MaybeParent());
        static private Safe<Transform> MaybeParent(this Component comp)
            => (comp != null && comp.transform.parent != null) ? comp.transform.parent : None;

        static public Safe<T> MaybeInParent<T>(this Component comp) where T : Component
            => comp.MaybeParent()
                   .Select(parent => parent.MaybeComponent<T>()); 

        static public Safe<T> MaybeInAncestors<T>(this Component comp, bool inclusive) where T : Component
            => (inclusive) ? comp.MaybeComponent<T>().IfNone(() => comp.MaybeInAncestors<T>()) :
                             comp.MaybeInAncestors<T>();

        static public Safe<T> MaybeInAncestors<T>(this Component comp)
            => SafeFunctions.Select(comp.MaybeParent(), parent => parent.MaybeInAncestors<T>());

        private static Safe<T> MaybeInAncestors<T>(this Transform parent)
            => parent.MaybeComponent<T>()
                     .IfNone(() => parent.MaybeInAncestors<T>());

        public static Safe<T> MaybeInFamily<T>(this Component comp, bool inclusive)
            => comp.MaybeInChild<T>()
                   .IfNone(() => comp.MaybeAncestorOrSelf<T>(inclusive));

        public static Safe<T> MaybeInFamily<T>(this Safe<GameObject> parent, bool inclusive) where T : Component
            => parent.Select(gameObject => gameObject.transform.MaybeInFamily<T>(inclusive));

        private static Safe<T> MaybeAncestorOrSelf<T>(this Component comp, bool inclusive)
            => comp.MaybeInAncestors<T>()
                   .SelectOut(ifSome: c => c,
                              ifNone: () => (inclusive) ? comp.MaybeComponent<T>() : None);


        public static List<T> AllDecendants<T>(this Transform transform) where T : Component
        {
            List<T> components = new List<T>();

            // Check if the current transform has the component
            T component = transform.GetComponent<T>();
            if (component != null)
            {
                components.Add(component);
            }

            // Recursively search in the children and grandchildren
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                components.AddRange(child.AllDecendants<T>());
            }

            return components;
        }

        static public IEnumerable<T> AllInTheFamily<T>(this Component comp) where T : Component
            => comp.AllInTheFamily()
                   .Where(relative => relative.HasComponent<T>())
                   .Select(relative => relative.GetComponent<T>());

        static public Transform OldestAncestor(this Component comp)
            => comp.transform.parent == null ? comp.transform : comp.transform.parent.OldestAncestor();

        static public IEnumerable<Transform> AllInTheFamily(this Component comp)
            => comp.OldestAncestor()
                   .GetAllChildrenAndDecendents(true);


        static public List<T> AllAncestors<T>(this Component comp, bool inclusive) where T : Component
        => inclusive && comp.GetComponent<T>() != null ?
            comp.AllAncestors<T>().AddTo(comp.GetComponent<T>()) :
            comp.AllAncestors<T>();

        
        static public List<T> AllAncestors<T>(this Component comp) where T : Component
        => comp.AllAncestors(new List<Transform>())
                .Where(t => t.GetComponent<T>() != null)
                .Select(t => t.GetComponent<T>())
                .ToList();

        static List<Transform> AllAncestors(this Component comp, List<Transform> items)
            => comp.transform.parent != null ? // Base Case            
               comp.transform.parent.AllAncestors(items.AddTo(comp.transform.parent)) : // Recursion
               items; // Exit Recursion


        public static IEnumerable<T> GetAllChildrenAndDecendents<T>(this Component c) where T : Component
        {
            foreach (var child in GetAllChildrenAndDecendents(c.transform))
            {
                T component;
                if (child.TryGetComponent<T>(out component))
                {
                    yield return component;
                }
            }
        }

        public static IEnumerable<T> GetAllChildren<T>(this Safe<Transform> transform) where T : Component
            => transform.SelectOut(t => t.GetAllChildrenAndDecendents<T>(), () => new List<T>());

        public static IEnumerable<Transform> GetAllChildrenAndDecendents(this Transform transform, bool inclusive = false)
        {
            if (inclusive)
            {
                yield return transform;
            }

            foreach (Transform child in transform)
            {
                // Return the child, then return the children's children
                yield return child;
                foreach (Transform grandChild in GetAllChildrenAndDecendents(child))
                {
                    yield return grandChild;
                }
            }
        }

        public static Safe<GameObject> ChildTo(this Safe<GameObject> obj, Transform parent)
            => obj.Select(o => o.transform.ChildTo(parent).gameObject);

        public static Safe<T> ChildTo<T>(this Safe<T> comp, Transform parent) where T : Component
            => comp.IfSome(t => t.ChildTo(parent));
        public static Safe<C> ChildTo<C>(this Safe<C> comp, Safe<Transform> parent) where C : Component
            => SafeFunctions.Select(parent, p => comp.ChildTo(p));

        public static T ChildTo<T>(this T t, Transform parent) where T : Component
        {
            t.transform.SetParent(parent);
            return t;
        }

        public static T SetSibling<T>(this T comp, Transform sibling) where T : Component
            => comp.ChildTo(sibling.parent);

        static public T SetPosAndRot<T>(this T comp, Transform t) where T : Component
            => comp.SetPos(t.position)
                   .SetRot(t.rotation);

        static public T SetPosX<T>(this T comp, float val) where T : Component
            => comp.SetPos(comp.transform.position.InsertX(val));

        static public T SetPosY<T>(this T comp, float val) where T : Component
            => comp.SetPos(comp.transform.position.InsertY(val));
        static public T SetPosZ<T>(this T comp, float val) where T : Component
            => comp.SetPos(comp.transform.position.InsertZ(val));

        static public T SetLocPosX<T>(this T comp, float val) where T : Component
            => comp.SetLocPos(comp.transform.localPosition.InsertX(val));
        static public T SetLocPosY<T>(this T comp, float val) where T : Component
            => comp.SetLocPos(comp.transform.localPosition.InsertY(val));
        static public T SetLocPosZ<T>(this T comp, float val) where T : Component
            => comp.SetLocPos(comp.transform.localPosition.InsertZ(val));

        static public T SetScale<T>(this T self, Safe<Vector3> scale) where T : Component
        {
            scale.IfSome(s => self.SetScale(s));
            return self;
        }

        static public T SetScale<T>(this T self, Vector3 scale) where T : Component
        {
            self.transform.localScale = scale;
            return self;
        }

        static public Safe<GameObject> SetScale(this GameObject obj, Safe<Vector3> scale)
            => obj.IfSome(o => scale.IfSome(v3 => o.transform.localScale = v3));

        static public Safe<GameObject> SetPos(this Safe<GameObject> gameObject, Safe<Vector3> pos)
            => gameObject.IfSome(go => pos.IfSome(p => go.SetPos(p)));

        static public GameObject SetPos(this GameObject gameObject, Vector3 pos)
        {
            gameObject.transform.position = pos;
            return gameObject;
        }

        static public GameObject SetPos(this GameObject gameObject, Safe<Vector3> pos)
            => gameObject != null ? gameObject.transform.SetPos(pos).gameObject : gameObject;
            
        static public Safe<T> SetPos<T>(this Safe<T> self, Safe<Vector3> pos) where T : Component
            => self.SelectOut(comp => comp.SetPos(pos), () => new Safe<T>());
        static public Safe<T> SetPos<T>(this Safe<T> comp, Vector3 pos) where T : Component
            => comp.SelectOut(c => c.SetPos(pos), () => new Safe<T>());

        static public T SetPos<T>(this T self, Safe<Vector3> pos) where T : Component
        {
            self.transform.position = pos.SelectOut(p => p, () => self.transform.position);
            return self;
        }
        static public T SetPos<T>(this T self, Vector3 pos) where T : Component
        {
            self.transform.position = pos;
            return self;
        }

        static public Safe<GameObject> SetRot(this Safe<GameObject> self, Safe<Quaternion> rot)
            => self.IfSome(go => rot.IfSome(rot => go.SetRot(rot)));

        static public GameObject SetRot(this GameObject self, Safe<Quaternion> rot)
        {
            self.transform.rotation = rot.Unbox(() => Quaternion.identity);
            return self;
        }

        static public GameObject SetRot(this GameObject self, Quaternion rot)
        {
            self.transform.rotation = rot;
            return self;
        }

        static public T SetRot<T>(this T self, Safe<Quaternion> rot) where T : Component
        {
            self.transform.rotation = rot.SelectOut(r => r, () => self.transform.rotation);
            return self;
        }

        static public T SetLocalRot<T>(this T self, Safe<Quaternion> rot) where T : Component
        {
            self.transform.localRotation = rot.SelectOut(r => r, () => self.transform.localRotation);
            return self;
        }

        static public GameObject SetLocalRot(this GameObject self, Safe<Quaternion> rot)
        {           
            self.transform.localRotation = rot.Unbox(() => Quaternion.identity);
            return self;
        }

        static public T SetLocPos<T>(this T comp, Vector3 pos) where T : Component
        {
            comp.transform.localPosition = pos;
            return comp;
        }

        static public Safe<T> SetRot<T>(this Safe<T> comp, Quaternion rot) where T : Component
            => comp.SelectOut(c => c.SetRot(rot), () => new Safe<T>());
        
        static public T SetRot<T>(this T self, Quaternion rot) where T : Component
        {
            self.transform.rotation = rot;
            return self;
        }

        static public T SetLocRot<T>(this T comp, Quaternion rot) where T : Component
        {
            comp.transform.localRotation = rot;
            return comp;
        }

        static public GameObject SetName(this GameObject obj, string name)
        {
            obj.name = name;
            return obj;
        }

        static public void DestroyAllObjects<T>(this IEnumerable<T> items) where T : Component
            => items.ToList()
                    .ForEach(item => GameObject.Destroy(item.gameObject));

        static public IEnumerable<T> ActivateAllObjects<T>(this IEnumerable<T> items) where T : Component
        {
            if (items == null)
            {
                return Enumerable.Empty<T>();
            }

            foreach (var item in items)
            {
                item.ActivateObject();
            }
            return items;
        }

        static public IEnumerable<T> DeactivateAllObjects<T>(this IEnumerable<T> items) where T : Component
        {
            if (items == null)
            {
                return Enumerable.Empty<T>();
            }

            foreach (var item in items)
            {
                item.DeactivateObject();
            }
            return items;
        }

        static public Safe<GameObject> DeactivateObject(this Safe<GameObject> gameObject)
            => gameObject.Select(DeactivateObject);

        static public GameObject DeactivateObject(this GameObject gameObject)
            => gameObject == null ? null : gameObject.transform.DeactivateObject().gameObject;

        public static IEnumerable<GameObject> DeactivateObjects(this IEnumerable<Safe<GameObject>> gameObjects)
            => gameObjects
                .Where(obj => obj.HasValue)
                .Select(obj => obj.UnboxRaw())
                .DeactivateObjects();

        public static IEnumerable<GameObject> DeactivateObjects(this IEnumerable<GameObject> gameObjects)
        {
            foreach(var gameObject in gameObjects)
            {
                gameObject.DeactivateObject();
            }
            return gameObjects;
        }

        public static GameObject ActivateObject(this GameObject gameObject)
            => gameObject == null ? null : gameObject.transform.ActivateObject().gameObject;

        public static IEnumerable<GameObject> ActivateObjects(this IEnumerable<Safe<GameObject>> gameObjects)
            => gameObjects
                .Where(obj => obj.HasValue)
                .Select(obj => obj.UnboxRaw())
                .ActivateObjects();

        public static IEnumerable<GameObject> ActivateObjects(this IEnumerable<GameObject> gameObjects)
        {
            foreach (var gameObject in gameObjects)
            {
                gameObject.ActivateObject();
            }
            return gameObjects;
        }


        public static T ActivateObject<T>(this T self) where T : Component
        {
            if (self != null)
            {
                self.gameObject.SetActive(true); 
            }
            
            return self;
        }

        public static T DeactivateObject<T>(this T self) where T : Component
        {
            if (self != null)
            {
                self.gameObject.SetActive(false);
            }
            
            return self;
        }

        public static Safe<T> DeactivateObject<T>(this Safe<T> self) where T : Component
            => self.IfSome(obj => obj.DeactivateObject());

        public static Safe<GameObject> ActivateObject(this Safe<GameObject> self)
            => self.Select(obj => obj.ActivateObject());

        public static Safe<T> ActivateObject<T>(this Safe<T> self) where T : Component
            => self.IfSome(obj => obj.ActivateObject());

        public static Safe<T> EnableComponent<T>(this Safe<T> self) where T : MonoBehaviour
            => self.Select(component =>
            {
                component.enabled = true;
                return component;
            });

        public static Safe<T> DisableComponent<T>(this Safe<T> self) where T : MonoBehaviour
            => self.Select(component =>
            {
                component.enabled = false;
                return component;
            });

        public static Safe<T> EnableCollider<T>(this Safe<T> self) where T : Collider
            => self.Select(collider =>
            {
                collider.enabled = true;
                return collider;
            });

        public static Safe<T> DisableCollider<T>(this Safe<T> self) where T : Collider
            => self.Select(collider =>
            {
                collider.enabled = false;
                return collider;
            });

        public static Vector3 Pos(this GameObject obj) => obj.transform.position;
        public static Safe<Vector3> Pos(this Safe<GameObject> obj) => obj.Select(o => o.Pos());

        public static Vector3 Pos<T>(this T obj) where T : Component => obj.transform.position;

        public static Safe<Vector3> Pos<T>(this Safe<T> t) where T : Component
           => t.SelectOut(ifSome: c => c.transform.position.Safe(), ifNone: () => None);

        public static Quaternion Rot(this GameObject obj) => obj.transform.rotation;
        public static Safe<Quaternion> Rot(this Safe<GameObject> obj) => obj.Select(o => o.transform.rotation);

        public static Quaternion LocalRot(this GameObject obj) => obj.transform.localRotation;
        public static Safe<Quaternion> LocalRot(this Safe<GameObject> obj) => obj.Select(o => o.transform.localRotation);
        public static Quaternion Rot<T>(this T obj) where T : Component => obj.transform.rotation;
        public static Safe<Quaternion> Rot<T>(this Safe<T> obj) where T : Component => obj.Select(o => o.Safe().Rot());

        public static Safe<Vector3> Scale(this Safe<GameObject> obj) => obj.Select(o => o.transform.localScale);
        public static Safe<GameObject> SetName(this Safe<GameObject> gameObject, string name)
        {
            gameObject.IfSome(g => g.name = name);
            return gameObject;
        }

        public static string Name(this Safe<GameObject> gameObject)
            => gameObject.SelectOut(obj => obj.name, () => "<No Obj>");

        public static string Name<T>(this Safe<T> component) where T : Component
            => component.SelectOut(c => c.name, () => "<No Obj>");

        public static LayerMask NameToLayer(this string maskName)
            => LayerMask.NameToLayer(maskName);

        public static Safe<GameObject> AddLayerMask(this Safe<GameObject> obj, string maskName)
            => obj.IfSome(o => o.AddLayerMask(maskName));

        public static T AddLayerMask<T>(this T component, string maskName) where T : Component
        {
            component.gameObject.AddLayerMask(maskName);
            return component;  
        }

        public static GameObject AddLayerMask(this GameObject obj, string maskName)
            => obj.AddLayerMask(maskName.NameToLayer());

        public static GameObject AddLayerMask(this GameObject obj, LayerMask layerMask)
        {
            obj.layer |= layerMask;
            return obj;
        }

        public static Safe<GameObject> RemoveLayerMask(this Safe<GameObject> obj, string maskName)
            => obj.IfSome(o => o.RemoveLayerMask(maskName));

        public static GameObject RemoveLayerMask(this GameObject obj, string maskName)
            => obj.RemoveLayerMask(maskName.NameToLayer());

        public static GameObject RemoveLayerMask(this GameObject obj, LayerMask layerMask)
        {
            obj.layer &= ~(1 << layerMask);
            return obj;
        }

        public static GameObject HideAllMeshRendersInParentAndChildren(this GameObject obj)
        {
            obj.MaybeComponent<MeshRenderer>().IfSome(renderer => renderer.enabled = false);
            foreach (var renderer in obj.GetComponentsInChildren<MeshRenderer>())
            {
                renderer.enabled = false;
            }

            return obj;
        }

        public struct PenetrationData
        {
            public bool penetrated;
            public Vector3 direction;
            public float distance;
        }

        public static PenetrationData ComputePenetration(Safe<Collider> colA, Safe<Collider> colB)
        {
            PenetrationData defaultResults = new PenetrationData
            {
                penetrated = false,
                direction = Vector3.zero,
                distance = -Mathf.Infinity
            };

            var result =
                colA.Select(a =>
                {
                    return colB.Select(b =>
                    {
                        // A and B colliders found
                        // Get Compute results and then return
                        float dist;
                        Vector3 dir;
                        bool penetrated = Physics.ComputePenetration(a, a.Pos(), a.Rot(), b, b.Pos(), b.Rot(), out dir, out dist);

                        return new PenetrationData
                        {
                            penetrated = penetrated,
                            direction = dir,
                            distance = dist
                        };
                    });
                });

            // If results were generated then return it, if not, return the default
            return result.SelectOut(res => res, () => defaultResults);
        } 
    }
}
