using System.Runtime.CompilerServices;
using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Jobs;

namespace Owlcat.Runtime.Visual.XPBD.Collisions.Jobs;

[BurstCompile]
public struct UpdateCollidersJob : IJobParallelForTransform
{
	public float ColliderCCD;

	[NativeDisableParallelForRestriction]
	[ReadOnly]
	public NativeArray<int> TransformColliderMap;

	[NativeDisableParallelForRestriction]
	[ReadOnly]
	public NativeArray<ColliderShape> ColliderShape;

	[NativeDisableParallelForRestriction]
	public NativeArray<Owlcat.Runtime.Visual.XPBD.DataStructures.AffineTransform> ColliderTransform;

	[NativeDisableParallelForRestriction]
	[WriteOnly]
	public NativeArray<Owlcat.Runtime.Visual.XPBD.DataStructures.AffineTransform> ColliderPrevTransform;

	[NativeDisableParallelForRestriction]
	[WriteOnly]
	public NativeArray<Aabb> ColliderAabb;

	[NativeDisableParallelForRestriction]
	public NativeArray<Aabb> ColliderPrevAabb;

	public void Execute(int index, TransformAccess transform)
	{
		int index2 = TransformColliderMap[index];
		Owlcat.Runtime.Visual.XPBD.DataStructures.AffineTransform transform2 = ColliderTransform[index2];
		Owlcat.Runtime.Visual.XPBD.DataStructures.AffineTransform other = transform2;
		transform2.FromTransform(transform);
		ColliderShape shape = ColliderShape[index2];
		Aabb other2 = CalculateColliderAabb(in transform2, in shape);
		Aabb aabb = ColliderPrevAabb[index2];
		Aabb value = other2;
		if (ColliderCCD > 0f)
		{
			value.Min = math.lerp(other2.Min, aabb.Min, ColliderCCD);
			value.Max = math.lerp(other2.Max, aabb.Max, ColliderCCD);
			value.Encapsulate(in other2);
			other = transform2.Interpolate(other, ColliderCCD, ColliderCCD, ColliderCCD);
		}
		else
		{
			other = transform2;
		}
		aabb = other2;
		ColliderPrevTransform[index2] = other;
		ColliderTransform[index2] = transform2;
		ColliderAabb[index2] = value;
		ColliderPrevAabb[index2] = aabb;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Aabb CalculateColliderAabb(in Owlcat.Runtime.Visual.XPBD.DataStructures.AffineTransform transform, in ColliderShape shape)
	{
		return (ShapeType)shape.ShapeType switch
		{
			ShapeType.Sphere => CalculateSphereAabb(transform, in shape), 
			ShapeType.Box => CalculateBoxAabb(in transform, in shape), 
			ShapeType.Capsule => CalculateCapsuleAabb(transform, in shape), 
			ShapeType.FrustumCapsule => CalculateFrustumCapsuleAabb(transform, in shape), 
			_ => CalculateSphereAabb(transform, in shape), 
		};
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Aabb CalculateSphereAabb(Owlcat.Runtime.Visual.XPBD.DataStructures.AffineTransform transform, in ColliderShape shape)
	{
		Aabb result = default(Aabb);
		result.Center = shape.Center.xyz;
		result.Size = shape.Size.xyz * 2f;
		float3 x = math.abs(transform.Scale.xyz);
		transform.Scale.xyz = math.cmax(x);
		result.Transform(in transform);
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Aabb CalculateBoxAabb(in Owlcat.Runtime.Visual.XPBD.DataStructures.AffineTransform transform, in ColliderShape shape)
	{
		Aabb result = default(Aabb);
		result.Center = shape.Center.xyz;
		result.Size = shape.Size.xyz;
		result.Transform(in transform);
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Aabb CalculateCapsuleAabb(Owlcat.Runtime.Visual.XPBD.DataStructures.AffineTransform transform, in ColliderShape shape)
	{
		float x = shape.Size.x;
		float num = shape.Size.y * 0.5f;
		int index = (int)shape.Size.z;
		float3 x2 = math.abs(transform.Scale.xyz);
		x2[index] = 0f;
		float3 xyz = math.cmax(x2);
		xyz[index] = transform.Scale[index];
		transform.Scale.xyz = xyz;
		float3 @float = 0;
		@float[index] = 1f;
		@float *= math.max(num - x, 0f);
		Aabb aabb = default(Aabb);
		aabb.Center = shape.Center.xyz + @float;
		aabb.Size = x * 2f;
		Aabb other = aabb;
		other.Center = shape.Center.xyz - @float;
		aabb.Transform(in transform);
		other.Transform(in transform);
		aabb.Encapsulate(in other);
		return aabb;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Aabb CalculateFrustumCapsuleAabb(Owlcat.Runtime.Visual.XPBD.DataStructures.AffineTransform transform, in ColliderShape shape)
	{
		Aabb result = default(Aabb);
		result.Center = shape.Center.xyz;
		result.Size = shape.Center.w * 2f;
		float3 x = math.abs(transform.Scale.xyz);
		transform.Scale.xyz = math.cmax(x);
		result.Transform(in transform);
		Aabb other = default(Aabb);
		other.Center = shape.Size.xyz;
		other.Size = shape.Size.w * 2f;
		other.Transform(in transform);
		result.Encapsulate(in other);
		return result;
	}
}
