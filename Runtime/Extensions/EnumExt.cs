using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;


public static class EnumExt
{
	public static IEnumerable<T> GetValues<T>()
	{
		return Enum.GetValues(typeof(T)).Cast<T>();
	}

	public static bool IsSet(this Enum input, Enum matchTo)
	{
		return (Convert.ToUInt32(input) & Convert.ToUInt32(matchTo)) != 0;
	}

    static Dictionary<string, List<string>> enumNameCache = new Dictionary<string, List<string>>();

    public static List<string> GetEnumNamesWithSpaces<T>()
    {
        string enumTypeName = typeof(T).ToString();
        if (enumNameCache.ContainsKey(enumTypeName) && 
            enumNameCache[enumTypeName] != null && 
            enumNameCache[enumTypeName].Count > 0)
        {
            return enumNameCache[enumTypeName];
        }

        if (!enumNameCache.ContainsKey(enumTypeName))
            enumNameCache.Add(enumTypeName, null);

        List<string> names = new List<string>();

        var r = new Regex(@"
                (?<=[A-Z])(?=[A-Z][a-z]) |
                 (?<=[^A-Z])(?=[A-Z]) |
                 (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);

        
        foreach (var name in Enum.GetNames(typeof(T)))
        {
            names.Add(r.Replace(name, " "));
        }

        enumNameCache[enumTypeName] = names;

        return names;
    }
}
