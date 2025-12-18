namespace Owlcat.Runtime.Visual.VirtualTexture;

public struct VirtualTexturePhysicalAtlasOverrides
{
	public SliceResolution MaxSliceResolution;

	public static VirtualTexturePhysicalAtlasOverrides Default
	{
		get
		{
			VirtualTexturePhysicalAtlasOverrides result = default(VirtualTexturePhysicalAtlasOverrides);
			result.MaxSliceResolution = SliceResolution.MaximumAvailable;
			return result;
		}
	}
}
