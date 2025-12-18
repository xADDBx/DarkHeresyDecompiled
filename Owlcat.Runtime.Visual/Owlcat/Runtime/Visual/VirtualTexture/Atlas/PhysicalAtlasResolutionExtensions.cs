using System.Runtime.CompilerServices;

namespace Owlcat.Runtime.Visual.VirtualTexture.Atlas;

public static class PhysicalAtlasResolutionExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int TotalTiles(this in PhysicalAtlasResolution resolution)
	{
		return resolution.TilesInSlice.x * resolution.TilesInSlice.y * resolution.ArraySlices;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int TotalPixels(this in PhysicalAtlasResolution resolution)
	{
		return resolution.PixelsInSlice.x * resolution.PixelsInSlice.y * resolution.ArraySlices;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int TotalSizeInBytes(this in PhysicalAtlasResolution resolution)
	{
		return resolution.TotalTiles() * 25920;
	}
}
