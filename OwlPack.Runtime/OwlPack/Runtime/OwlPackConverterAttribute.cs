using System;

namespace OwlPack.Runtime;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
public class OwlPackConverterAttribute : Attribute
{
	public OwlPackConverterAttribute(Type fromType, string methodName = "OwlPackConvert")
	{
	}

	public OwlPackConverterAttribute(Type converterClass, Type fromType, string methodName = "OwlPackConvert")
	{
	}
}
