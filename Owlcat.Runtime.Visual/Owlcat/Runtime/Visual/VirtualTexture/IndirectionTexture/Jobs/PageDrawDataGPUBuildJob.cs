using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.VirtualTexture.IndirectionTexture.Jobs;

[BurstCompile]
public struct PageDrawDataGPUBuildJob : IJob
{
	public int2 VirtualAtlasResolutionInTiles;

	[ReadOnly]
	internal NativeList<PageDrawData> DrawData;

	[WriteOnly]
	internal NativeList<PageDrawDataGPU> DrawDataGPU;

	public void Execute()
	{
		PageDrawDataGPU value = default(PageDrawDataGPU);
		for (int i = 0; i < DrawData.Length; i++)
		{
			float3 xyz = PackFloat2To888(DrawData[i].DrawPos.xy);
			value.PageData = new float4(xyz, PackByte2ToUNorm((byte)DrawData[i].MipLevel, (byte)DrawData[i].SliceIndex));
			float4 rect = DrawData[i].Rect;
			value.ScaleBias = new float4(rect.zw / VirtualAtlasResolutionInTiles, rect.xy / VirtualAtlasResolutionInTiles);
			DrawDataGPU.Add(in value);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint3 PackFloat2To888UInt(float2 f)
	{
		uint2 @uint = (uint2)(f * 4095.5f);
		uint2 uint2 = @uint >> 8;
		uint2 xy = @uint & 255u;
		uint3 result = 0u;
		result.xy = xy;
		result.z = uint2.x | (uint2.y << 4);
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float3 PackFloat2To888(float2 f)
	{
		return new float3(PackFloat2To888UInt(f)) / 255f;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float PackByte2ToUNorm(byte x, byte y)
	{
		return (float)(int)(byte)(((x & 0xF) << 4) + (y & 0xF)) / 255f;
	}
}
