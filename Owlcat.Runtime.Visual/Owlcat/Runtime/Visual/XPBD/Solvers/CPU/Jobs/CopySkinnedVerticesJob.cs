using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Solvers.CPU.Jobs;

[BurstCompile]
public struct CopySkinnedVerticesJob : IJobParallelFor
{
	private const int kPositionStrideInFloats = 3;

	public int BodyDescIndex;

	public int2 ParticleToVertexMapRange;

	public int2 ParticlesRange;

	public int2 SkinBufferRange;

	public int2 RestNormalsRange;

	public int VertexStride;

	[ReadOnly]
	public NativeArray<float4x4> BodyLocalToWorld;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float> MeshVertices;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int> ParticleToVertexMap;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	[NativeDisableContainerSafetyRestriction]
	public NativeArray<float3> ParticleBasePosition;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	[NativeDisableContainerSafetyRestriction]
	public NativeArray<float3> ParticlePosition;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	[NativeDisableContainerSafetyRestriction]
	public NativeArray<float3> ParticlePrevPosition;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	[NativeDisableContainerSafetyRestriction]
	public NativeArray<float> ParticleInvMass;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	[NativeDisableContainerSafetyRestriction]
	public NativeArray<float3> RestNormal;

	public void Execute(int index)
	{
		int index2 = index + ParticlesRange.x;
		int vertexIndex = ParticleToVertexMap[ParticleToVertexMapRange.x + index] - SkinBufferRange.x;
		float3 vertexPosition = GetVertexPosition(vertexIndex);
		float3 vertexNormal = GetVertexNormal(vertexIndex);
		float3 xyz = math.mul(BodyLocalToWorld[BodyDescIndex], new float4(vertexPosition, 1f)).xyz;
		float3 value = math.mul((float3x3)BodyLocalToWorld[BodyDescIndex], vertexNormal);
		if (ParticleInvMass[index2] <= 0f)
		{
			ParticlePosition[index2] = xyz;
			ParticlePrevPosition[index2] = xyz;
		}
		else
		{
			float3 @float = xyz - ParticleBasePosition[index2];
			ParticlePosition[index2] += @float;
			ParticlePrevPosition[index2] = ParticlePosition[index2];
		}
		ParticleBasePosition[index2] = xyz;
		int index3 = index + RestNormalsRange.x;
		RestNormal[index3] = value;
	}

	private float3 GetVertexPosition(int vertexIndex)
	{
		int num = vertexIndex * VertexStride;
		return new float3(MeshVertices[num], MeshVertices[num + 1], MeshVertices[num + 2]);
	}

	private float3 GetVertexNormal(int vertexIndex)
	{
		if (VertexStride > 3)
		{
			int num = 3 + vertexIndex * VertexStride;
			return new float3(MeshVertices[num], MeshVertices[num + 1], MeshVertices[num + 2]);
		}
		return new float3(0f, 1f, 0f);
	}
}
