using System;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Bodies;

[Serializable]
public struct BodyBone
{
	public float4x4 Bindpose;

	public float4x4 Bonepose;

	public int ParentIndex;

	public int ParticleIndex;

	[NonSerialized]
	public float4x4 SimulatedBindpose;
}
