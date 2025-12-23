using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.DataStructures;

[BurstCompile]
[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@141c9a01de77\\Runtime\\XPBD\\DataStructures\\Aabb.cs")]
public struct Aabb
{
	public float3 Min;

	public float3 Max;

	public float2 _Padding;

	public float3 Center
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return Min + Size * 0.5f;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			float3 @float = Size * 0.5f;
			Min = value - @float;
			Max = value + @float;
		}
	}

	public float3 Size
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return Max - Min;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			float3 center = Center;
			float3 @float = value * 0.5f;
			Min = center - @float;
			Max = center + @float;
		}
	}

	public Aabb(in float3 min, in float3 max)
	{
		Min = min;
		Max = max;
		_Padding = 0;
	}

	public Aabb(in float3 point)
	{
		Min = point;
		Max = point;
		_Padding = 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Encapsulate(in float3 point)
	{
		Min = math.min(Min, point);
		Max = math.max(Max, point);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Encapsulate(in Aabb other)
	{
		Encapsulate(in other.Min);
		Encapsulate(in other.Max);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void FromBounds(in Bounds bounds, in float thickness)
	{
		float3 @float = thickness;
		Min = (float3)bounds.min - @float;
		Max = (float3)bounds.max + @float;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void EncapsulateParticle(in float3 previousPosition, in float3 position, float radius)
	{
		Min = math.min(math.min(Min, position - radius), previousPosition - radius);
		Max = math.max(math.max(Max, position + radius), previousPosition + radius);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IntersectsAabb(in Aabb bounds)
	{
		if (Min[0] <= bounds.Max[0] && Max[0] >= bounds.Min[0] && Min[1] <= bounds.Max[1] && Max[1] >= bounds.Min[1])
		{
			if (Min[2] <= bounds.Max[2])
			{
				return Max[2] >= bounds.Min[2];
			}
			return false;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void Transform(in AffineTransform transform)
	{
		float4x4 transform2 = transform.TRS();
		Transform(in transform2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Transform(in Matrix4x4 transform)
	{
		float4x4 transform2 = transform;
		Transform(in transform2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Transform(in float4x4 transform)
	{
		float3 x = transform.c0.xyz * Min.x;
		float3 y = transform.c0.xyz * Max.x;
		float3 x2 = transform.c1.xyz * Min.y;
		float3 y2 = transform.c1.xyz * Max.y;
		float3 x3 = transform.c2.xyz * Min.z;
		float3 y3 = transform.c2.xyz * Max.z;
		Min = new float3(math.min(x, y) + math.min(x2, y2) + math.min(x3, y3) + transform.c3.xyz);
		Max = new float3(math.max(x, y) + math.max(x2, y2) + math.max(x3, y3) + transform.c3.xyz);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Sweep(float4 velocity)
	{
		Min = math.min(Min, Min + velocity.xyz);
		Max = math.max(Max, Max + velocity.xyz);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Bounds ToBounds()
	{
		return new Bounds(Center, Size);
	}
}
