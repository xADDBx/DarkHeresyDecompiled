using System;
using Unity.Burst;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.VirtualTexture.Atlas;

[BurstCompile]
public struct Page
{
	private static readonly int3 s_InvalidTileIndex = new int3(-1, -1, -1);

	public byte MipLevel;

	public int FrameId;

	public bool IsLoading;

	public int3 PhysicalTileCoord;

	public int TextureId;

	public bool IsReady => !PhysicalTileCoord.Equals(s_InvalidTileIndex);

	public override bool Equals(object obj)
	{
		if (obj is Page page && FrameId == page.FrameId && IsLoading == page.IsLoading)
		{
			return PhysicalTileCoord.Equals(page.PhysicalTileCoord);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(FrameId, IsLoading, PhysicalTileCoord);
	}

	public void ResetTileIndex()
	{
		PhysicalTileCoord = s_InvalidTileIndex;
	}
}
