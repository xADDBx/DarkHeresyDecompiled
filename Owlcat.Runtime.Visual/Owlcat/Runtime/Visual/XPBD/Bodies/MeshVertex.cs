using Unity.Burst;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Bodies;

[BurstCompile]
public struct MeshVertex
{
	public float3 Position;

	public float3 Normal;
}
