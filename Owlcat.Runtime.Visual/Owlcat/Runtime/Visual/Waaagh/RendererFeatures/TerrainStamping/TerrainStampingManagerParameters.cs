using System;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.TerrainStamping;

[Serializable]
public class TerrainStampingManagerParameters
{
	public enum TextureResolution
	{
		_32 = 0x20,
		_64 = 0x40,
		_128 = 0x80,
		_256 = 0x100,
		_512 = 0x200,
		_1024 = 0x400
	}

	[Header("Common")]
	public RenderingLayerMask TerrainLayerMask = RenderingLayerMask.defaultRenderingLayerMask;

	public CustomDecalSubset DecalSubset;

	public bool TransitionBlendDithering = true;

	[Header("Fade Out")]
	[Min(0f)]
	public float FadeOutDuration = 1f;

	[Min(0f)]
	public float OutOfBoundsFadeOutDuration = 1f;

	[Header("Chunk Pool")]
	[Min(0f)]
	public float ChunkSize = 5f;

	[Min(0f)]
	public float ChunkAllocationMaxDistance = 50f;

	[Min(1f)]
	public int ChunkTexturesCapacity = 16;

	[Range(1f, 32f)]
	public int VisibleRegionMaxLengthInChunks = 16;

	[Range(0f, 5f)]
	public float Padding = 1f;

	[Header("Resolution")]
	[Min(16f)]
	public TextureResolution Resolution = TextureResolution._256;

	public TextureResolution BakedNormalsResolution = TextureResolution._256;

	[Header("Normals")]
	[Range(0f, 50f)]
	public float NormalsStrength = 2f;

	[Range(0f, 1f)]
	public float NormalsStrengthDepthInfluence = 0.5f;

	[Header("Normals Noise")]
	public Texture2D NormalsNoise;

	public float2 NormalsTiling;

	[Range(0f, 1f)]
	public float NoisePower;

	[Header("Decals")]
	[Range(0f, 10f)]
	public float NormalsBlendFactor = 1f;

	[Range(0f, 1f)]
	public float DecalEdgeFade = 0.1f;

	[Range(0f, 1f)]
	public float DecalTerrainEdgeWidth = 0.1f;

	[Header("Lighting")]
	[Range(0f, 1f)]
	public float Occlusion = 0.5f;

	[Header("Shaders")]
	public Shader BrushShader;

	public Shader BakeNormalsShader;

	public Shader StencilMaskShader;

	[Header("Debug")]
	public bool DrawGizmos;
}
