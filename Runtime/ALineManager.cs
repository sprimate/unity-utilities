using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class ALineManager : MonoBehaviour
{
    protected bool isVisible;
    [SerializeField] protected float lineRendererWidth = 1f; //TODO - make these widths line-specific. Not doing it now, because I'm not sure if we're ever using this class

    public static ALineManager GetLineManager()
    {
        return GetLineManager<ALineManager>(true);
    }

    public static T GetLineManager<T>(bool dontCreateIfMissing = false) where T : ALineManager
    {
        var lineManagers = GameObject.FindObjectsOfType<T>();
        if (lineManagers.Length < 1)
        {
            if (dontCreateIfMissing)
            {
                Debug.LogError("No Line Managers available in the current scene");
                return null;
            }
            else
            {
                var go = new GameObject("Line Manager");
                var managersHolder = GameObject.Find("Managers");
                if (managersHolder != null)
                {
                    go.transform.SetParent(managersHolder.transform);
                }
                /* if (typeof(T) == typeof(ALineManager))
                 {
                     return go.AddComponent<VectrosityLineManager>() as T;
                 }
                 else*/
                {
                    return go.GetOrAddComponent<T>();
                }
            }
        }
        T ret = lineManagers[0];
        if (lineManagers.Length > 1)
        {
            Debug.Log("Multiple Line managers detected in the current scene. Using " + ret, ret);
        }
        return ret;
    }

    public abstract void DisplayShape(params int[] shapeId);
    public abstract void HideShape(params int[] shapeId);
    public abstract void DisplayShapes();
    public abstract void HideShapes();
    public abstract int AddShape(Vector3[] points, Color color);
    public abstract void RemoveShape(int fragmentId);
    public abstract void RemoveShapes(params int[] shapeIds);
    public abstract void RemoveAllShapes();
    public virtual void SetLineRendererWidth(float width)
    {
        lineRendererWidth = width;
    }
}