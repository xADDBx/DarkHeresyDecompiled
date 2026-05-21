namespace Owlcat.Runtime.Visual.VirtualTexture.Diagnostics;

public struct PaddingLoadRequestEventRecord
{
	public int FrameId;

	public byte Source;

	public int VirtualX;

	public int VirtualY;

	public int EntryIndex;

	public int RectX;

	public int RectY;

	public int RectW;

	public int RectH;

	public int TextureSizeInTilesX;

	public int TextureSizeInTilesY;

	public int EntryMipCount;

	public int PageMipLevel;

	public int PyramidX;

	public int PyramidY;

	public int PyramidZ;

	public int MipDimAtZX;

	public int MipDimAtZY;
}
