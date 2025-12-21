using UnityEngine;using UnityEngine.UI;

public static class RectTransformExt
{
	public static Rect GetScreenRectForWorldUi(this RectTransform worldUiElement)
	{
		Canvas c = worldUiElement.GetComponentInParent<Canvas>();
		Camera cam = c.worldCamera;

		Vector3[] corners = new Vector3[4];
		worldUiElement.GetWorldCorners(corners);

		corners[0] = cam.WorldToScreenPoint(corners[0]);
		corners[2] = cam.WorldToScreenPoint(corners[2]);

		Rect r = new Rect();
		r.width = corners[2].x - corners[0].x;
		r.height = corners[2].y - corners[0].y;
		r.x = corners[0].x;
		r.y = corners[0].y;

		return r;
	}

	public static string ToString(this Rect r)
	{
		return string.Format("x: {0}, y: {1}, w: {2}, h: {3}", r.x, r.y, r.width, r.height);
	}

}