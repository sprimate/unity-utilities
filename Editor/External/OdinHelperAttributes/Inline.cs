using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;

#endif
[AttributeUsage(AttributeTargets.All)]
public class InlineAttribute : Attribute
{
	/// <summary>
	/// Keeps the label as a prefix.
	/// </summary>
	public readonly bool keepLabel;

	/// <summary>
	/// A decoration feature.
	/// Draws the object as a title
	/// </summary>
	public readonly bool asGroup;

	public InlineAttribute(bool keepLabel = false, bool asGroup = false)
	{
		this.keepLabel = keepLabel;
		this.asGroup   = asGroup;
	}
}

/// <summary>
/// Inline with a box surrounding the properties, like BoxGroup.
/// </summary>
[AttributeUsage(AttributeTargets.All)]
public class InlineBoxAttribute : Attribute
{ }

/// Inline with a foldout box surrounding the properties, like FoldoutGroup.
[AttributeUsage(AttributeTargets.All)]
public class InlineFoldoutAttribute : Attribute
{ }

#if UNITY_EDITOR
public class InlineAttributeProcessor : OdinAttributeProcessor
{
	public override bool CanProcessSelfAttributes(InspectorProperty property)
	{
		return property.Attributes.GetAttribute<InlineAttribute>() != null || property.Attributes.GetAttribute<SerializableAttribute>() != null;
	}

	public override bool CanProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member)
	{
		InlineAttribute       inlineAttr    = member.GetAttribute<InlineAttribute>(true) ?? member.GetReturnType().GetAttribute<InlineAttribute>(true);
		SerializableAttribute serializeAttr = member.GetAttribute<SerializableAttribute>(true);

		return inlineAttr != null;
	}

	public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
	{
		InlineAttribute inlineAttr = attributes.OfType<InlineAttribute>().FirstOrDefault();
		if (inlineAttr == null)
			return;

		Type baseType = property.ValueEntry.TypeOfValue.BaseType;
		Set(inlineAttr, attributes, property.Info.TypeOfValue.IsAbstract);
	}

	public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
	{
		Type memberType = null;

		if (member is FieldInfo fi) memberType    = fi.FieldType;
		if (member is PropertyInfo pi) memberType = pi.PropertyType;

		if (memberType == null) return;

		if (memberType.GetCustomAttributes(true).FirstOrDefault() is InlineAttribute inlineAttr)
		{
			Type baseType = (member as FieldInfo)?.FieldType;
			Set(inlineAttr, attributes, baseType != null && baseType.IsAbstract);
		}
	}

	private static void Set(InlineAttribute inline, List<Attribute> attributes, bool isAbstract = false)
	{
		attributes.Add(new InlinePropertyAttribute());
		// attributes.Remove(inlineAttr);

		if (!inline.keepLabel || inline.asGroup)
		{
			attributes.Add(new HideLabelAttribute());
		}

		if (inline.asGroup)
		{
			attributes.Add(new DarkBoxAttribute());
		}

		if (!isAbstract)
		{
			// This will hide the reference picker.
			// Fields with abstract types still need this because the user must be able to choose the implementation to use.
			attributes.Add(new HideReferenceObjectPickerAttribute());
		}
	}
}
[DrawerPriority(0, 95)]
public class InlineAttributeDrawer<TAttribute> : OdinAttributeDrawer<TAttribute>
	where TAttribute : InlineAttribute
{
	protected override void DrawPropertyLayout(GUIContent label)
	{
		if (Property.ValueEntry.WeakSmartValue == null &&
			Property.Attributes.Any(attr => attr is SerializableAttribute) &&
			!Property.Info.TypeOfValue.IsAbstract)
		{
			Property.ValueEntry.WeakSmartValue = Activator.CreateInstance(Property.Info.TypeOfValue);
		}

		if (Attribute.asGroup)
		{
			SirenixEditorGUI.Title(Property.NiceName, "", TextAlignment.Left, false);
			label = null;
		}

		if (!Attribute.keepLabel)
		{
			label = null;
		}

		CallNextDrawer(label);
	}
}

public class InlineBoxProcessor : OdinAttributeProcessor
{
	public override bool CanProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member)
	{
		return member.GetAttribute<InlineBoxAttribute>() != null;
	}

	public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
	{
		attributes.Add(new HideLabelAttribute());
		attributes.Add(new HideReferenceObjectPickerAttribute());
		attributes.Add(new InlinePropertyAttribute());
		attributes.Add(new BoxGroupAttribute(member.GetNiceName()));
	}
}

public class InlineFoldoutProcessor : OdinAttributeProcessor
{
	public override bool CanProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member)
	{
		return member.GetAttribute<InlineFoldoutAttribute>() != null;
	}

	public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
	{
		attributes.Add(new HideLabelAttribute());
		attributes.Add(new HideReferenceObjectPickerAttribute());
		attributes.Add(new InlinePropertyAttribute());
		attributes.Add(new FoldoutGroupAttribute(member.GetNiceName()));
	}
}
#endif