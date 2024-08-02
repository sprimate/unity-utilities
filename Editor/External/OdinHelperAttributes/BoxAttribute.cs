using System;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using Random = System.Random;

/// <summary>
/// Draw the properties with a darker background and
/// borders, optionally.
/// </summary>
public class DarkBoxAttribute : Attribute
{
	/// <summary>
	/// Dark
	/// </summary>
	public readonly bool withBorders;

	public DarkBoxAttribute()
	{ }

	public DarkBoxAttribute(bool withBorders)
	{
		this.withBorders = withBorders;
	}
}

public class ColorBox : Attribute
{
}

namespace OdinExtensions
{
	[DrawerPriority(0, 99)]
	public class ColorBoxDrawer : OdinAttributeDrawer<ColorBox>
	{
		private Color _color;

		private const float GOLDEN_RATIO = 0.618033988749895f;

		protected override void DrawPropertyLayout(GUIContent label)
		{
			// float h = 0;
			// for (int i = 0; i < Property.Index; i++)
			// {
			// h += GOLDEN_RATIO;
			// while (h > 1) h -= 1;
			// }

			int hashCode = Property.ValueEntry.TypeOfValue.Name.GetHashCode();
			var h        = (float) ((hashCode + (double) int.MaxValue) / uint.MaxValue);
			_color   = Color.HSVToRGB(h, 0.95f, 0.75f);
			_color.a = 0.15f;
			BoxGUI.BeginBox(_color);
			CallNextDrawer(label);
			BoxGUI.EndBox();
		}
	}
	#if UNITY_EDITOR

	[DrawerPriority(0, 99)]
	public class DarkBoxDrawer : OdinAttributeDrawer<DarkBoxAttribute>
	{
		public static readonly Color Color = EditorGUIUtility.isProSkin
												 ? Color.Lerp(Color.black, Color.white, 0.1f)
												 : Color.gray;

		protected override void DrawPropertyLayout(GUIContent label)
		{
			BoxGUI.BeginBox(new Color(0, 0, 0, 0.15f));
			CallNextDrawer(label);

			BoxGUI.EndBox(Attribute.withBorders ? Color : (Color?) null);
		}
	}

	internal static class BoxGUI
	{
		private static Rect currentLayoutRect;

		public static void BeginBox(Color color)
		{
			currentLayoutRect = EditorGUILayout.BeginVertical(SirenixGUIStyles.None);

			// Rect currentLayoutRect = GUIHelper.GetCurrentLayoutRect();
			if (Event.current.type == EventType.Repaint)
			{
				SirenixEditorGUI.DrawSolidRect(currentLayoutRect, color);
			}
		}

		public static void EndBox(Color? borders = null)
		{
			EditorGUILayout.EndVertical();

			if (Event.current.type == EventType.Repaint && borders != null)
			{
				SirenixEditorGUI.DrawBorders(currentLayoutRect, 1, 1, 1, 1, borders.Value);
			}

			GUILayout.Space(1);
		}
	}
	#endif
}
