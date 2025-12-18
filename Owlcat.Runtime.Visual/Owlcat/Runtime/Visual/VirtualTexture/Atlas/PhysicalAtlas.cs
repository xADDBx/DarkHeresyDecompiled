using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.VirtualTexture.Atlas;

public class PhysicalAtlas
{
	private static class MemoryFitter
	{
		public static PhysicalAtlasResolution ComputeResolution(int atlasSizeInMegaBytes, SliceResolution requestedMaxSliceResolution)
		{
			long num = (long)atlasSizeInMegaBytes * 1048576L;
			long num2 = num / 25920;
			PhysicalAtlasResolution resolution = default(PhysicalAtlasResolution);
			resolution.TilesInSlice = (int)math.sqrt(num2) + 1;
			resolution.PixelsInSlice = resolution.TilesInSlice * 144;
			resolution.ArraySlices = 1;
			PhysicalAtlasResolution originalResolution = resolution;
			int num3 = ResolveMaxSliceResolution(requestedMaxSliceResolution);
			if (math.any(resolution.PixelsInSlice > num3))
			{
				CutResolutionInSlices(ref resolution, num3, in originalResolution);
			}
			SqueezeVertically(ref resolution, num);
			return resolution;
		}

		private static int ResolveMaxSliceResolution(SliceResolution requestedMaxSliceResolution)
		{
			int maxTextureSize = SystemInfo.maxTextureSize;
			if (requestedMaxSliceResolution == SliceResolution.MaximumAvailable || (int)requestedMaxSliceResolution > maxTextureSize)
			{
				return maxTextureSize;
			}
			return (int)requestedMaxSliceResolution;
		}

		private static void CutResolutionInSlices(ref PhysicalAtlasResolution resolution, int maxSliceResolution, in PhysicalAtlasResolution originalResolution)
		{
			resolution.TilesInSlice = (int)math.floor((float)maxSliceResolution / 144f);
			resolution.PixelsInSlice = resolution.TilesInSlice * 144;
			float2 @float = (float2)originalResolution.PixelsInSlice / (float2)resolution.PixelsInSlice;
			resolution.ArraySlices = (int)math.ceil(@float.x * @float.y);
			resolution.ArraySlices = math.min(resolution.ArraySlices, SystemInfo.maxTextureArraySlices);
		}

		private static void SqueezeVertically(ref PhysicalAtlasResolution resolution, long memoryBudgetInBytes)
		{
			float num = (float)memoryBudgetInBytes / (float)resolution.TotalSizeInBytes();
			resolution.TilesInSlice.y = (int)math.ceil((float)resolution.TilesInSlice.y * num);
			resolution.PixelsInSlice = resolution.TilesInSlice * 144;
		}
	}

	private Texture2DArray m_AtlasTex;

	private readonly PhysicalAtlasResolution m_Resolution;

	public Texture2DArray AtlasTex => m_AtlasTex;

	public int RequestedSizeInMegaBytes { get; }

	public ref readonly PhysicalAtlasResolution Resolution => ref m_Resolution;

	public PhysicalAtlas(int requestedSizeInMegaBytes, SliceResolution requiredMaxSliceResolution)
	{
		RequestedSizeInMegaBytes = requestedSizeInMegaBytes;
		m_Resolution = MemoryFitter.ComputeResolution(requestedSizeInMegaBytes, requiredMaxSliceResolution);
		CreateAtlas();
	}

	private void CreateAtlas()
	{
		m_AtlasTex = new Texture2DArray(m_Resolution.PixelsInSlice.x, m_Resolution.PixelsInSlice.y, m_Resolution.ArraySlices, TextureFormat.DXT5, 2, linear: true, createUninitialized: true);
		m_AtlasTex.ignoreMipmapLimit = true;
		m_AtlasTex.hideFlags = HideFlags.HideAndDontSave;
		m_AtlasTex.wrapMode = TextureWrapMode.Clamp;
		m_AtlasTex.filterMode = FilterMode.Trilinear;
		m_AtlasTex.anisoLevel = 8;
		m_AtlasTex.name = $"VirtualTextureAtlas[{m_Resolution.ArraySlices}]_{m_AtlasTex.graphicsFormat}";
		m_AtlasTex.Apply(updateMipmaps: false, makeNoLongerReadable: true);
	}

	public void Dispose()
	{
		if (m_AtlasTex != null)
		{
			Object.DestroyImmediate(m_AtlasTex);
			m_AtlasTex = null;
		}
	}
}
