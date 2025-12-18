using System;
using System.Collections.Generic;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Visual.VirtualTexture.Atlas;
using Owlcat.Runtime.Visual.VirtualTexture.Feedback;
using Owlcat.Runtime.Visual.VirtualTexture.IndirectionTexture;
using Owlcat.Runtime.Visual.VirtualTexture.Streaming;
using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.VirtualTexture.PostRender;

public class AsyncContext : IDisposable
{
	private VirtualAtlas m_VirtualAtlas;

	private PhysicalAtlas m_PhysicalAtlas;

	private FeedbackConsumptionTracker m_FeedbackConsumptionTracker;

	private AsyncReadbackProcessor m_ReadbackProcessor;

	private IndirectionTextureRenderer m_IndirectTextureRenderer;

	internal LruLayerCache LruCache;

	public NativeList<int2> ResidentTiles;

	public NativeList<PageLoadInfo> LoadRequests;

	internal AtomicCounter SeenResidentPagesCounter;

	public int FrameId;

	public VirtualTextureConstantBuffer ConstantBuffer;

	public List<StreamingBatch> PendingBatches;

	public VirtualAtlas VirtualAtlas => m_VirtualAtlas;

	public PhysicalAtlas PhysicalAtlas => m_PhysicalAtlas;

	public AsyncReadbackProcessor ReadbackProcessor => m_ReadbackProcessor;

	public FeedbackConsumptionTracker FeedbackConsumptionTracker => m_FeedbackConsumptionTracker;

	public IndirectionTextureRenderer IndirectTextureRenderer => m_IndirectTextureRenderer;

	public AsyncContext(VirtualAtlas virtualAtlas, PhysicalAtlas physicalAtlas, IndirectionTextureRenderer indirectTextureRenderer, FeedbackConsumptionTracker feedbackConsumptionTracker)
	{
		m_VirtualAtlas = virtualAtlas;
		m_PhysicalAtlas = physicalAtlas;
		m_IndirectTextureRenderer = indirectTextureRenderer;
		m_FeedbackConsumptionTracker = feedbackConsumptionTracker;
		SeenResidentPagesCounter = AtomicCounter.Create();
		int y = virtualAtlas.ResolutionInTiles.x * virtualAtlas.ResolutionInTiles.y;
		y = math.max(1, y);
		int num = physicalAtlas.Resolution.TilesInSlice.x * physicalAtlas.Resolution.TilesInSlice.y;
		int x = physicalAtlas.Resolution.TilesInSlice.x;
		int num2 = num * physicalAtlas.Resolution.ArraySlices;
		LruCache = new LruLayerCache((ushort)num2, (ushort)x);
		ResidentTiles = new NativeList<int2>(y, Allocator.Persistent);
		LoadRequests = new NativeList<PageLoadInfo>(y, Allocator.Persistent);
		m_ReadbackProcessor = new AsyncReadbackProcessor();
		ConstantBuffer = default(VirtualTextureConstantBuffer);
		PendingBatches = new List<StreamingBatch>();
	}

	public void Dispose()
	{
		LruCache.Dispose();
		SeenResidentPagesCounter.Dispose();
		ResidentTiles.Dispose();
		LoadRequests.Dispose();
		m_ReadbackProcessor.Dispose();
	}

	internal void BeforeRunAsync()
	{
		SeenResidentPagesCounter.Reset();
		ResidentTiles.Clear();
	}

	internal void Refresh(int2 virtualAtlasResolutionInTiles)
	{
		if (ResidentTiles.IsCreated)
		{
			ResidentTiles.Dispose();
		}
		if (LoadRequests.IsCreated)
		{
			LoadRequests.Dispose();
		}
		int y = virtualAtlasResolutionInTiles.x * virtualAtlasResolutionInTiles.y;
		y = math.max(1, y);
		ResidentTiles = new NativeList<int2>(y, Allocator.Persistent);
		LoadRequests = new NativeList<PageLoadInfo>(y, Allocator.Persistent);
		m_ReadbackProcessor.Refresh(virtualAtlasResolutionInTiles);
	}

	internal bool HasPendingBatches()
	{
		return PendingBatches.Count > 0;
	}
}
