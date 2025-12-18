using System.Runtime.CompilerServices;
using Owlcat.Runtime.Core.Math.Noise;
using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Owlcat.Runtime.Visual.XPBD.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Solvers.CPU.Jobs;

[BurstCompile]
public struct ApplyExternalForcesJob : IJobParallelFor
{
	private struct BodyDesc
	{
		public int2 ParticlesRange;

		public InertialForces InertialForces;

		public float4x4 WorldToLocal;

		public float4x4 LocalToWorld;

		public float4 BodyLinearInertiaScale;

		public float4 BodyAngularInertiaScale;

		public uint EnabledConstraints;

		public int2 ConstraintSettingsRange;

		public int2 VerticesRange;

		public bool WindEnabled;
	}

	public float Dt;

	public bool WindEnabled;

	public AmbientWindParameters WindParams;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int> VisibleBodyIndices;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int2> BodyDescriptorParticleRange;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<InertialForces> BodyDescriptorInertialForces;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float4x4> BodyDescriptorWorldToLocal;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float4x4> BodyDescriptorLocalToWorld;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float4> BodyDescriptorSimulationParameters;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<uint> BodyDescriptorEnabledConstraints;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int2> BodyDescriptorConstraintSettingsRange;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int2> BodyDescriptorVerticesRange;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float3> ParticlePosition;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float> ParticleInvMass;

	[NativeDisableParallelForRestriction]
	public NativeArray<float3> ParticleVelocity;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float4> ConstraintSettings;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float3> VertexNormal;

	public void Execute(int index)
	{
		BodyDesc bodyDesc = LoadBodyDescriptor(index);
		for (int i = 0; i < bodyDesc.ParticlesRange.y; i++)
		{
			int particleId = bodyDesc.ParticlesRange.x + i;
			ApplyInertialForces(in particleId, in bodyDesc);
			float4 wind = 0;
			float3 particlePosition;
			if (WindEnabled && bodyDesc.WindEnabled)
			{
				particlePosition = ParticlePosition[particleId];
				wind = CalculateWind(in particlePosition);
			}
			particlePosition = bodyDesc.InertialForces.LinearVel.xyz;
			ApplyAerodynamics(in i, in particlePosition, in wind, in bodyDesc);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void ApplyAerodynamics(in int index, in float3 linearVel, in float4 wind, in BodyDesc bodyDesc)
	{
		int index2 = bodyDesc.ParticlesRange.x + index;
		float3 wind2 = new float3(wind.x + wind.z, 0f, wind.y + wind.w);
		ref readonly uint enabledConstraints = ref bodyDesc.EnabledConstraints;
		int bitIndex = 5;
		if (XPBDMath.IsBitSetted(in enabledConstraints, in bitIndex))
		{
			float4 @float = ConstraintSettings[bodyDesc.ConstraintSettingsRange.x + 5];
			float dragCoeff = @float.x;
			float liftCoeff = @float.y;
			float z = @float.z;
			wind2 -= linearVel * z;
			if (bodyDesc.VerticesRange.x > -1)
			{
				int index3 = bodyDesc.VerticesRange.x + index;
				float3 particleNormal = VertexNormal[index3];
				float3 particleVel = ParticleVelocity[index2];
				wind2 = CalculateMeshAerodynamics(in wind2, in particleVel, particleNormal, in dragCoeff, in liftCoeff);
			}
		}
		ParticleVelocity[index2] += ParticleInvMass[index2] * wind2 * Dt;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private BodyDesc LoadBodyDescriptor(int index)
	{
		int index2 = VisibleBodyIndices[index];
		float4 @float = BodyDescriptorSimulationParameters[index2];
		XPBDMath.UnpackWindFlagAndDamping(@float.x, out var windEnabled, out var _);
		BodyDesc result = default(BodyDesc);
		result.ParticlesRange = BodyDescriptorParticleRange[index2];
		result.InertialForces = BodyDescriptorInertialForces[index2];
		result.WorldToLocal = BodyDescriptorWorldToLocal[index2];
		result.LocalToWorld = BodyDescriptorLocalToWorld[index2];
		result.EnabledConstraints = BodyDescriptorEnabledConstraints[index2];
		result.ConstraintSettingsRange = BodyDescriptorConstraintSettingsRange[index2];
		result.VerticesRange = BodyDescriptorVerticesRange[index2];
		result.BodyLinearInertiaScale = @float.y;
		result.BodyAngularInertiaScale = @float.z;
		result.WindEnabled = windEnabled;
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void ApplyInertialForces(in int particleId, in BodyDesc bodyDesc)
	{
		if (ParticleInvMass[particleId] > 0f)
		{
			float3 xyz = math.mul(bodyDesc.WorldToLocal, new float4(ParticlePosition[particleId], 1f)).xyz;
			float3 @float = math.mul((float3x3)bodyDesc.WorldToLocal, ParticleVelocity[particleId]);
			float4 float2 = new float4(math.cross(bodyDesc.InertialForces.EulerAccel.xyz, xyz.xyz), 0f);
			float4 float3 = new float4(math.cross(bodyDesc.InertialForces.AngularVel.xyz, math.cross(bodyDesc.InertialForces.AngularVel.xyz, xyz.xyz)), 0f);
			float4 float4 = 2f * new float4(math.cross(bodyDesc.InertialForces.AngularVel.xyz, @float.xyz), 0f);
			float4 float5 = float2 + float4 + float3;
			float5.xyz = math.mul((float3x3)bodyDesc.LocalToWorld, float5.xyz).xyz;
			float3 xyz2 = (bodyDesc.InertialForces.InertialAccel * bodyDesc.BodyLinearInertiaScale + float5 * bodyDesc.BodyAngularInertiaScale).xyz;
			ParticleVelocity[particleId] -= xyz2 * Dt;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private float4 CalculateWind(in float3 particlePosition)
	{
		ref float4 strengthOctave = ref WindParams.StrengthOctave0;
		bool normalized = true;
		float num = EvaluateOctave(in particlePosition, in strengthOctave, in normalized);
		ref float4 strengthOctave2 = ref WindParams.StrengthOctave1;
		bool normalized2 = true;
		float num2 = num + EvaluateOctave(in particlePosition, in strengthOctave2, in normalized2);
		float end = math.saturate(0.5f + WindParams.StrengthNoiseContrast * (num2 - 0.5f));
		end = math.lerp(1f, end, WindParams.StrengthNoiseWeight);
		ref float4 shiftOctave = ref WindParams.ShiftOctave0;
		normalized = false;
		float num3 = EvaluateOctave(in particlePosition, in shiftOctave, in normalized);
		ref float4 shiftOctave2 = ref WindParams.ShiftOctave1;
		normalized2 = false;
		float num4 = num3 + EvaluateOctave(in particlePosition, in shiftOctave2, in normalized2);
		float2 @float = WindParams.Velocity * end;
		float2 float2 = new float2(@float.y, 0f - @float.x) * num4;
		return new float4(@float.xy, float2.xy);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private float EvaluateOctave(in float3 positionWS, in float4 octave, in bool normalized = false)
	{
		float num = SimplexNoise2D.snoise(positionWS.xz * octave.y - octave.zw);
		if (normalized)
		{
			num = (num + 1f) * 0.5f;
		}
		return num * octave.x;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private float3 CalculateMeshAerodynamics(in float3 wind, in float3 particleVel, float3 particleNormal, in float dragCoeff, in float liftCoeff)
	{
		float3 result = wind;
		float num = math.length(particleNormal);
		particleNormal = math.normalizesafe(particleNormal);
		float3 @float = particleVel - wind;
		float num2 = math.lengthsq(@float);
		if (num2 < 1E-06f)
		{
			return result;
		}
		float3 float2 = @float / math.sqrt(num2);
		float3 x = particleNormal * math.sign(math.dot(particleNormal, float2));
		float x2 = 0.5f * num2 * num;
		float num3 = math.dot(x, float2);
		float3 float3 = math.normalizesafe(math.cross(math.cross(x.xyz, float2.xyz), float2.xyz));
		return ((0f - dragCoeff) * float2 + liftCoeff * float3) * num3 * math.min(x2, 1000f);
	}
}
