using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.TerrainStamping;

internal static class TerrainStampingUtils
{
	public static void ComputeMinMaxChunkRange(float2 boundsMin, float2 boundsMax, float chunkSize, out int2 chunksMin, out int2 chunksMax)
	{
		chunksMin = (int2)math.floor(boundsMin / chunkSize);
		chunksMax = (int2)math.ceil(boundsMax / chunkSize);
	}

	public static float4 ComputePaddingScaleOffset(TerrainStampingManagerParameters.TextureResolution textureResolution, float padding)
	{
		float num = padding / (float)textureResolution;
		float2 xy = 1f - 2f * num;
		float2 zw = num;
		return math.float4(xy, zw);
	}
}
