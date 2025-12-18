using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

internal static class GPUDrivenCullingUtils
{
	internal struct SplitInfo
	{
		public float4 CullingSphereLS;

		public float3x3 WorldToLightSpaceRotation;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool FrustumCulling(ref NativeArray<Plane>.ReadOnly cullingPlanes, float4 boundingSphere)
	{
		if (cullingPlanes.Length <= 0)
		{
			return true;
		}
		return (double)ComputeSphereToFrustumDistance(ref cullingPlanes, boundingSphere) > 0.0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool FrustumCulling(ref NativeArray<Plane>.ReadOnly cullingPlanes, float3 center, float3 extents)
	{
		if (cullingPlanes.Length <= 0)
		{
			return true;
		}
		int length = cullingPlanes.Length;
		for (int i = 0; i < length; i++)
		{
			Plane plane = cullingPlanes[i];
			float3 @float = plane.normal;
			float3 y = center + math.select(-extents, extents, @float >= 0f);
			if (math.dot(@float, y) + plane.distance < 0f)
			{
				return false;
			}
		}
		return true;
	}

	private static float ComputeSphereToFrustumDistance(ref NativeArray<Plane>.ReadOnly cullingPlanes, float4 boundingSphere)
	{
		float3 xyz = boundingSphere.xyz;
		float w = boundingSphere.w;
		int length = cullingPlanes.Length;
		float num = DistanceToPlane(cullingPlanes[0], xyz);
		for (int i = 1; i < length; i++)
		{
			num = math.min(num, DistanceToPlane(cullingPlanes[i], xyz));
		}
		return num + w;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float DistanceToPlane(Plane plane, float3 position)
	{
		return math.dot(math.float4(position, 1f), math.float4(plane.normal, plane.distance));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool LightSphereCulling(in SplitInfo cullingSplit, float4 casterSphereWS)
	{
		float4 cullingSphereLS = cullingSplit.CullingSphereLS;
		if (cullingSphereLS.w <= 0f)
		{
			return true;
		}
		float3 @float = math.mul(cullingSplit.WorldToLightSpaceRotation, casterSphereWS.xyz);
		float w = casterSphereWS.w;
		float3 xyz = cullingSphereLS.xyz;
		float w2 = cullingSphereLS.w;
		float3 float2 = @float - xyz;
		float num = w + w2;
		float num2 = num * num - math.dot(float2.xy, float2.xy);
		if (num2 < 0f)
		{
			return false;
		}
		if (float2.z > 0f && float2.z * float2.z > num2)
		{
			return false;
		}
		return true;
	}
}
