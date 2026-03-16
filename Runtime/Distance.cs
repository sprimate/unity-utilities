using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif

public enum DistanceUnit
{
    Yards,
    Meters,
    Millimeters,
    Feet,
    Inches,
    Kilometers,
    Miles
}
[System.Serializable]
public struct Distance
{
    public Distance(float value, DistanceUnit unit) : this(unit)
    {
        SetValue(value, unit);
        this.defaultDistanceUnit = unit;
    }

    public Distance(float value) : this(value, DistanceUnit.Meters) { }

    public Distance(DistanceUnit defaultDistanceUnit)
    {
        this.defaultDistanceUnit = defaultDistanceUnit;
        val = 0f;
    }

    public void SetValue(float value, DistanceUnit unit)
    {
        switch (unit)
        {
            case DistanceUnit.Meters:
                Meters = value;
                break;
            case DistanceUnit.Millimeters:
                Millimeters = value;
                break;
            case DistanceUnit.Feet:
                Feet = value;
                break;
            case DistanceUnit.Inches:
                Inches = value;
                break;
            case DistanceUnit.Yards:
                Yards = value;
                break;
            case DistanceUnit.Kilometers:
                Kilometers = value;
                break;
            case DistanceUnit.Miles:
                Miles = value;
                break;
            default:
                throw new System.Exception("Invalid distance unit");
        }
    }

    public float GetValue(DistanceUnit unit)
    {
        switch (unit)
        {
            case DistanceUnit.Meters:
                return Meters;
            case DistanceUnit.Millimeters:
                return Millimeters;
            case DistanceUnit.Feet:
                return Feet;
            case DistanceUnit.Inches:
                return Inches;
            case DistanceUnit.Yards:
                return Yards;
            case DistanceUnit.Kilometers:
                return Kilometers;
            case DistanceUnit.Miles:
                return Miles;
            default:
                throw new System.Exception("Invalid distance unit");
        }
    }
    //[HideInInspector]
    public DistanceUnit defaultDistanceUnit;
    [SerializeField]
    private float val; // Stored in meters

    // Meters property
    public float Meters
    {
        get => val;
        set => val = value;
    }

    // Millimeters property
    public float Millimeters
    {
        get => val * 1000f; // Convert meters to millimeters
        set => val = value / 1000f; // Convert millimeters to meters
    }

    // Feet property
    public float Feet
    {
        get => val * 3.28084f; // Convert meters to feet
        set => val = value / 3.28084f; // Convert feet to meters
    }

    // Inches property
    public float Inches
    {
        get => val * 39.3701f; // Convert meters to inches
        set => val = value / 39.3701f; // Convert inches to meters
    }

    // Yards property
    public float Yards
    {
        get => val * 1.09361f; // Convert meters to yards
        set => val = value / 1.09361f; // Convert yards to meters
    }

    // Kilometers property
    public float Kilometers
    {
        get => val * 0.001f; // Convert meters to kilometers
        set => val = value * 1000f; // Convert kilometers to meters
    }

    // Miles property
    public float Miles
    {
        get => val * 0.000621371f; // Convert meters to miles
        set => val = value / 0.000621371f; // Convert miles to meters
    }

    public string ToFeetAndInches(string noFeetDisplayFormat = "0.##", string feetAndInchesDisplayFormat = "0")
    {
        // Round to nearest integer to avoid floating point precision issues
        int totalInches = Mathf.RoundToInt(Inches);
        bool isNegative = totalInches < 0;
        int absInches = Mathf.Abs(totalInches);
        int ft = absInches / 12;
        int inch = absInches % 12;
        return $"{(isNegative ? "-" : "")}{ft.ToString(feetAndInchesDisplayFormat)}' {inch.ToString(feetAndInchesDisplayFormat)}\"";
    }

    // Implicit conversion to float (returns meters)
    public static implicit operator float(Distance distance) => distance.val;

    // Implicit conversion from float to Distance
    public static implicit operator Distance(float value) => new Distance(value);

    // Arithmetic operators: Distance op float
    public static Distance operator +(Distance a, float b) => new Distance { Meters = a.val + b };
    public static Distance operator -(Distance a, float b) => new Distance { Meters = a.val - b };
    public static Distance operator *(Distance a, float b) => new Distance { Meters = a.val * b };
    public static Distance operator /(Distance a, float b) => new Distance { Meters = a.val / b };

    // Arithmetic operators: float op Distance
    public static Distance operator +(float a, Distance b) => new Distance { Meters = a + b.val };
    public static Distance operator -(float a, Distance b) => new Distance { Meters = a - b.val };
    public static Distance operator *(float a, Distance b) => new Distance { Meters = a * b.val };
    public static Distance operator /(float a, Distance b) => new Distance { Meters = a / b.val };

    // Arithmetic operators: Distance op Distance
    public static Distance operator +(Distance a, Distance b) => new Distance { Meters = a.val + b.val };
    public static Distance operator -(Distance a, Distance b) => new Distance { Meters = a.val - b.val };
    public static Distance operator *(Distance a, Distance b) => new Distance { Meters = a.val * b.val };
    public static Distance operator /(Distance a, Distance b) => new Distance { Meters = a.val / b.val };

    // Unary operators
    public static Distance operator -(Distance a) => new Distance { Meters = -a.val };
    public static Distance operator +(Distance a) => a;

    // Comparison operators: Distance op Distance
    public static bool operator ==(Distance a, Distance b) => a.val == b.val;
    public static bool operator !=(Distance a, Distance b) => a.val != b.val;
    public static bool operator <(Distance a, Distance b) => a.val < b.val;
    public static bool operator >(Distance a, Distance b) => a.val > b.val;
    public static bool operator <=(Distance a, Distance b) => a.val <= b.val;
    public static bool operator >=(Distance a, Distance b) => a.val >= b.val;

    // Comparison operators: Distance op float
    public static bool operator ==(Distance a, float b) => a.val == b;
    public static bool operator !=(Distance a, float b) => a.val != b;
    public static bool operator <(Distance a, float b) => a.val < b;
    public static bool operator >(Distance a, float b) => a.val > b;
    public static bool operator <=(Distance a, float b) => a.val <= b;
    public static bool operator >=(Distance a, float b) => a.val >= b;

    // Comparison operators: float op Distance
    public static bool operator ==(float a, Distance b) => a == b.val;
    public static bool operator !=(float a, Distance b) => a != b.val;
    public static bool operator <(float a, Distance b) => a < b.val;
    public static bool operator >(float a, Distance b) => a > b.val;
    public static bool operator <=(float a, Distance b) => a <= b.val;
    public static bool operator >=(float a, Distance b) => a >= b.val;

    // Override Equals and GetHashCode for proper struct comparison
    public override bool Equals(object obj)
    {
        if (obj is Distance other)
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

        Debug.Log("=== Distance Test Suite ===");

        // Test property setters and getters
        Debug.Log("\n--- Testing Properties ---");

        Distance d1 = new Distance { Meters = 1.0f };
        AssertFloat(d1.Meters, 1.0f, "Meters getter");

        d1.Millimeters = 1000f;
        AssertFloat(d1.Meters, 1.0f, "Millimeters setter (1000mm = 1m)");

        d1.Feet = 3.28084f;
        AssertFloat(d1.Meters, 1.0f, "Feet setter (3.28084ft = 1m)");

        d1.Inches = 39.3701f;
        AssertFloat(d1.Meters, 1.0f, "Inches setter (39.3701in = 1m)");

        d1.Yards = 1.09361f;
        AssertFloat(d1.Meters, 1.0f, "Yards setter (1.09361yd = 1m)");

        d1.Kilometers = 0.001f;
        AssertFloat(d1.Meters, 1.0f, "Kilometers setter (0.001km = 1m)");

        // Test property getters (using reflection since they're private)
        d1.Meters = 2.0f;
        System.Reflection.PropertyInfo prop = typeof(Distance).GetProperty(nameof(Distance.Millimeters), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        float mm = (float)prop.GetValue(d1);
        AssertFloat(mm, 2000f, "Millimeters getter (2m = 2000mm)");

        prop = typeof(Distance).GetProperty(nameof(Distance.Feet), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        float ft = (float)prop.GetValue(d1);
        AssertFloat(ft, 6.56168f, "Feet getter (2m = ~6.56ft)");

        prop = typeof(Distance).GetProperty(nameof(Distance.Inches), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        float inVal = (float)prop.GetValue(d1);
        AssertFloat(inVal, 78.7402f, "Inches getter (2m = ~78.74in)");

        prop = typeof(Distance).GetProperty(nameof(Distance.Yards), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        float yd = (float)prop.GetValue(d1);
        AssertFloat(yd, 2.18722f, "Yards getter (2m = ~2.19yd)");

        prop = typeof(Distance).GetProperty(nameof(Distance.Kilometers), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        float km = (float)prop.GetValue(d1);
        AssertFloat(km, 0.002f, "Kilometers getter (2m = 0.002km)");

        // Test implicit conversion to float
        Debug.Log("\n--- Testing Implicit Conversion ---");
        d1.Meters = 5.5f;
        float f1 = d1; // Implicit conversion
        AssertFloat(f1, 5.5f, "Implicit conversion Distance to float");

        // Test arithmetic operators: Distance op float
        Debug.Log("\n--- Testing Distance op float ---");
        d1.Meters = 10f;
        Distance d2 = d1 + 5f;
        AssertFloat(d2.Meters, 15f, "Distance + float");

        d2 = d1 - 3f;
        AssertFloat(d2.Meters, 7f, "Distance - float");

        d2 = d1 * 2f;
        AssertFloat(d2.Meters, 20f, "Distance * float");

        d2 = d1 / 2f;
        AssertFloat(d2.Meters, 5f, "Distance / float");

        // Test arithmetic operators: float op Distance
        Debug.Log("\n--- Testing float op Distance ---");
        d1.Meters = 10f;
        d2 = 5f + d1;
        AssertFloat(d2.Meters, 15f, "float + Distance");

        d2 = 20f - d1;
        AssertFloat(d2.Meters, 10f, "float - Distance");

        d2 = 3f * d1;
        AssertFloat(d2.Meters, 30f, "float * Distance");

        d2 = 100f / d1;
        AssertFloat(d2.Meters, 10f, "float / Distance");

        // Test arithmetic operators: Distance op Distance
        Debug.Log("\n--- Testing Distance op Distance ---");
        d1.Meters = 10f;
        Distance d3 = new Distance { Meters = 5f };
        d2 = d1 + d3;
        AssertFloat(d2.Meters, 15f, "Distance + Distance");

        d2 = d1 - d3;
        AssertFloat(d2.Meters, 5f, "Distance - Distance");

        d2 = d1 * d3;
        AssertFloat(d2.Meters, 50f, "Distance * Distance");

        d2 = d1 / d3;
        AssertFloat(d2.Meters, 2f, "Distance / Distance");

        // Test unary operators
        Debug.Log("\n--- Testing Unary Operators ---");
        d1.Meters = 10f;
        d2 = -d1;
        AssertFloat(d2.Meters, -10f, "Unary - operator");

        d2 = +d1;
        AssertFloat(d2.Meters, 10f, "Unary + operator");

        // Test comparison operators: Distance op Distance
        Debug.Log("\n--- Testing Distance op Distance Comparisons ---");
        d1.Meters = 10f;
        d3.Meters = 5f;
        Distance d4 = new Distance { Meters = 10f };

        Assert(d1 == d4, "Distance == Distance (equal)");
        Assert(d1 != d3, "Distance != Distance (not equal)");
        Assert(d3 < d1, "Distance < Distance");
        Assert(d1 > d3, "Distance > Distance");
        Assert(d3 <= d1, "Distance <= Distance");
        Assert(d1 >= d3, "Distance >= Distance");
        Assert(d1 >= d4, "Distance >= Distance (equal)");
        Assert(d1 <= d4, "Distance <= Distance (equal)");

        // Test comparison operators: Distance op float
        Debug.Log("\n--- Testing Distance op float Comparisons ---");
        d1.Meters = 10f;
        Assert(d1 == 10f, "Distance == float");
        Assert(d1 != 5f, "Distance != float");
        Assert(d1 < 15f, "Distance < float");
        Assert(d1 > 5f, "Distance > float");
        Assert(d1 <= 10f, "Distance <= float (equal)");
        Assert(d1 <= 15f, "Distance <= float");
        Assert(d1 >= 10f, "Distance >= float (equal)");
        Assert(d1 >= 5f, "Distance >= float");

        // Test comparison operators: float op Distance
        Debug.Log("\n--- Testing float op Distance Comparisons ---");
        d1.Meters = 10f;
        Assert(10f == d1, "float == Distance");
        Assert(5f != d1, "float != Distance");
        Assert(5f < d1, "float < Distance");
        Assert(15f > d1, "float > Distance");
        Assert(10f <= d1, "float <= Distance (equal)");
        Assert(5f <= d1, "float <= Distance");
        Assert(10f >= d1, "float >= Distance (equal)");
        Assert(15f >= d1, "float >= Distance");

        // Test Equals method
        Debug.Log("\n--- Testing Equals Method ---");
        d1.Meters = 10f;
        d3.Meters = 10f;
        d4.Meters = 5f;
        Assert(d1.Equals(d3), "Distance.Equals(Distance) - equal");
        Assert(!d1.Equals(d4), "Distance.Equals(Distance) - not equal");
        Assert(d1.Equals(10f), "Distance.Equals(float) - equal");
        Assert(!d1.Equals(5f), "Distance.Equals(float) - not equal");

        // Test complex operations mixing properties and operators
        Debug.Log("\n--- Testing Complex Operations ---");
        Distance d5 = new Distance { Meters = 1f };
        Distance d6 = new Distance { Feet = 3.28084f }; // 1 meter in feet
        Distance d7 = d5 + d6;
        AssertFloat(d7.Meters, 2f, "Adding Distance set via Meters and Distance set via Feet");

        d5.Meters = 10f;
        d6.Inches = 39.3701f; // 1 meter in inches
        d7 = d5 - d6;
        AssertFloat(d7.Meters, 9f, "Subtracting Distance set via Inches from Distance set via Meters");

        d5.Yards = 1.09361f; // 1 meter
        d6.Kilometers = 0.001f; // 1 meter
        d7 = d5 * d6;
        AssertFloat(d7.Meters, 1f, "Multiplying Distance set via Yards and Distance set via Kilometers");

        d5.Meters = 100f;
        d6.Millimeters = 1000f; // 1 meter
        d7 = d5 / d6;
        AssertFloat(d7.Meters, 100f, "Dividing Distance by Distance set via Millimeters");

        // Test with float operations
        d5.Meters = 5f;
        d6 = d5 + 10f;
        AssertFloat(d6.Meters, 15f, "Distance + float after setting via Meters");

        d5.Feet = 3.28084f; // 1 meter
        d6 = 20f - d5;
        AssertFloat(d6.Meters, 19f, "float - Distance set via Feet");

        d5.Inches = 39.3701f; // 1 meter
        d6 = d5 * 3f;
        AssertFloat(d6.Meters, 3f, "Distance set via Inches * float");

        d5.Yards = 1.09361f; // 1 meter
        d6 = 10f / d5;
        AssertFloat(d6.Meters, 10f, "float / Distance set via Yards");

        d5.Kilometers = 0.001f; // 1 meter
        d6 = -d5;
        AssertFloat(d6.Meters, -1f, "Unary - on Distance set via Kilometers");

        // Test implicit conversion in operations
        Debug.Log("\n--- Testing Implicit Conversion in Operations ---");
        d1.Meters = 5f;
        float result = d1; // Implicit conversion
        AssertFloat(result, 5f, "Implicit conversion Distance to float");

        // Test implicit conversion in function calls
        float TestFunction(float value) => value * 2f;
        result = TestFunction(d1); // Should implicitly convert
        AssertFloat(result, 10f, "Implicit conversion in function parameter");

        // Test implicit conversion in comparisons
        Assert(d1 == 5f, "Implicit conversion in Distance == float comparison");
        Assert(5f == d1, "Implicit conversion in float == Distance comparison");

        Debug.Log($"\n=== Test Results: {passedTests}/{totalTests} tests passed ===");
    }
}

[System.Serializable]
public struct DistanceVector2
{
    public DistanceVector2(Vector2 v, DistanceUnit defaultDistanceUnit)
    {
        x = new Distance(v.x, defaultDistanceUnit);
        y = new Distance(v.y, defaultDistanceUnit);
    }

    public DistanceVector2(Vector2 v) : this(v, DistanceUnit.Meters) { }
    public DistanceVector2(float x, float y, DistanceUnit defaultDistanceUnit) : this(new Vector2(x, y), defaultDistanceUnit) { }
    public DistanceVector2(Distance x, Distance y) : this(new Vector2(x.Meters, y.Meters), DistanceUnit.Meters) { }
    public DistanceVector2(DistanceUnit defaultDistanceUnit) : this(Vector2.zero, defaultDistanceUnit) { }
    public Distance x;
    public Distance y;
    public Vector2 Meters { get => new Vector2(x.Meters, y.Meters); set { x.Meters = value.x; y.Meters = value.y; } }
    public Vector2 Millimeters { get => new Vector2(x.Millimeters, y.Millimeters); set { x.Millimeters = value.x; y.Millimeters = value.y; } }
    public Vector2 Feet { get => new Vector2(x.Feet, y.Feet); set { x.Feet = value.x; y.Feet = value.y; } }
    public Vector2 Inches { get => new Vector2(x.Inches, y.Inches); set { x.Inches = value.x; y.Inches = value.y; } }
    public Vector2 Yards { get => new Vector2(x.Yards, y.Yards); set { x.Yards = value.x; y.Yards = value.y; } }
    public Vector2 Kilometers { get => new Vector2(x.Kilometers, y.Kilometers); set { x.Kilometers = value.x; y.Kilometers = value.y; } }
    public Vector2 Miles { get => new Vector2(x.Miles, y.Miles); set { x.Miles = value.x; y.Miles = value.y; } }

    public void SetValue(Vector2 value, DistanceUnit unit)
    {
        switch (unit)
        {
            case DistanceUnit.Meters:
                Meters = value;
                break;
            case DistanceUnit.Millimeters:
                Millimeters = value;
                break;
            case DistanceUnit.Feet:
                Feet = value;
                break;
            case DistanceUnit.Inches:
                Inches = value;
                break;
            case DistanceUnit.Yards:
                Yards = value;
                break;
            case DistanceUnit.Kilometers:
                Kilometers = value;
                break;
            case DistanceUnit.Miles:
                Miles = value;
                break;
            default:
                throw new System.Exception("Invalid distance unit");
        }
    }

    public Vector2 GetValue(DistanceUnit unit)
    {
        switch (unit)
        {
            case DistanceUnit.Meters:
                return Meters;
            case DistanceUnit.Millimeters:
                return Millimeters;
            case DistanceUnit.Feet:
                return Feet;
            case DistanceUnit.Inches:
                return Inches;
            case DistanceUnit.Yards:
                return Yards;
            case DistanceUnit.Kilometers:
                return Kilometers;
            case DistanceUnit.Miles:
                return Miles;
            default:
                throw new System.Exception("Invalid distance unit");
        }
    }

    // Implicit conversion to Vector2 (returns meters)
    public static implicit operator Vector2(DistanceVector2 dv) => new Vector2(dv.x.Meters, dv.y.Meters);

    // Implicit conversion from Vector2 to DistanceVector2
    public static implicit operator DistanceVector2(Vector2 v) => new DistanceVector2 { x = v.x, y = v.y };

    public static implicit operator DistanceVector2(Vector3 v) => new DistanceVector2 { x = v.x, y = v.y };

    // Implicit conversion from DistanceVector3 to DistanceVector2
    public static implicit operator DistanceVector2(DistanceVector3 dv) => new DistanceVector2 { x = dv.x, y = dv.y };

    // Arithmetic operators: DistanceVector2 op Vector2
    public static DistanceVector2 operator +(DistanceVector2 a, Vector2 b) => new DistanceVector2 { x = a.x + b.x, y = a.y + b.y };
    public static DistanceVector2 operator -(DistanceVector2 a, Vector2 b) => new DistanceVector2 { x = a.x - b.x, y = a.y - b.y };
    public static DistanceVector2 operator *(DistanceVector2 a, Vector2 b) => new DistanceVector2 { x = a.x * b.x, y = a.y * b.y };
    public static DistanceVector2 operator /(DistanceVector2 a, Vector2 b) => new DistanceVector2 { x = a.x / b.x, y = a.y / b.y };

    // Arithmetic operators: Vector2 op DistanceVector2
    public static DistanceVector2 operator +(Vector2 a, DistanceVector2 b) => new DistanceVector2 { x = a.x + b.x, y = a.y + b.y };
    public static DistanceVector2 operator -(Vector2 a, DistanceVector2 b) => new DistanceVector2 { x = a.x - b.x, y = a.y - b.y };
    public static DistanceVector2 operator *(Vector2 a, DistanceVector2 b) => new DistanceVector2 { x = a.x * b.x, y = a.y * b.y };
    public static DistanceVector2 operator /(Vector2 a, DistanceVector2 b) => new DistanceVector2 { x = a.x / b.x, y = a.y / b.y };

    // Arithmetic operators: DistanceVector2 op DistanceVector2
    public static DistanceVector2 operator +(DistanceVector2 a, DistanceVector2 b) => new DistanceVector2 { x = a.x + b.x, y = a.y + b.y };
    public static DistanceVector2 operator -(DistanceVector2 a, DistanceVector2 b) => new DistanceVector2 { x = a.x - b.x, y = a.y - b.y };
    public static DistanceVector2 operator *(DistanceVector2 a, DistanceVector2 b) => new DistanceVector2 { x = a.x * b.x, y = a.y * b.y };
    public static DistanceVector2 operator /(DistanceVector2 a, DistanceVector2 b) => new DistanceVector2 { x = a.x / b.x, y = a.y / b.y };

    // Arithmetic operators: DistanceVector2 op float
    public static DistanceVector2 operator +(DistanceVector2 a, float b) => new DistanceVector2 { x = a.x + b, y = a.y + b };
    public static DistanceVector2 operator -(DistanceVector2 a, float b) => new DistanceVector2 { x = a.x - b, y = a.y - b };
    public static DistanceVector2 operator *(DistanceVector2 a, float b) => new DistanceVector2 { x = a.x * b, y = a.y * b };
    public static DistanceVector2 operator /(DistanceVector2 a, float b) => new DistanceVector2 { x = a.x / b, y = a.y / b };

    // Arithmetic operators: float op DistanceVector2
    public static DistanceVector2 operator +(float a, DistanceVector2 b) => new DistanceVector2 { x = a + b.x, y = a + b.y };
    public static DistanceVector2 operator -(float a, DistanceVector2 b) => new DistanceVector2 { x = a - b.x, y = a - b.y };
    public static DistanceVector2 operator *(float a, DistanceVector2 b) => new DistanceVector2 { x = a * b.x, y = a * b.y };
    public static DistanceVector2 operator /(float a, DistanceVector2 b) => new DistanceVector2 { x = a / b.x, y = a / b.y };

    // Arithmetic operators: DistanceVector2 op Distance
    public static DistanceVector2 operator +(DistanceVector2 a, Distance b) => new DistanceVector2 { x = a.x + b, y = a.y + b };
    public static DistanceVector2 operator -(DistanceVector2 a, Distance b) => new DistanceVector2 { x = a.x - b, y = a.y - b };
    public static DistanceVector2 operator *(DistanceVector2 a, Distance b) => new DistanceVector2 { x = a.x * b, y = a.y * b };
    public static DistanceVector2 operator /(DistanceVector2 a, Distance b) => new DistanceVector2 { x = a.x / b, y = a.y / b };

    // Arithmetic operators: Distance op DistanceVector2
    public static DistanceVector2 operator +(Distance a, DistanceVector2 b) => new DistanceVector2 { x = a + b.x, y = a + b.y };
    public static DistanceVector2 operator -(Distance a, DistanceVector2 b) => new DistanceVector2 { x = a - b.x, y = a - b.y };
    public static DistanceVector2 operator *(Distance a, DistanceVector2 b) => new DistanceVector2 { x = a * b.x, y = a * b.y };
    public static DistanceVector2 operator /(Distance a, DistanceVector2 b) => new DistanceVector2 { x = a / b.x, y = a / b.y };

    // Unary operators
    public static DistanceVector2 operator -(DistanceVector2 a) => new DistanceVector2 { x = -a.x, y = -a.y };
    public static DistanceVector2 operator +(DistanceVector2 a) => a;

    // Comparison operators: DistanceVector2 op DistanceVector2
    public static bool operator ==(DistanceVector2 a, DistanceVector2 b) => a.x == b.x && a.y == b.y;
    public static bool operator !=(DistanceVector2 a, DistanceVector2 b) => a.x != b.x || a.y != b.y;

    // Comparison operators: DistanceVector2 op Vector2
    public static bool operator ==(DistanceVector2 a, Vector2 b) => a.x == b.x && a.y == b.y;
    public static bool operator !=(DistanceVector2 a, Vector2 b) => a.x != b.x || a.y != b.y;

    // Comparison operators: Vector2 op DistanceVector2
    public static bool operator ==(Vector2 a, DistanceVector2 b) => a.x == b.x && a.y == b.y;
    public static bool operator !=(Vector2 a, DistanceVector2 b) => a.x != b.x || a.y != b.y;

    // Override Equals and GetHashCode
    public override bool Equals(object obj)
    {
        if (obj is DistanceVector2 other)
            return x == other.x && y == other.y;
        if (obj is Vector2 otherVec)
            return x == otherVec.x && y == otherVec.y;
        return false;
    }

    public override int GetHashCode() => (x.GetHashCode() * 397) ^ y.GetHashCode();
}

[System.Serializable]
public struct DistanceVector3
{

    public DistanceVector3(Vector3 v, DistanceUnit defaultDistanceUnit)
    {
        x = new Distance(v.x, defaultDistanceUnit);
        y = new Distance(v.y, defaultDistanceUnit);
        z = new Distance(v.z, defaultDistanceUnit);
    }

    public DistanceVector3(float x, float y, float z, DistanceUnit defaultDistanceUnit) : this(new Vector3(x, y, z), defaultDistanceUnit) { }
    public DistanceVector3(Vector3 v) : this(v, DistanceUnit.Meters) { }
    public DistanceVector3(DistanceUnit defaultDistanceUnit) : this(Vector3.zero, defaultDistanceUnit)
    {
    }

    public Distance x;
    public Distance y;
    public Distance z;
    public Vector3 Meters { get => new Vector3(x.Meters, y.Meters, z.Meters); set { x.Meters = value.x; y.Meters = value.y; z.Meters = value.z; } }
    public Vector3 Millimeters { get => new Vector3(x.Millimeters, y.Millimeters, z.Millimeters); set { x.Millimeters = value.x; y.Millimeters = value.y; z.Millimeters = value.z; } }
    public Vector3 Feet { get => new Vector3(x.Feet, y.Feet, z.Feet); set { x.Feet = value.x; y.Feet = value.y; z.Feet = value.z; } }
    public Vector3 Inches { get => new Vector3(x.Inches, y.Inches, z.Inches); set { x.Inches = value.x; y.Inches = value.y; z.Inches = value.z; } }
    public Vector3 Yards { get => new Vector3(x.Yards, y.Yards, z.Yards); set { x.Yards = value.x; y.Yards = value.y; z.Yards = value.z; } }
    public Vector3 Kilometers { get => new Vector3(x.Kilometers, y.Kilometers, z.Kilometers); set { x.Kilometers = value.x; y.Kilometers = value.y; z.Kilometers = value.z; } }
    public Vector3 Miles { get => new Vector3(x.Miles, y.Miles, z.Miles); set { x.Miles = value.x; y.Miles = value.y; z.Miles = value.z; } }

    public void SetValue(Vector3 value, DistanceUnit unit)
    {
        switch (unit)
        {
            case DistanceUnit.Meters:
                Meters = value;
                break;
            case DistanceUnit.Millimeters:
                Millimeters = value;
                break;
            case DistanceUnit.Feet:
                Feet = value;
                break;
            case DistanceUnit.Inches:
                Inches = value;
                break;
            case DistanceUnit.Yards:
                Yards = value;
                break;
            case DistanceUnit.Kilometers:
                Kilometers = value;
                break;
            case DistanceUnit.Miles:
                Miles = value;
                break;
            default:
                throw new System.Exception("Invalid distance unit");
        }
    }

    public Vector3 GetValue(DistanceUnit unit)
    {
        switch (unit)
        {
            case DistanceUnit.Meters:
                return Meters;
            case DistanceUnit.Millimeters:
                return Millimeters;
            case DistanceUnit.Feet:
                return Feet;
            case DistanceUnit.Inches:
                return Inches;
            case DistanceUnit.Yards:
                return Yards;
            case DistanceUnit.Kilometers:
                return Kilometers;
            case DistanceUnit.Miles:
                return Miles;
            default:
                throw new System.Exception("Invalid distance unit");
        }
    }

    // Implicit conversion to Vector3 (returns meters)
    public static implicit operator Vector3(DistanceVector3 dv) => new Vector3(dv.x.Meters, dv.y.Meters, dv.z.Meters);

    // Implicit conversion from Vector3 to DistanceVector3
    public static implicit operator DistanceVector3(Vector3 v) => new DistanceVector3 { x = v.x, y = v.y, z = v.z };

    public static implicit operator DistanceVector3(Vector2 v) => new DistanceVector3 { x = v.x, y = v.y, z = 0f };


    // Implicit conversion from DistanceVector2 to DistanceVector3
    public static implicit operator DistanceVector3(DistanceVector2 dv) => new DistanceVector3 { x = dv.x, y = dv.y, z = 0f };

    // Arithmetic operators: DistanceVector3 op Vector3
    public static DistanceVector3 operator +(DistanceVector3 a, Vector3 b) => new DistanceVector3 { x = a.x + b.x, y = a.y + b.y, z = a.z + b.z };
    public static DistanceVector3 operator -(DistanceVector3 a, Vector3 b) => new DistanceVector3 { x = a.x - b.x, y = a.y - b.y, z = a.z - b.z };
    public static DistanceVector3 operator *(DistanceVector3 a, Vector3 b) => new DistanceVector3 { x = a.x * b.x, y = a.y * b.y, z = a.z * b.z };
    public static DistanceVector3 operator /(DistanceVector3 a, Vector3 b) => new DistanceVector3 { x = a.x / b.x, y = a.y / b.y, z = a.z / b.z };

    // Arithmetic operators: Vector3 op DistanceVector3
    public static DistanceVector3 operator +(Vector3 a, DistanceVector3 b) => new DistanceVector3 { x = a.x + b.x, y = a.y + b.y, z = a.z + b.z };
    public static DistanceVector3 operator -(Vector3 a, DistanceVector3 b) => new DistanceVector3 { x = a.x - b.x, y = a.y - b.y, z = a.z - b.z };
    public static DistanceVector3 operator *(Vector3 a, DistanceVector3 b) => new DistanceVector3 { x = a.x * b.x, y = a.y * b.y, z = a.z * b.z };
    public static DistanceVector3 operator /(Vector3 a, DistanceVector3 b) => new DistanceVector3 { x = a.x / b.x, y = a.y / b.y, z = a.z / b.z };

    // Arithmetic operators: DistanceVector3 op DistanceVector3
    public static DistanceVector3 operator +(DistanceVector3 a, DistanceVector3 b) => new DistanceVector3 { x = a.x + b.x, y = a.y + b.y, z = a.z + b.z };
    public static DistanceVector3 operator -(DistanceVector3 a, DistanceVector3 b) => new DistanceVector3 { x = a.x - b.x, y = a.y - b.y, z = a.z - b.z };
    public static DistanceVector3 operator *(DistanceVector3 a, DistanceVector3 b) => new DistanceVector3 { x = a.x * b.x, y = a.y * b.y, z = a.z * b.z };
    public static DistanceVector3 operator /(DistanceVector3 a, DistanceVector3 b) => new DistanceVector3 { x = a.x / b.x, y = a.y / b.y, z = a.z / b.z };

    // Arithmetic operators: DistanceVector3 op float
    public static DistanceVector3 operator +(DistanceVector3 a, float b) => new DistanceVector3 { x = a.x + b, y = a.y + b, z = a.z + b };
    public static DistanceVector3 operator -(DistanceVector3 a, float b) => new DistanceVector3 { x = a.x - b, y = a.y - b, z = a.z - b };
    public static DistanceVector3 operator *(DistanceVector3 a, float b) => new DistanceVector3 { x = a.x * b, y = a.y * b, z = a.z * b };
    public static DistanceVector3 operator /(DistanceVector3 a, float b) => new DistanceVector3 { x = a.x / b, y = a.y / b, z = a.z / b };

    // Arithmetic operators: float op DistanceVector3
    public static DistanceVector3 operator +(float a, DistanceVector3 b) => new DistanceVector3 { x = a + b.x, y = a + b.y, z = a + b.z };
    public static DistanceVector3 operator -(float a, DistanceVector3 b) => new DistanceVector3 { x = a - b.x, y = a - b.y, z = a - b.z };
    public static DistanceVector3 operator *(float a, DistanceVector3 b) => new DistanceVector3 { x = a * b.x, y = a * b.y, z = a * b.z };
    public static DistanceVector3 operator /(float a, DistanceVector3 b) => new DistanceVector3 { x = a / b.x, y = a / b.y, z = a / b.z };

    // Arithmetic operators: DistanceVector3 op Distance
    public static DistanceVector3 operator +(DistanceVector3 a, Distance b) => new DistanceVector3 { x = a.x + b, y = a.y + b, z = a.z + b };
    public static DistanceVector3 operator -(DistanceVector3 a, Distance b) => new DistanceVector3 { x = a.x - b, y = a.y - b, z = a.z - b };
    public static DistanceVector3 operator *(DistanceVector3 a, Distance b) => new DistanceVector3 { x = a.x * b, y = a.y * b, z = a.z * b };
    public static DistanceVector3 operator /(DistanceVector3 a, Distance b) => new DistanceVector3 { x = a.x / b, y = a.y / b, z = a.z / b };

    // Arithmetic operators: Distance op DistanceVector3
    public static DistanceVector3 operator +(Distance a, DistanceVector3 b) => new DistanceVector3 { x = a + b.x, y = a + b.y, z = a + b.z };
    public static DistanceVector3 operator -(Distance a, DistanceVector3 b) => new DistanceVector3 { x = a - b.x, y = a - b.y, z = a - b.z };
    public static DistanceVector3 operator *(Distance a, DistanceVector3 b) => new DistanceVector3 { x = a * b.x, y = a * b.y, z = a * b.z };
    public static DistanceVector3 operator /(Distance a, DistanceVector3 b) => new DistanceVector3 { x = a / b.x, y = a / b.y, z = a / b.z };

    // Unary operators
    public static DistanceVector3 operator -(DistanceVector3 a) => new DistanceVector3 { x = -a.x, y = -a.y, z = -a.z };
    public static DistanceVector3 operator +(DistanceVector3 a) => a;

    // Comparison operators: DistanceVector3 op DistanceVector3
    public static bool operator ==(DistanceVector3 a, DistanceVector3 b) => a.x == b.x && a.y == b.y && a.z == b.z;
    public static bool operator !=(DistanceVector3 a, DistanceVector3 b) => a.x != b.x || a.y != b.y || a.z != b.z;

    // Comparison operators: DistanceVector3 op Vector3
    public static bool operator ==(DistanceVector3 a, Vector3 b) => a.x == b.x && a.y == b.y && a.z == b.z;
    public static bool operator !=(DistanceVector3 a, Vector3 b) => a.x != b.x || a.y != b.y || a.z != b.z;

    // Comparison operators: Vector3 op DistanceVector3
    public static bool operator ==(Vector3 a, DistanceVector3 b) => a.x == b.x && a.y == b.y && a.z == b.z;
    public static bool operator !=(Vector3 a, DistanceVector3 b) => a.x != b.x || a.y != b.y || a.z != b.z;

    // Override Equals and GetHashCode
    public override bool Equals(object obj)
    {
        if (obj is DistanceVector3 other)
            return x == other.x && y == other.y && z == other.z;
        if (obj is Vector3 otherVec)
            return x == otherVec.x && y == otherVec.y && z == otherVec.z;
        return false;
    }

    public override int GetHashCode() => (x.GetHashCode() * 397) ^ (y.GetHashCode() * 397) ^ z.GetHashCode();
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(Distance))]
public class DistanceDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty valueProperty = property.FindPropertyRelative("val");
        SerializedProperty defaultUnitProperty = property.FindPropertyRelative(nameof(Distance.defaultDistanceUnit));

        // Get current Distance struct with preserved defaultDistanceUnit
        DistanceUnit currentDefaultUnit = (DistanceUnit)defaultUnitProperty.enumValueIndex;
        Distance distance = new Distance(currentDefaultUnit) { Meters = valueProperty.floatValue };

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

        // Meters
        if (isExpanded || currentDefaultUnit == DistanceUnit.Meters)
        {
            distance = new Distance(currentDefaultUnit) { Meters = valueProperty.floatValue };
            if (isExpanded)
            {
                Rect metersRect = new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight);
                float newMeters = EditorGUI.FloatField(metersRect, "Meters", distance.Meters);
                if (newMeters != distance.Meters)
                {
                    distance.Meters = newMeters;
                    valueProperty.floatValue = distance.Meters;
                }
                yOffset += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            else
            {
                // Draw input field directly after the label
                Rect fieldRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
                float newMeters = EditorGUI.FloatField(fieldRect, distance.Meters);
                if (newMeters != distance.Meters)
                {
                    distance.Meters = newMeters;
                    valueProperty.floatValue = distance.Meters;
                }
            }
        }

        // Millimeters
        if (isExpanded || currentDefaultUnit == DistanceUnit.Millimeters)
        {
            distance = new Distance(currentDefaultUnit) { Meters = valueProperty.floatValue };
            float currentMillimeters = GetPropertyValue(distance, nameof(Distance.Millimeters));
            if (isExpanded)
            {
                Rect millimetersRect = new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight);
                float newMillimeters = EditorGUI.FloatField(millimetersRect, "Millimeters", currentMillimeters);
                if (newMillimeters != currentMillimeters)
                {
                    distance.Millimeters = newMillimeters;
                    valueProperty.floatValue = distance.Meters;
                }
                yOffset += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            else
            {
                // Draw input field directly after the label
                Rect fieldRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
                float newMillimeters = EditorGUI.FloatField(fieldRect, currentMillimeters);
                if (newMillimeters != currentMillimeters)
                {
                    distance.Millimeters = newMillimeters;
                    valueProperty.floatValue = distance.Meters;
                }
            }
        }

        // Feet
        if (isExpanded || currentDefaultUnit == DistanceUnit.Feet)
        {
            distance = new Distance(currentDefaultUnit) { Meters = valueProperty.floatValue };
            float currentFeet = GetPropertyValue(distance, nameof(Distance.Feet));
            if (isExpanded)
            {
                Rect feetRect = new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight);
                float newFeet = EditorGUI.FloatField(feetRect, "Feet", currentFeet);
                if (newFeet != currentFeet)
                {
                    distance.Feet = newFeet;
                    valueProperty.floatValue = distance.Meters;
                }
                yOffset += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            else
            {
                // Draw input field directly after the label
                Rect fieldRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
                float newFeet = EditorGUI.FloatField(fieldRect, currentFeet);
                if (newFeet != currentFeet)
                {
                    distance.Feet = newFeet;
                    valueProperty.floatValue = distance.Meters;
                }
            }
        }

        // Inches
        if (isExpanded || currentDefaultUnit == DistanceUnit.Inches)
        {
            distance = new Distance(currentDefaultUnit) { Meters = valueProperty.floatValue };
            float currentInches = GetPropertyValue(distance, nameof(Distance.Inches));
            if (isExpanded)
            {
                Rect inchesRect = new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight);
                float newInches = EditorGUI.FloatField(inchesRect, "Inches", currentInches);
                if (newInches != currentInches)
                {
                    distance.Inches = newInches;
                    valueProperty.floatValue = distance.Meters;
                }
                yOffset += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            else
            {
                // Draw input field directly after the label
                Rect fieldRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
                float newInches = EditorGUI.FloatField(fieldRect, currentInches);
                if (newInches != currentInches)
                {
                    distance.Inches = newInches;
                    valueProperty.floatValue = distance.Meters;
                }
            }
        }

        // Yards
        if (isExpanded || currentDefaultUnit == DistanceUnit.Yards)
        {
            distance = new Distance(currentDefaultUnit) { Meters = valueProperty.floatValue };
            float currentYards = GetPropertyValue(distance, nameof(Distance.Yards));
            if (isExpanded)
            {
                Rect yardsRect = new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight);
                float newYards = EditorGUI.FloatField(yardsRect, "Yards", currentYards);
                if (newYards != currentYards)
                {
                    distance.Yards = newYards;
                    valueProperty.floatValue = distance.Meters;
                }
                yOffset += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            else
            {
                // Draw input field directly after the label
                Rect fieldRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
                float newYards = EditorGUI.FloatField(fieldRect, currentYards);
                if (newYards != currentYards)
                {
                    distance.Yards = newYards;
                    valueProperty.floatValue = distance.Meters;
                }
            }
        }

        // Kilometers
        if (isExpanded || currentDefaultUnit == DistanceUnit.Kilometers)
        {
            distance = new Distance(currentDefaultUnit) { Meters = valueProperty.floatValue };
            float currentKilometers = GetPropertyValue(distance, nameof(Distance.Kilometers));
            if (isExpanded)
            {
                Rect kilometersRect = new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight);
                float newKilometers = EditorGUI.FloatField(kilometersRect, "Kilometers", currentKilometers);
                if (newKilometers != currentKilometers)
                {
                    distance.Kilometers = newKilometers;
                    valueProperty.floatValue = distance.Meters;
                }
                yOffset += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            else
            {
                // Draw input field directly after the label
                Rect fieldRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
                float newKilometers = EditorGUI.FloatField(fieldRect, currentKilometers);
                if (newKilometers != currentKilometers)
                {
                    distance.Kilometers = newKilometers;
                    valueProperty.floatValue = distance.Meters;
                }
            }
        }

        // Miles
        if (isExpanded || currentDefaultUnit == DistanceUnit.Miles)
        {
            distance = new Distance(currentDefaultUnit) { Meters = valueProperty.floatValue };
            float currentMiles = GetPropertyValue(distance, nameof(Distance.Miles));
            if (isExpanded)
            {
                Rect milesRect = new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight);
                float newMiles = EditorGUI.FloatField(milesRect, "Miles", currentMiles);
                if (newMiles != currentMiles)
                {
                    distance.Miles = newMiles;
                    valueProperty.floatValue = distance.Meters;
                }
                yOffset += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            else
            {
                // Draw input field directly after the label
                Rect fieldRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
                float newMiles = EditorGUI.FloatField(fieldRect, currentMiles);
                if (newMiles != currentMiles)
                {
                    distance.Miles = newMiles;
                    valueProperty.floatValue = distance.Meters;
                }
            }
        }

        if (isExpanded)
        {
            EditorGUI.indentLevel--;
        }


        EditorGUI.EndProperty();
    }

    private float GetPropertyValue(Distance distance, string propertyName)
    {
        PropertyInfo prop = typeof(Distance).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        return (float)prop.GetValue(distance);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        string foldoutKey = property.propertyPath;
        bool isExpanded = EditorPrefs.GetBool(foldoutKey, false);

        if (isExpanded)
        {
            // Height for foldout + 7 properties (Meters, Millimeters, Feet, Inches, Yards, Kilometers, Miles)
            return (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 8;
        }
        else
        {
            // Height for foldout with Meters on the same line
            return EditorGUIUtility.singleLineHeight;
        }
    }
}
#endif