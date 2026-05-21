using System;

namespace Owlcat.Runtime.Visual.VirtualTexture.Diagnostics;

public struct EntryRecord
{
	public int IndexInAllocator;

	public string NodeKind;

	public Guid Layer0;

	public Guid Layer1;

	public Guid Layer2;

	public Guid Layer3;

	public int RectX;

	public int RectY;

	public int RectW;

	public int RectH;

	public int TextureSizeInTilesX;

	public int TextureSizeInTilesY;

	public int MipCount;

	public float MipBias;

	public int LayerCount;

	public uint PackedLayerFlags;

	public uint RectAllocId;
}
