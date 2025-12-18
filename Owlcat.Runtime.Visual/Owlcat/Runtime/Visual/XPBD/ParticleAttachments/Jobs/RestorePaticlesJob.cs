using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Owlcat.Runtime.Visual.XPBD.ParticleAttachments.Jobs;

[BurstCompile]
public struct RestorePaticlesJob : IJobParallelFor
{
	[ReadOnly]
	public NativeList<int> ParticleIndexToRestore;

	[ReadOnly]
	public NativeList<float> ParticleInvMassToRestore;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float> ParticleInvMass;

	public void Execute(int index)
	{
		int index2 = ParticleIndexToRestore[index];
		ParticleInvMass[index2] = ParticleInvMassToRestore[index];
	}
}
