using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

public static class DropdownExtensions {

    static Dictionary<UnityEngine.UI.Dropdown, Action> onOpened = new Dictionary<Dropdown, Action>();
    static Dictionary<Dropdown, Action> onClosed = new Dictionary<Dropdown, Action>();

    static HashSet<Dropdown> openedDropdowns = new HashSet<Dropdown>();
	public static void OnOpened(this Dropdown dropdown, Action action)
    {
        OnEvent(dropdown, action, ref onOpened);
    }

    public static void OnClosed(this Dropdown dropdown, Action action)
    {
        OnEvent(dropdown, action, ref onClosed);
    }

    public static void RemoveOnOpenedCallbacks(this Dropdown d)
    {
        if (onOpened.ContainsKey(d))
        {
            onOpened.Remove(d);
        }
    }

    public static void RemoveOnClosedCallbacks(this Dropdown d)
    {
        if (onClosed.ContainsKey(d))
        {
            onClosed.Remove(d);
        }
    }

    static void OnEvent(this Dropdown dropdown, Action action, ref Dictionary<Dropdown, Action> dict)
    {
        TrackDropdown(dropdown);

        if (!dict.ContainsKey(dropdown))
        {
            dict[dropdown] = action;
        }
        else
        {
            dict[dropdown] += action;
        }

    }

    static void TrackDropdown(Dropdown dropdown)
    {
        if (onOpened.ContainsKey(dropdown) || onClosed.ContainsKey(dropdown))
        {
            return;
        }

        Action updateCallback = () => { };
        updateCallback += () =>
        {
            if (dropdown == null)
            {

            }
            //If nothing listening for it, stop running this function on update
            if (dropdown == null || (!onOpened.ContainsKey(dropdown) && !onClosed.ContainsKey(dropdown)))
            {
                GenericCoroutineManager.RunOnUpdate(updateCallback, dropdown, false);
            }
            //If dropdown list is closed
            else if (dropdown.transform.Find("Dropdown List") == null)
            {
                //Check if it was previously open
                if (openedDropdowns.Contains(dropdown))
                {
                    openedDropdowns.Remove(dropdown);
                    if (onClosed.ContainsKey(dropdown))//handle callbacks
                    {
                        onClosed[dropdown].SafeInvoke();
                    }
                }
            }
            //If dropdown list is open
            else
            {
                //check if it was previously closed
                if (!openedDropdowns.Contains(dropdown))
                {
                    openedDropdowns.Add(dropdown);
                    if (onOpened.ContainsKey(dropdown)) //handle callbacks
                    {
                        onOpened[dropdown].SafeInvoke(); 
                    }
                }
            }
        };

        GenericCoroutineManager.RunOnUpdate(updateCallback, dropdown);
    }
}