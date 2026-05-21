using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.TerrainStamping.Passes;

internal static class BakeNormalsPass
{
	private static class Profiling
	{
		public static readonly ProfilingSampler TerrainStampingBakeNormals = new ProfilingSampler("TerrainStampingBakeNormals");
	}

	private sealed class PassData
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

		public float ChunkSize;

		public MaterialPropertyBlock PropertyBlock;
	}

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

	public static void Record(in RecordContext context, TerrainStampingManager terrainStampingManager, TerrainStampingManagerParameters parameters)
	{
		PassData passData2;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<PassData>("Terrain Stamping Bake Normals", out passData2, Profiling.TerrainStampingBakeNormals, ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\RendererFeatures\\TerrainStamping\\Passes\\BakeNormalsPass.cs", 41);
		passData2.Material = terrainStampingManager.BakeNormalsMaterial;
		passData2.NormalsStrength = parameters.NormalsStrength;
		passData2.NormalsStrengthDepthInfluence = parameters.NormalsStrengthDepthInfluence;
		passData2.ActiveChunks = terrainStampingManager.GetActiveChunks(Allocator.Temp);
		passData2.ChunkMinIndex = terrainStampingManager.ChunksMinIndex;
		passData2.ChunkMaxIndex = terrainStampingManager.ChunksMaxIndex;
		passData2.StampingTexturePool = terrainStampingManager.StampingTexturePool;
		passData2.BakedNormalsPool = terrainStampingManager.BakedNormalsPool;
		passData2.Noise = ((parameters.NormalsNoise != null) ? parameters.NormalsNoise : Texture2D.blackTexture);
		passData2.NoisePower = parameters.NoisePower;
		passData2.NoiseTilingOffset = new float4(parameters.NormalsTiling, 0f, 0f);
		passData2.ChunkSize = parameters.ChunkSize;
		PassData passData3 = passData2;
		if (passData3.PropertyBlock == null)
		{
			passData3.PropertyBlock = new MaterialPropertyBlock();
		}
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(PassData passData, UnsafeGraphContext context)
		{
			foreach (TerrainStampingManager.ChunkData activeChunk in passData.ActiveChunks)
			{
				int2 index = activeChunk.Index;
				if (math.all(passData.ChunkMinIndex <= index) && math.all(index < passData.ChunkMaxIndex))
				{
					float4 @float = default(float4);
					@float.xy = (float2)index * passData.ChunkSize;
					@float.zw = @float.xy + passData.ChunkSize;
					int index2 = activeChunk.PoolAllocation.Index;
					RenderTargetIdentifier destination = new RenderTargetIdentifier(passData.BakedNormalsPool, 0, CubemapFace.Unknown, index2);
					Texture stampingTexturePool = passData.StampingTexturePool;
					BakeNormals(passData, context, destination, stampingTexturePool, index2, @float);
				}
			}
		});
	}

	private static void BakeNormals(PassData data, UnsafeGraphContext context, RenderTargetIdentifier destination, Texture source, int sourceIndex, Vector4 boxMinMax)
	{
		context.cmd.SetRenderTarget(destination);
		MaterialPropertyBlock propertyBlock = data.PropertyBlock;
		propertyBlock.SetTexture(ShaderIDs._Source, source);
		propertyBlock.SetFloat(ShaderIDs._SourceIndex, sourceIndex);
		propertyBlock.SetFloat(ShaderIDs._NormalsStrength, data.NormalsStrength);
		propertyBlock.SetFloat(ShaderIDs._NormalsStrengthDepthInfluence, data.NormalsStrengthDepthInfluence);
		propertyBlock.SetVector(ShaderIDs._BoxMinMax, boxMinMax);
		propertyBlock.SetTexture(ShaderIDs._Noise, data.Noise);
		propertyBlock.SetVector(ShaderIDs._Noise_ST, data.NoiseTilingOffset);
		propertyBlock.SetFloat(ShaderIDs._NoisePower, data.NoisePower);
		Material material = data.Material;
		context.cmd.DrawProcedural(Matrix4x4.identity, material, 0, MeshTopology.Quads, 4, 1, propertyBlock);
	}
}
