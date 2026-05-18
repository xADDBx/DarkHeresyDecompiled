using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.TerrainStamping.Passes;

internal static class SetupForRenderPass
{
	private static class Profiling
	{
		public static readonly ProfilingSampler TerrainStampingSetup = new ProfilingSampler("TerrainStampingSetup");
	}

	private sealed class PassData
	{
		public TerrainStampingConstantBuffer ConstantBuffer;

		public Texture Normals;

		public Texture Texture;
	}

	private static class ShaderIDs
	{
		public static readonly int _TerrainStampingTexturePool = Shader.PropertyToID("_TerrainStampingTexturePool");

		public static readonly int _TerrainStampingNormalsPool = Shader.PropertyToID("_TerrainStampingNormalsPool");

		public static readonly int ConstantBuffer = Shader.PropertyToID("TerrainStampingConstantBuffer");
	}

	public static void Record(in RecordContext context, TerrainStampingManager terrainStampingManager, TerrainStampingManagerParameters parameters)
	{
		PassData passData2;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<PassData>("Terrain Stamping Setup", out passData2, Profiling.TerrainStampingSetup, ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\RendererFeatures\\TerrainStamping\\Passes\\SetupForRenderPass.cs", 30);
		passData2.Texture = terrainStampingManager.StampingTexturePool;
		passData2.Normals = terrainStampingManager.BakedNormalsPool;
		passData2.ConstantBuffer = default(TerrainStampingConstantBuffer);
		passData2.ConstantBuffer._TerrainStamping_Occlusion = parameters.Occlusion;
		passData2.ConstantBuffer._TerrainStamping_NormalsBlendFactor = parameters.NormalsBlendFactor;
		passData2.ConstantBuffer._TerrainStamping_DecalEdgeFade = parameters.DecalEdgeFade;
		passData2.ConstantBuffer._TerrainStamping_DecalEdgeFade_TerrainEdgeWidth = parameters.DecalTerrainEdgeWidth;
		passData2.ConstantBuffer._TerrainStamping_TerrainLayerMask = parameters.TerrainLayerMask;
		passData2.ConstantBuffer._TerrainStamping_ChunkSize = math.float4(parameters.ChunkSize, 1f / parameters.ChunkSize, 0f, 0f);
		passData2.ConstantBuffer._TerrainStamping_ChunkMinIndex_Count = math.int4(terrainStampingManager.ChunksMinIndex, terrainStampingManager.ChunksMaxIndex - terrainStampingManager.ChunksMinIndex);
		passData2.ConstantBuffer._TerrainStamping_StampingPaddingScaleOffset = TerrainStampingUtils.ComputePaddingScaleOffset(parameters.Resolution, parameters.Padding);
		passData2.ConstantBuffer._TerrainStamping_NormalsPaddingScaleOffset = TerrainStampingUtils.ComputePaddingScaleOffset(parameters.BakedNormalsResolution, parameters.Padding);
		PackChunkAllocations(ref passData2.ConstantBuffer, terrainStampingManager);
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		unsafeRenderGraphBuilder.AllowGlobalStateModification(value: true);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(PassData passData, UnsafeGraphContext context)
		{
			ConstantBuffer.PushGlobal(context.cmd, in passData.ConstantBuffer, ShaderIDs.ConstantBuffer);
			context.cmd.SetGlobalTexture(ShaderIDs._TerrainStampingTexturePool, passData.Texture);
			context.cmd.SetGlobalTexture(ShaderIDs._TerrainStampingNormalsPool, passData.Normals);
			CoreUtils.SetKeyword(context.cmd, "_TERRAIN_STAMPING", state: true);
		});
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
}
