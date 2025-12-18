using Unity.Burst;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Bodies;

[BurstCompile]
public struct SkinnedVertex
{
	public float3 Position;

	public float3 Normal;

	public float4 Tangent;
}
