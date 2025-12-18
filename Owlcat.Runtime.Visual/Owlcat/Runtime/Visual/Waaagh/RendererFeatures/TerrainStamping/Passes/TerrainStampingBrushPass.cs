using System;
using Owlcat.Runtime.Core.Collections;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.TerrainStamping.Passes;

public class TerrainStampingBrushPass : ScriptableRenderPass<TerrainStampingBrushPassData>
{
	[BurstCompile]
	private struct BrushCullingJob : IJobParallelFor
	{
		public const int kBatchSize = 8;

		public float2 GlobalMin;

		public float2 GlobalMax;

		public float ChunkSize;

		[ReadOnly]
		public NativeArray<TerrainStampingBrushContainer.BrushData> BrushData;

		[ReadOnly]
		public NativeHashMap<int2, int> ChunkIndexToSlots;

		[NativeDisableContainerSafetyRestriction]
		public NativeArray<TerrainStampingBrushPassData.BrushCullingJobData.ChunkSlotData> ChunkSlots;

		public void Execute(int brushIndex)
		{
			TerrainStampingBrushContainer.BrushData brushData = BrushData[brushIndex];
			float2 @float = brushData.Position - brushData.Radius;
			float2 float2 = brushData.Position + brushData.Radius;
			if (!AABBIntersects2D(@float, float2, GlobalMin, GlobalMax))
			{
				return;
			}
			TerrainStampingUtils.ComputeMinMaxChunkRange(@float, float2, ChunkSize, out var chunksMin, out var chunksMax);
			for (int i = chunksMin.x; i < chunksMax.x; i++)
			{
				for (int j = chunksMin.y; j < chunksMax.y; j++)
				{
					int2 key = math.int2(i, j);
					if (ChunkIndexToSlots.TryGetValue(key, out var item))
					{
						UnsafeCollectionExtensions.ElementAsRef(in ChunkSlots, item).RelevantBrushIndicesAsParallel.AddNoResize(brushIndex);
					}
				}
			}
		}

		private static bool AABBIntersects2D(float2 aMin, float2 aMax, float2 bMin, float2 bMax)
		{
			if (aMin.x <= bMax.x && aMax.x >= bMin.x && aMin.y <= bMax.y)
			{
				return aMax.y >= bMin.y;
			}
			return false;
		}
	}

	[BurstCompile]
	private struct MergeBrushCullingResultsJob : IJob
	{
		[ReadOnly]
		public NativeArray<TerrainStampingManager.ChunkData> ActiveChunks;

		[ReadOnly]
		public NativeHashMap<int2, int> ChunkIndexToSlots;

		[ReadOnly]
		[NativeDisableContainerSafetyRestriction]
		public NativeArray<TerrainStampingBrushPassData.BrushCullingJobData.ChunkSlotData> ChunkSlots;

		public NativeList<int> DrawChunksSlots;

		public NativeList<TerrainStampingBrushPassData.FadeOnlyChunk> FadeOnlyChunks;

		public void Execute()
		{
			for (int i = 0; i < ChunkSlots.Length; i++)
			{
				if (ChunkSlots[i].RelevantBrushIndices.Length > 0)
				{
					DrawChunksSlots.Add(in i);
				}
			}
			foreach (TerrainStampingManager.ChunkData activeChunk in ActiveChunks)
			{
				bool inBounds = false;
				if (ChunkIndexToSlots.TryGetValue(activeChunk.Index, out var item))
				{
					inBounds = true;
					if (ChunkSlots[item].RelevantBrushIndices.Length > 0)
					{
						continue;
					}
				}
				FadeOnlyChunks.AddNoResize(new TerrainStampingBrushPassData.FadeOnlyChunk
				{
					ChunkIndex = activeChunk.Index,
					PoolAllocationIndex = activeChunk.PoolAllocation.Index,
					InBounds = inBounds
				});
			}
		}
	}

	private static class ShaderIDs
	{
		public static readonly int _PaddingScaleOffset = Shader.PropertyToID("_PaddingScaleOffset");

		public static readonly int _BrushScaleBias = Shader.PropertyToID("_BrushScaleBias");

		public static readonly int _BrushTexture = Shader.PropertyToID("_BrushTexture");

		public static readonly int _BrushStrength = Shader.PropertyToID("_BrushStrength");
	}

	private enum PassType
	{
		Brush,
		Fade
	}

	private static class Profiling
	{
		public static readonly ProfilingSampler BrushCulling = new ProfilingSampler("BrushCulling");

		public static readonly ProfilingSampler ApplyBrushes = new ProfilingSampler("ApplyBrushes");

		public static readonly ProfilingSampler FadeOut = new ProfilingSampler("FadeOut");
	}

	private readonly TerrainStampingManagerParameters m_Parameters;

	private readonly MaterialPropertyBlock m_PropertyBlock;

	public override string Name => "TerrainStampingBrushPass";

	public TerrainStampingBrushPass(TerrainStampingManagerParameters parameters, RenderPassEvent evt)
		: base(evt)
	{
		m_Parameters = parameters;
		m_PropertyBlock = new MaterialPropertyBlock();
	}

	protected override void Setup(RenderGraphBuilder builder, TerrainStampingBrushPassData data, ContextContainer frameData)
	{
		TerrainStampingManager.TryGetInstance(out var terrainStampingManager);
		data.Manager = terrainStampingManager;
		data.BrushMaterial = terrainStampingManager.BrushMaterial;
		data.FadeMaterial = terrainStampingManager.FadeMaterial;
		data.StampingTexturePool = terrainStampingManager.StampingTexturePool;
		data.DeltaTime = terrainStampingManager.GetNextDeltaTime();
		data.Skip = Mathf.Approximately(data.DeltaTime, 0f);
		if (data.Skip)
		{
			return;
		}
		using (new ProfilingScope(Profiling.BrushCulling))
		{
			data.PaddingScaleOffset = TerrainStampingUtils.ComputePaddingScaleOffset(m_Parameters.Resolution, m_Parameters.Padding);
			float chunkSize = m_Parameters.ChunkSize;
			int2 chunksMinIndex = terrainStampingManager.ChunksMinIndex;
			int2 chunksMaxIndex = terrainStampingManager.ChunksMaxIndex;
			int2 @int = chunksMaxIndex - chunksMinIndex;
			int num = @int.x * @int.y;
			NativeHashMap<int2, int> nativeHashMap = new NativeHashMap<int2, int>(num, Allocator.TempJob);
			NativeArray<TerrainStampingBrushPassData.BrushCullingJobData.ChunkSlotData> chunkSlots = new NativeArray<TerrainStampingBrushPassData.BrushCullingJobData.ChunkSlotData>(num, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			NativeArray<TerrainStampingBrushContainer.BrushData> data2 = TerrainStampingBrushContainer.GetData(chunkSize, Allocator.TempJob);
			NativeArray<TerrainStampingManager.ChunkData> activeChunks = terrainStampingManager.GetActiveChunks(Allocator.TempJob);
			NativeList<int> drawChunksSlots = new NativeList<int>(num, Allocator.TempJob);
			NativeList<TerrainStampingBrushPassData.FadeOnlyChunk> fadeOnlyChunks = new NativeList<TerrainStampingBrushPassData.FadeOnlyChunk>(activeChunks.Length, Allocator.TempJob);
			for (int i = chunksMinIndex.x; i < chunksMaxIndex.x; i++)
			{
				for (int j = chunksMinIndex.y; j < chunksMaxIndex.y; j++)
				{
					int2 int2 = math.int2(i, j);
					int count = nativeHashMap.Count;
					nativeHashMap.Add(int2, count);
					NativeList<int> relevantBrushIndices = new NativeList<int>(data2.Length, Allocator.TempJob);
					chunkSlots[count] = new TerrainStampingBrushPassData.BrushCullingJobData.ChunkSlotData
					{
						ChunkIndex = int2,
						RelevantBrushIndices = relevantBrushIndices,
						RelevantBrushIndicesAsParallel = relevantBrushIndices.AsParallelWriter(),
						ChunkMin = (float2)int2 * chunkSize,
						ChunkMax = (float2)(int2 + 1) * chunkSize
					};
				}
			}
			BrushCullingJob jobData = default(BrushCullingJob);
			jobData.ChunkSize = chunkSize;
			jobData.GlobalMin = (float2)chunksMinIndex * chunkSize;
			jobData.GlobalMax = (float2)chunksMaxIndex * chunkSize;
			jobData.ChunkSlots = chunkSlots;
			jobData.BrushData = data2;
			jobData.ChunkIndexToSlots = nativeHashMap;
			JobHandle dependsOn = IJobParallelForExtensions.Schedule(jobData, data2.Length, 8);
			MergeBrushCullingResultsJob jobData2 = default(MergeBrushCullingResultsJob);
			jobData2.ChunkSlots = chunkSlots;
			jobData2.ActiveChunks = activeChunks;
			jobData2.DrawChunksSlots = drawChunksSlots;
			jobData2.FadeOnlyChunks = fadeOnlyChunks;
			jobData2.ChunkIndexToSlots = nativeHashMap;
			dependsOn = jobData2.Schedule(dependsOn);
			data.BrushCullingJob = new TerrainStampingBrushPassData.BrushCullingJobData
			{
				ChunkIndexToSlot = nativeHashMap,
				ChunkSlots = chunkSlots,
				Brushes = data2,
				ActiveChunks = activeChunks,
				DrawChunksSlots = drawChunksSlots,
				FadeOnlyChunks = fadeOnlyChunks,
				JobHandle = dependsOn
			};
		}
	}

	protected override void Render(TerrainStampingBrushPassData data, RenderGraphContext context)
	{
		if (data.Skip)
		{
			return;
		}
		data.BrushCullingJob.JobHandle.Complete();
		foreach (int drawChunksSlot in data.BrushCullingJob.DrawChunksSlots)
		{
			ref TerrainStampingBrushPassData.BrushCullingJobData.ChunkSlotData reference = ref UnsafeCollectionExtensions.ElementAsRef(in data.BrushCullingJob.ChunkSlots, drawChunksSlot);
			int2 chunkIndex = reference.ChunkIndex;
			bool isNew;
			TerrainStampingManager.ChunkData orAllocateChunkData = data.Manager.GetOrAllocateChunkData(chunkIndex, out isNew);
			if (orAllocateChunkData.PoolAllocation.Index == -1)
			{
				continue;
			}
			context.cmd.SetRenderTarget(new RenderTargetIdentifier(data.StampingTexturePool, 0, CubemapFace.Unknown, orAllocateChunkData.PoolAllocation.Index));
			if (isNew)
			{
				context.cmd.ClearRenderTarget(RTClearFlags.Color, Color.black);
			}
			FadeOut(data, context, chunkIndex, inBounds: true);
			using (new ProfilingScope(context.cmd, Profiling.ApplyBrushes))
			{
				NativeList<int> relevantBrushIndices = reference.RelevantBrushIndices;
				foreach (int item in relevantBrushIndices)
				{
					ref TerrainStampingBrushContainer.BrushData reference2 = ref UnsafeCollectionExtensions.ElementAsRef(in data.BrushCullingJob.Brushes, item);
					float4 @float = default(float4);
					@float.xy = reference2.RelativeRadius;
					@float.zw = math.unlerp(reference.ChunkMin, reference.ChunkMax, reference2.Position) - reference2.RelativeRadius * 0.5f;
					Texture2D texture = TerrainStampingBrushContainer.GetTexture(reference2.Index);
					data.Manager.ApplyBrushVirtually(chunkIndex, reference2.Strength, out var physicalBrushStrength, TerrainStampingManager.BrushMode.Render);
					if (physicalBrushStrength > 0f)
					{
						ApplyBrushPhysically(data, ref context, PassType.Brush, data.PaddingScaleOffset, @float, texture, physicalBrushStrength);
					}
				}
			}
		}
		foreach (TerrainStampingBrushPassData.FadeOnlyChunk fadeOnlyChunk in data.BrushCullingJob.FadeOnlyChunks)
		{
			context.cmd.SetRenderTarget(new RenderTargetIdentifier(data.StampingTexturePool, 0, CubemapFace.Unknown, fadeOnlyChunk.PoolAllocationIndex));
			FadeOut(data, context, fadeOnlyChunk.ChunkIndex, fadeOnlyChunk.InBounds);
		}
		data.BrushCullingJob.Dispose();
		data.BrushCullingJob = default(TerrainStampingBrushPassData.BrushCullingJobData);
	}

	private void FadeOut(TerrainStampingBrushPassData data, RenderGraphContext context, int2 chunkIndex, bool inBounds)
	{
		using (new ProfilingScope(context.cmd, Profiling.FadeOut))
		{
			float num = (inBounds ? m_Parameters.FadeOutDuration : m_Parameters.OutOfBoundsFadeOutDuration);
			TerrainStampingManager.BrushMode brushMode = (inBounds ? TerrainStampingManager.BrushMode.Render : TerrainStampingManager.BrushMode.Accumulate);
			float num2 = data.DeltaTime / num;
			data.Manager.ApplyBrushVirtually(chunkIndex, 0f - num2, out var physicalBrushStrength, brushMode);
			physicalBrushStrength = math.max(0f, 0f - physicalBrushStrength);
			if (brushMode == TerrainStampingManager.BrushMode.Render && physicalBrushStrength > 0f)
			{
				Vector4 vector = new Vector4(1f, 1f, 0f, 0f);
				ApplyBrushPhysically(data, ref context, PassType.Fade, vector, vector, Texture2D.whiteTexture, physicalBrushStrength);
			}
		}
	}

	private void ApplyBrushPhysically(TerrainStampingBrushPassData data, ref RenderGraphContext context, PassType passType, Vector4 paddingScaleOffset, Vector4 scaleBias, Texture2D texture, float strength)
	{
		m_PropertyBlock.SetVector(ShaderIDs._PaddingScaleOffset, paddingScaleOffset);
		m_PropertyBlock.SetVector(ShaderIDs._BrushScaleBias, scaleBias);
		m_PropertyBlock.SetTexture(ShaderIDs._BrushTexture, texture);
		m_PropertyBlock.SetFloat(ShaderIDs._BrushStrength, strength);
		Material material = passType switch
		{
			PassType.Brush => data.BrushMaterial, 
			PassType.Fade => data.FadeMaterial, 
			_ => throw new ArgumentOutOfRangeException("passType", passType, null), 
		};
		context.cmd.DrawProcedural(Matrix4x4.identity, material, 0, MeshTopology.Quads, 4, 1, m_PropertyBlock);
	}
}
