using Owlcat.Runtime.Visual.VirtualTexture.Atlas;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.VirtualTexture.IndirectionTexture.Jobs;

[BurstCompile]
public struct PageDrawDataBuildJob : IJob
{
	public int2 VirtualAtlasResolutionInTiles;

	public float2 InvPhysicalAtlasSliceResolutionInTiles;

	public int FrameId;

	[ReadOnly]
	public NativeList<VirtualAtlasEntry> Entries;

	[WriteOnly]
	internal NativeList<PageDrawData> DrawData;

	[ReadOnly]
	internal NativeParallelHashMap<int3, int2>.Enumerator PageEnumerator;

	[ReadOnly]
	public NativeArray<Page> Pages;

	public void Execute()
	{
		PageDrawData value = default(PageDrawData);
		while (PageEnumerator.MoveNext())
		{
			int2 tileCoord = PageEnumerator.Current.Value;
			Page page = Pages[tileCoord.y * VirtualAtlasResolutionInTiles.x + tileCoord.x];
			int textureId = page.TextureId;
			if (textureId < 0)
			{
				continue;
			}
			VirtualAtlasEntry virtualAtlasEntry = Entries[textureId];
			if (page.IsReady)
			{
				for (int i = 0; i <= page.MipLevel; i++)
				{
					value.MipLevel = page.MipLevel;
					value.Rect = virtualAtlasEntry.CalculateRect(in tileCoord, page.MipLevel, i);
					value.DrawPos = math.float2(page.PhysicalTileCoord.xy) * InvPhysicalAtlasSliceResolutionInTiles;
					value.SliceIndex = page.PhysicalTileCoord.z;
					DrawData.Add(in value);
				}
			}
		}
	}
}
