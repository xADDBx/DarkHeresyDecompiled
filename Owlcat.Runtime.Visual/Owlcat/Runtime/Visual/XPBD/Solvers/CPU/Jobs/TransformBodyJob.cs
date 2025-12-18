using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Solvers.CPU.Jobs;

[BurstCompile]
public struct TransformBodyJob : IJobParallelFor
{
	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int> BodyDescriptorMap;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int2> BodyDescriptorParticleRange;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float4x4> BodyDescriptorLocalToWorld;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float4x4> BodyDescriptorPrevWorldToLocal;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int2> BodyDescriptorSkinBufferRange;

	[NativeDisableParallelForRestriction]
	public NativeArray<float3> ParticlePosition;

	[NativeDisableParallelForRestriction]
	public NativeArray<float3> ParticlePrevPosition;

	public void Execute(int index)
	{
		int index2 = BodyDescriptorMap[index];
		int2 @int = BodyDescriptorParticleRange[index2];
		int2 int2 = BodyDescriptorSkinBufferRange[index2];
		float4x4 a = math.mul(BodyDescriptorLocalToWorld[index2], BodyDescriptorPrevWorldToLocal[index2]);
		if (int2.x < 0)
		{
			for (int i = 0; i < @int.y; i++)
			{
				int index3 = @int.x + i;
				ParticlePosition[index3] = math.mul(a, new float4(ParticlePosition[index3], 1f)).xyz;
				ParticlePrevPosition[index3] = ParticlePosition[index3];
			}
		}
	}
}
