using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.VirtualTerrain.Streaming;

internal sealed class UploadTextureLayout
{
	private readonly int m_TextureSize;

	public UploadTextureLayout(int textureSize)
	{
		m_TextureSize = textureSize;
	}

	public int GetLayerCapacity(int layerLod)
	{
		return layerLod switch
		{
			0 => 1, 
			1 => 4, 
			2 => 16, 
			_ => 0, 
		};
	}

	public int GetSlotCapacity(int layerLod)
	{
		return GetLayerCapacity(layerLod) * 3;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetSlotSize(int layerLod)
	{
		int num = layerLod * 2;
		return (m_TextureSize >> num) + 32;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int2 GetSlotPosition(int layerLod, int slotId)
	{
		return new int2(0, GetSlotSize(layerLod) * slotId);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public long GetSlotSizeInBytes(int layerLod)
	{
		int slotSize = GetSlotSize(layerLod);
		return slotSize * slotSize;
	}

	public int2 GetUploadTextureSize(int layerLod)
	{
		int slotSize = GetSlotSize(layerLod);
		int slotCapacity = GetSlotCapacity(layerLod);
		return new int2(slotSize, slotSize * slotCapacity);
	}
}
