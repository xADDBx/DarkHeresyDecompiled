using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine.Jobs;

namespace Owlcat.Runtime.Visual.XPBD.Solvers.CPU.Jobs;

[BurstCompile]
public struct CopyBonesJob : IJobParallelForTransform
{
	public int BodyDescIndex;

	public int2 BonesRange;

	public int2 ParticlesRange;

	[ReadOnly]
	public NativeArray<float4x4> BodyWorldToLocal;

	[NativeDisableParallelForRestriction]
	[NativeDisableContainerSafetyRestriction]
	[WriteOnly]
	public NativeArray<float4x4> Boneposes;

	[NativeDisableParallelForRestriction]
	[NativeDisableContainerSafetyRestriction]
	[WriteOnly]
	public NativeArray<float3> ParticleBasePosition;

	[NativeDisableParallelForRestriction]
	[ReadOnly]
	public NativeArray<int> BoneToParticleMap;

	public void Execute(int index, TransformAccess transform)
	{
		int index2 = BonesRange.x + index;
		int num = BoneToParticleMap[index2];
		Boneposes[index2] = math.mul(BodyWorldToLocal[BodyDescIndex], transform.localToWorldMatrix);
		if (num > -1)
		{
			ParticleBasePosition[ParticlesRange.x + num] = transform.position;
		}
	}
}
