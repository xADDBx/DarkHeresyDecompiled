using System;

namespace Owlcat.Blueprints;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class AddComponentOnCreateBlueprintAttribute : Attribute
{
	public Type Type { get; private set; }

	public AddComponentOnCreateBlueprintAttribute(Type type)
	{
		Type = type;
	}
}
