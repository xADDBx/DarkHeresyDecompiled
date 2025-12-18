using System;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.TerrainStamping.Passes;

public class TerrainStampingSetupForRenderPass : ScriptableRenderPass<TerrainStampingSetupForRenderPassData>
{
	private static class ShaderIDs
	{
		public static readonly int _TerrainStampingTexturePool = Shader.PropertyToID("_TerrainStampingTexturePool");

		public static readonly int _TerrainStampingNormalsPool = Shader.PropertyToID("_TerrainStampingNormalsPool");

		public static readonly int ConstantBuffer = Shader.PropertyToID("TerrainStampingConstantBuffer");
	}

	private readonly TerrainStampingManagerParameters m_Parameters;

	public override string Name => "TerrainStampingSetupForRenderPass";

	public TerrainStampingSetupForRenderPass(TerrainStampingManagerParameters parameters, RenderPassEvent evt)
		: base(evt)
	{
		m_Parameters = parameters;
	}

	protected override void Setup(RenderGraphBuilder builder, TerrainStampingSetupForRenderPassData data, ContextContainer frameData)
	{
		TerrainStampingManager.TryGetInstance(out var terrainStampingManager);
		data.Texture = terrainStampingManager.StampingTexturePool;
		data.Normals = terrainStampingManager.BakedNormalsPool;
		ref TerrainStampingConstantBuffer constantBuffer = ref data.ConstantBuffer;
		constantBuffer = default(TerrainStampingConstantBuffer);
		constantBuffer._TerrainStamping_Occlusion = m_Parameters.Occlusion;
		constantBuffer._TerrainStamping_NormalsBlendFactor = m_Parameters.NormalsBlendFactor;
		constantBuffer._TerrainStamping_DecalEdgeFade = m_Parameters.DecalEdgeFade;
		constantBuffer._TerrainStamping_DecalEdgeFade_TerrainEdgeWidth = m_Parameters.DecalTerrainEdgeWidth;
		constantBuffer._TerrainStamping_TerrainLayerMask = m_Parameters.TerrainLayerMask;
		constantBuffer._TerrainStamping_ChunkSize = math.float4(m_Parameters.ChunkSize, 1f / m_Parameters.ChunkSize, 0f, 0f);
		constantBuffer._TerrainStamping_ChunkMinIndex_Count = math.int4(terrainStampingManager.ChunksMinIndex, terrainStampingManager.ChunksMaxIndex - terrainStampingManager.ChunksMinIndex);
		constantBuffer._TerrainStamping_StampingPaddingScaleOffset = TerrainStampingUtils.ComputePaddingScaleOffset(m_Parameters.Resolution, m_Parameters.Padding);
		constantBuffer._TerrainStamping_NormalsPaddingScaleOffset = TerrainStampingUtils.ComputePaddingScaleOffset(m_Parameters.BakedNormalsResolution, m_Parameters.Padding);
		PackChunkAllocations(ref constantBuffer, terrainStampingManager);
		builder.AllowPassCulling(value: false);
	}

	private unsafe static void PackChunkAllocations(ref TerrainStampingConstantBuffer constantBuffer, TerrainStampingManager terrainStampingManager)
	{
		int2 chunksMinIndex = terrainStampingManager.ChunksMinIndex;
		int2 chunksMaxIndex = terrainStampingManager.ChunksMaxIndex;
		fixed (uint* pointer = constantBuffer._TerrainStamping_ChunkAllocations)
		{
			Span<byte> span = new Span<byte>(pointer, 1024);
			int2 indexCount2 = chunksMaxIndex - chunksMinIndex;
			for (int i = 0; i < indexCount2.y; i++)
			{
				for (int j = 0; j < indexCount2.x; j++)
				{
					int2 @int = math.int2(j, i);
					int index = FlattenIndex(@int, indexCount2);
					int2 chunkIndex = @int + chunksMinIndex;
					span[index] = (byte)(terrainStampingManager.TryGetChunkData(chunkIndex, out var chunkData) ? ((uint)(1 + chunkData.PoolAllocation.Index)) : 0u);
				}
			}
		}
		static int FlattenIndex(int2 relativeIndex, int2 indexCount)
		{
			return relativeIndex.y * indexCount.x + relativeIndex.x;
		}
	}

	protected override void Render(TerrainStampingSetupForRenderPassData data, RenderGraphContext context)
	{
		ConstantBuffer.PushGlobal(context.cmd, in data.ConstantBuffer, ShaderIDs.ConstantBuffer);
		context.cmd.SetGlobalTexture(ShaderIDs._TerrainStampingTexturePool, data.Texture);
		context.cmd.SetGlobalTexture(ShaderIDs._TerrainStampingNormalsPool, data.Normals);
		CoreUtils.SetKeyword(context.cmd, "_TERRAIN_STAMPING", state: true);
	}
}
