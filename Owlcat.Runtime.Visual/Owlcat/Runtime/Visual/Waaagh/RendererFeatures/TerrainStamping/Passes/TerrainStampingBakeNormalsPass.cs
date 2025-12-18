using Owlcat.Runtime.Visual.Waaagh.Passes;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.TerrainStamping.Passes;

public class TerrainStampingBakeNormalsPass : ScriptableRenderPass<TerrainStampingBakeNormalsPassData>
{
	private static class ShaderIDs
	{
		public static readonly int _Source = Shader.PropertyToID("_Source");

		public static readonly int _SourceIndex = Shader.PropertyToID("_SourceIndex");

		public static readonly int _NormalsStrength = Shader.PropertyToID("_NormalsStrength");

		public static readonly int _NormalsStrengthDepthInfluence = Shader.PropertyToID("_NormalsStrengthDepthInfluence");

		public static readonly int _BoxMinMax = Shader.PropertyToID("_BoxMinMax");

		public static readonly int _Noise = Shader.PropertyToID("_Noise");

		public static readonly int _Noise_ST = Shader.PropertyToID("_Noise_ST");

		public static readonly int _NoisePower = Shader.PropertyToID("_NoisePower");
	}

	private readonly TerrainStampingManagerParameters m_Parameters;

	private readonly MaterialPropertyBlock m_PropertyBlock;

	public override string Name => "TerrainStampingBakeNormalsPass";

	public TerrainStampingBakeNormalsPass(TerrainStampingManagerParameters parameters, RenderPassEvent evt)
		: base(evt)
	{
		m_Parameters = parameters;
		m_PropertyBlock = new MaterialPropertyBlock();
	}

	protected override void Setup(RenderGraphBuilder builder, TerrainStampingBakeNormalsPassData data, ContextContainer frameData)
	{
		TerrainStampingManager.TryGetInstance(out var terrainStampingManager);
		data.Material = terrainStampingManager.BakeNormalsMaterial;
		data.NormalsStrength = m_Parameters.NormalsStrength;
		data.NormalsStrengthDepthInfluence = m_Parameters.NormalsStrengthDepthInfluence;
		data.ActiveChunks = terrainStampingManager.GetActiveChunks(Allocator.Temp);
		data.ChunkMinIndex = terrainStampingManager.ChunksMinIndex;
		data.ChunkMaxIndex = terrainStampingManager.ChunksMaxIndex;
		data.StampingTexturePool = terrainStampingManager.StampingTexturePool;
		data.BakedNormalsPool = terrainStampingManager.BakedNormalsPool;
		data.Noise = ((m_Parameters.NormalsNoise != null) ? m_Parameters.NormalsNoise : Texture2D.blackTexture);
		data.NoisePower = m_Parameters.NoisePower;
		data.NoiseTilingOffset = new float4(m_Parameters.NormalsTiling, 0f, 0f);
	}

	protected override void Render(TerrainStampingBakeNormalsPassData data, RenderGraphContext context)
	{
		foreach (TerrainStampingManager.ChunkData activeChunk in data.ActiveChunks)
		{
			int2 index = activeChunk.Index;
			if (math.all(data.ChunkMinIndex <= index) && math.all(index < data.ChunkMaxIndex))
			{
				float4 @float = default(float4);
				@float.xy = (float2)index * m_Parameters.ChunkSize;
				@float.zw = @float.xy + m_Parameters.ChunkSize;
				int index2 = activeChunk.PoolAllocation.Index;
				RenderTargetIdentifier destination = new RenderTargetIdentifier(data.BakedNormalsPool, 0, CubemapFace.Unknown, index2);
				Texture stampingTexturePool = data.StampingTexturePool;
				BakeNormals(data, context, destination, stampingTexturePool, index2, @float);
			}
		}
	}

	private void BakeNormals(TerrainStampingBakeNormalsPassData data, RenderGraphContext context, RenderTargetIdentifier destination, Texture source, int sourceIndex, Vector4 boxMinMax)
	{
		context.cmd.SetRenderTarget(destination);
		m_PropertyBlock.SetTexture(ShaderIDs._Source, source);
		m_PropertyBlock.SetFloat(ShaderIDs._SourceIndex, sourceIndex);
		m_PropertyBlock.SetFloat(ShaderIDs._NormalsStrength, data.NormalsStrength);
		m_PropertyBlock.SetFloat(ShaderIDs._NormalsStrengthDepthInfluence, data.NormalsStrengthDepthInfluence);
		m_PropertyBlock.SetVector(ShaderIDs._BoxMinMax, boxMinMax);
		m_PropertyBlock.SetTexture(ShaderIDs._Noise, data.Noise);
		m_PropertyBlock.SetVector(ShaderIDs._Noise_ST, data.NoiseTilingOffset);
		m_PropertyBlock.SetFloat(ShaderIDs._NoisePower, data.NoisePower);
		Material material = data.Material;
		context.cmd.DrawProcedural(Matrix4x4.identity, material, 0, MeshTopology.Quads, 4, 1, m_PropertyBlock);
	}
}
