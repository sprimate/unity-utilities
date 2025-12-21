using Cysharp.Threading.Tasks;
using HitTrax.CoreUtilities;
using HitTrax.UnityUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

public static class DebugHelperExtensions
{
    public static void LogGui(this string text, Object context = null) => DebugHelper.LogGui(text, context);
}

[ExecuteInEditMode]
public class DebugHelper : AMonoSingleton<DebugHelper>
{
    protected override bool ShouldDestroyOnLoad => true;

    void OnCreate<T>(T created) where T : Component
    {
        created.transform.SetParent(transform);
        created.transform.Normalize();
    }

    protected List<LogObj> _toLog = new List<LogObj>();
    protected Dictionary<TextMeshProUGUI, float> _textObjects = new Dictionary<TextMeshProUGUI, float>();
    protected Dictionary<TextMeshProUGUI, float> _textObjectsFixed = new Dictionary<TextMeshProUGUI, float>();
    protected Dictionary<LogObj, bool> _toLogFixed = new Dictionary<LogObj, bool>();
    protected Dictionary<LineRenderer, float> _lineToDraw = new Dictionary<LineRenderer, float>();
    protected Dictionary<LineRenderer, float> _lineToDrawFixed = new Dictionary<LineRenderer, float>();
    protected Dictionary<GameObject, float> _primitiveToDraw = new Dictionary<GameObject, float>();
    protected Dictionary<GameObject, float> _primitiveToDrawFixed = new Dictionary<GameObject, float>();
    static Color _defaultColor = Color.red;
    public static Color color = _defaultColor;
    public TextMeshProUGUI textTemplate;
    public Action onDrawGizmos;
    public void OnDrawGizmos()
    {
        onDrawGizmos?.Invoke();
    }
    public static void RevertColor()
    {
        color = _defaultColor;
    }

    public static float lineWidth = 0.08f;

    static HashSet<object> _keys = new HashSet<object>();
    public static void LogOnce(object message, object uniqueKeyOrCallingObject)
    {
        LogOnce(message, null, uniqueKeyOrCallingObject);
    }

    public static void LogOnce(object message, Object context, object uniqueKeyOrCallingObject)
    {
        if (uniqueKeyOrCallingObject == null)
        {
            Debug.Log(message, context);
        }

        else if (!_keys.Contains(uniqueKeyOrCallingObject))
        {
            _keys.Add(uniqueKeyOrCallingObject);
            Debug.Log(message, context);
        }
    }

    static float _defaultLogTime = 3.5f;
    public static void Log(string text, Object context = null)
    {
        LogGui(text, _defaultLogTime, context);
    }

    public static void LogGui(string text, Object context = null)
    {
        Instance.LogGuiInternal(new LogObj(text), ref Instance._toLog, ref Instance._textObjects, null, context);
    }

    public static void LogGui(LogObj logObj, Object context = null)
    {
        Instance.LogGuiInternal(logObj, ref Instance._toLog, ref Instance._textObjects, null, context);
    }

    public static void LogGuiFixed(string text, Object context = null)
    {
        LogGuiFixed(new LogObj(text), context);
    }

    public static void LogGuiFixed(LogObj logObj, Object context = null)
    {
        if (Instance.textTemplate?.canvas?.isActiveAndEnabled == true)
        {
            Instance.LogGuiInternal(logObj, ref Instance._toLog, ref Instance._textObjectsFixed, null, context);
        }
        else
        {
            UnityAsyncOperations.RunAfterFixedFrames(1, () =>
            {
                Instance._toLogFixed[logObj] = false;
            });

            if (context)
            {
                Debug.Log(logObj.text, context);
            }
        }
    }

    void LogGuiInternal(LogObj logObj, ref List<LogObj> list, ref Dictionary<TextMeshProUGUI, float> dict2, float? lifetime = null, Object context = null)
    {
        if (textTemplate?.canvas?.isActiveAndEnabled == true)
        {
            var textJawn = Instantiate(textTemplate);
            textJawn.transform.SetParent(textTemplate.transform.parent);
            textJawn.transform.localScale = textTemplate.transform.localScale;
            textJawn.transform.localRotation = textTemplate.transform.localRotation;
            textJawn.transform.localPosition = Vector3.zero;
            textJawn.transform.SetAsLastSibling();
            textJawn.text = logObj.text;
            dict2[textJawn] = lifetime.HasValue ? lifetime.Value : 0f;
            textJawn.gameObject.SetActive(true);
            textJawn.enabled = true;
        }
        else
        {
            list.Add(logObj);
        }
        if (context)
        {
            Debug.Log(logObj.text, context);
        }
    }

    public static void LogGui(string text, float lengthOfTime, Object context = null)
    {
        if (context)
        {
            Debug.Log(text, context);
        }

        Instance.StartCoroutine(Instance.LogForSeconds(text, lengthOfTime));
    }
    protected IEnumerator LogForSeconds(string text, float secs)
    {
        float startTime = Time.time;
        while (Time.time < startTime + secs)
        {
            LogGui(text);
            yield return null;
        }
    }

    public static void DrawPrimitive(Vector3 position, PrimitiveType primitiveType, float scale = 1, float? lifetime = null, Color? color = null)
    {
        Instance.DrawPrimitiveInternal(position, primitiveType, Vector3.one * scale, lifetime, false, color).Forget();
    }

    public static void DrawPrimitive(Vector3 position, PrimitiveType primitiveType, Vector3? scale = null, float? lifetime = null, Color? color = null)
    {
        Instance.DrawPrimitiveInternal(position, primitiveType, scale, lifetime, false, color).Forget();
    }

    public static void DrawPrimitiveFixed(Vector3 position, PrimitiveType primitiveType, float scale = 1, float? lifetime = null, Color? color = null)
    {
        Instance.DrawPrimitiveInternal(position, primitiveType, Vector3.one * scale, lifetime, true, color).Forget();
    }

    public static void DrawPrimitiveFixed(Vector3 position, PrimitiveType primitiveType, Vector3? scale = null, float? lifetime = null, Color? color = null)
    {
        Instance.DrawPrimitiveInternal(position, primitiveType, scale, lifetime, true, color).Forget();
    }

    public static void DrawLine(Ray ray, float distance, float? duration = null)
    {
        DrawLine(distance, ray.origin, ray.direction, duration);
    }

    public static void DrawLine(params Vector3[] points)
    {
        Instance.DrawLineInternal(points, ref Instance._lineToDraw, null);
    }

    public static void DrawLine(float lifetime, Vector3[] points)
    {
        Instance.DrawLineInternal(points, ref Instance._lineToDraw, (float)lifetime);
    }

    public static void DrawLine(float distance, Vector3 origin, Vector3 direction, float? lifeTime = null)
    {
        Vector3[] points = new Vector3[2] { origin, origin + direction.normalized * distance };
        Instance.DrawLineInternal(points, ref Instance._lineToDraw, lifeTime);

    }

    public static void DrawLineFixed(params Vector3[] points)
    {
        Instance.DrawLineInternal(points, ref Instance._lineToDrawFixed, null);
    }

    public static void DrawLineFixed(Vector3 origin, Vector3 direction, float distance)
    {
        Vector3[] points = new Vector3[2] { origin, origin + direction.normalized * distance };
        Instance.DrawLineInternal(points, ref Instance._lineToDrawFixed, null);
    }

    protected void DrawLineInternal(Vector3[] points, ref Dictionary<LineRenderer, float> dict, float? lifeTime)
    {
        LineRenderer rend = new GameObject().AddComponent<LineRenderer>();// LineRendererPool.Get();
        OnCreate(rend);
        rend.startColor = color;
        rend.endColor = color;
        rend.widthMultiplier = lineWidth;
        rend.material = UiMaterial;
        rend.SetPositions(points);
        dict[rend] = lifeTime.HasValue ? lifeTime.Value : 0f;
    }

    Material _uiMaterial;

    public Material UiMaterial
    {
        get
        {
            if (_uiMaterial == null)
            {
                try
                {
                    _uiMaterial = new Material(Shader.Find("UI/Default"));
                }
                catch (System.Exception)
                {
                    _uiMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
                }
            }

            return _uiMaterial;
        }
    }

    Material _primitiveMaterial;

    public Material PrimitiveMaterial
    {
        get
        {
            if (_primitiveMaterial == null)
            {
                try
                {
                    _primitiveMaterial = new Material(Shader.Find("Unlit/Color"));
                }
                catch (System.Exception)
                {
                    _primitiveMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
                }
            }

            return _primitiveMaterial;
        }
    }
    protected async UniTask DrawPrimitiveInternal(Vector3 position, PrimitiveType primitiveType, Vector3? scale, float? lifetime, bool useFixed, Color? color = null)
    {
        color = color ?? DebugHelper.color;
        GameObject go = null;
        go = GameObject.CreatePrimitive(primitiveType);
        go.transform.SetParent(transform);
        go.transform.localRotation = Quaternion.identity;

        go.transform.position = position;

        if (scale.HasValue)
        {
            go.transform.localScale = scale.Value;
        }

        var renderer = go.GetComponent<Renderer>();
        renderer.material = PrimitiveMaterial;
        renderer.material.color = color.Value;
        go.gameObject.SetLayerRecursive(2);//ignore raycast
        var collider = go.transform.GetComponent<Collider>();
        if (Application.isPlaying)
        {
            Destroy(collider);
        }
        else
        {
            DestroyImmediate(collider);
        }

        if (lifetime.HasValue)
        {
            await UniTask.WaitForSeconds(lifetime.Value);
        }
        else if (useFixed)
        {
            await UniTask.WaitForFixedUpdate();
        }
        else
        {
            await UniTask.Yield();
        }

        if (Application.isPlaying)
        {
            Destroy(go);
        }
        else
        {
            DestroyImmediate(go);
        }
    }

    protected override void Awake()
    {
        base.Awake();
    }

    protected void Start()
    {
        StartCoroutine(Clear());
        StartCoroutine(ClearFixed());
    }

    List<LogObj> _lastLogged = new List<LogObj>();
    GUIStyle _defaultStyle;
    private void OnGUI()
    {
        _defaultStyle = new GUIStyle(GUI.skin.textField);
        _defaultStyle.wordWrap = true;
        _defaultStyle.fontSize = Mathf.RoundToInt(Screen.height / 20 / 1.75f);

#if UNITY_EDITOR
        if (UnityEditor.EditorApplication.isPaused)
        {
            _toLog = new List<LogObj>(_lastLogged);
        }

        _lastLogged.Clear();
#endif

        int thisRoundIndex = 0;

        foreach (var str in _toLog)
        {
            Display(str, ref thisRoundIndex); //x, ref y, width, style);
#if UNITY_EDITOR
            _lastLogged.Add(str);
#endif
        }

        async UniTask ClearAfterFrame()
        {
            await UniTask.WaitForEndOfFrame();
            _toLog.Clear();
        }

        ClearAfterFrame().Forget();

        foreach (var str in _toLogFixed.Keys.ToArray())
        {
            Display(str, ref thisRoundIndex);//, x, ref y, width, style);
            _toLogFixed[str] = true;
#if UNITY_EDITOR
            _lastLogged.Add(str);
#endif
        }
    }

    void Display(LogObj logObj, ref int thisRoundIndex)
    {
        if (logObj.guiStyle == null)
        {
            logObj.guiStyle = _defaultStyle;
        }

        if (!logObj.rect.HasValue)
        {
            var x = 8;
            var y = 60;
            var calc = logObj.guiStyle.CalcSize(new GUIContent(logObj.text));
            var neededWidth = calc.x + 4;
            var width = Mathf.RoundToInt(Screen.width / 2.5f);
            var numLinesNeeded = Mathf.CeilToInt(neededWidth / width);
            float height = calc.y * numLinesNeeded;

            logObj.rect = new Rect(x, y + height * thisRoundIndex, width, height);
            thisRoundIndex++;
        }

        GUI.TextArea(logObj.rect.Value, logObj.text, logObj.guiStyle);
    }

    IEnumerator Clear()
    {
        while (this != null)
        {
            yield return null;// UnityAsyncOperations.WaitForEndOfFrame;
            ClearTextObjects(ref _textObjects);
            ClearLines(ref _lineToDraw);
        }
    }

    IEnumerator ClearFixed()
    {
        while (this != null)
        {
            yield return UnityAsyncOperations.WaitForFixedUpdate;
            foreach (var key in _toLogFixed.Keys.ToArray())
            {
                if (_toLogFixed[key])
                {
                    _toLogFixed.Remove(key);
                }
            }

            ClearTextObjects(ref _textObjectsFixed);
            ClearLines(ref _lineToDrawFixed);
        }
    }

    void ClearTextObjects(ref Dictionary<TextMeshProUGUI, float> dict)
    {
        foreach (var tmp in dict.Keys.ToArray())
        {
            if (!tmp)
            {
                dict.Remove(tmp);
            }
            else
            {
                UnityAsyncOperations.RunInSeconds(dict[tmp], () =>
                {
                    if (tmp?.gameObject)
                    {
                        Destroy(tmp.gameObject);
                    }
                });
            }
        }
    }


    void ClearLines(ref Dictionary<LineRenderer, float> dict)
    {
        foreach (var key in dict.Keys.ToArray())
        {
            var rend = key;
            UnityAsyncOperations.RunInSeconds(dict[key], () =>
            {
                if (rend?.gameObject)
                {
                    Destroy(rend.gameObject);
                }
            });
            dict.Remove(key);
        }
    }
}

public class LogObj
{
    public string text;
    public Rect? rect;
    public GUIStyle guiStyle;
    public LogObj(string _text)
    {
        text = _text;
    }
    /// <summary>
    /// {0,0} is Top Left. {Screen.width, Screen.height} is Bottom Right
    /// </summary>

    /// <returns></returns>
    public LogObj(string _text, Rect _rect) : this(_text)
    {
        rect = _rect;
    }
}