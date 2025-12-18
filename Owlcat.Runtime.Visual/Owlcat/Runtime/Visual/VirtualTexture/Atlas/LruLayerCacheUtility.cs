using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.VirtualTexture.Atlas;

[BurstCompile]
internal static class LruLayerCacheUtility
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int PhysicalTileCoordToCacheId(int3 physicalTileCoord, in PhysicalAtlasResolution physicalAtlasResolution)
	{
		return math.dot(physicalTileCoord, math.int3(1, physicalAtlasResolution.TilesInSlice.x, physicalAtlasResolution.TilesInSlice.x * physicalAtlasResolution.TilesInSlice.y));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int3 CacheIdToPhysicalTileCoord(int cacheId, in PhysicalAtlasResolution physicalAtlasResolution)
	{
		int num = physicalAtlasResolution.TilesInSlice.x * physicalAtlasResolution.TilesInSlice.y;
		int z = cacheId / num;
		int num2 = cacheId % num;
		return new int3(num2 % physicalAtlasResolution.TilesInSlice.x, num2 / physicalAtlasResolution.TilesInSlice.x, z);
	}
}
