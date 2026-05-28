using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Particles;

public struct ParticleSoASlice
{
	public NativeSlice<int3> JacobiPosDelta;

	public NativeSlice<float3> Velocity;

	public NativeSlice<float> InvMass;

	public NativeSlice<float3> PrevPosition;

	public NativeSlice<float3> Position;

	public NativeSlice<float3> BasePosition;

	public NativeSlice<float> Radius;

	public NativeSlice<int> JacobiPosCount;
}
