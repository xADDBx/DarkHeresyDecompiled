using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Bodies;

public struct BodyBoneSoASlice
{
	public NativeSlice<int> ParticleIndex;

	public NativeSlice<int> ParentIndex;

	public NativeSlice<float4x4> Bindpose;

	public NativeSlice<float4x4> SimulatedBindpose;

	public NativeSlice<float4x4> Bonepose;
}
