using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.OccludedObjectHighlighting;
using Owlcat.Runtime.Visual.Utilities;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.Highlighting;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.OccludedObjectHighlighting;

internal sealed class OccludedObjectHighlightingRendererFeature : IRendererFeature, IDisposable
{
	private static readonly Plane[] s_CameraPlanesTemp = new Plane[6];

	private readonly OccludedObjectHighlightingRendererFeatureAsset m_Asset;

	private readonly List<RendererInfo> m_RendererInfos = new List<RendererInfo>();

	private readonly Materials m_Materials;

	private NativeArray<Plane> m_CameraPlanes;

	private NativeArray<Bounds> m_Bounds;

	private NativeArray<TestPlanesResults> m_BoundsVisibility;

	private NativeReference<int> m_Count;

	public OccludedObjectHighlightingRendererFeature(OccludedObjectHighlightingRendererFeatureAsset asset)
	{
		m_Asset = asset;
		OccludedObjectHighlightingFeatureResources renderPipelineSettings = GraphicsSettings.GetRenderPipelineSettings<OccludedObjectHighlightingFeatureResources>();
		m_Materials.Highlighter = CoreUtils.CreateEngineMaterial(renderPipelineSettings.HighlighterShader);
		m_Materials.Blur = CoreUtils.CreateEngineMaterial(renderPipelineSettings.BlurShader);
		m_Materials.Composite = CoreUtils.CreateEngineMaterial(renderPipelineSettings.CompositeShader);
		m_CameraPlanes = new NativeArray<Plane>(6, Allocator.Persistent);
		m_Count = new NativeReference<int>(0, Allocator.Persistent);
	}

	public void Dispose()
	{
		m_CameraPlanes.Dispose();
		m_Bounds.Dispose();
		m_BoundsVisibility.Dispose();
		m_Count.Dispose();
		CoreUtils.Destroy(m_Materials.Highlighter);
		CoreUtils.Destroy(m_Materials.Blur);
		CoreUtils.Destroy(m_Materials.Composite);
	}

	public void RegisterExtensions(RendererFeatureExtensionRegistry registry)
	{
		registry.AddScheduleSetupJobsDelegate(OnScheduleSetupJobs);
		registry.AddRecordDelegate(RecordExtensionPoint.AfterDrawPostProcess, OnAfterDrawPostProcess);
	}

	private JobHandle OnScheduleSetupJobs(in SetupContext context, JobHandle dependency)
	{
		m_RendererInfos.Clear();
		InvasiveLinkedList<OccludedObjectHighlighter>.Enumerator enumerator = OccludedObjectHighlighter.Instances.GetEnumerator();
		while (enumerator.MoveNext())
		{
			OccludedObjectHighlighter current = enumerator.Current;
			foreach (OccludedObjectHighlighter.RendererInfo rendererInfo in current.GetRendererInfos())
			{
				if (!(rendererInfo.renderer == null))
				{
					m_RendererInfos.Add(new RendererInfo
					{
						Highlighter = current,
						Renderer = rendererInfo.renderer,
						ExpectedMaterialsCount = rendererInfo.expectedMaterialsCount
					});
				}
			}
		}
		if (!m_Bounds.IsCreated || m_Bounds.Length < m_RendererInfos.Count)
		{
			m_Bounds.Dispose();
			m_BoundsVisibility.Dispose();
			int length = Mathf.NextPowerOfTwo(m_RendererInfos.Count);
			m_Bounds = new NativeArray<Bounds>(length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			m_BoundsVisibility = new NativeArray<TestPlanesResults>(length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
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
		JobHandle dependsOn = IJobParallelForExtensions.Schedule(jobData, m_RendererInfos.Count, 32);
		return jobData2.Schedule(dependsOn);
	}

	private void OnAfterDrawPostProcess(in RecordContext context)
	{
		if (m_Count.Value > 0)
		{
			Settings settings = m_Asset.GetSettings();
			OccludedObjectHighlighterPass.Record(in context, in settings, in m_Materials, m_RendererInfos, m_BoundsVisibility.AsReadOnlySpan().Slice(0, m_RendererInfos.Count));
		}
	}
}
