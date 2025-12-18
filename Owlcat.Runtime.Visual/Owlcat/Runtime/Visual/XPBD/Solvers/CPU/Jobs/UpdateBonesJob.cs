using Owlcat.Runtime.Visual.XPBD.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Solvers.CPU.Jobs;

[BurstCompile]
public struct UpdateBonesJob : IJobParallelFor
{
	public int BodyDescIndex;

	public int2 ParticlesRange;

	public int2 BonesRange;

	[ReadOnly]
	public NativeArray<float4x4> BodyWorldToLocal;

	[NativeDisableParallelForRestriction]
	[ReadOnly]
	public NativeArray<float3> ParticlePosition;

	[NativeDisableParallelForRestriction]
	[ReadOnly]
	public NativeArray<float3> BaseParticlePosition;

	[NativeDisableParallelForRestriction]
	[ReadOnly]
	public NativeArray<float4x4> Bindposes;

	[NativeDisableParallelForRestriction]
	[ReadOnly]
	public NativeArray<float4x4> Boneposes;

	[NativeDisableParallelForRestriction]
	[ReadOnly]
	public NativeArray<int> ParentIndices;

	[NativeDisableParallelForRestriction]
	[ReadOnly]
	public NativeArray<int> BoneToParticleMap;

	[NativeDisableParallelForRestriction]
	[NativeDisableContainerSafetyRestriction]
	[WriteOnly]
	public NativeArray<float4x4> SimulatedBindposes;

	public void Execute(int index)
	{
		float4x4 a = BodyWorldToLocal[BodyDescIndex];
		int index2 = BonesRange.x + index;
		int num = BoneToParticleMap[index2];
		int num2 = ParentIndices[index2];
		float4x4 float4x = Boneposes[index2];
		if (num > -1)
		{
			float3 xyz = ParticlePosition[ParticlesRange.x + num];
			xyz = math.mul(a, new float4(xyz, 1f)).xyz;
			float4x.c3.xyz = xyz.xyz;
			if (num2 > -1)
			{
				num2 = BoneToParticleMap[BonesRange.x + num2];
				float3 xyz2 = math.mul(a, new float4(BaseParticlePosition[ParticlesRange.x + num], 1f)).xyz;
				float3 xyz3 = math.mul(a, new float4(ParticlePosition[ParticlesRange.x + num2], 1f)).xyz;
				float3 xyz4 = math.mul(a, new float4(BaseParticlePosition[ParticlesRange.x + num2], 1f)).xyz;
				float3 from = xyz2 - xyz4;
				float3 to = xyz - xyz3;
				float4 @float = XPBDMath.FromToRotationAxisAngle(in from, in to);
				float4x.c3.xyz = 0;
				float4x = math.mul(float4x4.AxisAngle(@float.xyz, @float.w), float4x);
				float4x.c3.xyz = xyz.xyz;
			}
		}
		SimulatedBindposes[index2] = math.mul(float4x, Bindposes[index2]);
	}
}
