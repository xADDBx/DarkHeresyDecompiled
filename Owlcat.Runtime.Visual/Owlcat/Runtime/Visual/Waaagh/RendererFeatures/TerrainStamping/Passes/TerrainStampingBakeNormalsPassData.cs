using Owlcat.Runtime.Visual.Waaagh.Passes;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.TerrainStamping.Passes;

public class TerrainStampingBakeNormalsPassData : PassDataBase
{
	public NativeArray<TerrainStampingManager.ChunkData> ActiveChunks;

	public RenderTexture BakedNormalsPool;

	public int2 ChunkMaxIndex;

	public int2 ChunkMinIndex;

	public Material Material;

	public Texture2D Noise;

	public float NoisePower;

	public Vector4 NoiseTilingOffset;

	public float NormalsStrength;

	public float NormalsStrengthDepthInfluence;

	public RenderTexture StampingTexturePool;
}
