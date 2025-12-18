using System.Runtime.CompilerServices;
using Owlcat.Runtime.Visual.VirtualTexture.Atlas;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.VirtualTexture.PostRender.Jobs;

[BurstCompile]
public struct CollectFeedbackTilesJob : IJobParallelFor
{
	public bool PreloadSmallestMips;

	public int2 VirtualAtlasResolutionInTiles;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<uint> EncodedFeedback;

	[ReadOnly]
	public NativeArray<VirtualAtlasEntry> Entries;

	public NativeList<int2>.ParallelWriter RequestedTiles;

	public void Execute(int index)
	{
		VirtualAtlasEntry virtualAtlasEntry = Entries[index];
		for (int i = 0; i < virtualAtlasEntry.MipCount; i++)
		{
			if (PreloadSmallestMips && i == virtualAtlasEntry.MipCount - 1)
			{
				continue;
			}
			int2 @int = virtualAtlasEntry.TextureSizeInTiles >> i;
			for (int j = 0; j < @int.y; j++)
			{
				for (int k = 0; k < @int.x; k++)
				{
					int2 int2 = new int2(k, j);
					if (i > 0)
					{
						int2.x += virtualAtlasEntry.TextureSizeInTiles.x;
						int2.y += @int.y;
					}
					int2 += virtualAtlasEntry.RectInTiles.xy;
					if (IsRequestedByFeedback(int2))
					{
						RequestedTiles.AddNoResize(int2);
					}
				}
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool IsRequestedByFeedback(int2 virtualCoords)
	{
		int num = virtualCoords.y * VirtualAtlasResolutionInTiles.x + virtualCoords.x;
		int index = num / 32;
		int num2 = 1 << num % 32;
		return (EncodedFeedback[index] & num2) != 0;
	}
}
