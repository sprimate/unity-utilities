using System;
using System.Collections.Generic;
using System.Linq;

public static class EnumExtensions
{
    public static IEnumerable<T> GetSelectedEnumValues<T>(this T e) where T : Enum
    {
        foreach (T value in GetEnumValues<T>())
        {
            if (Has(e, value))
            {
                yield return value;
            }
        }
    }
    public static IEnumerable<T> GetAllEnumValues<T>(this T e) where T : Enum
    {
        return GetEnumValues<T>();
    }
    public static IEnumerable<T> GetEnumValues<T>() where T : Enum
    {
        return Enum.GetValues(typeof(T)).Cast<T>();
    }

    public static bool Has<T>(this T input, T potentiallyContainedValue) where T : Enum
    {
        ulong potentiallyContainedInt = ToUInt64(potentiallyContainedValue);
        ulong inputInt = ToUInt64(input);

        if (potentiallyContainedInt == 0) //the bitwise op always returns true for 0
        {
            return inputInt == 0;
        }

        return potentiallyContainedInt == inputInt || (inputInt & potentiallyContainedInt) == potentiallyContainedInt;
    }

    /// <summary>Converts enum to ulong, preserving bit pattern so that -1 (all flags) works in checked contexts.</summary>
    private static ulong ToUInt64<T>(T value) where T : Enum
    {
        return unchecked((ulong)Convert.ToInt64(value));
    }
}