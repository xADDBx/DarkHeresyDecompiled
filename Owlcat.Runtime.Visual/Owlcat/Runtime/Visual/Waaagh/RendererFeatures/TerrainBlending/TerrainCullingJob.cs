using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.TerrainBlending;

[BurstCompile]
internal struct TerrainCullingJob : IJob
{
	public struct Box
	{
		public float3 Min;

		public float3 Max;
	}

	[ReadOnly]
	public NativeArray<float4> FrustumPlanes;

	[ReadOnly]
	public NativeArray<float3> FrustumPoints;

	[ReadOnly]
	public NativeArray<Box> TerrainBoundingBoxes;

	[WriteOnly]
	public NativeArray<bool> TerrainVisibleStatuses;

	public void Execute()
	{
		ReadOnlySpan<Box> readOnlySpan = TerrainBoundingBoxes.AsReadOnlySpan();
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			ref NativeArray<bool> terrainVisibleStatuses = ref TerrainVisibleStatuses;
			int index = i;
			Box box = TerrainBoundingBoxes[i];
			terrainVisibleStatuses[index] = IsBoxInsideOfFrustum(in box);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool IsBoxInsideOfFrustum(in Box box)
	{
		ReadOnlySpan<float4> readOnlySpan = FrustumPlanes.AsReadOnlySpan();
		for (int i = 0; i < FrustumPlanes.Length; i++)
		{
			if (0 + ((math.dot(readOnlySpan[i], new float4(box.Min.x, box.Min.y, box.Min.z, 1f)) < 0f) ? 1 : 0) + ((math.dot(readOnlySpan[i], new float4(box.Max.x, box.Min.y, box.Min.z, 1f)) < 0f) ? 1 : 0) + ((math.dot(readOnlySpan[i], new float4(box.Min.x, box.Max.y, box.Min.z, 1f)) < 0f) ? 1 : 0) + ((math.dot(readOnlySpan[i], new float4(box.Max.x, box.Max.y, box.Min.z, 1f)) < 0f) ? 1 : 0) + ((math.dot(readOnlySpan[i], new float4(box.Min.x, box.Min.y, box.Max.z, 1f)) < 0f) ? 1 : 0) + ((math.dot(readOnlySpan[i], new float4(box.Max.x, box.Min.y, box.Max.z, 1f)) < 0f) ? 1 : 0) + ((math.dot(readOnlySpan[i], new float4(box.Min.x, box.Max.y, box.Max.z, 1f)) < 0f) ? 1 : 0) + ((math.dot(readOnlySpan[i], new float4(box.Max.x, box.Max.y, box.Max.z, 1f)) < 0f) ? 1 : 0) == 8)
			{
				return false;
			}
		}
		ReadOnlySpan<float3> readOnlySpan2 = FrustumPoints.AsReadOnlySpan();
		int num = 0;
		for (int j = 0; j < 8; j++)
		{
			num += ((readOnlySpan2[j].x > box.Max.x) ? 1 : 0);
		}
		if (num == 8)
		{
			return false;
		}
		num = 0;
		for (int k = 0; k < 8; k++)
		{
			num += ((readOnlySpan2[k].x < box.Min.x) ? 1 : 0);
		}
		if (num == 8)
		{
			return false;
		}
		num = 0;
		for (int l = 0; l < 8; l++)
		{
			num += ((readOnlySpan2[l].y > box.Max.y) ? 1 : 0);
		}
		if (num == 8)
		{
			return false;
		}
		num = 0;
		for (int m = 0; m < 8; m++)
		{
			num += ((readOnlySpan2[m].y < box.Min.y) ? 1 : 0);
		}
		if (num == 8)
		{
			return false;
		}
		num = 0;
		for (int n = 0; n < 8; n++)
		{
			num += ((readOnlySpan2[n].z > box.Max.z) ? 1 : 0);
		}
		if (num == 8)
		{
			return false;
		}
		num = 0;
		for (int num2 = 0; num2 < 8; num2++)
		{
			num += ((readOnlySpan2[num2].z < box.Min.z) ? 1 : 0);
		}
		if (num == 8)
		{
			return false;
		}
		return true;
	}
}
