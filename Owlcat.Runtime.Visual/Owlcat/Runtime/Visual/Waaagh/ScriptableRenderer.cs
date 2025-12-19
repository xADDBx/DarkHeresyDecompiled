using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Owlcat.Runtime.Visual.Waaagh.Data;
using Owlcat.Runtime.Visual.Waaagh.Debugging;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.History;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using Owlcat.Runtime.Visual.Waaagh.Passes.Base;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VolumetricLighting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh;

public abstract class ScriptableRenderer : IDisposable
{
	private List<ScriptableRenderPass> m_ActiveRenderPassQueue = new List<ScriptableRenderPass>(32);

	private List<ScriptableRendererFeature> m_RendererFeatures = new List<ScriptableRendererFeature>(10);

	private List<RendererList> m_ActiveRendererLists = new List<RendererList>(32);

	private BasePasses m_BasePasses;

	private ContextContainer m_FrameData = new ContextContainer();

	private RTHandle m_CurrentColorBuffer;

	private RTHandle m_CurrentDepthBuffer;

	private static Dictionary<int, ProfilingSampler> s_HashSamplerCache = new Dictionary<int, ProfilingSampler>();

	private static readonly ProfilingSampler s_UnknownSampler = new ProfilingSampler("Unknown");

	internal static ScriptableRenderer Current = null;

	public List<ScriptableRenderPass> ActiveRenderPassQueue => m_ActiveRenderPassQueue;

	public List<ScriptableRendererFeature> RendererFeatures => m_RendererFeatures;

	[CanBeNull]
	public DebugHandler DebugHandler { get; }

	public ContextContainer FrameData => m_FrameData;

	public bool IsVolumetricLightingEnabled
	{
		get
		{
			foreach (ScriptableRendererFeature rendererFeature in m_RendererFeatures)
			{
				if (rendererFeature is VolumetricLightingFeature)
				{
					return true;
				}
			}
			return false;
		}
	}

	public RTHandle CurrentColorBuffer => m_CurrentColorBuffer;

	public RTHandle CurrentDepthBuffer => m_CurrentDepthBuffer;

	public ScriptableRenderer(ScriptableRendererData data)
	{
		if (Debug.isDebugBuild)
		{
			DebugHandler = new DebugHandler(data, this);
		}
		foreach (ScriptableRendererFeature rendererFeature in data.RendererFeatures)
		{
			if (!(rendererFeature == null))
			{
				rendererFeature.Create();
				m_RendererFeatures.Add(rendererFeature);
			}
		}
		m_ActiveRenderPassQueue.Clear();
	}

	public static ProfilingSampler TryGetOrAddCameraSampler(Camera camera)
	{
		ProfilingSampler value = null;
		int hashCode = camera.GetHashCode();
		if (!s_HashSamplerCache.TryGetValue(hashCode, out value))
		{
			value = new ProfilingSampler(camera.name ?? "");
			s_HashSamplerCache.Add(hashCode, value);
		}
		return value;
	}

	public void Dispose()
	{
		for (int i = 0; i < m_RendererFeatures.Count; i++)
		{
			if (!(m_RendererFeatures[i] == null))
			{
				m_RendererFeatures[i].Dispose();
			}
		}
		if (Debug.isDebugBuild)
		{
			DebugHandler?.Dispose();
		}
		m_FrameData.Dispose();
		if (m_CurrentColorBuffer != null)
		{
			RTHandles.Release(m_CurrentColorBuffer);
			m_CurrentColorBuffer = null;
		}
		if (m_CurrentDepthBuffer != null)
		{
			RTHandles.Release(m_CurrentDepthBuffer);
			m_CurrentDepthBuffer = null;
		}
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
	}

	public virtual void SetupCullingParameters(ref ScriptableCullingParameters cullingParameters, WaaaghCameraData cameraData)
	{
	}

	public void EnqueuePass(ScriptableRenderPass pass)
	{
		m_ActiveRenderPassQueue.Add(pass);
	}

	private void SortStable(List<ScriptableRenderPass> list)
	{
		for (int i = 1; i < list.Count; i++)
		{
			ScriptableRenderPass scriptableRenderPass = list[i];
			int num = i - 1;
			while (num >= 0 && scriptableRenderPass < list[num])
			{
				list[num + 1] = list[num];
				num--;
			}
			list[num + 1] = scriptableRenderPass;
		}
	}

	internal void SetupInternal(ScriptableRenderContext context, ContextContainer frameData)
	{
		using (new ProfilingScope(ProfilingSampler.Get(WaaaghProfileId.RendererSetupBasePasses)))
		{
			SetupBasePasses(frameData);
		}
		using (new ProfilingScope(ProfilingSampler.Get(WaaaghProfileId.RendererSetup)))
		{
			Setup(context, frameData);
		}
		using (new ProfilingScope(ProfilingSampler.Get(WaaaghProfileId.RendererConfigureRendererLists)))
		{
			ConfigureRendererLists(context, frameData);
		}
	}

	private void SetupBasePasses(ContextContainer frameData)
	{
		if (m_BasePasses == null)
		{
			m_BasePasses = new BasePasses();
		}
		m_BasePasses.Setup(this, frameData);
	}

	protected abstract void Setup(ScriptableRenderContext context, ContextContainer frameData);

	private void ConfigureRendererLists(ScriptableRenderContext context, ContextContainer frameData)
	{
		using (new ProfilingScope(ProfilingSampler.Get(WaaaghProfileId.RendererInitSharedRendererLists)))
		{
			InitSharedRendererLists(context, frameData);
		}
		using (new ProfilingScope(ProfilingSampler.Get(WaaaghProfileId.RendererConfigurePassesRendererLists)))
		{
			foreach (ScriptableRenderPass item in m_ActiveRenderPassQueue)
			{
				item.ConfigureRendererLists(context, frameData);
				m_ActiveRendererLists.AddRange(item.UsedRendererLists);
			}
		}
		using (new ProfilingScope(ProfilingSampler.Get(WaaaghProfileId.RendererPrepareRendererListsAsync)))
		{
			context.PrepareRendererListsAsync(m_ActiveRendererLists);
		}
	}

	private void InitSharedRendererLists(ScriptableRenderContext context, ContextContainer frameData)
	{
		WaaaghRendererListData waaaghRendererListData = frameData.Get<WaaaghRendererListData>();
		WaaaghCameraData cameraData = frameData.Get<WaaaghCameraData>();
		WaaaghRenderingData renderingData = frameData.Get<WaaaghRenderingData>();
		waaaghRendererListData.Init(context, renderingData, cameraData);
	}

	public void Execute(ScriptableRenderContext context, ContextContainer frameData)
	{
		WaaaghRenderingData waaaghRenderingData = frameData.Get<WaaaghRenderingData>();
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		ref TimeData timeData = ref waaaghRenderingData.TimeData;
		Camera camera = waaaghCameraData.camera;
		RenderGraph renderGraph = waaaghRenderingData.RenderGraph;
		CommandBuffer commandBuffer = CommandBufferPool.Get();
		using (new ProfilingScope(ProfilingSampler.Get(WaaaghProfileId.RendererSortRenderPasses)))
		{
			SortStable(m_ActiveRenderPassQueue);
		}
		using (new ProfilingScope(ProfilingSampler.Get(WaaaghProfileId.RendererRecordRenderGraph)))
		{
			using (new ProfilingScope(ProfilingSampler.Get(WaaaghProfileId.RenderGraphBeginRecording)))
			{
				RenderGraphParameters renderGraphParameters = default(RenderGraphParameters);
				renderGraphParameters.commandBuffer = commandBuffer;
				renderGraphParameters.currentFrameIndex = timeData.FrameId;
				renderGraphParameters.executionName = GetExecutionName(waaaghCameraData);
				renderGraphParameters.rendererListCulling = false;
				renderGraphParameters.scriptableRenderContext = context;
				RenderGraphParameters parameters = renderGraphParameters;
				renderGraph.BeginRecording(in parameters);
			}
			renderGraph.BeginProfilingSampler(TryGetOrAddCameraSampler(camera), ".\\Library\\PackageCache\\com.owlcat.visual@2c5e70bf14b9\\Runtime\\Waaagh\\ScriptableRenderer.cs", 301);
			OnBeginRenderGraphFrame(frameData);
			using (new ProfilingScope(ProfilingSampler.Get(WaaaghProfileId.RendererCreateResources)))
			{
				CreateRenderGraphResources(frameData);
			}
			using (new ProfilingScope(ProfilingSampler.Get(WaaaghProfileId.RendererRecordPasses)))
			{
				foreach (ScriptableRenderPass item in m_ActiveRenderPassQueue)
				{
					using (new ProfilingScope(item.RecordProfilingSampler))
					{
						if (item.HasAnyCustomDependencyThatPreventsPassCulling(context, frameData) || !item.AreRendererListsEmpty(context))
						{
							item.RecordRenderGraph(frameData);
						}
						item.ClearRendererLists();
					}
				}
				m_ActiveRenderPassQueue.Clear();
			}
			OnEndRenderGraphFrame(context, frameData);
			renderGraph.EndProfilingSampler(TryGetOrAddCameraSampler(camera), ".\\Library\\PackageCache\\com.owlcat.visual@2c5e70bf14b9\\Runtime\\Waaagh\\ScriptableRenderer.cs", 333);
		}
		using (new ProfilingScope(ProfilingSampler.Get(WaaaghProfileId.RendererExecuteRenderGraph)))
		{
			renderGraph.EndRecordingAndExecute();
			context.ExecuteCommandBuffer(commandBuffer);
			CommandBufferPool.Release(commandBuffer);
		}
		m_ActiveRendererLists.Clear();
	}

	private void OnBeginRenderGraphFrame(ContextContainer frameData)
	{
		frameData.Get<WaaaghResourceData>().InitFrame();
	}

	private void OnEndRenderGraphFrame(ScriptableRenderContext context, ContextContainer frameData)
	{
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		frameData.Get<WaaaghRendererListData>();
		waaaghResourceData.EndFrame();
	}

	private void CreateRenderGraphResources(ContextContainer frameData)
	{
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		frameData.Get<WaaaghRendererListData>();
		WaaaghCameraData cameraData = frameData.Get<WaaaghCameraData>();
		WaaaghRenderingData waaaghRenderingData = frameData.Get<WaaaghRenderingData>();
		RenderGraph renderGraph = waaaghRenderingData.RenderGraph;
		WaaaghShadowData shadowData = frameData.Get<WaaaghShadowData>();
		ImportBackbuffer(cameraData);
		waaaghResourceData.ImportCameraData(renderGraph, cameraData);
		waaaghResourceData.ImportShadowData(renderGraph, shadowData);
		waaaghResourceData.ImportVTResources(renderGraph, waaaghRenderingData.VirtualTextureManager);
		UpdateCameraHistory(cameraData);
		waaaghResourceData.CreateGBuffer(renderGraph, cameraData);
		CreateResources(frameData);
	}

	private void ImportBackbuffer(WaaaghCameraData cameraData)
	{
		RenderTargetIdentifier renderTargetIdentifier = BuiltinRenderTextureType.CameraTarget;
		RenderTargetIdentifier renderTargetIdentifier2 = BuiltinRenderTextureType.CameraTarget;
		if (cameraData.targetTexture != null)
		{
			renderTargetIdentifier = new RenderTargetIdentifier(cameraData.targetTexture);
		}
		if (cameraData.TargetDepthTexture != null)
		{
			renderTargetIdentifier2 = new RenderTargetIdentifier(cameraData.TargetDepthTexture);
		}
		else if (cameraData.targetTexture != null)
		{
			renderTargetIdentifier2 = new RenderTargetIdentifier(cameraData.targetTexture);
		}
		if (m_CurrentColorBuffer == null)
		{
			m_CurrentColorBuffer = RTHandles.Alloc(renderTargetIdentifier);
		}
		else
		{
			RTHandleStaticHelpers.SetRTHandleUserManagedWrapper(ref m_CurrentColorBuffer, renderTargetIdentifier);
		}
		if (m_CurrentDepthBuffer == null)
		{
			m_CurrentDepthBuffer = RTHandles.Alloc(renderTargetIdentifier2);
		}
		else
		{
			RTHandleStaticHelpers.SetRTHandleUserManagedWrapper(ref m_CurrentDepthBuffer, renderTargetIdentifier2);
		}
	}

	protected abstract void CreateResources(ContextContainer frameData);

	private void UpdateCameraHistory(WaaaghCameraData cameraData)
	{
		if (cameraData != null && cameraData.historyManager != null)
		{
			int num = 0;
			if (0 == 0 || num == 0)
			{
				WaaaghCameraHistory historyManager = cameraData.historyManager;
				historyManager.GatherHistoryRequests();
				historyManager.ReleaseUnusedHistory();
				historyManager.SwapAndSetReferenceSize(cameraData.cameraTargetDescriptor.width, cameraData.cameraTargetDescriptor.height);
			}
		}
	}

	private bool IsSceneFilteringEnabled(Camera camera)
	{
		return false;
	}

	private string GetExecutionName(WaaaghCameraData cameraData)
	{
		switch (cameraData.cameraType)
		{
		case CameraType.Game:
			if (cameraData.renderType == CameraRenderType.Base)
			{
				return "GameBase";
			}
			return "GameOverlay";
		case CameraType.SceneView:
			return "SceneView";
		case CameraType.Preview:
			return "Preview";
		case CameraType.VR:
			return "VR";
		case CameraType.Reflection:
			return "Reflection";
		default:
			return "Unknown";
		}
	}

	protected internal virtual bool SupportsMotionVectors()
	{
		return false;
	}
}
