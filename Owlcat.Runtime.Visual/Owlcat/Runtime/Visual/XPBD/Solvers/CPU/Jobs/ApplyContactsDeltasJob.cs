using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Solvers.CPU.Jobs;

[BurstCompile]
public struct ApplyContactsDeltasJob : IJobParallelFor
{
	[NativeDisableParallelForRestriction]
	public NativeArray<float3> ParticlePosition;

	[NativeDisableParallelForRestriction]
	public NativeArray<int3> ParticleDeltas;

	[NativeDisableParallelForRestriction]
	public NativeArray<int> ParticleCounts;

	public void Execute(int index)
	{
		int3 x = ParticleDeltas[index];
		int num = ParticleCounts[index];
		if (num > 0)
		{
			float3 @float = math.asfloat(x) / num;
			ParticlePosition[index] += @float;
		}
		ParticleDeltas[index] = int3.zero;
		ParticleCounts[index] = 0;
	}
}
