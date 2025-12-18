using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.VirtualTerrain.Streaming;

internal sealed class Uploader : IDisposable
{
	private static readonly int VirtualTerrainAtlasDiffuse = Shader.PropertyToID("_VirtualTerrainAtlasDiffuse");

	private static readonly int VirtualTerrainAtlasNormal = Shader.PropertyToID("_VirtualTerrainAtlasNormal");

	private static readonly int VirtualTerrainAtlasMask = Shader.PropertyToID("_VirtualTerrainAtlasMask");

	private static readonly int VirtualTerrainAtlasParams = Shader.PropertyToID("_VirtualTerrainAtlasParams");

	private readonly UploadTextureLayout m_UploadTextureLayout;

	private readonly AtlasTextureLayout m_AtlasTextureLayout;

	private readonly UploadTexture m_UploadTexture;

	private readonly AtlasTextures m_AtlasTextures;

	private readonly CommandBuffer m_CommandBuffer;

	public Uploader(UploadTextureLayout uploadTextureLayout, AtlasTextureLayout atlasTextureLayout, UploadTexture uploadTexture, AtlasTextures atlasTextures)
	{
		m_UploadTextureLayout = uploadTextureLayout;
		m_AtlasTextureLayout = atlasTextureLayout;
		m_UploadTexture = uploadTexture;
		m_AtlasTextures = atlasTextures;
		m_CommandBuffer = new CommandBuffer
		{
			name = "TerrainStreaming"
		};
		Shader.SetGlobalTexture(VirtualTerrainAtlasDiffuse, m_AtlasTextures.DiffuseTexture);
		Shader.SetGlobalTexture(VirtualTerrainAtlasNormal, m_AtlasTextures.NormalTexture);
		Shader.SetGlobalTexture(VirtualTerrainAtlasMask, m_AtlasTextures.MaskTexture);
	}

	public void Dispose()
	{
		m_CommandBuffer.Dispose();
	}

	public void EnqueueUpload(int uploadTextureSlotId, int atlasTextureSlotId, int pageLod)
	{
		int2 slotPosition = m_UploadTextureLayout.GetSlotPosition(pageLod, uploadTextureSlotId);
		int2 slotPosition2 = m_AtlasTextureLayout.GetSlotPosition(pageLod, atlasTextureSlotId);
		int slotSize = m_AtlasTextureLayout.LodLayouts[pageLod].SlotSize;
		for (int i = 0; i < 3; i++)
		{
			int srcX = slotPosition.x >> i;
			int num = slotPosition.y >> i;
			int dstX = slotPosition2.x >> i;
			int dstY = slotPosition2.y >> i;
			int num2 = slotSize >> i;
			m_CommandBuffer.CopyTexture(m_UploadTexture.Texture, 0, i, srcX, num, num2, num2, m_AtlasTextures.DiffuseTexture, 0, i, dstX, dstY);
			m_CommandBuffer.CopyTexture(m_UploadTexture.Texture, 0, i, srcX, num + num2, num2, num2, m_AtlasTextures.NormalTexture, 0, i, dstX, dstY);
			m_CommandBuffer.CopyTexture(m_UploadTexture.Texture, 0, i, srcX, num + 2 * num2, num2, num2, m_AtlasTextures.MaskTexture, 0, i, dstX, dstY);
		}
	}

	public void FlushUpload()
	{
		try
		{
			m_UploadTexture.Texture.Apply(updateMipmaps: false, makeNoLongerReadable: false);
			m_CommandBuffer.SetGlobalTexture(VirtualTerrainAtlasDiffuse, m_AtlasTextures.DiffuseTexture);
			m_CommandBuffer.SetGlobalTexture(VirtualTerrainAtlasNormal, m_AtlasTextures.NormalTexture);
			m_CommandBuffer.SetGlobalTexture(VirtualTerrainAtlasMask, m_AtlasTextures.MaskTexture);
			m_CommandBuffer.SetGlobalVector(VirtualTerrainAtlasParams, new float4(new float2(m_AtlasTextureLayout.LayerTextureSize) / m_AtlasTextureLayout.AtlasTextureSize, default(float2)));
			Graphics.ExecuteCommandBuffer(m_CommandBuffer);
		}
		finally
		{
			m_CommandBuffer.Clear();
		}
	}
}
