using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.VirtualTexture.Diagnostics;

public struct PaddingLoadRequestEvent
{
	public int FrameId;

	public byte Source;

	public int2 TileCoords;

	public int EntryIndex;

	public int4 RectInTiles;

	public int2 TextureSizeInTiles;

	public int EntryMipCount;

	public byte PageMipLevel;

	public int3 PyramidCoord;

	public int2 MipDimAtZ;
}
