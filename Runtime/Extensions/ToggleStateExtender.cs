using UnityEngine.UI;
using System.Linq;

public static class ToggleStateExtender
{

	public static Toggle GetActive(this ToggleGroup aGroup)
	{
		return aGroup.ActiveToggles().FirstOrDefault();
	}

	public static int GetCurrentSelectedToggleIndex(this Toggle[] toggles, ToggleGroup group)
	{
		Toggle activeToggle = group.GetActive();

		for (int i = 0; i < toggles.Length; i++)
		{
			if (toggles[i] == activeToggle)
				return i;
		}

		throw new System.Exception("Could not find active toggle in group: " + group.name);
	}

    public static string GetSelectedText(this Dropdown dropdown)
    {
        return dropdown.options[dropdown.value].text;
    }
}

