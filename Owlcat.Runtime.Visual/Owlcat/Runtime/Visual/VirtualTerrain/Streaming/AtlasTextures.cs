using System;
using Owlcat.Runtime.Visual.Utilities;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Owlcat.Runtime.Visual.VirtualTerrain.Streaming;

internal sealed class AtlasTextures : IDisposable
{
	public readonly Texture2D DiffuseTexture;

	public readonly Texture2D NormalTexture;

	public readonly Texture2D MaskTexture;

	public AtlasTextures(AtlasTextureLayout layout)
	{
		AtlasTextureLayout.LodLayout lodLayout = layout.LodLayouts[2];
		int fallbackRegionSizeInBlocks = lodLayout.SlotSize / 4;
		DiffuseTexture = CreateTexture("TerrainAtlasDiffuse", GraphicsFormat.RGBA_DXT5_SRGB, Dxt5Block.Gray);
		NormalTexture = CreateTexture("TerrainAtlasNormal", GraphicsFormat.RGBA_DXT5_UNorm, Dxt5Block.Normal);
		MaskTexture = CreateTexture("TerrainAtlasMask", GraphicsFormat.RGBA_DXT5_UNorm, Dxt5Block.Red);
		Texture2D CreateTexture(string name, GraphicsFormat graphicsFormat, Dxt5Block fillBlock)
		{
			Texture2D texture2D = new Texture2D(layout.AtlasTextureSize.x, layout.AtlasTextureSize.y, graphicsFormat, layout.AtlasTextureMipCount, TextureCreationFlags.DontUploadUponCreate)
			{
				name = name
			};
			texture2D.hideFlags |= HideFlags.DontSave;
			for (int i = 0; i < layout.AtlasTextureMipCount; i++)
			{
				NativeArray<Dxt5Block> source = texture2D.GetPixelData<Dxt5Block>(i);
				Dxt5TextureUtility.FillBlocks(fillBlock, source, (layout.AtlasTextureSize >> i) / 4, 0, fallbackRegionSizeInBlocks >> i);
			}
			texture2D.Apply(updateMipmaps: false, makeNoLongerReadable: true);
			return texture2D;
		}
	}

	public void Dispose()
	{
		UnityEngine.Object.DestroyImmediate(DiffuseTexture);
		UnityEngine.Object.DestroyImmediate(NormalTexture);
		UnityEngine.Object.DestroyImmediate(MaskTexture);
	}
}
