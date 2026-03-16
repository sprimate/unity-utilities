using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif

public enum DurationMetric
{
    Seconds,
    Milliseconds,
    Minutes,
    Hours,
    Days,
    Frames
}

[System.Serializable]
public struct Duration
{
    public Duration(float value, DurationMetric unit) : this(unit)
    {
        SetValue(value, unit);
        this.defaultDurationMetric = unit;
    }

    public Duration(float value) : this(value, DurationMetric.Seconds) { }

    public Duration(DurationMetric defaultDurationMetric) 
    {
        this.defaultDurationMetric = defaultDurationMetric;
        val = 0f;
    }

    public void SetValue(float value, DurationMetric unit)
    {
        switch (unit)
        {
            case DurationMetric.Seconds:
                Seconds = value;
                break;
            case DurationMetric.Milliseconds:
                Milliseconds = value;
                break;
            case DurationMetric.Minutes:
                Minutes = value;
                break;
            case DurationMetric.Hours:
                Hours = value;
                break;
            case DurationMetric.Days:
                Days = value;
                break;
            case DurationMetric.Frames:
                Frames = value;
                break;
            default:
                throw new System.Exception("Invalid duration unit");
        }
    }

    public float GetValue(DurationMetric unit)
    {
        switch (unit)
        {
            case DurationMetric.Seconds:
                return Seconds;
            case DurationMetric.Milliseconds:
                return Milliseconds;
            case DurationMetric.Minutes:
                return Minutes;
            case DurationMetric.Hours:
                return Hours;
            case DurationMetric.Days:
                return Days;
            case DurationMetric.Frames:
                return Frames;
            default:
                throw new System.Exception("Invalid duration unit");
        }
        return 0f;
    }

    public DurationMetric defaultDurationMetric;
    [SerializeField]
    private float val; // Stored in seconds

    // Seconds property
    public float Seconds
    {
        get => val;
        set => val = value;
    }

    // Milliseconds property
    public float Milliseconds
    {
        get => val * 1000f; // Convert seconds to milliseconds
        set => val = value / 1000f; // Convert milliseconds to seconds
    }

    // Minutes property
    public float Minutes
    {
        get => val / 60f; // Convert seconds to minutes
        set => val = value * 60f; // Convert minutes to seconds
    }

    // Hours property
    public float Hours
    {
        get => val / 3600f; // Convert seconds to hours
        set => val = value * 3600f; // Convert hours to seconds
    }

    // Days property
    public float Days
    {
        get => val / 86400f; // Convert seconds to days
        set => val = value * 86400f; // Convert days to seconds
    }

    // Frames property (1 frame = 1/60th of a second)
    public float Frames
    {
        get => val * 60f; // Convert seconds to frames
        set => val = value / 60f; // Convert frames to seconds
    }

    // Implicit conversion to float (returns seconds)
    public static implicit operator float(Duration duration) => duration.val;
    public static implicit operator Duration(float value) => new Duration(value);

    // Arithmetic operators: Duration op float
    public static Duration operator +(Duration a, float b) => new Duration { Seconds = a.val + b };
    public static Duration operator -(Duration a, float b) => new Duration { Seconds = a.val - b };
    public static Duration operator *(Duration a, float b) => new Duration { Seconds = a.val * b };
    public static Duration operator /(Duration a, float b) => new Duration { Seconds = a.val / b };

    // Arithmetic operators: float op Duration
    public static Duration operator +(float a, Duration b) => new Duration { Seconds = a + b.val };
    public static Duration operator -(float a, Duration b) => new Duration { Seconds = a - b.val };
    public static Duration operator *(float a, Duration b) => new Duration { Seconds = a * b.val };
    public static Duration operator /(float a, Duration b) => new Duration { Seconds = a / b.val };

    // Arithmetic operators: Duration op Duration
    public static Duration operator +(Duration a, Duration b) => new Duration { Seconds = a.val + b.val };
    public static Duration operator -(Duration a, Duration b) => new Duration { Seconds = a.val - b.val };
    public static Duration operator *(Duration a, Duration b) => new Duration { Seconds = a.val * b.val };
    public static Duration operator /(Duration a, Duration b) => new Duration { Seconds = a.val / b.val };

    // Unary operators
    public static Duration operator -(Duration a) => new Duration { Seconds = -a.val };
    public static Duration operator +(Duration a) => a;

    // Comparison operators: Duration op Duration
    public static bool operator ==(Duration a, Duration b) => a.val == b.val;
    public static bool operator !=(Duration a, Duration b) => a.val != b.val;
    public static bool operator <(Duration a, Duration b) => a.val < b.val;
    public static bool operator >(Duration a, Duration b) => a.val > b.val;
    public static bool operator <=(Duration a, Duration b) => a.val <= b.val;
    public static bool operator >=(Duration a, Duration b) => a.val >= b.val;

    // Comparison operators: Duration op float
    public static bool operator ==(Duration a, float b) => a.val == b;
    public static bool operator !=(Duration a, float b) => a.val != b;
    public static bool operator <(Duration a, float b) => a.val < b;
    public static bool operator >(Duration a, float b) => a.val > b;
    public static bool operator <=(Duration a, float b) => a.val <= b;
    public static bool operator >=(Duration a, float b) => a.val >= b;

    // Comparison operators: float op Duration
    public static bool operator ==(float a, Duration b) => a == b.val;
    public static bool operator !=(float a, Duration b) => a != b.val;
    public static bool operator <(float a, Duration b) => a < b.val;
    public static bool operator >(float a, Duration b) => a > b.val;
    public static bool operator <=(float a, Duration b) => a <= b.val;
    public static bool operator >=(float a, Duration b) => a >= b.val;

    // Override Equals and GetHashCode for proper struct comparison
    public override bool Equals(object obj)
    {
        if (obj is Duration other)
            return val == other.val;
        if (obj is float otherFloat)
            return val == otherFloat;
        return false;
    }

    public override int GetHashCode() => val.GetHashCode();

    // Test function to validate all operators and properties
    public static void Test()
    {
        int passedTests = 0;
        int totalTests = 0;
        const float epsilon = 0.0001f; // Tolerance for floating point comparisons

        void Assert(bool condition, string testName)
        {
            totalTests++;
            if (condition)
            {
                passedTests++;
                Debug.Log($"[PASS] {testName}");
            }
            else
            {
                Debug.LogError($"[FAIL] {testName}");
            }
        }

        void AssertFloat(float actual, float expected, string testName)
        {
            totalTests++;
            bool passed = Mathf.Abs(actual - expected) < epsilon;
            if (passed)
            {
                passedTests++;
                Debug.Log($"[PASS] {testName} - Expected: {expected}, Got: {actual}");
            }
            else
            {
                Debug.LogError($"[FAIL] {testName} - Expected: {expected}, Got: {actual}");
            }
        }

        Debug.Log("=== Duration Test Suite ===");

        // Test property setters and getters
        Debug.Log("\n--- Testing Properties ---");

        Duration d1 = new Duration { Seconds = 1.0f };
        AssertFloat(d1.Seconds, 1.0f, "Seconds getter");

        d1.Milliseconds = 1000f;
        AssertFloat(d1.Seconds, 1.0f, "Milliseconds setter (1000ms = 1s)");

        d1.Minutes = 1.0f / 60f;
        AssertFloat(d1.Seconds, 1.0f, "Minutes setter (1/60 min = 1s)");

        d1.Hours = 1.0f / 3600f;
        AssertFloat(d1.Seconds, 1.0f, "Hours setter (1/3600 hr = 1s)");

        d1.Days = 1.0f / 86400f;
        AssertFloat(d1.Seconds, 1.0f, "Days setter (1/86400 day = 1s)");

        d1.Frames = 60f;
        AssertFloat(d1.Seconds, 1.0f, "Frames setter (60 frames = 1s)");

        // Test property getters
        d1.Seconds = 2.0f;
        System.Reflection.PropertyInfo prop = typeof(Duration).GetProperty(nameof(Duration.Milliseconds), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        float ms = (float)prop.GetValue(d1);
        AssertFloat(ms, 2000f, "Milliseconds getter (2s = 2000ms)");

        prop = typeof(Duration).GetProperty(nameof(Duration.Minutes), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        float min = (float)prop.GetValue(d1);
        AssertFloat(min, 2.0f / 60f, "Minutes getter (2s = 2/60 min)");

        prop = typeof(Duration).GetProperty(nameof(Duration.Hours), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        float hr = (float)prop.GetValue(d1);
        AssertFloat(hr, 2.0f / 3600f, "Hours getter (2s = 2/3600 hr)");

        prop = typeof(Duration).GetProperty(nameof(Duration.Days), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        float day = (float)prop.GetValue(d1);
        AssertFloat(day, 2.0f / 86400f, "Days getter (2s = 2/86400 day)");

        prop = typeof(Duration).GetProperty(nameof(Duration.Frames), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        float frames = (float)prop.GetValue(d1);
        AssertFloat(frames, 120f, "Frames getter (2s = 120 frames)");

        // Test implicit conversion to float
        Debug.Log("\n--- Testing Implicit Conversion ---");
        d1.Seconds = 5.5f;
        float f1 = d1; // Implicit conversion
        AssertFloat(f1, 5.5f, "Implicit conversion Duration to float");

        // Test arithmetic operators: Duration op float
        Debug.Log("\n--- Testing Duration op float ---");
        d1.Seconds = 10f;
        Duration d2 = d1 + 5f;
        AssertFloat(d2.Seconds, 15f, "Duration + float");

        d2 = d1 - 3f;
        AssertFloat(d2.Seconds, 7f, "Duration - float");

        d2 = d1 * 2f;
        AssertFloat(d2.Seconds, 20f, "Duration * float");

        d2 = d1 / 2f;
        AssertFloat(d2.Seconds, 5f, "Duration / float");

        // Test arithmetic operators: float op Duration
        Debug.Log("\n--- Testing float op Duration ---");
        d1.Seconds = 10f;
        d2 = 5f + d1;
        AssertFloat(d2.Seconds, 15f, "float + Duration");

        d2 = 20f - d1;
        AssertFloat(d2.Seconds, 10f, "float - Duration");

        d2 = 3f * d1;
        AssertFloat(d2.Seconds, 30f, "float * Duration");

        d2 = 100f / d1;
        AssertFloat(d2.Seconds, 10f, "float / Duration");

        // Test arithmetic operators: Duration op Duration
        Debug.Log("\n--- Testing Duration op Duration ---");
        d1.Seconds = 10f;
        Duration d3 = new Duration { Seconds = 5f };
        d2 = d1 + d3;
        AssertFloat(d2.Seconds, 15f, "Duration + Duration");

        d2 = d1 - d3;
        AssertFloat(d2.Seconds, 5f, "Duration - Duration");

        d2 = d1 * d3;
        AssertFloat(d2.Seconds, 50f, "Duration * Duration");

        d2 = d1 / d3;
        AssertFloat(d2.Seconds, 2f, "Duration / Duration");

        // Test unary operators
        Debug.Log("\n--- Testing Unary Operators ---");
        d1.Seconds = 10f;
        d2 = -d1;
        AssertFloat(d2.Seconds, -10f, "Unary - operator");

        d2 = +d1;
        AssertFloat(d2.Seconds, 10f, "Unary + operator");

        // Test comparison operators: Duration op Duration
        Debug.Log("\n--- Testing Duration op Duration Comparisons ---");
        d1.Seconds = 10f;
        d3.Seconds = 5f;
        Duration d4 = new Duration { Seconds = 10f };

        Assert(d1 == d4, "Duration == Duration (equal)");
        Assert(d1 != d3, "Duration != Duration (not equal)");
        Assert(d3 < d1, "Duration < Duration");
        Assert(d1 > d3, "Duration > Duration");
        Assert(d3 <= d1, "Duration <= Duration");
        Assert(d1 >= d3, "Duration >= Duration");
        Assert(d1 >= d4, "Duration >= Duration (equal)");
        Assert(d1 <= d4, "Duration <= Duration (equal)");

        // Test comparison operators: Duration op float
        Debug.Log("\n--- Testing Duration op float Comparisons ---");
        d1.Seconds = 10f;
        Assert(d1 == 10f, "Duration == float");
        Assert(d1 != 5f, "Duration != float");
        Assert(d1 < 15f, "Duration < float");
        Assert(d1 > 5f, "Duration > float");
        Assert(d1 <= 10f, "Duration <= float (equal)");
        Assert(d1 <= 15f, "Duration <= float");
        Assert(d1 >= 10f, "Duration >= float (equal)");
        Assert(d1 >= 5f, "Duration >= float");

        // Test comparison operators: float op Duration
        Debug.Log("\n--- Testing float op Duration Comparisons ---");
        d1.Seconds = 10f;
        Assert(10f == d1, "float == Duration");
        Assert(5f != d1, "float != Duration");
        Assert(5f < d1, "float < Duration");
        Assert(15f > d1, "float > Duration");
        Assert(10f <= d1, "float <= Duration (equal)");
        Assert(5f <= d1, "float <= Duration");
        Assert(10f >= d1, "float >= Duration (equal)");
        Assert(15f >= d1, "float >= Duration");

        // Test Equals method
        Debug.Log("\n--- Testing Equals Method ---");
        d1.Seconds = 10f;
        d3.Seconds = 10f;
        d4.Seconds = 5f;
        Assert(d1.Equals(d3), "Duration.Equals(Duration) - equal");
        Assert(!d1.Equals(d4), "Duration.Equals(Duration) - not equal");
        Assert(d1.Equals(10f), "Duration.Equals(float) - equal");
        Assert(!d1.Equals(5f), "Duration.Equals(float) - not equal");

        // Test complex operations mixing properties and operators
        Debug.Log("\n--- Testing Complex Operations ---");
        Duration d5 = new Duration { Seconds = 1f };
        Duration d6 = new Duration { Minutes = 1f / 60f }; // 1 second in minutes
        Duration d7 = d5 + d6;
        AssertFloat(d7.Seconds, 2f, "Adding Duration set via Seconds and Duration set via Minutes");

        d5.Seconds = 10f;
        d6.Milliseconds = 1000f; // 1 second in milliseconds
        d7 = d5 - d6;
        AssertFloat(d7.Seconds, 9f, "Subtracting Duration set via Milliseconds from Duration set via Seconds");

        d5.Hours = 1f / 3600f; // 1 second
        d6.Days = 1f / 86400f; // 1 second
        d7 = d5 * d6;
        AssertFloat(d7.Seconds, 1f, "Multiplying Duration set via Hours and Duration set via Days");

        d5.Seconds = 100f;
        d6.Frames = 60f; // 1 second
        d7 = d5 / d6;
        AssertFloat(d7.Seconds, 100f, "Dividing Duration by Duration set via Frames");

        // Test with float operations
        d5.Seconds = 5f;
        d6 = d5 + 10f;
        AssertFloat(d6.Seconds, 15f, "Duration + float after setting via Seconds");

        d5.Minutes = 1f / 60f; // 1 second
        d6 = 20f - d5;
        AssertFloat(d6.Seconds, 19f, "float - Duration set via Minutes");

        d5.Milliseconds = 1000f; // 1 second
        d6 = d5 * 3f;
        AssertFloat(d6.Seconds, 3f, "Duration set via Milliseconds * float");

        d5.Hours = 1f / 3600f; // 1 second
        d6 = 10f / d5;
        AssertFloat(d6.Seconds, 10f, "float / Duration set via Hours");

        d5.Frames = 60f; // 1 second
        d6 = -d5;
        AssertFloat(d6.Seconds, -1f, "Unary - on Duration set via Frames");

        // Test implicit conversion in operations
        Debug.Log("\n--- Testing Implicit Conversion in Operations ---");
        d1.Seconds = 5f;
        float result = d1; // Implicit conversion
        AssertFloat(result, 5f, "Implicit conversion Duration to float");

        // Test implicit conversion in function calls
        float TestFunction(float value) => value * 2f;
        result = TestFunction(d1); // Should implicitly convert
        AssertFloat(result, 10f, "Implicit conversion in function parameter");

        // Test implicit conversion in comparisons
        Assert(d1 == 5f, "Implicit conversion in Duration == float comparison");
        Assert(5f == d1, "Implicit conversion in float == Duration comparison");

        Debug.Log($"\n=== Test Results: {passedTests}/{totalTests} tests passed ===");
    }
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(Duration))]
public class DurationDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty valueProperty = property.FindPropertyRelative("val");
        SerializedProperty defaultUnitProperty = property.FindPropertyRelative(nameof(Duration.defaultDurationMetric));

        // Get current Duration struct with preserved defaultDurationMetric
        DurationMetric currentDefaultUnit = (DurationMetric)defaultUnitProperty.enumValueIndex;
        Duration duration = new Duration(currentDefaultUnit) { Seconds = valueProperty.floatValue };

        // Create a unique key for storing foldout state per property
        string foldoutKey = property.propertyPath;
        bool wasExpanded = EditorPrefs.GetBool(foldoutKey, false);

        // Create modified label with unit type when collapsed
        GUIContent modifiedLabel = new GUIContent(label);
        if (!wasExpanded)
        {
            string unitName = currentDefaultUnit.ToString();
            modifiedLabel.text = $"{label.text} ({unitName})";
        }

        // Draw foldout
        Rect foldoutRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
        bool isExpanded = EditorGUI.Foldout(foldoutRect, wasExpanded, modifiedLabel, true);
        EditorPrefs.SetBool(foldoutKey, isExpanded);

        if (isExpanded)
        {
            EditorGUI.indentLevel++;
        }

        float yOffset = isExpanded ? EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing : 0f;

        // Seconds
        if (isExpanded || currentDefaultUnit == DurationMetric.Seconds)
        {
            duration = new Duration(currentDefaultUnit) { Seconds = valueProperty.floatValue };
            if (isExpanded)
            {
                Rect secondsRect = new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight);
                float newSeconds = EditorGUI.FloatField(secondsRect, "Seconds", duration.Seconds);
                if (newSeconds != duration.Seconds)
                {
                    duration.Seconds = newSeconds;
                    valueProperty.floatValue = duration.Seconds;
                }
                yOffset += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            else
            {
                // Draw input field directly after the label
                Rect fieldRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
                float newSeconds = EditorGUI.FloatField(fieldRect, duration.Seconds);
                if (newSeconds != duration.Seconds)
                {
                    duration.Seconds = newSeconds;
                    valueProperty.floatValue = duration.Seconds;
                }
            }
        }

        // Milliseconds
        if (isExpanded || currentDefaultUnit == DurationMetric.Milliseconds)
        {
            duration = new Duration(currentDefaultUnit) { Seconds = valueProperty.floatValue };
            float currentMilliseconds = GetPropertyValue(duration, nameof(Duration.Milliseconds));
            if (isExpanded)
            {
                Rect millisecondsRect = new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight);
                float newMilliseconds = EditorGUI.FloatField(millisecondsRect, "Milliseconds", currentMilliseconds);
                if (newMilliseconds != currentMilliseconds)
                {
                    duration.Milliseconds = newMilliseconds;
                    valueProperty.floatValue = duration.Seconds;
                }
                yOffset += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            else
            {
                // Draw input field directly after the label
                Rect fieldRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
                float newMilliseconds = EditorGUI.FloatField(fieldRect, currentMilliseconds);
                if (newMilliseconds != currentMilliseconds)
                {
                    duration.Milliseconds = newMilliseconds;
                    valueProperty.floatValue = duration.Seconds;
                }
            }
        }

        // Minutes
        if (isExpanded || currentDefaultUnit == DurationMetric.Minutes)
        {
            duration = new Duration(currentDefaultUnit) { Seconds = valueProperty.floatValue };
            float currentMinutes = GetPropertyValue(duration, nameof(Duration.Minutes));
            if (isExpanded)
            {
                Rect minutesRect = new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight);
                float newMinutes = EditorGUI.FloatField(minutesRect, "Minutes", currentMinutes);
                if (newMinutes != currentMinutes)
                {
                    duration.Minutes = newMinutes;
                    valueProperty.floatValue = duration.Seconds;
                }
                yOffset += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            else
            {
                // Draw input field directly after the label
                Rect fieldRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
                float newMinutes = EditorGUI.FloatField(fieldRect, currentMinutes);
                if (newMinutes != currentMinutes)
                {
                    duration.Minutes = newMinutes;
                    valueProperty.floatValue = duration.Seconds;
                }
            }
        }

        // Hours
        if (isExpanded || currentDefaultUnit == DurationMetric.Hours)
        {
            duration = new Duration(currentDefaultUnit) { Seconds = valueProperty.floatValue };
            float currentHours = GetPropertyValue(duration, nameof(Duration.Hours));
            if (isExpanded)
            {
                Rect hoursRect = new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight);
                float newHours = EditorGUI.FloatField(hoursRect, "Hours", currentHours);
                if (newHours != currentHours)
                {
                    duration.Hours = newHours;
                    valueProperty.floatValue = duration.Seconds;
                }
                yOffset += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            else
            {
                // Draw input field directly after the label
                Rect fieldRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
                float newHours = EditorGUI.FloatField(fieldRect, currentHours);
                if (newHours != currentHours)
                {
                    duration.Hours = newHours;
                    valueProperty.floatValue = duration.Seconds;
                }
            }
        }

        // Days
        if (isExpanded || currentDefaultUnit == DurationMetric.Days)
        {
            duration = new Duration(currentDefaultUnit) { Seconds = valueProperty.floatValue };
            float currentDays = GetPropertyValue(duration, nameof(Duration.Days));
            if (isExpanded)
            {
                Rect daysRect = new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight);
                float newDays = EditorGUI.FloatField(daysRect, "Days", currentDays);
                if (newDays != currentDays)
                {
                    duration.Days = newDays;
                    valueProperty.floatValue = duration.Seconds;
                }
                yOffset += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            else
            {
                // Draw input field directly after the label
                Rect fieldRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
                float newDays = EditorGUI.FloatField(fieldRect, currentDays);
                if (newDays != currentDays)
                {
                    duration.Days = newDays;
                    valueProperty.floatValue = duration.Seconds;
                }
            }
        }

        // Frames
        if (isExpanded || currentDefaultUnit == DurationMetric.Frames)
        {
            duration = new Duration(currentDefaultUnit) { Seconds = valueProperty.floatValue };
            float currentFrames = GetPropertyValue(duration, nameof(Duration.Frames));
            if (isExpanded)
            {
                Rect framesRect = new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight);
                float newFrames = EditorGUI.FloatField(framesRect, "Frames", currentFrames);
                if (newFrames != currentFrames)
                {
                    duration.Frames = newFrames;
                    valueProperty.floatValue = duration.Seconds;
                }
                yOffset += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            else
            {
                // Draw input field directly after the label
                Rect fieldRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
                float newFrames = EditorGUI.FloatField(fieldRect, currentFrames);
                if (newFrames != currentFrames)
                {
                    duration.Frames = newFrames;
                    valueProperty.floatValue = duration.Seconds;
                }
            }
        }

        if (isExpanded)
        {
            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    private string GetPropertyNameFromUnit(DurationMetric unit)
    {
        switch (unit)
        {
            case DurationMetric.Seconds:
                return nameof(Duration.Seconds);
            case DurationMetric.Milliseconds:
                return nameof(Duration.Milliseconds);
            case DurationMetric.Minutes:
                return nameof(Duration.Minutes);
            case DurationMetric.Hours:
                return nameof(Duration.Hours);
            case DurationMetric.Days:
                return nameof(Duration.Days);
            case DurationMetric.Frames:
                return nameof(Duration.Frames);
            default:
                throw new System.ArgumentException($"Unknown DurationMetric: {unit}");
        }
    }

    private float GetPropertyValue(Duration duration, string propertyName)
    {
        PropertyInfo prop = typeof(Duration).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        return (float)prop.GetValue(duration);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        string foldoutKey = property.propertyPath;
        bool isExpanded = EditorPrefs.GetBool(foldoutKey, false);

        if (isExpanded)
        {
            // Height for foldout + 6 properties (Seconds, Milliseconds, Minutes, Hours, Days, Frames)
            return (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 7;
        }
        else
        {
            // Height for foldout with Seconds on the same line
            return EditorGUIUtility.singleLineHeight;
        }
    }
}
#endif

[System.Serializable]
public struct Speed
{
    public Distance distance;
    public Duration duration;

    public Speed(Distance distance, Duration duration)
    {
        this.distance = distance;
        this.duration = duration;
    }

    public Speed(float metersPerSecond)
    {
        this.distance = new Distance(metersPerSecond);
        this.duration = new Duration(1f); // 1 second
    }

    // Value property returns meters per second
    public float Value
    {
        get
        {
            if (duration.Seconds == 0f)
                return 0f;
            return distance.Meters / duration.Seconds;
        }
        set
        {
            // If duration is zero, set it to 1 second
            if (duration.Seconds == 0f)
            {
                duration.Seconds = 1f;
            }
            distance.Meters = value * duration.Seconds;
        }
    }

    // Implicit conversion to float (returns meters per second)
    public static implicit operator float(Speed speed) => speed.Value;

    // Arithmetic operators: Speed op float
    public static Speed operator +(Speed a, float b) => new Speed { Value = a.Value + b };
    public static Speed operator -(Speed a, float b) => new Speed { Value = a.Value - b };
    public static Speed operator *(Speed a, float b) => new Speed { Value = a.Value * b };
    public static Speed operator /(Speed a, float b) => new Speed { Value = a.Value / b };

    // Arithmetic operators: float op Speed
    public static Speed operator +(float a, Speed b) => new Speed { Value = a + b.Value };
    public static Speed operator -(float a, Speed b) => new Speed { Value = a - b.Value };
    public static Speed operator *(float a, Speed b) => new Speed { Value = a * b.Value };
    public static Speed operator /(float a, Speed b) => new Speed { Value = a / b.Value };

    // Arithmetic operators: Speed op Speed
    public static Speed operator +(Speed a, Speed b) => new Speed { Value = a.Value + b.Value };
    public static Speed operator -(Speed a, Speed b) => new Speed { Value = a.Value - b.Value };
    public static Speed operator *(Speed a, Speed b) => new Speed { Value = a.Value * b.Value };
    public static Speed operator /(Speed a, Speed b) => new Speed { Value = a.Value / b.Value };

    // Unary operators
    public static Speed operator -(Speed a) => new Speed { Value = -a.Value };
    public static Speed operator +(Speed a) => a;

    // Comparison operators: Speed op Speed
    public static bool operator ==(Speed a, Speed b) => a.Value == b.Value;
    public static bool operator !=(Speed a, Speed b) => a.Value != b.Value;
    public static bool operator <(Speed a, Speed b) => a.Value < b.Value;
    public static bool operator >(Speed a, Speed b) => a.Value > b.Value;
    public static bool operator <=(Speed a, Speed b) => a.Value <= b.Value;
    public static bool operator >=(Speed a, Speed b) => a.Value >= b.Value;

    // Comparison operators: Speed op float
    public static bool operator ==(Speed a, float b) => a.Value == b;
    public static bool operator !=(Speed a, float b) => a.Value != b;
    public static bool operator <(Speed a, float b) => a.Value < b;
    public static bool operator >(Speed a, float b) => a.Value > b;
    public static bool operator <=(Speed a, float b) => a.Value <= b;
    public static bool operator >=(Speed a, float b) => a.Value >= b;

    // Comparison operators: float op Speed
    public static bool operator ==(float a, Speed b) => a == b.Value;
    public static bool operator !=(float a, Speed b) => a != b.Value;
    public static bool operator <(float a, Speed b) => a < b.Value;
    public static bool operator >(float a, Speed b) => a > b.Value;
    public static bool operator <=(float a, Speed b) => a <= b.Value;
    public static bool operator >=(float a, Speed b) => a >= b.Value;

    // Override Equals and GetHashCode for proper struct comparison
    public override bool Equals(object obj)
    {
        if (obj is Speed other)
            return Value == other.Value;
        if (obj is float otherFloat)
            return Value == otherFloat;
        return false;
    }

    public override int GetHashCode() => Value.GetHashCode();
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(Speed))]
public class SpeedDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty distanceProperty = property.FindPropertyRelative(nameof(Speed.distance));
        SerializedProperty durationProperty = property.FindPropertyRelative(nameof(Speed.duration));

        // Get current Speed values
        Distance distance = new Distance();
        Duration duration = new Duration();
        
        if (distanceProperty != null)
        {
            SerializedProperty distanceVal = distanceProperty.FindPropertyRelative("val");
            SerializedProperty distanceUnit = distanceProperty.FindPropertyRelative(nameof(Distance.defaultDistanceUnit));
            if (distanceVal != null && distanceUnit != null)
            {
                distance = new Distance((DistanceUnit)distanceUnit.enumValueIndex) { Meters = distanceVal.floatValue };
            }
        }
        
        if (durationProperty != null)
        {
            SerializedProperty durationVal = durationProperty.FindPropertyRelative("val");
            SerializedProperty DurationMetric = durationProperty.FindPropertyRelative(nameof(Duration.defaultDurationMetric));
            if (durationVal != null && DurationMetric != null)
            {
                duration = new Duration((DurationMetric)DurationMetric.enumValueIndex) { Seconds = durationVal.floatValue };
            }
        }

        Speed speed = new Speed(distance, duration);
        float currentValue = speed.Value;

        // Create a unique key for storing foldout state per property
        string foldoutKey = property.propertyPath;
        bool wasExpanded = EditorPrefs.GetBool(foldoutKey, false);

        // Create modified label with value when collapsed
        GUIContent modifiedLabel = new GUIContent(label);
        if (!wasExpanded)
        {
            modifiedLabel.text = $"{label.text} ({currentValue:F2} m/s)";
        }

        // Draw foldout
        Rect foldoutRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
        bool isExpanded = EditorGUI.Foldout(foldoutRect, wasExpanded, modifiedLabel, true);
        EditorPrefs.SetBool(foldoutKey, isExpanded);

        if (isExpanded)
        {
            EditorGUI.indentLevel++;
        }

        float yOffset = isExpanded ? EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing : 0f;

        // Value (meters per second)
        if (isExpanded || !wasExpanded)
        {
            if (isExpanded)
            {
                Rect valueRect = new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight);
                float newValue = EditorGUI.FloatField(valueRect, "Meters Per Second", currentValue);
                if (newValue != currentValue)
                {
                    speed.Value = newValue;
                    // Update the serialized properties
                    if (distanceProperty != null && distanceProperty.FindPropertyRelative("val") != null)
                    {
                        distanceProperty.FindPropertyRelative("val").floatValue = speed.distance.Meters;
                    }
                    if (durationProperty != null && durationProperty.FindPropertyRelative("val") != null)
                    {
                        durationProperty.FindPropertyRelative("val").floatValue = speed.duration.Seconds;
                    }
                }
                yOffset += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            else
            {
                // Draw input field directly after the label
                Rect fieldRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
                float newValue = EditorGUI.FloatField(fieldRect, currentValue);
                if (newValue != currentValue)
                {
                    speed.Value = newValue;
                    // Update the serialized properties
                    if (distanceProperty != null && distanceProperty.FindPropertyRelative("val") != null)
                    {
                        distanceProperty.FindPropertyRelative("val").floatValue = speed.distance.Meters;
                    }
                    if (durationProperty != null && durationProperty.FindPropertyRelative("val") != null)
                    {
                        durationProperty.FindPropertyRelative("val").floatValue = speed.duration.Seconds;
                    }
                }
            }
        }

        // Distance and Duration fields when expanded
        if (isExpanded)
        {
            Rect distanceRect = new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight);
            float distanceHeight = EditorGUI.GetPropertyHeight(distanceProperty, true);
            EditorGUI.PropertyField(distanceRect, distanceProperty, new GUIContent("Distance"), true);
            yOffset += distanceHeight + EditorGUIUtility.standardVerticalSpacing;

            Rect durationRect = new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(durationRect, durationProperty, new GUIContent("Duration"), true);
        }

        if (isExpanded)
        {
            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        string foldoutKey = property.propertyPath;
        bool isExpanded = EditorPrefs.GetBool(foldoutKey, false);

        if (isExpanded)
        {
            SerializedProperty distanceProperty = property.FindPropertyRelative(nameof(Speed.distance));
            SerializedProperty durationProperty = property.FindPropertyRelative(nameof(Speed.duration));
            
            float height = EditorGUIUtility.singleLineHeight; // Foldout
            height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // Value field
            if (distanceProperty != null)
                height += EditorGUI.GetPropertyHeight(distanceProperty, true) + EditorGUIUtility.standardVerticalSpacing;
            if (durationProperty != null)
                height += EditorGUI.GetPropertyHeight(durationProperty, true);
            
            return height;
        }
        else
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}
#endif

