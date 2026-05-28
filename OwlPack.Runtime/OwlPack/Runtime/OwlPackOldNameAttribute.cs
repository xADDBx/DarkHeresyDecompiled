using System;

namespace OwlPack.Runtime;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Interface, AllowMultiple = true, Inherited = false)]
public class OwlPackOldNameAttribute : Attribute
{
	public OwlPackOldNameAttribute(string name)
	{
	}
}
