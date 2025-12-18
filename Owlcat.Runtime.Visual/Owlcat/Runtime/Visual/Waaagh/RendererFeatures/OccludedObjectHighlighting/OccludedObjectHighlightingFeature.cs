using System.Collections.Generic;
using Owlcat.Runtime.Visual.OccludedObjectHighlighting;
using Owlcat.Runtime.Visual.Utilities;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.Highlighting;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.OccludedObjectHighlighting.Passes;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.OccludedObjectHighlighting;

[CreateAssetMenu(menuName = "Renderer Features/Waaagh/OccludedObjectHighlighting")]
public class OccludedObjectHighlightingFeature : ScriptableRendererFeature
{
	internal struct RendererInfo
	{
		public OccludedObjectHighlighter highlighter;

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

	private OccludedObjectHighlightingFeatureResources m_Resources;

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
	private float m_ScanLineFreq0 = 1000f;

	[SerializeField]
	private float m_ScanLineFreq1 = 1500f;

	[SerializeField]
	private float m_ScanLineSpeed = 10f;

	[SerializeField]
	[Range(0f, 1f)]
	private float m_ScanLineOpacity = 0.5f;

	private Plane[] m_CameraPlanesTemp = new Plane[6];

	private List<RendererInfo> m_RendererInfos = new List<RendererInfo>(64);

	private int m_CurrentCount;

	private NativeArray<Plane> m_CameraPlanes;

	private NativeArray<BoundsVisibility> m_Bounds;

	private NativeReference<int> m_Count;

	private JobHandle m_JobHandle;

	private OccludedObjectHighlighterPass m_HighlighterPass;

	private Material m_HighlighterMaterial;

	private Material m_BlurMaterial;

	private Material m_CompositeMaterial;

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

	internal List<RendererInfo> RendererInfos => m_RendererInfos;

	public float ScanLineFreq0 => m_ScanLineFreq0;

	public float ScanLineFreq1 => m_ScanLineFreq1;

	public float ScanLineSpeed => m_ScanLineSpeed;

	public float ScanLineOpacity => m_ScanLineOpacity;

	public OccludedObjectHighlightingFeatureResources Resources => m_Resources;

	public override void Create()
	{
		m_Resources = GraphicsSettings.GetRenderPipelineSettings<OccludedObjectHighlightingFeatureResources>();
		if (!m_CameraPlanes.IsCreated)
		{
			m_CameraPlanes = new NativeArray<Plane>(6, Allocator.Persistent);
		}
		m_HighlighterMaterial = CoreUtils.CreateEngineMaterial(m_Resources.HighlighterShader);
		m_BlurMaterial = CoreUtils.CreateEngineMaterial(m_Resources.BlurShader);
		m_CompositeMaterial = CoreUtils.CreateEngineMaterial(m_Resources.CompositeShader);
		m_HighlighterPass = new OccludedObjectHighlighterPass(RenderPassEvent.AfterRenderingPostProcessing, this, m_HighlighterMaterial, m_BlurMaterial, m_CompositeMaterial);
	}

	internal bool IsRendererVisible(int index)
	{
		return m_Bounds[index].Visibility != TestPlanesResults.Outside;
	}

	protected override void Dispose(bool disposing)
	{
		if (m_CameraPlanes.IsCreated)
		{
			m_CameraPlanes.Dispose();
		}
		if (m_Bounds.IsCreated)
		{
			m_Bounds.Dispose();
		}
		if (m_Count.IsCreated)
		{
			m_Count.Dispose();
		}
		CoreUtils.Destroy(m_HighlighterMaterial);
		CoreUtils.Destroy(m_BlurMaterial);
		CoreUtils.Destroy(m_CompositeMaterial);
	}

	internal override void StartSetupJobs(ContextContainer frameData)
	{
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
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
						highlighter = current,
						renderer = rendererInfo.renderer,
						expectedMaterialsCount = rendererInfo.expectedMaterialsCount
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
		m_JobHandle = IJobParallelForExtensions.Schedule(jobData, RendererInfos.Count, 32);
		CountJob jobData2 = default(CountJob);
		jobData2.Bounds = m_Bounds.Slice(0, RendererInfos.Count);
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
}
