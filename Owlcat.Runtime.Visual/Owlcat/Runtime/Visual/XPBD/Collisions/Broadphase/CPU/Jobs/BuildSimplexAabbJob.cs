using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Owlcat.Runtime.Visual.XPBD.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Collisions.Broadphase.CPU.Jobs;

[BurstCompile]
public struct BuildSimplexAabbJob : IJobParallelFor
{
	public int2 ParticlesRange;

	public int2 ConstraintsRange;

	public int2 SimplexConstraintsRange;

	public float ContinousCollisionDetection;

	public float Dt;

	public float CollisionMargin;

	public int BodyLayer;

	[NativeDisableParallelForRestriction]
	[ReadOnly]
	public NativeArray<int4> SimplexIndices;

	[NativeDisableParallelForRestriction]
	[NativeDisableContainerSafetyRestriction]
	[WriteOnly]
	public NativeArray<float4> SimplexParameters0;

	[NativeDisableParallelForRestriction]
	[NativeDisableContainerSafetyRestriction]
	[WriteOnly]
	public NativeArray<float4> SimplexParameters1;

	[NativeDisableParallelForRestriction]
	[ReadOnly]
	public NativeArray<float3> ParticlePosition;

	[NativeDisableParallelForRestriction]
	[ReadOnly]
	public NativeArray<float3> ParticleVelocity;

	[NativeDisableParallelForRestriction]
	[ReadOnly]
	public NativeArray<float> ParticleRadius;

	public void Execute(int index)
	{
		int index2 = ConstraintsRange.x + SimplexConstraintsRange.x + index;
		int4 indices = SimplexIndices[index2];
		XPBDMath.GetSimplexIndicesAndSize(ref indices, in ParticlesRange.x, out var size);
		float3 min = float.MaxValue;
		float3 max = float.MinValue;
		Aabb aabb = new Aabb(in min, in max);
		for (int i = 0; i < size; i++)
		{
			int index3 = indices[i];
			float num = 0f;
			float3 @float = ParticleVelocity[index3];
			float x = math.max(1E-06f, math.length(@float));
			float3 float2 = @float * math.rcp(x);
			x = math.min(x, 10f);
			@float = float2 * x;
			min = ParticlePosition[index3];
			max = ParticlePosition[index3] + @float * ContinousCollisionDetection * Dt;
			aabb.EncapsulateParticle(in min, in max, ParticleRadius[index3] + num + CollisionMargin);
		}
		SimplexParameters0[index2] = new float4(aabb.Min, BodyLayer);
		SimplexParameters1[index2] = new float4(aabb.Max, ParticlesRange.x);
	}
}
