using System.Collections.Generic;
using Owlcat.Runtime.Visual.Highlighting;
using Owlcat.Runtime.Visual.Utilities;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.Highlighting.Passes;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.Highlighting;

[CreateAssetMenu(menuName = "Renderer Features/Waaagh/Highlighting")]
public class HighlightingFeature : ScriptableRendererFeature
{
	public struct RendererInfo
	{
		public Highlighter highlighter;

		public Renderer renderer;

		public int expectedMaterialsCount;
	}

	public enum Downsample
	{
		None = 1,
		Half = 2,
		Quarter = 4
	}

	public enum BlurDirections
	{
		Diagonal,
		Straight,
		All
	}

	public enum ZTestMode
	{
		None,
		SceneBuffer,
		EmptyBuffer
	}

	[SerializeField]
	private Downsample m_DownsampleFactor = Downsample.None;

	[SerializeField]
	[Range(0f, 50f)]
	private int m_BlurIterations = 2;

	[SerializeField]
	[Range(0f, 3f)]
	private float m_BlurMinSpread = 0.65f;

	[SerializeField]
	[Range(0f, 3f)]
	private float m_BlurSpread = 0.25f;

	[SerializeField]
	private BlurDirections m_BlurDirections;

	[SerializeField]
	private ZTestMode m_ZTestMode;

	private HighlightingFeatureResources m_Resources;

	private NativeArray<BoundsVisibility> m_Bounds;

	private NativeArray<Plane> m_CameraPlanes;

	private NativeReference<int> m_Count;

	private List<RendererInfo> m_RendererInfos = new List<RendererInfo>(64);

	private Plane[] m_CameraPlanesTemp = new Plane[6];

	private JobHandle m_JobHandle;

	private int m_CurrentCount;

	private Material m_HighlighterMaterial;

	private Material m_BlurMaterial;

	private Material m_CutMaterial;

	private Material m_CompositeMaterial;

	private HighlighterPass m_HighlighterPass;

	public Downsample DownsampleFactor
	{
		get
		{
			return m_DownsampleFactor;
		}
		set
		{
			m_DownsampleFactor = value;
		}
	}

	public int BlurIterations
	{
		get
		{
			return m_BlurIterations;
		}
		set
		{
			m_BlurIterations = Mathf.Clamp(value, 0, 50);
		}
	}

	public float BlurMinSpread
	{
		get
		{
			return m_BlurMinSpread;
		}
		set
		{
			m_BlurMinSpread = Mathf.Clamp(value, 0f, 3f);
		}
	}

	public float BlurSpread
	{
		get
		{
			return m_BlurSpread;
		}
		set
		{
			m_BlurSpread = Mathf.Clamp(value, 0f, 3f);
		}
	}

	public BlurDirections BlurDirectons
	{
		get
		{
			return m_BlurDirections;
		}
		set
		{
			m_BlurDirections = value;
		}
	}

	public ZTestMode ZTest
	{
		get
		{
			return m_ZTestMode;
		}
		set
		{
			m_ZTestMode = value;
		}
	}

	internal List<RendererInfo> RendererInfos => m_RendererInfos;

	public HighlightingFeatureResources Resources => m_Resources;

	public bool IsRendererVisible(int index)
	{
		return m_Bounds[index].Visibility != TestPlanesResults.Outside;
	}

	internal override void StartSetupJobs(ContextContainer frameData)
	{
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
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
						highlighter = current,
						renderer = item.renderer,
						expectedMaterialsCount = item.expectedMaterialsCount
					});
				}
			}
		}
		if (!m_Bounds.IsCreated || m_Bounds.Length < m_RendererInfos.Count)
		{
			if (m_Bounds.IsCreated)
			{
				m_Bounds.Dispose();
			}
			m_Bounds = new NativeArray<BoundsVisibility>((int)((float)m_RendererInfos.Count * 1.5f), Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
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
			m_Bounds[i] = new BoundsVisibility
			{
				Bounds = m_RendererInfos[i].renderer.bounds,
				Visibility = TestPlanesResults.Outside
			};
		}
		GeometryUtility.CalculateFrustumPlanes(waaaghCameraData.camera, m_CameraPlanesTemp);
		m_CameraPlanes.CopyFrom(m_CameraPlanesTemp);
		CullingJob jobData = default(CullingJob);
		jobData.Bounds = m_Bounds;
		jobData.CameraPlanes = m_CameraPlanes;
		m_JobHandle = IJobParallelForExtensions.Schedule(jobData, m_RendererInfos.Count, 32);
		CountJob jobData2 = default(CountJob);
		jobData2.Bounds = m_Bounds.Slice(0, m_RendererInfos.Count);
		jobData2.Count = m_Count;
		m_JobHandle = jobData2.Schedule(m_JobHandle);
	}

	internal override void CompleteSetupJobs()
	{
		m_JobHandle.Complete();
		m_CurrentCount = m_Count.Value;
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ContextContainer frameData)
	{
		if (m_CurrentCount > 0)
		{
			renderer.EnqueuePass(m_HighlighterPass);
		}
	}

	public override void Create()
	{
		m_Resources = GraphicsSettings.GetRenderPipelineSettings<HighlightingFeatureResources>();
		m_HighlighterMaterial = CoreUtils.CreateEngineMaterial(m_Resources.HighlighterShader);
		m_BlurMaterial = CoreUtils.CreateEngineMaterial(m_Resources.BlurShader);
		m_CutMaterial = CoreUtils.CreateEngineMaterial(m_Resources.CutShader);
		m_CompositeMaterial = CoreUtils.CreateEngineMaterial(m_Resources.CompositeShader);
		m_HighlighterPass = new HighlighterPass(RenderPassEvent.AfterRenderingPostProcessing, this, m_HighlighterMaterial, m_BlurMaterial, m_CutMaterial, m_CompositeMaterial);
	}

	protected override void Dispose(bool disposing)
	{
		if (m_Bounds.IsCreated)
		{
			m_Bounds.Dispose();
		}
		if (m_Count.IsCreated)
		{
			m_Count.Dispose();
		}
		if (m_CameraPlanes.IsCreated)
		{
			m_CameraPlanes.Dispose();
		}
		CoreUtils.Destroy(m_HighlighterMaterial);
		CoreUtils.Destroy(m_BlurMaterial);
		CoreUtils.Destroy(m_CutMaterial);
		CoreUtils.Destroy(m_CompositeMaterial);
	}
}
