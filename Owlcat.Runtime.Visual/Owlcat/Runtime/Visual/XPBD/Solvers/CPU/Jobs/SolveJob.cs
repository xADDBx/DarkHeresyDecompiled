using System.Runtime.CompilerServices;
using Owlcat.Runtime.Visual.XPBD.Constraints;
using Owlcat.Runtime.Visual.XPBD.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Solvers.CPU.Jobs;

[BurstCompile]
public struct SolveJob : IJobParallelFor
{
	private struct BodyDesc
	{
		public int2 ParticlesRange;

		public int2 ConstraintsBatchesRange;

		public uint EnabledConstraints;

		public int2 ConstraintSettingsRange;

		public int2 VerticesRange;
	}

	public float SubstepDt;

	public int SubstepIndex;

	public int Substeps;

	public float3 Gravity;

	public float SleepThreshold;

	public float DeltaTimeRcpSqr;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int> VisibleBodyIndices;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int2> BodyDescriptorParticleRange;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int2> BodyDescriptorConstraintsRange;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int2> BodyDescriptorConstraintBatchesRange;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int2> BodyDescriptorSimplexConstraintsRange;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<uint> BodyDescriptorEnabledConstraints;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int2> BodyDescriptorConstraintSettingsRange;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int2> BodyDescriptorVerticesRange;

	[NativeDisableParallelForRestriction]
	public NativeArray<float3> ParticlePosition;

	[NativeDisableParallelForRestriction]
	public NativeArray<float3> ParticleBasePosition;

	[NativeDisableParallelForRestriction]
	public NativeArray<float3> ParticlePrevPosition;

	[NativeDisableParallelForRestriction]
	public NativeArray<float3> ParticleVelocity;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float> ParticleInvMass;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float> ParticleRadius;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int3> ConstraintsBatches;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float4> ConstraintSettings;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int4> ConstraintIndices;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float4> ConstraintParameters0;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float3> VertexRestNormal;

	public void Execute(int index)
	{
		BodyDesc bodyDesc = LoadBodyDescriptor(index);
		PredictPositions(in bodyDesc);
		ApplyConstraints(SubstepIndex, in bodyDesc);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private BodyDesc LoadBodyDescriptor(int index)
	{
		int index2 = VisibleBodyIndices[index];
		BodyDesc result = default(BodyDesc);
		result.ParticlesRange = BodyDescriptorParticleRange[index2];
		result.ConstraintsBatchesRange = BodyDescriptorConstraintBatchesRange[index2];
		result.EnabledConstraints = BodyDescriptorEnabledConstraints[index2];
		result.ConstraintSettingsRange = BodyDescriptorConstraintSettingsRange[index2];
		result.VerticesRange = BodyDescriptorVerticesRange[index2];
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void PredictPositions(in BodyDesc bodyDesc)
	{
		for (int i = 0; i < bodyDesc.ParticlesRange.y; i++)
		{
			int index = bodyDesc.ParticlesRange.x + i;
			ParticlePrevPosition[index] = ParticlePosition[index];
			if (ParticleInvMass[index] > 0f)
			{
				ParticleVelocity[index] += Gravity * SubstepDt;
			}
			ParticlePosition[index] = XPBDMath.IntegrateLinear(ParticlePosition[index], ParticleVelocity[index], SubstepDt);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void ApplyConstraints(int substep, in BodyDesc bodyDesc)
	{
		for (int i = 0; i < bodyDesc.ConstraintsBatchesRange.y; i++)
		{
			int3 @int = ConstraintsBatches[bodyDesc.ConstraintsBatchesRange.x + i];
			ConstraintType constraintType = (ConstraintType)@int.z;
			if (constraintType != ConstraintType.Simplex && XPBDMath.IsBitSetted(in bodyDesc.EnabledConstraints, in @int.z))
			{
				int2 constraintsRange = @int.xy;
				SolveConstraints(substep, in constraintsRange, in constraintType, in bodyDesc);
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void SolveConstraints(int substep, in int2 constraintsRange, in ConstraintType constraintType, in BodyDesc bodyDesc)
	{
		float4 constraintSettings = ConstraintSettings[(int)(bodyDesc.ConstraintSettingsRange.x + constraintType)];
		switch (constraintType)
		{
		case ConstraintType.Distance:
			SolveDistane(in constraintsRange, in constraintSettings, in bodyDesc);
			break;
		case ConstraintType.Bend:
			SolveBend(in constraintsRange, in constraintSettings, in bodyDesc);
			break;
		case ConstraintType.Angular:
			SolveAngular(in substep, in constraintsRange, in constraintSettings, in bodyDesc);
			break;
		case ConstraintType.Foliage:
			SolveFoliage(in constraintsRange, in constraintSettings, in bodyDesc);
			break;
		case ConstraintType.Shape:
			SolveShape(in constraintsRange, in constraintSettings, in bodyDesc);
			break;
		case ConstraintType.Simplex:
		case ConstraintType.Aerodynamics:
			break;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void SolveDistane(in int2 constraintsRange, in float4 constraintSettings, in BodyDesc bodyDesc)
	{
		for (int i = 0; i < constraintsRange.y; i++)
		{
			int index = constraintsRange.x + i;
			int4 @int = ConstraintIndices[index] + bodyDesc.ParticlesRange.x;
			int x = @int.x;
			int y = @int.y;
			float num = ParticleInvMass[x];
			float num2 = ParticleInvMass[y];
			float3 @float = ParticlePosition[x];
			float3 float2 = ParticlePosition[y];
			float num3 = constraintSettings.x * DeltaTimeRcpSqr;
			float num4 = num + num2;
			float3 float3 = @float - float2;
			float num5 = math.length(float3);
			float3 /= num5 + 1E-06f;
			float num6 = math.distance(ParticleBasePosition[x], ParticleBasePosition[y]);
			float num7 = num5 - num6;
			float num8 = (0f - (num7 - math.max(math.min(num7, 0f), 0f - constraintSettings.y))) / (num4 + num3 + 1E-06f);
			ParticlePosition[x] = @float + float3 * num8 * num;
			ParticlePosition[y] = float2 - float3 * num8 * num2;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void SolveBend(in int2 constraintsRange, in float4 constraintSettings, in BodyDesc bodyDesc)
	{
		for (int i = 0; i < constraintsRange.y; i++)
		{
			int index = constraintsRange.x + i;
			int4 @int = ConstraintIndices[index] + bodyDesc.ParticlesRange.x;
			int x = @int.x;
			int y = @int.y;
			int z = @int.z;
			float num = ParticleInvMass[x];
			float num2 = ParticleInvMass[y];
			float num3 = ParticleInvMass[z];
			float num4 = constraintSettings.x * DeltaTimeRcpSqr;
			float num5 = num + num2 + 2f * num3;
			float3 @float = ParticlePosition[z] - (ParticlePosition[x] + ParticlePosition[y] + ParticlePosition[z]) / 3f;
			float num6 = math.length(@float);
			float3 positionA = ParticleBasePosition[x];
			float3 positionB = ParticleBasePosition[y];
			float3 positionC = ParticleBasePosition[z];
			float num7 = Bend(in positionA, in positionB, in positionC);
			float3 float2 = @float / (num6 + 1E-06f);
			float num8 = num6 - num7;
			num8 = math.max(0f, num8 - constraintSettings.y) + math.min(0f, num8 + constraintSettings.y);
			float num9 = (0f - num8) / (num5 + num4 + 1E-06f);
			ParticlePosition[x] += -float2 * 2f * num9 * num;
			ParticlePosition[y] += -float2 * 2f * num9 * num2;
			ParticlePosition[z] += float2 * 4f * num9 * num3;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Bend(in float3 positionA, in float3 positionB, in float3 positionC)
	{
		float3 @float = (positionA + positionB + positionC) / 3f;
		return math.length(positionC - @float);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void SolveAngular(in int substep, in int2 constraintsRange, in float4 constraintSettings, in BodyDesc bodyDesc)
	{
		for (int i = 0; i < constraintsRange.y; i++)
		{
			int index = constraintsRange.x + i;
			int4 @int = ConstraintIndices[index] + bodyDesc.ParticlesRange.x;
			int x = @int.x;
			int y = @int.y;
			int z = @int.z;
			float t = constraintSettings.x;
			float y2 = constraintSettings.y;
			bool num = constraintSettings.z > 0f;
			float t2 = (float)substep / (float)Substeps;
			float num2 = math.lerp(0.1f, 0.5f, t2);
			float3 @float = ParticlePosition[x];
			float3 float2 = ParticleBasePosition[x];
			float num3 = ParticleInvMass[x];
			float3 float3 = ParticlePosition[y];
			float3 float4 = ParticleBasePosition[y];
			float num4 = ParticleInvMass[y];
			float3 from = @float - float3;
			float3 to = float2 - float4;
			if (num && z >= 0 && z != y)
			{
				float3 float5 = ParticlePosition[z];
				float3 float6 = ParticleBasePosition[z];
				float3 to2 = float3 - float5;
				float3 from2 = float4 - float6;
				float t3 = 1f;
				to = math.rotate(XPBDMath.FromToRotation(in from2, in to2, in t3), to);
			}
			float3 float7 = math.mul(XPBDMath.FromToRotation(in from, in to, in t), from);
			float3 float8 = float3 + from * num2;
			float3 float9 = float8 - float7 * num2;
			float3 float10 = float8 + float7 * (1f - num2);
			float3 float11 = float9 - float3;
			float3 float12 = float10 - @float;
			float11 *= num4;
			float12 *= num3;
			if (num4 > 0f)
			{
				ParticlePosition[y] = float3 + float11;
				ParticlePrevPosition[y] += float11 * y2;
			}
			if (num3 > 0f)
			{
				ParticlePosition[x] = @float + float12;
				ParticlePrevPosition[x] += float12 * y2;
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void SolveFoliage(in int2 constraintsRange, in float4 constraintSettings, in BodyDesc bodyDesc)
	{
		for (int i = 0; i < constraintsRange.y; i++)
		{
			int index = constraintsRange.x + i;
			int4 @int = ConstraintIndices[index] + bodyDesc.ParticlesRange.x;
			int x = @int.x;
			int y = @int.y;
			int z = @int.z;
			float x2 = constraintSettings.x;
			float y2 = constraintSettings.y;
			bool num = constraintSettings.z > 0f;
			float3 @float = ParticlePosition[x];
			float3 float2 = ParticleBasePosition[x];
			float num2 = ParticleInvMass[x];
			float3 float3 = ParticlePosition[y];
			float3 float4 = ParticleBasePosition[y];
			_ = ParticleInvMass[y];
			float3 float5 = float2 - float4;
			if (num && z >= 0 && z != y)
			{
				float3 float6 = ParticlePosition[z];
				float3 float7 = ParticleBasePosition[z];
				float3 to = float3 - float6;
				float3 from = float4 - float7;
				float t = 1f;
				float5 = math.rotate(XPBDMath.FromToRotation(in from, in to, in t), float5);
			}
			float3 float8 = @float - float3;
			float3 float9 = (math.normalize(float5) * math.length(float8) - float8) * x2 * SubstepDt;
			if (num2 > 0f)
			{
				ParticlePosition[x] = @float + float9 * num2;
				ParticlePrevPosition[x] = math.lerp(ParticlePrevPosition[x], ParticlePosition[x], y2);
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void SolveShape(in int2 constraintsRange, in float4 constraintSettings, in BodyDesc bodyDesc)
	{
		for (int i = 0; i < constraintsRange.y; i++)
		{
			int index = constraintsRange.x + i;
			int4 @int = ConstraintIndices[index];
			float4 @float = ConstraintParameters0[index];
			int index2 = @int.x + bodyDesc.ParticlesRange.x;
			float num = @float.x * DeltaTimeRcpSqr;
			float y = @float.y;
			float z = @float.z;
			bool flag = constraintSettings.x > 0f;
			z += constraintSettings.y;
			if (!(ParticleInvMass[index2] > 0f))
			{
				continue;
			}
			float3 float2 = ParticlePosition[index2] - ParticleBasePosition[index2];
			float num2 = math.length(float2);
			float3 float3 = float2 / (num2 + 1E-06f);
			float num3 = (0f - math.max(0f, num2 - y)) / (1f + num);
			float3 float4 = float3 * num3;
			if (flag && bodyDesc.VerticesRange.x > -1)
			{
				int index3 = @int.x + bodyDesc.VerticesRange.x;
				float3 float5 = VertexRestNormal[index3];
				float4 plane = XPBDMath.CreatePlane(ParticleBasePosition[index2] + float5 * z, float5);
				float3 point = ParticlePosition[index2];
				float num4 = XPBDMath.DistanceToPlane(in plane, in point);
				if (num4 < 0f)
				{
					float4 += (0f - num4) * float5;
				}
			}
			ParticlePosition[index2] += float4;
		}
	}
}
