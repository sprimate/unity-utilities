


using System;
using System.Collections.Generic;
using UnityEngine;
public class NotImplementedNote
{
    public NotImplementedNote()
    {
        Debug.Log("WARNING: Not Currently Implemented");
    }

    public NotImplementedNote(string s)
    {
        Debug.Log("WARNING: Not Currently Implemented: [" + s + "]");
    }

    static Dictionary<Type, HashSet<string>> types = new Dictionary<Type, HashSet<string>>();
    public NotImplementedNote(Type t) : this(null, t)
    {

    }

    public NotImplementedNote(string err, Type t) : this(err, t, null)
    {

    }

    public NotImplementedNote(string err, Type t, UnityEngine.Object context)
    {
        if (string.IsNullOrWhiteSpace(err))
        {
            err = "";
        }

        if (!types.ContainsKey(t))
        {
            types[t] = new HashSet<string>();
        }

        if (!types[t].Contains(err))
        {
            types[t].Add(err);
            Debug.Log("WARNING: Not Currently Implemented" + (string.IsNullOrWhiteSpace(err) ? "." : ": [" + err + "] | ") + "Type: [" + t + "]", context);
        }
    }

    public NotImplementedNote(string err, UnityEngine.Object o) : this(err, o.GetType(), o)
    {

    }

    public NotImplementedNote(UnityEngine.Object o) : this(null, o.GetType(), o)
    {

    }
}
