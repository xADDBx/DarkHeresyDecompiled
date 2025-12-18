using System;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Constraints;

[Serializable]
public struct Constraint
{
	public int4 Indices;

	public float4 Parameters0;

	public float4 Parameters1;
}
