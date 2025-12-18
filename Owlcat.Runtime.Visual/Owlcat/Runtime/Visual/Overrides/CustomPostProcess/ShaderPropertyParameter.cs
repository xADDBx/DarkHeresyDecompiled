using System;
using Owlcat.Runtime.Visual.CustomPostProcess;

namespace Owlcat.Runtime.Visual.Overrides.CustomPostProcess;

[Serializable]
public class ShaderPropertyParameter
{
	public bool OverrideState;

	public int PassIndex;

	public ShaderPropertyDescriptor Property;

	public ShaderPropertyParameter Clone()
	{
		return new ShaderPropertyParameter
		{
			OverrideState = OverrideState,
			PassIndex = PassIndex,
			Property = new ShaderPropertyDescriptor(Property)
		};
	}

	public int GetContentHash()
	{
		return (int)((17 * 31 + PassIndex.GetHashCode()) * 31 + Property.Type) * 31 + Property.Name.GetHashCode();
	}
}
