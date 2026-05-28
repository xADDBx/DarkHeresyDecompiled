using System;

namespace OwlPack.Runtime;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
public class OwlPackExternalSerializerAttribute : Attribute
{
	public Type Type;

	public Type Serializer;

	public OwlPackExternalSerializerAttribute(Type type, Type serializer)
	{
		Type = type;
		Serializer = serializer;
	}
}
