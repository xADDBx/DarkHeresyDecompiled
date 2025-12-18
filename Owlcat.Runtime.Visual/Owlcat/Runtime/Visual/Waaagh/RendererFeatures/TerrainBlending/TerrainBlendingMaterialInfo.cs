namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.TerrainBlending;

internal struct TerrainBlendingMaterialInfo
{
	private const int kInvalidPass = -1;

	public int BlendingDecalPass;

	public int BlendingMaskReducePass;

	public int BlendingMaskPopulatePass;

	public bool Valid;

	public static TerrainBlendingMaterialInfo MakeInvalid()
	{
		TerrainBlendingMaterialInfo result = default(TerrainBlendingMaterialInfo);
		result.BlendingDecalPass = -1;
		result.BlendingMaskReducePass = -1;
		result.BlendingMaskPopulatePass = -1;
		result.Valid = false;
		return result;
	}

	public bool IsValid()
	{
		return Valid;
	}
}
