using System.Runtime.CompilerServices;

namespace Owlcat.Runtime.Visual.VirtualTerrain.Streaming;

internal sealed class DatabaseLayout
{
	private readonly int m_DataOffset;

	private readonly int m_LayerDataSize;

	private readonly int[,] m_LayerDataOffsets;

	public readonly int TextureSize;

	public readonly TerrainLayerGuid[] TerrainLayerGuids;

	public DatabaseLayout(int textureSize, TerrainLayerGuid[] terrainLayerGuids, int dataOffset)
	{
		TextureSize = textureSize;
		TerrainLayerGuids = terrainLayerGuids;
		m_DataOffset = dataOffset;
		m_LayerDataSize = 0;
		m_LayerDataOffsets = new int[3, 3];
		for (int i = 0; i < 3; i++)
		{
			int pageSize = GetPageSize(i);
			int num = pageSize * pageSize;
			for (int j = 0; j < 3; j++)
			{
				m_LayerDataOffsets[i, j] = m_LayerDataSize;
				m_LayerDataSize += 3 * num;
				num /= 4;
			}
		}
	}

	public int GetLayerCount()
	{
		return TerrainLayerGuids.Length;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetPageSize(int pageLod)
	{
		return GetPageSizeWithoutPadding(pageLod) + 32;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetPageSizeWithoutPadding(int pageLevel)
	{
		int num = pageLevel * 2;
		return TextureSize >> num;
	}

	public long GetAddress(int layerIdx, int pageLod, int pageMip, int pageTypeIdx)
	{
		int num = m_LayerDataOffsets[pageLod, pageMip];
		int num2 = GetPageSize(pageLod) >> pageMip;
		int num3 = num2 * num2;
		return m_DataOffset + (long)layerIdx * (long)m_LayerDataSize + num + pageTypeIdx * num3;
	}
}
