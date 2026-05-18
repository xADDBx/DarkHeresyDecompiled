using System;
using Unity.Jobs;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.TerrainBlending;

internal sealed class TerrainBlendingRendererFeature : IRendererFeature, IDisposable
{
	private readonly TerrainBlendingDecalDrawer m_DecalDrawer;

	public TerrainBlendingRendererFeature(TerrainBlendingRendererFeatureAsset asset)
	{
		m_DecalDrawer = new TerrainBlendingDecalDrawer(asset);
	}

	public void Dispose()
	{
	}

	public void RegisterExtensions(RendererFeatureExtensionRegistry registry)
	{
		registry.AddScheduleSetupJobsDelegate(OnScheduleSetupJobsDelegate);
		registry.AddCompleteSetupJobsDelegate(OnCompleteSetupJobsDelegate);
		registry.AddRecordDelegate(RecordExtensionPoint.BeforeDrawDeferredDecals, OnBeforeDrawDeferredDecals);
	}

	private JobHandle OnScheduleSetupJobsDelegate(in SetupContext context, JobHandle dependency)
	{
		if (SupportsCameraType(context.CameraData.cameraType))
		{
			return m_DecalDrawer.ScheduleCullingJob(context.CameraData.camera, dependency);
		}
		return dependency;
	}

	private void OnCompleteSetupJobsDelegate(in SetupContext context)
	{
		m_DecalDrawer.CompleteCullingJob();
	}

	private void OnBeforeDrawDeferredDecals(in RecordContext context)
	{
		if (SupportsCameraType(context.CameraData.cameraType))
		{
			m_DecalDrawer.Draw(in context);
		}
	}

	private static bool SupportsCameraType(CameraType cameraType)
	{
		if (cameraType != CameraType.Game)
		{
			return cameraType == CameraType.SceneView;
		}
		return true;
	}
}
