using System;
using Owlcat.Runtime.Core.Collections;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.VirtualTexture.Atlas.Jobs;

[BurstCompile]
internal struct InvalidatePagesJob : IJob
{
	public int PrevPagesCount;

	public int AtlasWidth;

	public NativeArray<Page> Pages;

	public NativeList<EntryToAllocate> DataToAllocate;

	public void Execute()
	{
		for (int i = PrevPagesCount; i < Pages.Length; i++)
		{
			ref Page reference = ref UnsafeCollectionExtensions.ElementAsRef(in Pages, i);
			reference.TextureId = -1;
			reference.FrameId = -1;
			reference.PhysicalTileCoord = -1;
			reference.IsLoading = false;
			reference.MipLevel = 0;
		}
		Span<EntryToAllocate> span = DataToAllocate.AsArray().AsSpan();
		for (int j = 0; j < span.Length; j++)
		{
			ref EntryToAllocate reference2 = ref span[j];
			int indexInAllocator = reference2.IndexInAllocator;
			int4 rectInTiles = reference2.Entry.RectInTiles;
			for (int k = 0; k < rectInTiles.w; k++)
			{
				for (int l = 0; l < rectInTiles.z; l++)
				{
					int num = rectInTiles.x + l;
					int num2 = rectInTiles.y + k;
					int index = num2 * AtlasWidth + num;
					ref Page reference3 = ref UnsafeCollectionExtensions.ElementAsRef(in Pages, index);
					reference3.TextureId = indexInAllocator;
					ref VirtualAtlasEntry entry = ref reference2.Entry;
					int2 virtualTileCoord = new int2(num, num2);
					reference3.MipLevel = (byte)entry.CalculateMipLevel(in virtualTileCoord);
					reference3.ResetTileIndex();
				}
			}
		}
	}
}
