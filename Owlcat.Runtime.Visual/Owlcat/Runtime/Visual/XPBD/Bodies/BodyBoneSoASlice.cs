using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Bodies;

public struct BodyBoneSoASlice
{
	public NativeSlice<int> ParentIndex;

	public NativeSlice<float4x4> SimulatedBindpose;

	public NativeSlice<int> ParticleIndex;

	public NativeSlice<float4x4> Bonepose;

	public NativeSlice<float4x4> Bindpose;
}
