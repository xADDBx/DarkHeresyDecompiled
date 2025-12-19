using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Particles;

public struct ParticleSoASlice
{
	public NativeSlice<float3> Velocity;

	public NativeSlice<float3> Position;

	public NativeSlice<int> JacobiPosCount;

	public NativeSlice<float> Radius;

	public NativeSlice<float3> BasePosition;

	public NativeSlice<float3> PrevPosition;

	public NativeSlice<float> InvMass;

	public NativeSlice<int3> JacobiPosDelta;
}
