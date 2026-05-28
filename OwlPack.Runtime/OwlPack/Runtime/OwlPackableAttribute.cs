using System;

namespace OwlPack.Runtime;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
public class OwlPackableAttribute : Attribute
{
	public OwlPackableAttribute(OwlPackableMode mode = OwlPackableMode.Generate)
	{
	}
}
