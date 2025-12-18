using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Jobs;

namespace Owlcat.Runtime.Visual.XPBD.ParticleAttachments.Jobs;

[BurstCompile]
public struct UpdateTransformsJob : IJobParallelForTransform
{
	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int> TransformAttachmentMap;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float4x4> LocalToWorld;

	public void Execute(int index, TransformAccess transform)
	{
		int index2 = TransformAttachmentMap[index];
		LocalToWorld[index2] = transform.localToWorldMatrix;
	}
}
