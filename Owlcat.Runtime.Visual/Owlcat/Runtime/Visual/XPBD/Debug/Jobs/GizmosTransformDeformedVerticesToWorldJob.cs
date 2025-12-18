using Owlcat.Runtime.Visual.XPBD.Bodies;
using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Debug.Jobs;

[BurstCompile]
public struct GizmosTransformDeformedVerticesToWorldJob : IJobParallelFor
{
	[ReadOnly]
	internal NativeArray<int> DeformerIndicesMap;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	internal NativeArray<int2> VerticesRange;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	internal NativeArray<float4x4> DeformerLocalToWorld;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	internal NativeArray<DeformableSkinnedVertex> DeformedSkinnedVertices;

	[WriteOnly]
	internal NativeList<GizmoDeformedVertex>.ParallelWriter GizmoDeformedVertices;

	public void Execute(int index)
	{
		int index2 = DeformerIndicesMap[index];
		float4x4 float4x = DeformerLocalToWorld[index2];
		int2 @int = VerticesRange[index2];
		for (int i = 0; i < @int.y; i++)
		{
			int index3 = @int.x + i;
			DeformableSkinnedVertex deformableSkinnedVertex = DeformedSkinnedVertices[index3];
			float3 xyz = math.mul(float4x, new float4(deformableSkinnedVertex.Position, 1f)).xyz;
			float3 x = math.mul((float3x3)float4x, deformableSkinnedVertex.Normal);
			float3 x2 = math.mul((float3x3)float4x, deformableSkinnedVertex.Tangent);
			GizmoDeformedVertices.AddNoResize(new GizmoDeformedVertex
			{
				Position = xyz,
				Normal = math.normalizesafe(x),
				Tangent = math.normalizesafe(x2)
			});
		}
	}
}
