using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.XPBD.DataStructures;

[BurstCompile]
public struct InertialFrame
{
	public float4 Position;

	public quaternion Rotation;

	public float4 Velocity;

	public float4 AngularVelocity;

	public float4 Acceleration;

	public float4 AngularAcceleration;

	public static InertialFrame CreateFromTransform(Transform transform)
	{
		InertialFrame result = default(InertialFrame);
		result.Position = new float4(transform.position, 0f);
		result.Rotation = transform.rotation;
		return result;
	}
}
