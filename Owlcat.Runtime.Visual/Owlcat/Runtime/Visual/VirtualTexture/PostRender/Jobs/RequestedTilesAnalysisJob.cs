using Owlcat.Runtime.Core.Collections;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Visual.VirtualTexture.Atlas;
using Owlcat.Runtime.Visual.VirtualTexture.Feedback;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.VirtualTexture.PostRender.Jobs;

[BurstCompile]
public struct RequestedTilesAnalysisJob : IJob
{
	internal int MaxMip;

	internal int2 VirtualAtlasResolutionInTiles;

	internal PhysicalAtlasResolution PhysicalAtlasResolution;

	internal int FrameId;

	[ReadOnly]
	public NativeList<int2> RequestedTiles;

	[ReadOnly]
	public NativeList<VirtualAtlasEntry> Entries;

	internal NativeArray<Page> Pages;

	public LruLayerCache LruCache;

	internal AtomicCounter SeenResidentPagesCounter;

	[WriteOnly]
	internal NativeList<PageLoadInfo> LoadRequests;

	public void Execute()
	{
		for (int i = 0; i < RequestedTiles.Length; i++)
		{
			int2 @int = RequestedTiles[i];
			if (@int.x < 0 || @int.y < 0 || @int.x >= VirtualAtlasResolutionInTiles.x || @int.y >= VirtualAtlasResolutionInTiles.y)
			{
				continue;
			}
			int index = @int.y * VirtualAtlasResolutionInTiles.x + @int.x;
			ref Page reference = ref UnsafeCollectionExtensions.ElementAsRef(in Pages, index);
			if (reference.TextureId < 0)
			{
				continue;
			}
			VirtualAtlasEntry virtualAtlasEntry = Entries[reference.TextureId];
			if (!reference.IsReady && !reference.IsLoading)
			{
				reference.IsLoading = true;
				ref NativeList<PageLoadInfo> loadRequests = ref LoadRequests;
				ref int x = ref @int.x;
				ref int y = ref @int.y;
				int mipLevel = reference.MipLevel;
				PageLoadInfo value = new PageLoadInfo(in x, in y, in mipLevel);
				loadRequests.Add(in value);
			}
			if (reference.IsReady)
			{
				SeenResidentPagesCounter.Increment();
				if (reference.FrameId != FrameId)
				{
					reference.FrameId = FrameId;
					int id = LruLayerCacheUtility.PhysicalTileCoordToCacheId(reference.PhysicalTileCoord, in PhysicalAtlasResolution);
					LruCache.SetActive(id, virtualAtlasEntry.LayerCount);
				}
			}
		}
	}
}
