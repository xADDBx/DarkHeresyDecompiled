using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.Experimental.Geometry;

[BurstCompile]
public struct Obb
{
	public float3 Position;

	public float3 Extents;

	public float3x3 Orientation;

	public Aabb Bounds
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			float3 @float = Orientation.c0 * Extents.x;
			float3 float2 = Orientation.c1 * Extents.y;
			float3 float3 = Orientation.c2 * Extents.z;
			float3 float4 = new float3(math.csum(math.abs(new float3(@float.x, float2.x, float3.x))), math.csum(math.abs(new float3(@float.y, float2.y, float3.y))), math.csum(math.abs(new float3(@float.z, float2.z, float3.z))));
			return new Aabb(Position - float4, Position + float4);
		}
	}

	public float3 Size
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return Extents * 2f;
		}
	}

	public Obb(float3 position, float3 extents, quaternion orientation)
	{
		Position = position;
		Extents = extents;
		Orientation = new float3x3(orientation);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float3 ClosestPoint(float4 sphereSq)
	{
		float3 y = sphereSq.xyz - Position;
		return Position + Orientation.c0 * math.clamp(math.dot(Orientation.c0, y), 0f - Extents.x, Extents.x) + Orientation.c1 * math.clamp(math.dot(Orientation.c1, y), 0f - Extents.y, Extents.y) + Orientation.c2 * math.clamp(math.dot(Orientation.c2, y), 0f - Extents.z, Extents.z);
	}
}
