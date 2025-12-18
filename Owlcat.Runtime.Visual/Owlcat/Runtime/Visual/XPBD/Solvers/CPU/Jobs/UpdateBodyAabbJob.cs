using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Solvers.CPU.Jobs;

[BurstCompile]
public struct UpdateBodyAabbJob : IJobParallelFor
{
	[ReadOnly]
	public NativeArray<int> BodiesMap;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int2> ParticleRanges;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float3> ParticlePosition;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float> ParticleRadius;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<Aabb> BodyAabb;

	public void Execute(int index)
	{
		int index2 = BodiesMap[index];
		int2 @int = ParticleRanges[index2];
		float3 point = ParticlePosition[@int.x];
		Aabb value = new Aabb(in point);
		for (int i = 1; i < @int.y; i++)
		{
			int index3 = @int.x + i;
			float3 @float = ParticlePosition[index3];
			float num = ParticleRadius[index3];
			value.Min = math.min(value.Min, @float - num);
			value.Max = math.max(value.Max, @float + num);
		}
		BodyAabb[index2] = value;
	}
}
