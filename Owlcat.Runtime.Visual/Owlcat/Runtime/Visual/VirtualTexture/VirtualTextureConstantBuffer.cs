using Unity.Burst;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.VirtualTexture;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@01c4fcbc474f\\Runtime\\VirtualTexture\\VirtualTextureConstantBuffer.cs", needAccessors = false, generateCBuffer = true)]
[BurstCompile]
public struct VirtualTextureConstantBuffer
{
	public const int kTileSizeMip0 = 128;

	public const int kTilePaddingMip0 = 8;

	public const int kTileSizeWithPaddingMip0 = 144;

	public const int kTileSizeWithPaddingMip1 = 72;

	public const int kTileSizeMip1 = 64;

	public const int kTilePaddingMip1 = 4;

	public const int kMaxLayersPerStack = 4;

	public const int kTileSizeInBytesMip0 = 20736;

	public const int kTileSizeInBytesMip1 = 5184;

	public const int kTileSizeInBytes = 25920;

	public const int kTileMipCount = 2;

	public float4 _VTPageParams;

	public float4 _VTTileParams;

	public float4 _VTPhysicalAtlasSize;

	public float _VTFeedbackMipBias;
}
