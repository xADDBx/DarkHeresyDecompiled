using Unity.Burst;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Bodies;

[BurstCompile]
public struct MeshDeformerDescriptor
{
	public int2 BindingsRange;

	public int2 DeformableVerticesRange;

	public int2 SkinnedVerticesRange;

	public int2 VertexToSkinnedVertexMapRange;

	public float4x4 WorldToLocal;

	public float4x4 LocalToWorld;
}
