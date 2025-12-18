using System.Runtime.CompilerServices;
using Owlcat.Runtime.Visual.VirtualTexture.Atlas;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.VirtualTexture.PostRender.Jobs;

[BurstCompile]
public struct CollectResidentTilesJob : IJobParallelFor
{
	public bool LoadSmallestMips;

	public int2 VirtualAtlasResolutionInTiles;

	[ReadOnly]
	public NativeArray<VirtualAtlasEntry> Entries;

	public NativeList<int2>.ParallelWriter RequestedTiles;

	public void Execute(int index)
	{
		VirtualAtlasEntry entry = Entries[index];
		if (LoadSmallestMips)
		{
			int mipLevel = entry.MipCount - 1;
			CollectTilesForMipLevel(in entry, mipLevel);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void CollectTilesForMipLevel(in VirtualAtlasEntry entry, int mipLevel)
	{
		int2 @int = entry.TextureSizeInTiles >> mipLevel;
		for (int i = 0; i < @int.y; i++)
		{
			for (int j = 0; j < @int.x; j++)
			{
				int2 value = new int2(j, i);
				if (mipLevel > 0)
				{
					value.x += entry.TextureSizeInTiles.x;
					value.y += @int.y;
				}
				value += entry.RectInTiles.xy;
				RequestedTiles.AddNoResize(value);
			}
		}
	}
}
