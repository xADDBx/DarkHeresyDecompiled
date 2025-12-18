using System;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.VirtualTexture.Streaming;

public struct TileReadTask
{
	public Guid Guid;

	public int LayerCount;

	public int LayerIndex;

	public int2 VirtualCoord;

	public int3 PyramidCoord;

	public int4 RectInTiles;

	public int MipCount;

	public int FrameId;
}
