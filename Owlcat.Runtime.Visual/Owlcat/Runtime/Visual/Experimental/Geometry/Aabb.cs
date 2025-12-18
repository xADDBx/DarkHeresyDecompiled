using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.Experimental.Geometry;

[BurstCompile]
public struct Aabb
{
	public float3 Min;

	public float3 Max;

	public readonly float3 Center
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return math.lerp(Min, Max, 0.5f);
		}
	}

	public readonly float3 Size
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return Max - Min;
		}
	}

	public readonly float Area
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			float3 size = Size;
			return 2f * (size.x * size.y + size.y * size.z + size.z * size.x);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Aabb(float3 min, float3 max)
	{
		Min = min;
		Max = max;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Aabb Union(in Aabb a, in Aabb b)
	{
		return new Aabb(math.min(a.Min, b.Min), math.max(a.Max, b.Max));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Overlaps(in Aabb other)
	{
		if (math.all(Min <= other.Max))
		{
			return math.all(Max >= other.Min);
		}
		return false;
	}
}
