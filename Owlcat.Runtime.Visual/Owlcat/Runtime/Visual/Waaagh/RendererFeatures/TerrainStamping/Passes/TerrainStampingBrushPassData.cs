using System;
using Owlcat.Runtime.Core.Collections;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.TerrainStamping.Passes;

public class TerrainStampingBrushPassData : PassDataBase
{
	public struct FadeOnlyChunk
	{
		public int PoolAllocationIndex;

		public int2 ChunkIndex;

		public bool InBounds;
	}

	public struct BrushCullingJobData : IDisposable
	{
		public struct ChunkSlotData
		{
			public NativeList<int> RelevantBrushIndices;

			public int2 ChunkIndex;

			public float2 ChunkMin;

			public float2 ChunkMax;

			public NativeList<int>.ParallelWriter RelevantBrushIndicesAsParallel;
		}

		public JobHandle JobHandle;

		public NativeArray<TerrainStampingBrushContainer.BrushData> Brushes;

		public NativeHashMap<int2, int> ChunkIndexToSlot;

		public NativeArray<ChunkSlotData> ChunkSlots;

		public NativeArray<TerrainStampingManager.ChunkData> ActiveChunks;

		public NativeList<int> DrawChunksSlots;

		public NativeList<FadeOnlyChunk> FadeOnlyChunks;

		public void Dispose()
		{
			for (int i = 0; i < ChunkSlots.Length; i++)
			{
				UnsafeCollectionExtensions.ElementAsRef(in ChunkSlots, i).RelevantBrushIndices.Dispose();
			}
			Brushes.Dispose();
			ChunkIndexToSlot.Dispose();
			ChunkSlots.Dispose();
			ActiveChunks.Dispose();
			DrawChunksSlots.Dispose();
			FadeOnlyChunks.Dispose();
		}
	}

	public BrushCullingJobData BrushCullingJob;

	public Material BrushMaterial;

	public float DeltaTime;

	public Material FadeMaterial;

	public TerrainStampingManager Manager;

	public float4 PaddingScaleOffset;

	public bool Skip;

	public RenderTexture StampingTexturePool;
}
