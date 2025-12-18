using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Debug.Jobs;

[BurstCompile]
public struct GizmosTransformVerticesToWorldJob : IJobParallelFor
{
	public NativeArray<int> BodyIndicesMap;

	public NativeArray<int2> VerticesRange;

	public NativeArray<float4x4> BodyLocalToWorld;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float3> VertexPosition;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float3> VertexNormal;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float3> VertexRestNormal;

	[WriteOnly]
	public NativeList<GizmoMeshVertex>.ParallelWriter GizmoVertices;

	public void Execute(int index)
	{
		int index2 = BodyIndicesMap[index];
		int2 @int = VerticesRange[index2];
		float4x4 float4x = BodyLocalToWorld[index2];
		for (int i = 0; i < @int.y; i++)
		{
			int index3 = @int.x + i;
			float3 xyz = math.mul(float4x, new float4(VertexPosition[index3], 1f)).xyz;
			float3 x = math.mul((float3x3)float4x, VertexNormal[index3]);
			float3 x2 = math.mul((float3x3)float4x, VertexRestNormal[index3]);
			GizmoVertices.AddNoResize(new GizmoMeshVertex
			{
				Position = xyz,
				Normal = math.normalizesafe(x),
				RestNormal = math.normalizesafe(x2)
			});
		}
	}
}
