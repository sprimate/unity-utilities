using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;

[ExecuteInEditMode]
public class DebugHelper : MonoSingleton<DebugHelper>
{
    protected override bool ShouldDestroyOnLoad => true;
    public GenericObjectPool<LineRenderer> LineRendererPool { get; protected set; }
    public GenericObjectPool<Renderer> SpherePool { get; protected set; }

    void OnCreate<T>(T created) where T : Component
    {
        created.transform.SetParent(transform);
        created.transform.Normalize();
        created.gameObject.layer = Constants.UI_LAYER;
    }

    protected List<LogObj> ToLog = new List<LogObj>();
    protected Dictionary<TextMeshProUGUI, float> TextObjects = new Dictionary<TextMeshProUGUI, float>();
    protected Dictionary<TextMeshProUGUI, float> TextObjectsFixed = new Dictionary<TextMeshProUGUI, float>();
    protected Dictionary<LogObj, bool> ToLogFixed = new Dictionary<LogObj, bool>();
    protected Dictionary<LineRenderer, float> LineToDraw = new Dictionary<LineRenderer, float>();
    protected Dictionary<LineRenderer, float> LineToDrawFixed = new Dictionary<LineRenderer, float>();
    protected Dictionary<GameObject, float> PrimitiveToDraw = new Dictionary<GameObject, float>();
    protected Dictionary<GameObject, float> PrimitiveToDrawFixed = new Dictionary<GameObject, float>();
    static Color defaultColor = Color.red;
    public static Color Color = defaultColor;
    public TextMeshProUGUI textTemplate;

    public static void RevertColor()
    {
        Color = defaultColor;
    }

    public static float lineWidth = 0.08f;

    static HashSet<object> keys = new HashSet<object>();
    public static void LogOnce(object message, object uniqueKeyOrCallingObject)
    {
        LogOnce(message, null, uniqueKeyOrCallingObject);
    }

    public static void LogOnce(object message, UnityEngine.Object context, object uniqueKeyOrCallingObject)
    {
        if (uniqueKeyOrCallingObject == null)
        {
            Debug.Log(message, context);
        }

        else if (!keys.Contains(uniqueKeyOrCallingObject))
        {
            keys.Add(uniqueKeyOrCallingObject);
            Debug.Log(message, context);
        }
    }

    static float defaultLogTime = 3.5f;
    public static void Log(string text, Object context = null)
    {
        LogGui(text, defaultLogTime, context);
    }

    public static void LogGui(string text, Object context = null)
    {
        instance.LogGuiInternal(new LogObj(text), ref instance.ToLog, ref instance.TextObjects, null, context);
    }

    public static void LogGui(LogObj logObj, Object context = null)
    {
        instance.LogGuiInternal(logObj, ref instance.ToLog, ref instance.TextObjects, null, context);
    }

    public static void LogGuiFixed(string text, Object context = null)
    {
        LogGuiFixed(new LogObj(text), context);
    }

    public static void LogGuiFixed(LogObj logObj, Object context = null)
    {
        if (instance.textTemplate?.canvas?.isActiveAndEnabled == true)
        {
            instance.LogGuiInternal(logObj, ref instance.ToLog, ref instance.TextObjectsFixed, null, context);
        }
        else
        {
            GenericCoroutineManager.RunAfterFixedFrames(1, () =>
            {
                instance.ToLogFixed[logObj] = false;
            }, null);

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

        instance.StartCoroutine(instance.LogForSeconds(text, lengthOfTime));
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

    public static void DrawPrimitive(Vector3 position, PrimitiveType primitiveType, float scale = 1, float? lifetime = null)
    {
        instance.DrawPrimitiveInternal(position, ref instance.PrimitiveToDraw, primitiveType, Vector3.one * scale, lifetime);
    }

    public static void DrawPrimitive(Vector3 position, PrimitiveType primitiveType, Vector3? scale = null, float? lifetime = null)
    {
        instance.DrawPrimitiveInternal(position, ref instance.PrimitiveToDraw, primitiveType, scale, lifetime);
    }

    public static void DrawPrimitiveFixed(Vector3 position, PrimitiveType primitiveType, float scale = 1, float? lifetime = null)
    {
        instance.DrawPrimitiveInternal(position, ref instance.PrimitiveToDrawFixed, primitiveType, Vector3.one * scale, lifetime);
    }

    public static void DrawPrimitiveFixed(Vector3 position, PrimitiveType primitiveType, Vector3? scale = null, float? lifetime = null)
    {
        instance.DrawPrimitiveInternal(position, ref instance.PrimitiveToDrawFixed, primitiveType, scale, lifetime);
    }

    public static void DrawLine(Ray ray, float distance, float? duration = null)
    {
        DrawLine(distance, ray.origin, ray.direction, duration);
    }

    public static void DrawLine(params Vector3[] points)
    {
        instance.DrawLineInternal(points, ref instance.LineToDraw, null);
    }

    public static void DrawLine(float lifetime, Vector3[] points)
    {
        instance.DrawLineInternal(points, ref instance.LineToDraw, (float) lifetime);
    }

    public static void DrawLine(float distance, Vector3 origin, Vector3 direction, float? lifeTime = null)
    {
        Vector3[] points = new Vector3[2] { origin, origin + direction.normalized * distance };
        instance.DrawLineInternal(points, ref instance.LineToDraw, lifeTime);

    }

    public static void DrawLineFixed(params Vector3[] points)
    {
        instance.DrawLineInternal(points, ref instance.LineToDrawFixed, null);

    }

    public static void DrawLineFixed(Vector3 origin, Vector3 direction, float distance)
    {
        Vector3[] points = new Vector3[2] { origin, origin + direction.normalized * distance };
        instance.DrawLineInternal(points, ref instance.LineToDrawFixed, null);
    }

    protected void DrawLineInternal(Vector3[] points, ref Dictionary<LineRenderer, float> dict, float? lifeTime)
    {
        LineRenderer rend = LineRendererPool.Get();
        rend.startColor = Color;
        rend.endColor = Color;
        rend.widthMultiplier = lineWidth;
        rend.material = uiMaterial;
        rend.SetPositions(points);
        dict[rend] = lifeTime.HasValue ? lifeTime.Value : 0f;
    }

    Material _uiMaterial;

    public Material uiMaterial
    {
        get
        {
            if (_uiMaterial == null)
            {
                _uiMaterial = new Material(Shader.Find("UI/Default"));
            }

            return _uiMaterial;
        }
    }

    Material _primitiveMaterial;

    public Material primitiveMaterial
    {
        get
        {
            if (_primitiveMaterial == null)
            {
                try{
                    _primitiveMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                }
                catch(Exception)
                {
                    _primitiveMaterial = new Material(Shader.Find("Unlit/Color"));
                }
            }

            return _primitiveMaterial;
        }
    }
    protected void DrawPrimitiveInternal(Vector3 position, ref Dictionary<GameObject, float> dict, PrimitiveType primitiveType, Vector3? scale, float? lifetime)
    {
        GameObject go = null;
        if (primitiveType == PrimitiveType.Sphere)
        {
            go = SpherePool.Get().gameObject;
        }
        else
        {
            go = GameObject.CreatePrimitive(primitiveType);
            go.transform.SetParent(transform);
            go.transform.localRotation = Quaternion.identity;
        }


        go.transform.position = position;

        if (scale.HasValue)
        {
            go.transform.localScale = scale.Value;
        }
        var renderer = go.GetComponent<Renderer>();
        renderer.material = primitiveMaterial;
        renderer.material.color = Color;
        go.gameObject.SetLayerRecursive(2);//ignore raycast
        Destroy(go.transform.GetComponent<Collider>());
        dict[go] = lifetime.HasValue ? lifetime.Value : 0f;
    }

    protected override void Awake()
    {
        base.Awake();
    }

    protected void Start()
    {
        var gcm = GenericCoroutineManager.instance;//create if it's not there, to allow for proper clearing and cleaning up on scene exit
        StartCoroutine(Clear());
        StartCoroutine(ClearFixed());
        LineRendererPool = new GenericObjectPool<LineRenderer>(OnCreate<LineRenderer>);
        SpherePool = new GenericObjectPool<Renderer>(() =>
        {
            var primitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            var renderer = primitive.GetComponent<Renderer>();
            OnCreate(renderer);
            return renderer;
        });
    }

    List<LogObj> lastLogged = new List<LogObj>();
    GUIStyle defaultStyle;
    private void OnGUI()
    {
        defaultStyle = new GUIStyle(GUI.skin.textField);
        defaultStyle.wordWrap = true;
        defaultStyle.fontSize = Mathf.RoundToInt(Screen.height / 20 / 1.75f);
#if UNITY_EDITOR
        if (UnityEditor.EditorApplication.isPaused)
        {
            ToLog = new List<LogObj>(lastLogged);
        }

        lastLogged.Clear();
#endif

        int thisRoundIndex = 0;

        foreach (var str in ToLog)
        {
            Display(str, ref thisRoundIndex); //x, ref y, width, style);
#if UNITY_EDITOR
            lastLogged.Add(str);
#endif
        }

        foreach (var str in ToLogFixed.Keys.ToArray())
        {
            Display(str, ref thisRoundIndex);//, x, ref y, width, style);
            ToLogFixed[str] = true;
#if UNITY_EDITOR
            lastLogged.Add(str);
#endif
        }
    }

    void Display(LogObj logObj, ref int thisRoundIndex)
    {
        if (logObj.guiStyle == null)
        {
            logObj.guiStyle = defaultStyle;
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
        while (true)
        {
            if (GenericCoroutineManager.hasInstance)
            {
                yield return GenericCoroutineManager.instance.waitForEndOfFrame;
            }
            else
            {
                break;
            }

            ToLog.Clear();// = new List<string>(ToLogFixed);
            ClearTextObjects(ref TextObjects);
            ClearLines(ref LineToDraw);
            ClearPrimitives(ref PrimitiveToDraw);
        }
    }

    IEnumerator ClearFixed()
    {
        while (this && GenericCoroutineManager.hasInstance)
        {
            yield return GenericCoroutineManager.instance.waitForFixedUpdate;
            foreach (var key in ToLogFixed.Keys.ToArray())
            {
                if (ToLogFixed[key])
                {
                    ToLogFixed.Remove(key);
                }
            }

            ClearTextObjects(ref TextObjectsFixed);
            ClearLines(ref LineToDrawFixed);
            ClearPrimitives(ref PrimitiveToDrawFixed);
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
                GenericCoroutineManager.RunInSeconds(dict[tmp], () =>
                {
                    if (tmp?.gameObject)
                    {
                        Destroy(tmp.gameObject);
                    }
                }, null);
            }
        }
    }

    void ClearPrimitives(ref Dictionary<GameObject, float> dict)
    {
        foreach (var go in dict.Keys.ToArray())
        {
            if (!go)
            {
                dict.Remove(go);
            }
            else
            {
                GenericCoroutineManager.RunInSeconds(dict[go], () =>
                {
                    Destroy(go);
                }, null);
            }
        }
    }

    void ClearLines(ref Dictionary<LineRenderer, float> dict)
    {
        foreach (var key in dict.Keys.ToArray())
        {
            var rend = key;
            GenericCoroutineManager.RunInSeconds(dict[key], () =>
            {
                if (rend?.gameObject)
                {
                    Destroy(rend.gameObject);
                }
            }, null);
            dict.Remove(key);
        }
    }


    public static bool CheckDebugMode(KeyCode keyCode, string successfulMessage = null)
    {
        if (Input.GetKey(Constants.instance.debugKeyCode) && Input.GetKeyDown(keyCode))
        {
            if (!string.IsNullOrWhiteSpace(successfulMessage))
            {
                LogGui(successfulMessage, 4f);
            }

            return true;
        }

        return false;
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

class DropOutStack<T>
{
    private T[] items;
    private int top = 0;
    public DropOutStack(int capacity, params T[] data)
    {
        items = new T[capacity];
        if (data != null)
        {
            foreach (var d in data)
            {
                Push(d);
            }
        }
    }

    public void Push(T item)
    {
        items[top] = item;
        top = (top + 1) % items.Length;
    }
    public T Pop()
    {
        top = (items.Length + top - 1) % items.Length;
        return items[top];
    }

    public T Peek()
    {
        return items[top];
    }
}