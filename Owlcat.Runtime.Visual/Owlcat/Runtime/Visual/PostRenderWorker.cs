using System;
using Owlcat.Runtime.Visual.VirtualTexture;
using Owlcat.Runtime.Visual.VirtualTexture.Atlas;
using Owlcat.Runtime.Visual.VirtualTexture.Feedback;
using Owlcat.Runtime.Visual.VirtualTexture.PostRender;
using Owlcat.Runtime.Visual.VirtualTexture.PostRender.Jobs;
using Owlcat.Runtime.Visual.VirtualTexture.Streaming;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual;

public class PostRenderWorker : IDisposable
{
	private BackgroundThreadWorker m_Worker;

	private VirtualTextureSettings m_Settings;

	private NativeList<int2> m_ResidentRequestedTiles;

	private NativeList<PageLoadInfo> m_ResidentTilesLoadRequests;

	private NativeList<int2> m_FeedbackRequestedTiles;

	private NativeList<PageLoadInfo> m_FeedbackTilesLoadRequests;

	private AsyncContext m_Context;

	private bool m_LoadFeedback;

	private bool m_VirtualAtlasWasResized;

	private TileStreamer m_Streamer;

	private TileUploader m_Uploader;

	public bool IsBusy => m_Worker.IsBusy;

	public int TilesLoadedPerFrame => m_Uploader.TilesLoadedPerFrame;

	public int TilesLoadedTotal => m_Uploader.TilesLoadedTotal;

	public int TilesLoadingLag => m_Uploader.TilesLoadingLag;

	public TileUploader TileUploader => m_Uploader;

	public PostRenderWorker(VirtualTextureSettings settings, ComputeShader copyTileShader)
	{
		m_Settings = settings;
		m_Worker = new BackgroundThreadWorker("VTPostRenderWorker", DoWork);
		m_Streamer = new TileStreamer(copyTileShader, settings.TilesInBatchLimit, settings.UseAsyncReadManager);
		m_Uploader = new TileUploader(settings.TilesInBatchLimit, copyTileShader);
	}

	public void Dispose()
	{
		m_Worker?.Dispose();
		m_Streamer?.Dispose();
		m_Uploader?.Dispose();
		if (m_ResidentRequestedTiles.IsCreated)
		{
			m_ResidentRequestedTiles.Dispose();
		}
		if (m_ResidentTilesLoadRequests.IsCreated)
		{
			m_ResidentTilesLoadRequests.Dispose();
		}
		if (m_FeedbackRequestedTiles.IsCreated)
		{
			m_FeedbackRequestedTiles.Dispose();
		}
		if (m_FeedbackTilesLoadRequests.IsCreated)
		{
			m_FeedbackTilesLoadRequests.Dispose();
		}
	}

	internal void LoadResidentTiles(AsyncContext context, CommandBuffer cmd)
	{
		if (m_Worker.IsBusy)
		{
			throw new InvalidOperationException("PostRenderWorker is busy, can't schedule RequestedTilesAnalysisJob because it writes to Pages array which is used by worker thread.");
		}
		if (m_Settings.PreloadSmallestMips && CheckIfNeedLoadTilesOnMainThread(context))
		{
			UploadBatches(context, cmd);
			m_Streamer.StreamAll(context, m_ResidentTilesLoadRequests);
			UploadBatches(context, cmd);
			context.IndirectTextureRenderer.PrepareData(context, default(JobHandle)).Complete();
		}
	}

	internal bool CheckIfNeedLoadTilesOnMainThread(AsyncContext context)
	{
		m_ResidentRequestedTiles.Clear();
		m_ResidentTilesLoadRequests.Clear();
		JobHandle dependsOn = default(JobHandle);
		CollectResidentTilesJob collectResidentTilesJob = default(CollectResidentTilesJob);
		collectResidentTilesJob.LoadSmallestMips = m_Settings.PreloadSmallestMips;
		collectResidentTilesJob.VirtualAtlasResolutionInTiles = context.VirtualAtlas.ResolutionInTiles;
		collectResidentTilesJob.Entries = context.VirtualAtlas.Entries.AsArray();
		collectResidentTilesJob.RequestedTiles = m_ResidentRequestedTiles.AsParallelWriter();
		CollectResidentTilesJob jobData = collectResidentTilesJob;
		dependsOn = IJobParallelForExtensions.ScheduleByRef(ref jobData, context.VirtualAtlas.Entries.Length, 8, dependsOn);
		RequestedTilesAnalysisJob requestedTilesAnalysisJob = default(RequestedTilesAnalysisJob);
		requestedTilesAnalysisJob.FrameId = context.FrameId;
		requestedTilesAnalysisJob.MaxMip = context.VirtualAtlas.MipCount - 1;
		requestedTilesAnalysisJob.VirtualAtlasResolutionInTiles = context.VirtualAtlas.ResolutionInTiles;
		requestedTilesAnalysisJob.PhysicalAtlasResolution = context.PhysicalAtlas.Resolution;
		requestedTilesAnalysisJob.Entries = context.VirtualAtlas.Entries;
		requestedTilesAnalysisJob.Pages = context.VirtualAtlas.Pages;
		requestedTilesAnalysisJob.LruCache = context.LruCache;
		requestedTilesAnalysisJob.SeenResidentPagesCounter = context.SeenResidentPagesCounter;
		requestedTilesAnalysisJob.RequestedTiles = m_ResidentRequestedTiles;
		requestedTilesAnalysisJob.LoadRequests = m_ResidentTilesLoadRequests;
		RequestedTilesAnalysisJob jobData2 = requestedTilesAnalysisJob;
		IJobExtensions.ScheduleByRef(ref jobData2, dependsOn).Complete();
		float consumption = (float)context.SeenResidentPagesCounter.Count / (float)context.PhysicalAtlas.Resolution.TotalTiles();
		context.FeedbackConsumptionTracker.Update(consumption);
		return m_ResidentTilesLoadRequests.Length > 0;
	}

	internal void UploadBatches(AsyncContext context, CommandBuffer cmd)
	{
		if (context.HasPendingBatches())
		{
			m_Uploader.Upload(context, cmd);
			m_Streamer.ReleaseBatches(context);
		}
	}

	internal void RunAsync(AsyncContext context, CommandBuffer cmd, bool loadFeedback)
	{
		m_Context = context;
		m_LoadFeedback = loadFeedback;
		if (m_VirtualAtlasWasResized)
		{
			context.Refresh(context.VirtualAtlas.ResolutionInTiles);
			int2 virtualAtlasResolutionInTiles = context.VirtualAtlas.ResolutionInTiles;
			ResizeFeedbackTileLists(in virtualAtlasResolutionInTiles);
			m_VirtualAtlasWasResized = false;
		}
		m_Worker.RunAsync();
	}

	private void ResizeFeedbackTileLists(in int2 virtualAtlasResolutionInTiles)
	{
		int num = virtualAtlasResolutionInTiles.x * virtualAtlasResolutionInTiles.y;
		if (m_FeedbackRequestedTiles.IsCreated)
		{
			if (num > m_FeedbackRequestedTiles.Capacity)
			{
				m_FeedbackRequestedTiles.Capacity = num;
				m_FeedbackTilesLoadRequests.Capacity = num;
			}
		}
		else
		{
			m_FeedbackRequestedTiles = new NativeList<int2>(num, Allocator.Persistent);
			m_FeedbackTilesLoadRequests = new NativeList<PageLoadInfo>(num, Allocator.Persistent);
		}
	}

	private void ResizeResidentTilesLists(in int2 virtualAtlasResolutionInTiles)
	{
		int num = virtualAtlasResolutionInTiles.x * virtualAtlasResolutionInTiles.y;
		if (m_ResidentRequestedTiles.IsCreated)
		{
			if (num > m_ResidentRequestedTiles.Capacity)
			{
				m_ResidentRequestedTiles.Capacity = num;
				m_ResidentTilesLoadRequests.Capacity = num;
			}
		}
		else
		{
			m_ResidentRequestedTiles = new NativeList<int2>(num, Allocator.Persistent);
			m_ResidentTilesLoadRequests = new NativeList<PageLoadInfo>(num, Allocator.Persistent);
		}
	}

	private void DoWork()
	{
		AsyncReadbackProcessor.Request pendingRequest = m_Context.ReadbackProcessor.GetPendingRequest();
		if (m_LoadFeedback && pendingRequest != null)
		{
			AnalyzeFeedbackAndBuildStreamingBatches(pendingRequest);
			if (m_FeedbackTilesLoadRequests.Length > 0)
			{
				m_Streamer.Stream(m_Context, m_FeedbackTilesLoadRequests, m_Settings.MaxBatchesPerFrame);
			}
			m_Context.IndirectTextureRenderer.PrepareData(m_Context, default(JobHandle)).Complete();
		}
		if (pendingRequest != null)
		{
			m_Context.ReadbackProcessor.FreeRequest(pendingRequest);
		}
	}

	private void AnalyzeFeedbackAndBuildStreamingBatches(AsyncReadbackProcessor.Request feedbackRequest)
	{
		JobHandle dependsOn = default(JobHandle);
		m_FeedbackRequestedTiles.Clear();
		CollectFeedbackTilesJob collectFeedbackTilesJob = default(CollectFeedbackTilesJob);
		collectFeedbackTilesJob.PreloadSmallestMips = m_Settings.PreloadSmallestMips;
		collectFeedbackTilesJob.VirtualAtlasResolutionInTiles = m_Context.VirtualAtlas.ResolutionInTiles;
		collectFeedbackTilesJob.Entries = m_Context.VirtualAtlas.Entries.AsArray();
		collectFeedbackTilesJob.EncodedFeedback = feedbackRequest.ReadbackData;
		collectFeedbackTilesJob.RequestedTiles = m_FeedbackRequestedTiles.AsParallelWriter();
		CollectFeedbackTilesJob jobData = collectFeedbackTilesJob;
		dependsOn = IJobParallelForExtensions.ScheduleByRef(ref jobData, m_Context.VirtualAtlas.Entries.Length, 8, dependsOn);
		RequestedTilesAnalysisJob requestedTilesAnalysisJob = default(RequestedTilesAnalysisJob);
		requestedTilesAnalysisJob.FrameId = m_Context.FrameId;
		requestedTilesAnalysisJob.VirtualAtlasResolutionInTiles = m_Context.VirtualAtlas.ResolutionInTiles;
		requestedTilesAnalysisJob.PhysicalAtlasResolution = m_Context.PhysicalAtlas.Resolution;
		requestedTilesAnalysisJob.MaxMip = m_Context.VirtualAtlas.MipCount - 1;
		requestedTilesAnalysisJob.Entries = m_Context.VirtualAtlas.Entries;
		requestedTilesAnalysisJob.Pages = m_Context.VirtualAtlas.Pages;
		requestedTilesAnalysisJob.LruCache = m_Context.LruCache;
		requestedTilesAnalysisJob.RequestedTiles = m_FeedbackRequestedTiles;
		requestedTilesAnalysisJob.LoadRequests = m_FeedbackTilesLoadRequests;
		requestedTilesAnalysisJob.SeenResidentPagesCounter = m_Context.SeenResidentPagesCounter;
		RequestedTilesAnalysisJob jobData2 = requestedTilesAnalysisJob;
		dependsOn = IJobExtensions.ScheduleByRef(ref jobData2, dependsOn);
		SortListJob<PageLoadInfo> sortListJob = default(SortListJob<PageLoadInfo>);
		sortListJob.List = m_FeedbackTilesLoadRequests;
		SortListJob<PageLoadInfo> jobData3 = sortListJob;
		IJobExtensions.ScheduleByRef(ref jobData3, dependsOn).Complete();
		float consumption = (float)m_Context.SeenResidentPagesCounter.Count / (float)m_Context.PhysicalAtlas.Resolution.TotalTiles();
		m_Context.FeedbackConsumptionTracker.Update(consumption);
	}

	internal void Wait()
	{
		m_Worker.Wait();
	}

	internal void OnVirtualAtlasResize(in int2 virtualAtlasResolutionInTiles)
	{
		m_VirtualAtlasWasResized = true;
		ResizeResidentTilesLists(in virtualAtlasResolutionInTiles);
	}
}
