using System;
using System.IO;
using Owlcat.Runtime.Core.Collections;
using Owlcat.Runtime.Visual.VirtualTexture.Atlas;
using Owlcat.Runtime.Visual.VirtualTexture.Feedback;
using Owlcat.Runtime.Visual.VirtualTexture.PostRender;
using Owlcat.Runtime.Visual.VirtualTexture.TiledTexture;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.IO.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.VirtualTexture.Streaming;

public class TileStreamer : IDisposable
{
	private StreamingBatchPool m_Pool = new StreamingBatchPool();

	private ComputeShader m_CopyTileShader;

	private TilesInBatchLimit m_Limit;

	private bool m_UseAsyncReadManager;

	private int m_BatchSizeInBytes;

	public int Limit => (int)m_Limit;

	public TileStreamer(ComputeShader copyTileShader, TilesInBatchLimit limit, bool useAsyncReadManager)
	{
		m_CopyTileShader = copyTileShader;
		m_Limit = limit;
		m_UseAsyncReadManager = useAsyncReadManager;
		m_BatchSizeInBytes = 25920 * (int)m_Limit;
	}

	public void Dispose()
	{
		m_Pool.Dispose();
	}

	public void StreamAll(AsyncContext context, NativeList<PageLoadInfo> loadingRequests)
	{
		Stream(context, loadingRequests, int.MaxValue);
	}

	public void Stream(AsyncContext context, NativeList<PageLoadInfo> loadingRequests, int maxBatchCount)
	{
		FillBatches(context, loadingRequests, maxBatchCount);
		StreamTilesFromDisk(context);
	}

	private void FillBatches(AsyncContext context, NativeList<PageLoadInfo> loadingRequests, int maxBatchCount)
	{
		StreamingBatch streamingBatch = null;
		_ = loadingRequests.Length;
		int num = 0;
		while (loadingRequests.Length > 0)
		{
			if (context.PendingBatches.Count == 0)
			{
				streamingBatch = m_Pool.Get(m_Limit);
				context.PendingBatches.Add(streamingBatch);
			}
			PageLoadInfo pageLoadInfo = loadingRequests[loadingRequests.Length - 1];
			int2 virtualTileCoord = new int2(pageLoadInfo.VirtualTileX, pageLoadInfo.VirtualTileY);
			NativeArray<Page> nativeArray = context.VirtualAtlas.Pages;
			ref Page reference = ref UnsafeCollectionExtensions.ElementAsRef(in nativeArray, virtualTileCoord.y * context.VirtualAtlas.ResolutionInTiles.x + virtualTileCoord.x);
			if (reference.TextureId == -1)
			{
				loadingRequests.RemoveAtSwapBack(loadingRequests.Length - 1);
				continue;
			}
			NativeList<VirtualAtlasEntry> nativeArray2 = context.VirtualAtlas.Entries;
			ref VirtualAtlasEntry reference2 = ref UnsafeCollectionExtensions.ElementAsRef(in nativeArray2, reference.TextureId);
			int3 pyramidCoord = reference2.ConvertVirtualToPyramidCoord(in virtualTileCoord);
			if (streamingBatch.Tasks.Length + reference2.LayerCount > Limit)
			{
				if (context.PendingBatches.Count >= maxBatchCount)
				{
					break;
				}
				streamingBatch = m_Pool.Get(m_Limit);
				context.PendingBatches.Add(streamingBatch);
			}
			loadingRequests.RemoveAtSwapBack(loadingRequests.Length - 1);
			for (int i = 0; i < reference2.LayerCount; i++)
			{
				TileReadTask value = default(TileReadTask);
				value.Guid = reference2.ValidGuids[i];
				value.LayerCount = reference2.LayerCount;
				value.LayerIndex = i;
				value.MipCount = reference2.MipCount;
				value.VirtualCoord = virtualTileCoord;
				value.PyramidCoord = pyramidCoord;
				value.RectInTiles = reference2.RectInTiles;
				value.FrameId = context.FrameId;
				streamingBatch.Tasks.AddNoResize(value);
				num++;
			}
		}
	}

	private void StreamTilesFromDisk(AsyncContext context)
	{
		foreach (StreamingBatch pendingBatch in context.PendingBatches)
		{
			for (int i = 0; i < pendingBatch.Tasks.Length; i++)
			{
				ref TileReadTask reference = ref UnsafeCollectionExtensions.ElementAsRef(in pendingBatch.Tasks, i);
				TiledTextureDB.TryGetAssetPath(reference.Guid, out var assetPath);
				TiledTextureFileHeader header = TiledTextureDB.GetHeader(assetPath);
				int2 @int = new int2(header.Width, header.Height) / 128;
				int num = header.MipCount - reference.MipCount;
				int num2 = math.clamp(reference.PyramidCoord.z, 0, reference.MipCount - 1) + num;
				int2 int2 = @int >> num2;
				int num3 = 0;
				for (int j = 0; j < num2; j++)
				{
					int2 int3 = @int >> j;
					int num4 = int3.x * int3.y;
					num3 += num4;
				}
				num3 += reference.PyramidCoord.y * int2.x + reference.PyramidCoord.x;
				num3 *= 25920;
				num3 += header.GetSizeInBytes();
				LoadTileForBufferLayout(in assetPath, num3, i * 25920, pendingBatch.RawData);
			}
		}
	}

	private unsafe void LoadTileForBufferLayout(in string filePath, int offsetInFile, int offsetInBuffer, NativeArray<byte> rawBuffer)
	{
		if (m_UseAsyncReadManager)
		{
			ReadCommand* ptr = stackalloc ReadCommand[1];
			ptr->Buffer = (byte*)rawBuffer.GetUnsafePtr() + offsetInBuffer;
			ptr->Offset = offsetInFile;
			ptr->Size = 25920L;
			AsyncReadManager.Read(filePath, ptr, 1u, "", 0uL).JobHandle.Complete();
			return;
		}
		using FileStream fileStream = File.OpenRead(filePath);
		long length = fileStream.Length;
		long valueToClamp = offsetInFile;
		valueToClamp = math.clamp(valueToClamp, 0L, length - 25920);
		Span<byte> buffer = rawBuffer.AsSpan().Slice(offsetInBuffer, 25920);
		fileStream.Seek(valueToClamp, SeekOrigin.Begin);
		fileStream.Read(buffer);
	}

	internal void ReleaseBatches(AsyncContext context)
	{
		for (int i = 0; i < context.PendingBatches.Count; i++)
		{
			m_Pool.Release(context.PendingBatches[i]);
		}
		context.PendingBatches.Clear();
	}
}
