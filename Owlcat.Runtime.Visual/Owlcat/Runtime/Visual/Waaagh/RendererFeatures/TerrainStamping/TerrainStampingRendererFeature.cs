using System;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.TerrainStamping.Passes;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.TerrainStamping;

internal sealed class TerrainStampingRendererFeature : IRendererFeature, IDisposable
{
	private readonly TerrainStampingRendererFeatureAsset m_Asset;

	private readonly RendererFeaturePipelineService m_ManagerService;

	private readonly Material m_StencilMaskMaterial;

	private BrushPass.BrushCullingJobData? m_BrushPassScheduledJobData;

	public TerrainStampingRendererFeature(TerrainStampingRendererFeatureAsset asset)
	{
		m_Asset = asset;
		m_ManagerService = new RendererFeaturePipelineService(delegate(WaaaghPipeline pipeline)
		{
			TerrainStampingManager.Init(pipeline, m_Asset.ManagerParameters);
		}, delegate
		{
			TerrainStampingManager.Cleanup();
		}, () => TerrainStampingManager.IsInitialized);
		m_ManagerService.OnCreate();
		m_StencilMaskMaterial = CoreUtils.CreateEngineMaterial(m_Asset.ManagerParameters.StencilMaskShader);
	}

	public void Dispose()
	{
		m_ManagerService.OnDispose();
		CoreUtils.Destroy(m_StencilMaskMaterial);
	}

	public void RegisterExtensions(RendererFeatureExtensionRegistry registry)
	{
		registry.AddSetupDelegate(OnSetup);
		registry.AddScheduleSetupJobsDelegate(OnScheduleSetupJobsDelegate);
		registry.AddRecordDelegate(RecordExtensionPoint.BeforeRendering, OnBeforeRendering);
		registry.AddRecordDelegate(RecordExtensionPoint.BeforeDrawDeferredDecals, OnBeforeDrawDeferredDecals);
		registry.AddRecordDelegate(RecordExtensionPoint.AfterDrawDeferredDecals, OnAfterDrawDeferredDecals);
	}

	private void OnSetup(in SetupContext context)
	{
		TerrainStampingManager.TryGetInstance(out var terrainStampingManager);
		ChunkUpdatePass.Setup(in context, terrainStampingManager, m_Asset.ManagerParameters);
	}

	private JobHandle OnScheduleSetupJobsDelegate(in SetupContext context, JobHandle dependency)
	{
		TerrainStampingManager.TryGetInstance(out var terrainStampingManager);
		if (BrushPass.TryScheduleSetupJobs(in context, terrainStampingManager, m_Asset.ManagerParameters, out var scheduledJobData))
		{
			m_BrushPassScheduledJobData = scheduledJobData;
			return scheduledJobData.JobHandle;
		}
		m_BrushPassScheduledJobData = null;
		return dependency;
	}

	private void OnBeforeRendering(in RecordContext context)
	{
		TerrainStampingManager.TryGetInstance(out var terrainStampingManager);
		BrushPass.BrushCullingJobData? brushPassScheduledJobData = m_BrushPassScheduledJobData;
		if (brushPassScheduledJobData.HasValue)
		{
			BrushPass.BrushCullingJobData valueOrDefault = brushPassScheduledJobData.GetValueOrDefault();
			m_BrushPassScheduledJobData = null;
			BrushPass.Record(in context, terrainStampingManager, m_Asset.ManagerParameters, valueOrDefault);
		}
		SetupForRenderPass.Record(in context, terrainStampingManager, m_Asset.ManagerParameters);
		BakeNormalsPass.Record(in context, terrainStampingManager, m_Asset.ManagerParameters);
	}

	private void OnBeforeDrawDeferredDecals(in RecordContext context)
	{
		if (m_Asset.ManagerParameters.DecalSubset == CustomDecalSubset.BeforeBuiltIn)
		{
			DrawDecalsPass.Record(in context, m_Asset.ManagerParameters, m_StencilMaskMaterial);
		}
	}

	private void OnAfterDrawDeferredDecals(in RecordContext context)
	{
		if (m_Asset.ManagerParameters.DecalSubset == CustomDecalSubset.AfterBuiltIn)
		{
			DrawDecalsPass.Record(in context, m_Asset.ManagerParameters, m_StencilMaskMaterial);
		}
	}
}
