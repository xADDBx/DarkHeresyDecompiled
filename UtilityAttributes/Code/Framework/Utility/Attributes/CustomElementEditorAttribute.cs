using System;

namespace Code.Framework.Utility.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class CustomElementEditorAttribute : Attribute
{
	public readonly Type InspectedType;

	public CustomElementEditorAttribute(Type inspectedType)
	{
		InspectedType = inspectedType;
	}
}
