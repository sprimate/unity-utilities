using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HitTrax.UnityUtilities
{
    [RequireComponent(typeof(LineRenderer))]
    public class DynamicLineRenderer : MonoBehaviour
    {
        public LineRenderer LineRenderer { get; private set; }
        private List<Vector3> _points = new List<Vector3>();

        void Awake()
        {
            LineRenderer = GetComponent<LineRenderer>();
            LineRenderer.positionCount = 0;
            LineRenderer.useWorldSpace = true;
        }

        /// <summary>
        /// Adds a new point to the line.
        /// </summary>
        /// <param name="newPoint">The new point in world space.</param>
        public void AddPoint(Vector3 newPoint)
        {
            // Optionally prevent duplicate points
            if (_points.Count == 0 || _points[_points.Count - 1] != newPoint)
            {
                _points.Add(newPoint);
                LineRenderer.positionCount = _points.Count;
                LineRenderer.SetPositions(_points.ToArray());
            }
        }

        public void UpdatePoints(IEnumerable<Vector3> points)
        {
            // _points = points.ToList();

            // for (int i = 0; i < _points.Count; i++)
            // {

            //     LineRenderer.SetPosition(i, _points[i]);
            // }
            _points = points.ToList();

            if (LineRenderer == null)
            {
                LineRenderer = transform.GetOrAddComponent<LineRenderer>();
            }

            if (LineRenderer.positionCount == 0)
            {
                LineRenderer.positionCount = _points.Count;
            }

            for (int i = 0; i < _points.Count; i++)
            {
                LineRenderer.SetPosition(i, _points[i]);
            }

            //LineRenderer.SetPositions(_points.ToArray());
        }

        /// <summary>
        /// Clears the line.
        /// </summary>
        public void ClearLine()
        {
            _points.Clear();
            LineRenderer.positionCount = 0;
        }
    }

    public static class LineManager
    {
        private const string GLOBAL = "Global";

        // Category, (id, lineRenderer)
        private static Dictionary<string, Dictionary<int, DynamicLineRenderer>> _lines = new();
        private static GameObject _lineParent;
        private static Material _defaultMaterial;

        private static Material DefaultMaterial
        {
            get
            {
                if (_defaultMaterial == null)
                {
                    // URP unlit shader
                    var shader = Shader.Find("Unlit/Color");
                    if (shader == null)
                    {
                        shader = Shader.Find("Hidden/InternalErrorShader");
                        Debug.LogError("URP Unlit shader not found in build. Using fallback.");
                    }

                    _defaultMaterial = new Material(shader)
                    {
                        color = Color.cyan
                    };
                }

                return _defaultMaterial;
            }
        }

        private static Dictionary<int, DynamicLineRenderer> GetLineGroup(string name)
        {
            if (_lines == null)
            {
                _lines = new();
            }

            if (!_lines.ContainsKey(name))
            {
                _lines.Add(name, new Dictionary<int, DynamicLineRenderer>());
            }

            return _lines[name];
        }

        private static Dictionary<int, DynamicLineRenderer> GetGlobalLineGroup() => GetLineGroup(GLOBAL);

        private static int LineCategoryCount(string category)
            => _lines.TryGetValue(category, out var dictionary) ? dictionary.Count : 0;

        public static (int id, DynamicLineRenderer line) CreateLine(string category, Material lineMaterial = null, float width = 0.1f)
            => CreateLine(category, LineCategoryCount(category), lineMaterial, width);

        /// <summary>
        /// Creates a new DynamicLine with the given ID.
        /// </summary>
        private static (int, DynamicLineRenderer) CreateLine(string category, int id, Material lineMaterial = null, float width = 0.1f)
        {
            if (_lineParent == null)
            {
                _lineParent = new GameObject("LineManager_Lines");
            }

            GameObject lineObj = new GameObject($"Line_{id}");
            lineObj.transform.parent = _lineParent.transform;

            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            DynamicLineRenderer dl = lineObj.AddComponent<DynamicLineRenderer>();

            lr.material = lineMaterial != null ? lineMaterial : DefaultMaterial;

            lr.startWidth = width;
            lr.endWidth = width;
            lr.positionCount = 0;


            GetLineGroup(category)[id] = dl;

            dl.name = $"LINE_{category}_{id}";

            return (id, dl);
        }

        /// <summary>
        /// Adds a point to the DynamicLine with the given ID.
        /// </summary>
        public static void AddPoint(string category, int id, Vector3 point)
        {
            if (GetLineGroup(category).TryGetValue(id, out DynamicLineRenderer dl))
            {
                dl.AddPoint(point);
            }
            else
            {
                //   Debug.LogWarning($"Line with ID '{id}' does not exist.");
            }
        }

        public static void UpdatePoints(string category, int id, IEnumerable<Vector3> points)
        {
            if (GetLineGroup(category).TryGetValue(id, out DynamicLineRenderer dl))
            {
                if (dl == null)
                {
                    dl = CreateLine(category, id).Item2;
                }

                dl.UpdatePoints(points);
            }
        }

        /// <summary>
        /// Clears all points from the specified line.
        /// </summary>
        public static void ClearLine(string category, int id)
        {
            if (GetLineGroup(category).TryGetValue(id, out DynamicLineRenderer dl))
            {
                dl.ClearLine();
            }
        }

        /// <summary>
        /// Destroys the line GameObject and removes it from the manager.
        /// </summary>

        //public static void RemoveLine(string category, int id)
        //{
        //    if (GetLineGroup(category).TryGetValue(id, out DynamicLineRenderer dl))
        //    {
        //        Object.Destroy(dl.gameObject);
        //        GetLineGroup(category).Remove(id);
        //    }
        //}

        /// <summary>
        /// Destroys and removes all lines.
        /// </summary>
        public static void DestroyAll(string category)
        {
            var dict = GetLineGroup(category);
            if (dict == null)
            {
                return;
            }

            foreach (var (_, dl) in dict)
            {
                try
                {
                    if (dl?.gameObject && dl?.gameObject != null)
                    {
                        Object.Destroy(dl.gameObject);
                    }
                }
                
                catch (MissingReferenceException){}
            }

            dict.Clear();
        }
    }
}
