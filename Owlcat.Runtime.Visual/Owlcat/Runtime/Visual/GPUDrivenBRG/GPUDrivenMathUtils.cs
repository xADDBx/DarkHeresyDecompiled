using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

public static class GPUDrivenMathUtils
{
	private static readonly float3 s_PositiveInfinity3 = float.PositiveInfinity;

	private static readonly float3 s_NegativeInfinity3 = float.NegativeInfinity;

	[BurstCompile]
	private static (float3 center, float3 extents) LocalToWorldBounds(Bounds bounds, float4x4 localToWorldMatrix)
	{
		Vector3 min = bounds.min;
		Vector3 max = bounds.max;
		Span<float3> span = stackalloc float3[8];
		span[0] = math.float3(min.x, min.y, min.z);
		span[1] = math.float3(min.x, min.y, max.z);
		span[2] = math.float3(min.x, max.y, min.z);
		span[3] = math.float3(min.x, max.y, max.z);
		span[4] = math.float3(max.x, min.y, min.z);
		span[5] = math.float3(max.x, min.y, max.z);
		span[6] = math.float3(max.x, max.y, min.z);
		span[7] = math.float3(max.x, max.y, max.z);
		float3 @float = s_PositiveInfinity3;
		float3 float2 = s_NegativeInfinity3;
		Span<float3> span2 = span;
		for (int i = 0; i < span2.Length; i++)
		{
			float3 xyz = span2[i];
			float3 xyz2 = math.mul(localToWorldMatrix, math.float4(xyz, 1f)).xyz;
			@float = math.min(@float, xyz2);
			float2 = math.max(float2, xyz2);
		}
		float3 float3 = (@float + float2) * 0.5f;
		float3 item = float2 - float3;
		return (center: float3, extents: item);
	}

	[BurstCompile]
	public static float4 LocalBoundsToBoundingSphere(Bounds bounds, float4x4 localToWorldMatrix, out float3 aabbExtents)
	{
		(float3 center, float3 extents) tuple = LocalToWorldBounds(bounds, localToWorldMatrix);
		float3 item = tuple.center;
		float3 item2 = tuple.extents;
		item2 = math.max(0.001f, item2);
		aabbExtents = item2;
		float w = math.length(aabbExtents);
		return math.float4(item, w);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static (uint low, uint high) SplitULong(ulong value)
	{
		return (low: (uint)(value & 0xFFFFFFFFu), high: (uint)(value >> 32));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ulong CombineULong(uint low, uint high)
	{
		return ((ulong)high << 32) | low;
	}

	public static float3x3 Inverse3X3(float3x3 m)
	{
		float3 @float = m[0];
		float3 float2 = m[1];
		float3 float3 = m[2];
		float3 float4 = math.cross(float2, float3);
		float3 c = math.cross(float3, @float);
		float3 c2 = math.cross(@float, float2);
		float num = math.dot(@float, float4);
		return math.transpose(math.float3x3(float4, c, c2) / num);
	}

	public static float4x4 AffineInverse3D(float4x4 m)
	{
		float3x3 m2 = (float3x3)m;
		float3 xyz = m.c3.xyz;
		float3x3 a = Inverse3X3(m2);
		float3 @float = -math.mul(a, xyz);
		return math.float4x4(a.c0.x, a.c1.x, a.c2.x, @float.x, a.c0.y, a.c1.y, a.c2.y, @float.y, a.c0.z, a.c1.z, a.c2.z, @float.z, 0f, 0f, 0f, 1f);
	}

	public static float2 ApproximateScreenSpaceSize(float4 boundingSphereVS, float4x4 projectionMatrix)
	{
		float3 xyz = boundingSphereVS.xyz;
		float w = boundingSphereVS.w;
		float num = math.sqrt(math.dot(xyz, xyz) - w * w);
		float3 xyz2 = w / num * math.float3(0f - xyz.z, 0f, xyz.x);
		float3 xyz3 = math.float3(0f, w, 0f);
		float4 @float = math.mul(projectionMatrix, math.float4(xyz2, 0f));
		float4 float2 = math.mul(projectionMatrix, math.float4(xyz3, 0f));
		float4 float3 = math.mul(projectionMatrix, math.float4(xyz, 1f));
		float4 float4 = float3 + float2;
		float4 float5 = float3 + @float;
		float4 float6 = float3 - float2;
		float4 float7 = float3 - @float;
		float4.xy /= float4.w;
		float5.xy /= float5.w;
		float7.xy /= float7.w;
		float6.xy /= float6.w;
		float2 float8 = math.min(math.min(math.min(float5.xy, float7.xy), float4.xy), float6.xy);
		return (math.max(math.max(math.max(float5.xy, float7.xy), float4.xy), float6.xy) - float8) * 0.5f;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float3x4 PackTransformMatrix(float4x4 transformMatrix)
	{
		float3x4 result = default(float3x4);
		result.c0 = transformMatrix.c0.xyz;
		result.c1 = transformMatrix.c1.xyz;
		result.c2 = transformMatrix.c2.xyz;
		result.c3 = transformMatrix.c3.xyz;
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float4x4 UnpackTransformMatrix(float3x4 transformMatrix)
	{
		float4x4 result = default(float4x4);
		result.c0 = math.float4(transformMatrix.c0, 0f);
		result.c1 = math.float4(transformMatrix.c1, 0f);
		result.c2 = math.float4(transformMatrix.c2, 0f);
		result.c3 = math.float4(transformMatrix.c3, 1f);
		return result;
	}
}
