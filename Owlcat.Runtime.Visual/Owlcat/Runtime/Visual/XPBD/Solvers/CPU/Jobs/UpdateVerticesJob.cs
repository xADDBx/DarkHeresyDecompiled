using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Solvers.CPU.Jobs;

[BurstCompile]
public struct UpdateVerticesJob : IJobParallelFor
{
	public int2 ParticlesRange;

	public int2 TrianglesRange;

	public int2 VerticesRange;

	public int2 VertexTrianglesMapRange;

	public int2 VertexTrianglesMapRangesRange;

	public float4x4 BodyWorldToLocal;

	[ReadOnly]
	public NativeArray<int> Triangles;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int> VertexTrianglesMap;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int2> VertexTrianglesMapRanges;

	[ReadOnly]
	public NativeArray<float3> ParticlePosition;

	[NativeDisableContainerSafetyRestriction]
	public NativeArray<float3> VertexPosition;

	[NativeDisableContainerSafetyRestriction]
	public NativeArray<float3> VertexNormal;

	public void Execute(int index)
	{
		int index2 = index + VerticesRange.x;
		VertexPosition[index2] = math.mul(BodyWorldToLocal, new float4(ParticlePosition[index + ParticlesRange.x], 1f)).xyz;
		float3 x = 0;
		int2 @int = VertexTrianglesMapRanges[index + VertexTrianglesMapRangesRange.x];
		for (int i = 0; i < @int.y; i++)
		{
			int index3 = VertexTrianglesMapRange.x + i + @int.x;
			int num = VertexTrianglesMap[index3] * 3 + TrianglesRange.x;
			int index4 = Triangles[num] + ParticlesRange.x;
			int index5 = Triangles[num + 1] + ParticlesRange.x;
			int index6 = Triangles[num + 2] + ParticlesRange.x;
			float3 @float = ParticlePosition[index4];
			float3 float2 = ParticlePosition[index5];
			float3 float3 = math.cross(y: ParticlePosition[index6] - @float, x: float2 - @float);
			x += float3;
		}
		VertexNormal[index2] = math.mul((float3x3)BodyWorldToLocal, math.normalizesafe(x));
	}
}
