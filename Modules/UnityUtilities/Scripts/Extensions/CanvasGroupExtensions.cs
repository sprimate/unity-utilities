using UnityEngine;
using UnityEngine.UI;


public static class CanvasGroupExtensions 
{
	public static void SetActive(this CanvasGroup canvasGroup, bool active)
	{
		canvasGroup.interactable = active;
		canvasGroup.gameObject.SetActive(active);
		canvasGroup.alpha = (active ? 1 : 0);
		canvasGroup.blocksRaycasts = active;
	}
}
