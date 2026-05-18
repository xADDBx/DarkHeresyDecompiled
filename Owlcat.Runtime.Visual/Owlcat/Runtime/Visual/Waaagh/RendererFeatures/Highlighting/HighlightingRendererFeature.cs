using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.Highlighting;
using Owlcat.Runtime.Visual.Utilities;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.Highlighting;

internal sealed class HighlightingRendererFeature : IRendererFeature, IDisposable
{
	private static readonly Plane[] s_CameraPlanesTemp = new Plane[6];

	private readonly HighlightingRendererFeatureAsset m_Asset;

	private readonly List<RendererInfo> m_RendererInfos = new List<RendererInfo>();

	private readonly Materials m_Materials;

	private NativeArray<Bounds> m_Bounds;

	private NativeArray<TestPlanesResults> m_BoundsVisibility;

	private NativeArray<Plane> m_CameraPlanes;

	private NativeReference<int> m_Count;

	public HighlightingRendererFeature(HighlightingRendererFeatureAsset asset)
	{
		m_Asset = asset;
		HighlightingFeatureResources renderPipelineSettings = GraphicsSettings.GetRenderPipelineSettings<HighlightingFeatureResources>();
		m_Materials.Highlighter = CoreUtils.CreateEngineMaterial(renderPipelineSettings.HighlighterShader);
		m_Materials.Blur = CoreUtils.CreateEngineMaterial(renderPipelineSettings.BlurShader);
		m_Materials.Cut = CoreUtils.CreateEngineMaterial(renderPipelineSettings.CutShader);
		m_Materials.Composite = CoreUtils.CreateEngineMaterial(renderPipelineSettings.CompositeShader);
	}

	public void Dispose()
	{
		m_Bounds.Dispose();
		m_BoundsVisibility.Dispose();
		m_CameraPlanes.Dispose();
		m_Count.Dispose();
		CoreUtils.Destroy(m_Materials.Highlighter);
		CoreUtils.Destroy(m_Materials.Blur);
		CoreUtils.Destroy(m_Materials.Cut);
		CoreUtils.Destroy(m_Materials.Composite);
	}

	public void RegisterExtensions(RendererFeatureExtensionRegistry registry)
	{
		registry.AddScheduleSetupJobsDelegate(OnScheduleSetupJobs);
		registry.AddRecordDelegate(RecordExtensionPoint.AfterDrawPostProcess, OnAfterDrawPostProcess);
	}

	private JobHandle OnScheduleSetupJobs(in SetupContext context, JobHandle dependency)
	{
		MultiHighlighter.UpdateInstances();
		Highlighter.UpdateInstances();
		m_RendererInfos.Clear();
		InvasiveLinkedList<Highlighter>.Enumerator enumerator = Highlighter.Instances.GetEnumerator();
		while (enumerator.MoveNext())
		{
			Highlighter current = enumerator.Current;
			List<Highlighter.RendererInfo> rendererInfos = current.GetRendererInfos();
			if (rendererInfos == null || rendererInfos.Count == 0)
			{
				continue;
			}
			foreach (Highlighter.RendererInfo item in rendererInfos)
			{
				if (!(item.renderer == null))
				{
					m_RendererInfos.Add(new RendererInfo
					{
						Highlighter = current,
						Renderer = item.renderer,
						ExpectedMaterialsCount = item.expectedMaterialsCount
					});
				}
			}
		}
		if (!m_Bounds.IsCreated || m_Bounds.Length < m_RendererInfos.Count)
		{
			m_Bounds.Dispose();
			m_BoundsVisibility.Dispose();
			int length = (int)((float)m_RendererInfos.Count * 1.5f);
			m_Bounds = new NativeArray<Bounds>(length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			m_BoundsVisibility = new NativeArray<TestPlanesResults>(length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		}
		if (!m_CameraPlanes.IsCreated)
		{
			m_CameraPlanes = new NativeArray<Plane>(6, Allocator.Persistent);
		}
		if (!m_Count.IsCreated)
		{
			m_Count = new NativeReference<int>(0, Allocator.Persistent);
		}
		for (int i = 0; i < m_RendererInfos.Count; i++)
		{
			m_Bounds[i] = m_RendererInfos[i].Renderer.bounds;
			m_BoundsVisibility[i] = TestPlanesResults.Outside;
		}
		GeometryUtility.CalculateFrustumPlanes(context.CameraData.camera, s_CameraPlanesTemp);
		m_CameraPlanes.CopyFrom(s_CameraPlanesTemp);
		CullingJob cullingJob = default(CullingJob);
		cullingJob.Bounds = m_Bounds;
		cullingJob.BoundsVisibility = m_BoundsVisibility;
		cullingJob.CameraPlanes = m_CameraPlanes;
		CullingJob jobData = cullingJob;
		CountJob countJob = default(CountJob);
		countJob.BoundsVisibility = m_BoundsVisibility.Slice(0, m_RendererInfos.Count);
		countJob.Count = m_Count;
		CountJob jobData2 = countJob;
		JobHandle dependsOn = IJobParallelForExtensions.Schedule(jobData, m_RendererInfos.Count, 32, dependency);
		return jobData2.Schedule(dependsOn);
	}

	private void OnAfterDrawPostProcess(in RecordContext context)
	{
		if (m_Count.Value > 0)
		{
			Settings settings = m_Asset.GetSettings();
			HighlightingPass.Record(in context, in settings, in m_Materials, m_RendererInfos, m_BoundsVisibility.AsReadOnlySpan().Slice(0, m_RendererInfos.Count));
		}
	}
}
