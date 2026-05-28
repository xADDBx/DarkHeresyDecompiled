using System;

namespace Owlcat.BehaviourTrees;

[AttributeUsage(AttributeTargets.Class)]
public class VariableTypeNameAttribute : Attribute
{
	public readonly string Name;

	public readonly Type Type;

	public VariableTypeNameAttribute(string name, Type type)
	{
		Name = name;
		Type = type;
	}
}
