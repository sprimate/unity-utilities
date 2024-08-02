#if UNITY_EDITOR

namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

	using Utilities.Editor;
	using UnityEngine;
	using Sirenix.OdinInspector.Editor.ValueResolvers;

	public class SizedFoldoutGroupAttributeDrawer : OdinGroupDrawer<SizedFoldoutGroupAttribute>
	{
		private ValueResolver<string> titleGetter;
		private ValueResolver<Color> colorGetter;

		protected override void Initialize()
		{
			this.titleGetter = ValueResolver.GetForString(this.Property, this.Attribute.GroupName);
			if (this.Attribute.HasDefinedExpanded)
			{
				this.Property.State.Expanded = this.Attribute.Expanded;
			}
		}

		protected override void DrawPropertyLayout(GUIContent label)
		{
			var property = this.Property;
			var attribute = this.Attribute;

			if (this.titleGetter.HasError)
			{
				SirenixEditorGUI.ErrorMessageBox(this.titleGetter.ErrorMessage);
			}


			if (this.Attribute.HasDefinedColor)
			{
				GUIHelper.PushColor(new Color(this.Attribute.R, this.Attribute.G, this.Attribute.B, this.Attribute.A));
			}
			SirenixEditorGUI.BeginBox();
			SirenixEditorGUI.BeginBoxHeader();
			if (this.Attribute.HasDefinedColor)
			{
				GUIHelper.PopColor();
			}
			GUIStyle s = new GUIStyle(SirenixGUIStyles.Foldout);
			s.richText = true;
			var content = GUIHelper.TempContent(this.titleGetter.HasError ? property.Label.text : this.titleGetter.GetValue());
			s.fixedWidth = 450;
			s.wordWrap = true;
			s.alignment = TextAnchor.UpperLeft;
			var height = s.CalcHeight(content, 450);

			GUILayout.BeginVertical();
			switch(height)
			{
				default:
					GUILayout.Space((height / 2) - 10);
					break;
				case 45:
					GUILayout.Space((height / 2) - 18);
					break;
				case 15:
					GUILayout.Space((height / 2));
					break;
			}
			this.Property.State.Expanded = SirenixEditorGUI.Foldout(this.Property.State.Expanded, content, s);
			switch (height)
			{
				default:
					GUILayout.Space((height / 2));
					break;
				case 45:
					GUILayout.Space((height / 2) + 8);
					break;
			}
			GUILayout.EndVertical();
			SirenixEditorGUI.EndBoxHeader();
			if (SirenixEditorGUI.BeginFadeGroup(this, this.Property.State.Expanded))
			{
				for (int i = 0; i < property.Children.Count; i++)
				{
					var child = property.Children[i];
					child.Draw(child.Label);
				}
			}
			SirenixEditorGUI.EndFadeGroup();
			SirenixEditorGUI.EndBox();
		}
	}
}
#endif