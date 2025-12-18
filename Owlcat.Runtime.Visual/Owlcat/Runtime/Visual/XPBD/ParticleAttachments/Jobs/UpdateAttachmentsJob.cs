using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.ParticleAttachments.Jobs;

[BurstCompile]
public struct UpdateAttachmentsJob : IJobParallelFor
{
	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int> AttachmentIndicesMap;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int2> AttachmentParticleDataRange;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float4x4> AttachmentLocalToWorld;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int2> BodyParticleRange;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int> AttachmentParticleIndices;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float3> AttachmentParticleOffsets;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float3> ParticlePosition;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float> ParticleInvMass;

	public void Execute(int index)
	{
		int index2 = AttachmentIndicesMap[index];
		int2 @int = AttachmentParticleDataRange[index2];
		float4x4 a = AttachmentLocalToWorld[index2];
		int2 int2 = BodyParticleRange[index2];
		for (int i = 0; i < @int.y; i++)
		{
			int index3 = int2.x + AttachmentParticleIndices[@int.x + i];
			float3 xyz = AttachmentParticleOffsets[@int.x + i];
			float3 xyz2 = math.mul(a, new float4(xyz, 1f)).xyz;
			ParticlePosition[index3] = xyz2;
			ParticleInvMass[index3] = 0f;
		}
	}
}
