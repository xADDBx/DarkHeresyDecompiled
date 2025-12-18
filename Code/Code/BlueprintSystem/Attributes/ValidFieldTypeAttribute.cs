using System;

namespace Code.BlueprintSystem.Attributes;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class ValidFieldTypeAttribute : Attribute
{
	public Type Type;

	public ValidFieldTypeAttribute(Type type)
	{
		Type = type;
	}
}
