using System;

public static class ActionExtensions
{
    /// <summary>
    /// Returns a copy of thisAction with actionToRunOnce applied. actionToRunOnce will remove itself once its done with its thing
    /// If your Action is a property, assign it to a copy of itself with the "once" (since you can't ref to properties.)
    /// Internally, I believe its essentially just as efficient, since adding to a Action makes a copy internally anyway
    /// </summary>
    /// <param name="thisAction"></param>
    /// <param name="actionToRunOnce"></param>
    /// <returns></returns>
    public static Action CopyWithAddOnce(this Action thisAction, Action actionToRunOnce) // )
    {
        AddOnce(ref thisAction, actionToRunOnce);
        return thisAction;
    }

    /// <summary>
    /// thisAction and actionToModify are the same. Since Actions are delegates (value types), you need to pass a ref for this to work
    /// "thisAction" is jus there so it shows up via intellisense as an extension method.
    /// The parameter passed when using the extension method is the same as the second parameter, but as a reference
    /// </summary>
    public static void AddOnce(this Action thisAction, ref Action actionToModify, Action actionToRunOnce)
    {
        AddOnce(ref actionToModify, actionToRunOnce);
    }

    public static void AddOnce(ref Action a, Action actionToRunOnce)
    {
        bool shouldRun = true;
        Action toAdd = () =>
        {
            if (shouldRun)
            {
                shouldRun = false;
                actionToRunOnce();
            }
        };

        a += toAdd;
    }

    /// <summary>
    /// thisAction and actionToModify are the same. Since Actions are delegates (value types), you need to pass a ref for this to work
    /// "thisAction" is jus there so it shows up via intellisense as an extension method.
    /// The parameter passed when using the extension method is the same as the second parameter, but as a reference
    /// </summary>
    public static void AddOnce<T>(this Action<T> thisAction, ref Action<T> actionToModify, Action<T> actionToRunOnce)
    {
        AddOnce(ref actionToModify, actionToRunOnce);
    }

    public static void AddOnce<T>(ref Action<T> a, Action<T> actionToRunOnce)
    {
        bool shouldRun = true;
        Action<T> toAdd = (val) =>
        {
            if (shouldRun)
            {
                shouldRun = false;
                actionToRunOnce(val);
            }
        };

        a += toAdd;
    }

    /// <summary>
    /// thisAction and actionToModify are the same. Since Actions are delegates (value types), you need to pass a ref for this to work
    /// "thisAction" is jus there so it shows up via intellisense as an extension method.
    /// The parameter passed when using the extension method is the same as the second parameter, but as a reference
    /// </summary>
    public static void AddOnce<T1, T2>(this Action<T1, T2> thisAction, ref Action<T1, T2> actionToModify, Action<T1, T2> actionToRunOnce)
    {
        AddOnce(ref actionToModify, actionToRunOnce);
    }

    public static void AddOnce<T1, T2>(ref Action<T1, T2> a, Action<T1, T2> actionToRunOnce)
    {
        bool shouldRun = true;
        Action<T1, T2> toAdd = (val1, val2) =>
        {
            if (shouldRun)
            {
                shouldRun = false;
                actionToRunOnce(val1, val2);
            }
        };

        a += toAdd;
    }
}