using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class FindObjectsUsingLayerWindow : EditorWindow
{
    public int layerToLookFor;
    private int objectsFound;
    private List<GameObject> selected;
    private Vector2 scrollPos;

    [MenuItem("Tools/FindObjects Using Layer", false, 1)]
    static void Init()
    {
        var findObjectsUsingLayerWindow = GetWindow<FindObjectsUsingLayerWindow>();
        findObjectsUsingLayerWindow.titleContent = new GUIContent("Find Objects Using Layer");
        findObjectsUsingLayerWindow.Show();
    }

    public void OnGUI()
    {
        minSize = new Vector2(290, 380);
        maxSize = new Vector2(290, 380);
  
        layerToLookFor = EditorGUILayout.IntField("Layer Number", layerToLookFor);

        EditorGUILayout.LabelField("Objects Found = " + objectsFound);
        if (GUILayout.Button("Get Objects"))
        {
            SelectObjectsInLayer(layerToLookFor);
        }
        
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, 
            GUILayout.Width(minSize.x), GUILayout.Height(minSize.y - 100));
        if (objectsFound > 0)
        {
            foreach (var go in selected)
            {
                var currentGo = EditorGUILayout.ObjectField(go, typeof(GameObject), true) as GameObject;
            }
        }
        EditorGUILayout.EndScrollView();
                
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
    }
    
    public void SelectObjectsInLayer(int layerToUse)
    {
        var objects = GetSceneObjects();
        GetObjectsInLayer(objects, layerToUse);
    }
 
    private GameObject[] GetSceneObjects()
    {
        return Resources.FindObjectsOfTypeAll<GameObject>()
            .Where(go => go.hideFlags == HideFlags.None).ToArray();
    }
    
    private void GetObjectsInLayer(GameObject[] root, int layer)
    {
        selected = new List<GameObject>();
        foreach (GameObject t in root)
        {
            if (t.layer == layer)
            {
                selected.Add(t);
            }
        }
        Selection.objects = selected.ToArray();
        objectsFound = selected.Count;
    }
}

