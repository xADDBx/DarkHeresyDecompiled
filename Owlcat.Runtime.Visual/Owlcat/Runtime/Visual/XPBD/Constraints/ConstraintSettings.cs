using System;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Constraints;

[Serializable]
public abstract class ConstraintSettings
{
	public bool Enabled = true;

	public abstract ConstraintType ConstraintType { get; }

	public abstract float4 GetPackedSettings();

	internal int CalculateHash()
	{
		return HashCode.Combine(Enabled, GetPackedSettings());
	}
}
