using System.Runtime.CompilerServices;
using Owlcat.Runtime.Visual.XPBD.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Solvers.CPU.Jobs;

[BurstCompile]
public struct UpdateVelocitiesJob : IJobParallelFor
{
	private struct BodyDesc
	{
		public int2 ParticlesRange;

		public float Damping;
	}

	public float SubstepDt;

	public float SleepThreshold;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int> VisibleBodyIndices;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int2> BodyDescriptorParticleRange;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float4> BodyDescriptorBodySimulationParameters;

	[NativeDisableParallelForRestriction]
	public NativeArray<float3> ParticlePosition;

	[NativeDisableParallelForRestriction]
	public NativeArray<float3> ParticlePrevPosition;

	[NativeDisableParallelForRestriction]
	public NativeArray<float3> ParticleVelocity;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float> ParticleInvMass;

	public void Execute(int index)
	{
		BodyDesc bodyDesc = LoadBodyDescriptor(index);
		UpdateVelocities(in bodyDesc);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private BodyDesc LoadBodyDescriptor(int index)
	{
		int index2 = VisibleBodyIndices[index];
		XPBDMath.UnpackWindFlagAndDamping(BodyDescriptorBodySimulationParameters[index2].x, out var _, out var damping);
		BodyDesc result = default(BodyDesc);
		result.ParticlesRange = BodyDescriptorParticleRange[index2];
		result.Damping = math.pow(1f - math.saturate(damping), SubstepDt);
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void UpdateVelocities(in BodyDesc bodyDesc)
	{
		for (int i = 0; i < bodyDesc.ParticlesRange.y; i++)
		{
			int index = bodyDesc.ParticlesRange.x + i;
			float3 @float = 0;
			if (ParticleInvMass[index] > 0f)
			{
				@float = XPBDMath.DifferentiateLinear(ParticlePosition[index], ParticlePrevPosition[index], SubstepDt);
			}
			@float *= bodyDesc.Damping;
			if (math.dot(@float, @float) <= SleepThreshold)
			{
				@float = 0;
				ParticlePosition[index] = ParticlePrevPosition[index];
			}
			ParticleVelocity[index] = @float;
		}
	}
}
