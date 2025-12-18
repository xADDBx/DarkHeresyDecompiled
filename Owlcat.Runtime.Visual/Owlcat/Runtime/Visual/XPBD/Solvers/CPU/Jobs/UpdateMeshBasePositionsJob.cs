using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Solvers.CPU.Jobs;

[BurstCompile]
public struct UpdateMeshBasePositionsJob : IJobParallelFor
{
	public int BodyDescIndex;

	public int2 ParticlesRange;

	public int2 RestNormalsRange;

	public int2 MeshLocalVerticesRange;

	[ReadOnly]
	public NativeArray<float4x4> BodyLocalToWorld;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	[NativeDisableContainerSafetyRestriction]
	public NativeArray<float3> MeshLocalVertexPosition;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	[NativeDisableContainerSafetyRestriction]
	public NativeArray<float3> MeshLocalVertexNormal;

	[NativeDisableParallelForRestriction]
	[NativeDisableContainerSafetyRestriction]
	public NativeArray<float3> ParticleBasePosition;

	[NativeDisableParallelForRestriction]
	[NativeDisableContainerSafetyRestriction]
	public NativeArray<float3> RestNormals;

	public void Execute(int index)
	{
		int index2 = ParticlesRange.x + index;
		int index3 = RestNormalsRange.x + index;
		int index4 = MeshLocalVerticesRange.x + index;
		ParticleBasePosition[index2] = math.mul(BodyLocalToWorld[BodyDescIndex], new float4(MeshLocalVertexPosition[index4], 1f)).xyz;
		RestNormals[index3] = math.mul((float3x3)BodyLocalToWorld[BodyDescIndex], MeshLocalVertexNormal[index4]);
	}
}
