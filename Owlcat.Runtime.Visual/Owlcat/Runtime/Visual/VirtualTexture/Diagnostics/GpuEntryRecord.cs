namespace Owlcat.Runtime.Visual.VirtualTexture.Diagnostics;

public struct GpuEntryRecord
{
	public int EntryIndex;

	public int RectX;

	public int RectY;

	public int RectW;

	public int RectH;

	public int MaxMip;

	public float MipBias;

	public int LayerCount;

	public int PackedLayerFlags;

	public int TextureWidthInTiles;
}
