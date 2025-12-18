using System;
using System.Runtime.CompilerServices;
using Owlcat.Runtime.Core.Allocators.Guillotiere;
using Owlcat.Runtime.Core.Collections;
using Owlcat.Runtime.Visual.Utilities;
using Unity.Burst;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.VirtualTexture.Atlas;

[BurstCompile]
public struct VirtualAtlasEntry
{
	public TextureStackId StackId;

	public FixedArray4<Guid> ValidGuids;

	public int MipCount;

	public float MipBias;

	public int LayerCount;

	public uint PackedLayerFlags;

	public int4 RectInTiles;

	public int2 TextureSizeInTiles;

	public uint RectAllocId;

	public NodeKind NodeKind;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int CalculateMipLevel(in int2 virtualTileCoord)
	{
		int2 tileCoordLocal = virtualTileCoord - RectInTiles.xy;
		return CalculateMipLevelLocal(in tileCoordLocal);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private int CalculateMipLevelLocal(in int2 tileCoordLocal)
	{
		int result = 0;
		int2 textureSizeInTiles = TextureSizeInTiles;
		if (tileCoordLocal.x < textureSizeInTiles.x)
		{
			return result;
		}
		result = 1;
		textureSizeInTiles >>= 1;
		while (tileCoordLocal.y < textureSizeInTiles.y)
		{
			result++;
			textureSizeInTiles >>= 1;
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal float4 CalculateRect(in int2 tileCoord, int mipLevel, int targetMipLevel)
	{
		BurstAssert.IsTrue(mipLevel >= targetMipLevel);
		int num = mipLevel - targetMipLevel;
		int3 @int = ConvertVirtualToPyramidCoord(in tileCoord);
		BurstAssert.IsTrue(mipLevel == @int.z);
		int4 int2 = 0;
		int2.xy = @int.xy << num;
		if (targetMipLevel > 0)
		{
			int2.x += TextureSizeInTiles.x;
			int2.y += TextureSizeInTiles.y >> targetMipLevel;
		}
		int2.xy += RectInTiles.xy;
		int2.zw = 1 << num;
		return int2;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal int3 ConvertVirtualToPyramidCoord(in int2 virtualTileCoord)
	{
		int2 xy = virtualTileCoord - RectInTiles.xy;
		int3 result = new int3(xy, 0);
		int2 textureSizeInTiles = TextureSizeInTiles;
		if (xy.x < textureSizeInTiles.x)
		{
			return result;
		}
		result.x -= textureSizeInTiles.x;
		result.z = 1;
		textureSizeInTiles >>= 1;
		while (xy.y < textureSizeInTiles.y)
		{
			result.z++;
			textureSizeInTiles >>= 1;
		}
		result.y -= textureSizeInTiles.y;
		return result;
	}

	public void SetMipBiasPow2(float newMipBias)
	{
		uint num = (uint)(MipCount - 1);
		newMipBias = math.clamp(newMipBias, 0L - (long)num, num);
		MipBias = math.pow(2f, newMipBias);
	}
}
