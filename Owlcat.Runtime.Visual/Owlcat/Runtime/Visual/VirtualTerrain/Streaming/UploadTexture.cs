using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Owlcat.Runtime.Visual.VirtualTerrain.Streaming;

internal sealed class UploadTexture : IDisposable
{
	private readonly UploadTextureLayout m_Layout;

	private Texture2D m_Texture;

	private int m_PageLod = -1;

	public int PageLod => m_PageLod;

	public Texture2D Texture => m_Texture;

	public UploadTexture(UploadTextureLayout layout)
	{
		m_Layout = layout;
	}

	public void Dispose()
	{
		if (m_Texture != null)
		{
			UnityEngine.Object.DestroyImmediate(m_Texture);
		}
	}

	public void Setup(int layerLod)
	{
		if (m_PageLod != layerLod)
		{
			m_PageLod = layerLod;
			if (m_Texture != null)
			{
				UnityEngine.Object.DestroyImmediate(m_Texture);
			}
			int2 uploadTextureSize = m_Layout.GetUploadTextureSize(layerLod);
			m_Texture = new Texture2D(uploadTextureSize.x, uploadTextureSize.y, GraphicsFormat.RGBA_DXT5_UNorm, 3, TextureCreationFlags.DontInitializePixels | TextureCreationFlags.DontUploadUponCreate);
			m_Texture.name = "TerrainStreamingUpload";
			m_Texture.hideFlags |= HideFlags.DontSave;
		}
	}

	public NativeArray<ulong> GetBuffer()
	{
		return m_Texture.GetRawTextureData<ulong>();
	}

	public long GetBufferOffsetForMipLevel(int mipLevel)
	{
		return (mipLevel > 0) ? GraphicsFormatUtility.ComputeMipChainSize(m_Texture.width, m_Texture.height, m_Texture.graphicsFormat, mipLevel) : 0;
	}
}
