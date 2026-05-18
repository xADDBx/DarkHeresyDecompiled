using System;
using UnityEngine;

namespace Owlcat.Runtime.Visual;

public class DebugMipMapTexture : IDisposable
{
	private readonly Color[] m_DebugColors = new Color[6]
	{
		new Color(0f, 0f, 1f, 0.8f),
		new Color(0f, 0.5f, 1f, 0.4f),
		new Color(1f, 1f, 1f, 0f),
		new Color(1f, 0.7f, 0f, 0.2f),
		new Color(1f, 0.3f, 0f, 0.6f),
		new Color(1f, 0f, 0f, 0.8f)
	};

	private Texture2D m_MipMapTexture;

	public Texture2D MipMapTexture
	{
		get
		{
			if (m_MipMapTexture == null)
			{
				CreateTexture();
			}
			return m_MipMapTexture;
		}
	}

	public DebugMipMapTexture()
	{
		CreateTexture();
	}

	private void CreateTexture()
	{
		int num = 32;
		int num2 = 0;
		m_MipMapTexture = new Texture2D(num, num, TextureFormat.RGBA32, mipChain: true);
		m_MipMapTexture.name = "_MipMapDebugMap";
		while (num >= 1)
		{
			Color[] array = new Color[num * num];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = m_DebugColors[num2];
			}
			m_MipMapTexture.SetPixels(array, num2);
			num2++;
			num /= 2;
		}
		m_MipMapTexture.filterMode = FilterMode.Trilinear;
		m_MipMapTexture.Apply(updateMipmaps: false);
	}

	public void Dispose()
	{
		if (m_MipMapTexture != null)
		{
			UnityEngine.Object.DestroyImmediate(m_MipMapTexture);
			m_MipMapTexture = null;
		}
	}
}
