using System.Runtime.CompilerServices;
using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Owlcat.Runtime.Visual.XPBD.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace Owlcat.Runtime.Visual.XPBD.Solvers.CPU.Jobs;

[BurstCompile]
public struct UpdateBodyTransformsJob : IJobParallelForTransform
{
	public float StepDt;

	public float DtBetweenSimulations;

	[ReadOnly]
	public NativeArray<int> BodyIndicesMap;

	[ReadOnly]
	public NativeArray<float4> BodySimulationParameters;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	[NativeDisableContainerSafetyRestriction]
	public NativeArray<float4x4> BodyLocalToWorld;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	[NativeDisableContainerSafetyRestriction]
	public NativeArray<float4x4> BodyPrevWorldToLocal;

	[NativeDisableParallelForRestriction]
	[NativeDisableContainerSafetyRestriction]
	public NativeArray<float4x4> BodyWorldToLocal;

	[NativeDisableParallelForRestriction]
	public NativeArray<InertialFrame> BodyInertialFrame;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<InertialForces> BodyInertialForces;

	public void Execute(int index, TransformAccess transform)
	{
		if (transform.isValid)
		{
			int bodyIndex = BodyIndicesMap[index];
			BodyPrevWorldToLocal[bodyIndex] = BodyWorldToLocal[bodyIndex];
			float4x4 m = BodyLocalToWorld[bodyIndex];
			float4x4 m2 = transform.localToWorldMatrix;
			if (!XPBDMath.AreEqual(in m, in m2))
			{
				Matrix4x4 localToWorldMatrix = transform.localToWorldMatrix;
				BodyLocalToWorld[bodyIndex] = localToWorldMatrix;
				BodyWorldToLocal[bodyIndex] = math.inverse(localToWorldMatrix);
			}
			UpdateInertialFrame(in bodyIndex, in transform);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void UpdateInertialFrame(in int bodyIndex, in TransformAccess transform)
	{
		InertialFrame inertialFrame = BodyInertialFrame[bodyIndex];
		float w = BodySimulationParameters[bodyIndex].w;
		float4 @float = inertialFrame.Position;
		quaternion quaternion = inertialFrame.Rotation;
		float4 velocity = inertialFrame.Velocity;
		float4 angularVelocity = inertialFrame.AngularVelocity;
		inertialFrame.Position = new float4(transform.position, 0f);
		inertialFrame.Rotation = transform.rotation;
		float num = math.distance(@float, inertialFrame.Position);
		if (w > 0f && num > w)
		{
			@float = inertialFrame.Position;
			quaternion = inertialFrame.Rotation;
		}
		else
		{
			if (DtBetweenSimulations > StepDt)
			{
				float t = StepDt / DtBetweenSimulations;
				@float = math.lerp(inertialFrame.Position, @float, t);
				quaternion = math.slerp(inertialFrame.Rotation, quaternion, t);
			}
			num = math.distance(@float, inertialFrame.Position);
		}
		inertialFrame.Velocity = new float4(XPBDMath.DifferentiateLinear(inertialFrame.Position.xyz, @float.xyz, StepDt), 0f);
		inertialFrame.AngularVelocity = XPBDMath.DifferentiateAngular(inertialFrame.Rotation, quaternion, StepDt);
		inertialFrame.Acceleration = new float4(XPBDMath.DifferentiateLinear(inertialFrame.Velocity.xyz, velocity.xyz, StepDt), 0f);
		inertialFrame.AngularAcceleration = new float4(XPBDMath.DifferentiateLinear(inertialFrame.AngularVelocity.xyz, angularVelocity.xyz, StepDt), 0f);
		UpdateInertialForces(in bodyIndex, in inertialFrame, in transform);
		BodyInertialFrame[bodyIndex] = inertialFrame;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void UpdateInertialForces(in int bodyIndex, in InertialFrame inertialFrame, in TransformAccess transform)
	{
		Matrix4x4 localToWorldMatrix = transform.localToWorldMatrix;
		float4x4 float4x = float4x4.TRS(float3.zero, transform.rotation, math.rcp(localToWorldMatrix.lossyScale));
		float4x4 a = math.transpose(float4x);
		InertialForces value = default(InertialForces);
		float4x4 value2 = math.mul(a, math.mul(float4x4.Scale(inertialFrame.AngularVelocity.xyz), float4x));
		value.AngularVel = value2.diagonal();
		value2 = math.mul(a, math.mul(float4x4.Scale(inertialFrame.AngularAcceleration.xyz), float4x));
		value.EulerAccel = value2.diagonal();
		value.LinearVel = inertialFrame.Velocity;
		value.InertialAccel = inertialFrame.Acceleration;
		BodyInertialForces[bodyIndex] = value;
	}
}
