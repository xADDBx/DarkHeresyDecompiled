using System;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Particles;

[Serializable]
public struct Particle
{
	public float3 BasePosition;

	public float InvMass;

	public float Radius;

	[NonSerialized]
	public float3 Position;

	[NonSerialized]
	public float3 PrevPosition;

	[NonSerialized]
	public float3 Velocity;

	[NonSerialized]
	public int3 JacobiPosDelta;

	[NonSerialized]
	public int JacobiPosCount;
}
