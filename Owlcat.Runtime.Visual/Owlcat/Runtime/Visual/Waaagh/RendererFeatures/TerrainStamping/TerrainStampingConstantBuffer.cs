using Unity.Mathematics;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.TerrainStamping;

[GenerateHLSL(PackingRules.Exact, false, false, false, 1, false, false, true, -1, ".\\Library\\PackageCache\\com.owlcat.visual@f3d4bf622f68\\Runtime\\Waaagh\\RendererFeatures\\TerrainStamping\\TerrainStampingConstantBuffer.cs")]
public struct TerrainStampingConstantBuffer
{
	public const int kChunkAllocationsCapacity = 256;

	[HLSLArray(64, typeof(uint4))]
	public unsafe fixed uint _TerrainStamping_ChunkAllocations[256];

	public float4 _TerrainStamping_ChunkMinIndex_Count;

	public float4 _TerrainStamping_ChunkSize;

	public float4 _TerrainStamping_StampingPaddingScaleOffset;

	public float4 _TerrainStamping_NormalsPaddingScaleOffset;

	public float _TerrainStamping_Occlusion;

	public float _TerrainStamping_NormalsBlendFactor;

	public float _TerrainStamping_DecalEdgeFade;

	public float _TerrainStamping_DecalEdgeFade_TerrainEdgeWidth;

	public uint _TerrainStamping_TerrainLayerMask;
}
