using System;
using System.Collections.Generic;
using Owlcat.Runtime.Core.ObjectTracking;
using Owlcat.Runtime.Visual.Effects.GlobalEffects;
using Owlcat.Runtime.Visual.GPUDrivenBRG;
using Owlcat.Runtime.Visual.GPUDrivenBRG.BakedLighting;
using Owlcat.Runtime.Visual.IndirectRendering;
using Owlcat.Runtime.Visual.Lighting;
using Owlcat.Runtime.Visual.OcclusionClipping;
using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.Utilities;
using Owlcat.Runtime.Visual.VirtualTexture;
using Owlcat.Runtime.Visual.Waaagh.Data;
using Owlcat.Runtime.Visual.Waaagh.Debugging;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.History;
using Owlcat.Runtime.Visual.Waaagh.PostProcess;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.FogOfWar;
using Owlcat.Runtime.Visual.Waaagh.Shadows;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.SceneManagement;

namespace Owlcat.Runtime.Visual.Waaagh;

public class WaaaghPipeline : UnityEngine.Rendering.RenderPipeline
{
	private readonly struct CameraRenderingScope : IDisposable
	{
		private static readonly ProfilingSampler beginCameraRenderingSampler = new ProfilingSampler("RenderPipeline.BeginCameraRendering");

		private static readonly ProfilingSampler endCameraRenderingSampler = new ProfilingSampler("RenderPipeline.EndCameraRendering");

		private readonly ScriptableRenderContext m_Context;

		private readonly Camera m_Camera;

		public CameraRenderingScope(ScriptableRenderContext context, Camera camera)
		{
			using (new ProfilingScope(beginCameraRenderingSampler))
			{
				m_Context = context;
				m_Camera = camera;
				UnityEngine.Rendering.RenderPipeline.BeginCameraRendering(context, camera);
			}
		}

		public void Dispose()
		{
			using (new ProfilingScope(endCameraRenderingSampler))
			{
				UnityEngine.Rendering.RenderPipeline.EndCameraRendering(m_Context, m_Camera);
			}
		}
	}

	private readonly struct ContextRenderingScope : IDisposable
	{
		private static readonly ProfilingSampler beginContextRenderingSampler = new ProfilingSampler("RenderPipeline.BeginContextRendering");

		private static readonly ProfilingSampler endContextRenderingSampler = new ProfilingSampler("RenderPipeline.EndContextRendering");

		private readonly ScriptableRenderContext m_Context;

		private readonly List<Camera> m_Cameras;

		public ContextRenderingScope(ScriptableRenderContext context, List<Camera> cameras)
		{
			m_Context = context;
			m_Cameras = cameras;
			using (new ProfilingScope(beginContextRenderingSampler))
			{
				UnityEngine.Rendering.RenderPipeline.BeginContextRendering(m_Context, m_Cameras);
			}
		}

		public void Dispose()
		{
			using (new ProfilingScope(endContextRenderingSampler))
			{
				UnityEngine.Rendering.RenderPipeline.EndContextRendering(m_Context, m_Cameras);
			}
		}
	}

	private const int k_DefaultRenderingLayerMask = 1;

	private const float kHexRatio = 1.7320508f;

	private const int kWrapTime = 14400;

	private const float kRenderScaleThreshold = 0.05f;

	public const string kRenderPipelineName = "OwlcatPipeline";

	public static Action<Camera> VolumeManagerUpdated;

	private Scene m_ActiveScene;

	private readonly Comparison<Camera> m_CameraComparison = (Camera camera1, Camera camera2) => (int)camera1.depth - (int)camera2.depth;

	private WaaaghDebugData m_DebugData;

	private WaaaghPipelineGlobalSettings m_GlobalSettings;

	private readonly LightCookieManager m_LightCookieManager;

	private readonly LocalVolumetricFogManager m_LocalVolumetricFogManager;

	private RenderGraph m_RenderGraph;

	private ShadowManager m_ShadowManager;

	private List<Camera> m_Cameras;

	private WaaaghCameraStack m_CameraStack;

	private bool m_ApvIsEnabled;

	public static WaaaghPipelineAsset Asset => GraphicsSettings.currentRenderPipeline as WaaaghPipelineAsset;

	public static float MinRenderScale => 0.1f;

	public static float MaxRenderScale => 2f;

	public static float MaxShadowBias => 10f;

	public VirtualTextureManager VirtualTextureManager { get; }

	public GPUDrivenBatchRendererGroup GPUDrivenBatchRendererGroup { get; }

	public GPUDrivenCommandQueue CommandQueue { get; }

	public GPUDrivenBufferUtils BufferUtils { get; }

	internal static int MaxVisibleReflectionProbes => 64;

	public static event Action<WaaaghPipeline> Created;

	public static event Action<WaaaghPipeline> Destroying;

	public WaaaghPipeline(WaaaghPipelineAsset asset)
	{
		m_GlobalSettings = WaaaghPipelineGlobalSettings.Instance;
		PlatformAutoDetect.Initialize();
		SetSupportedRenderingFeatures();
		Shader.globalRenderPipeline = "OwlcatPipeline";
		VolumeManager.instance.Initialize();
		Lightmapping.SetDelegate(LightmappingDelegate);
		RenderingUtils.ClearSystemInfoCache();
		m_RenderGraph = new RenderGraph("WaaaghGraph");
		m_ShadowManager = new ShadowManager(asset);
		DebugManager.instance.RefreshEditor();
		m_DebugData = asset.DebugData;
		if (m_DebugData != null)
		{
			m_DebugData.RegisterDebug(this);
		}
		RTHandles.Initialize(Screen.width, Screen.height);
		DebugManager.instance.enableRuntimeUI = false;
		RenderGraph renderGraph = m_RenderGraph;
		LightCookieManager.Settings settings = new LightCookieManager.Settings
		{
			atlasTextureResolution = asset.LightCookieSettings.Resolution,
			atlasTextureFormat = asset.LightCookieSettings.Format
		};
		m_LightCookieManager = new LightCookieManager(renderGraph, in settings);
		m_LocalVolumetricFogManager = new LocalVolumetricFogManager(asset.LocalVolumetricFogSettings, asset.RuntimeResources.Texture3DAtlasCS);
		QualitySettings.enableLODCrossFade = asset.DitheringSettings.EnableLODCrossFade;
		m_ApvIsEnabled = asset != null && asset.SupportProbeVolumes;
		SupportedRenderingFeatures.active.overridesLightProbeSystem = m_ApvIsEnabled;
		SupportedRenderingFeatures.active.skyOcclusion = m_ApvIsEnabled;
		if (m_ApvIsEnabled)
		{
			ProbeReferenceVolume instance = ProbeReferenceVolume.instance;
			ProbeVolumeSystemParameters parameters = new ProbeVolumeSystemParameters
			{
				memoryBudget = asset.ProbeVolumesSettings.MemoryBudget,
				blendingMemoryBudget = asset.ProbeVolumesSettings.BlendingMemoryBudget,
				shBands = asset.ProbeVolumesSettings.SHBands,
				supportGPUStreaming = asset.ProbeVolumesSettings.SupportGPUStreaming,
				supportDiskStreaming = asset.ProbeVolumesSettings.SupportDiskStreaming,
				supportScenarios = asset.ProbeVolumesSettings.SupportScenarios,
				supportScenarioBlending = asset.ProbeVolumesSettings.SupportScenarioBlending
			};
			instance.Initialize(in parameters);
		}
		VirtualTexturePhysicalAtlasOverrides physicalAtlasOverrides = VirtualTexturePhysicalAtlasOverrides.Default;
		GPUDrivenBRGDebug debugData = null;
		CommandQueue = new GPUDrivenCommandQueue();
		BufferUtils = new GPUDrivenBufferUtils(asset.RuntimeResources);
		GPUDrivenBatchRendererGroup = new GPUDrivenBatchRendererGroup(asset.GPUDrivenBRGSettings, asset.RuntimeResources, CommandQueue, BufferUtils, debugData, m_DebugData);
		GPUDrivenInstanceCommandQueue.Init(GPUDrivenBatchRendererGroup);
		GPUDrivenMaterialOverrides.Init(GPUDrivenBatchRendererGroup);
		GPUDrivenLightmapping.Init();
		VirtualTextureManager = new VirtualTextureManager(asset.VirtualTextureSettings, asset.RuntimeResources, in physicalAtlasOverrides, GPUDrivenBatchRendererGroup);
		m_CameraStack = new WaaaghCameraStack();
		ShaderGlobalKeywords.Initialize();
		WaaaghPipeline.Created?.Invoke(this);
		SceneManager.sceneUnloaded += OnSceneUnloaded;
	}

	protected override void Dispose(bool disposing)
	{
		WaaaghPipeline.Destroying?.Invoke(this);
		SceneManager.sceneUnloaded -= OnSceneUnloaded;
		if (m_DebugData != null)
		{
			m_DebugData.UnregisterDebug();
			m_DebugData = null;
		}
		if (m_ApvIsEnabled)
		{
			ProbeReferenceVolume.instance.Cleanup();
		}
		Blitter.Cleanup();
		base.Dispose(disposing);
		Shader.globalRenderPipeline = "";
		SupportedRenderingFeatures.active = new SupportedRenderingFeatures();
		if (VolumeManager.instance.isInitialized)
		{
			VolumeManager.instance.Deinitialize();
		}
		Lightmapping.ResetDelegate();
		m_RenderGraph.Cleanup();
		m_RenderGraph = null;
		m_ShadowManager.Dispose();
		m_ShadowManager = null;
		ConstantBuffer.ReleaseAll();
		m_LightCookieManager.Dispose();
		m_LocalVolumetricFogManager.ReleaseAtlas();
		VirtualTextureManager.Dispose();
		GPUDrivenBatchRendererGroup.Dispose();
		GPUDrivenInstanceCommandQueue.Cleanup();
		GPUDrivenMaterialOverrides.Cleanup();
		GPUDrivenLightmapping.Cleanup();
		CommandQueue?.Dispose();
		m_CameraStack.Dispose();
		WaaaghCameraHistoryManager.DisposeAll();
	}

	protected override void Render(ScriptableRenderContext context, Camera[] cameras)
	{
		if (m_Cameras == null)
		{
			m_Cameras = new List<Camera>();
		}
		m_Cameras.Clear();
		m_Cameras.AddRange(cameras);
		Render(context, m_Cameras);
	}

	protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
	{
		RenderUsingContextContainer(context, cameras);
	}

	private static void OnSceneUnloaded(Scene scene)
	{
		ObjectDispatcherService.ProcessUpdates();
	}

	private void RenderUsingContextContainer(ScriptableRenderContext context, List<Camera> cameras)
	{
		using (new ProfilingScope(ProfilingSampler.Get(WaaaghProfileId.Prepare)))
		{
			WaaaghCameraHistoryManager.GC();
			AdjustUIOverlayOwnership(cameras.Count);
		}
		using (new ContextRenderingScope(context, cameras))
		{
			using (new ProfilingScope(ProfilingSampler.Get(WaaaghProfileId.Prepare)))
			{
				UpdateFrameCount(cameras);
				GraphicsSettings.lightsUseLinearIntensity = QualitySettings.activeColorSpace == ColorSpace.Linear;
				GraphicsSettings.lightsUseColorTemperature = true;
				GraphicsSettings.useScriptableRenderPipelineBatching = Asset.UseSRPBatcher;
				SetupPerFrameShaderConstants();
				SortCameras(cameras);
			}
			using (new ProfilingScope(ProfilingSampler.Get(WaaaghProfileId.PreRender)))
			{
				CommandBuffer commandBuffer = CommandBufferPool.Get();
				commandBuffer.BeginSample("PreRender");
				IndirectRenderingSystem.Instance.PreRender();
				VirtualTextureManager.PreRender(commandBuffer, cameras);
				GPUDrivenInstanceCommandQueue.Flush();
				GPUDrivenMaterialOverrides.Flush();
				if (GPUDrivenBatchRendererGroup.IsEnabledAndInitialized)
				{
					GPUDrivenBatchRendererGroup.PreRender(commandBuffer);
				}
				commandBuffer.EndSample("PreRender");
				context.ExecuteCommandBuffer(commandBuffer);
				CommandBufferPool.Release(commandBuffer);
				CommandQueue.Flush(context);
			}
			foreach (Camera camera in cameras)
			{
				if (!(camera == null) && m_CameraStack.Build(camera))
				{
					RenderCameraStack(context, m_CameraStack);
				}
			}
			m_RenderGraph.EndFrame();
		}
		using (new ProfilingScope(ProfilingSampler.Get(WaaaghProfileId.PostRender)))
		{
			CommandBuffer commandBuffer2 = CommandBufferPool.Get();
			commandBuffer2.BeginSample("PostRender");
			if (GPUDrivenBatchRendererGroup.IsEnabledAndInitialized)
			{
				GPUDrivenBatchRendererGroup.PostRender();
			}
			CommandQueue.PostRender();
			VirtualTextureManager.PostRender(commandBuffer2, cameras);
			if (cameras.Count == 0)
			{
				commandBuffer2.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
			}
			commandBuffer2.EndSample("PostRender");
			using (new ProfilingScope(ProfilingSampler.Get(WaaaghProfileId.PostRenderSubmit)))
			{
				context.ExecuteCommandBuffer(commandBuffer2);
				context.Submit();
			}
			CommandBufferPool.Release(commandBuffer2);
		}
	}

	private void RenderCameraStack(ScriptableRenderContext context, WaaaghCameraStack cameraStack)
	{
		using (new ProfilingScope(ProfilingSampler.Get(WaaaghProfileId.RenderCameraStack)))
		{
			for (int i = 0; i < cameraStack.AdditionalCameraDataList.Count; i++)
			{
				Camera camera = cameraStack.Cameras[i];
				using (new CameraRenderingScope(context, camera))
				{
					using (new ProfilingScope(ProfilingSampler.Get(WaaaghProfileId.UpdateVolumeFramework)))
					{
						UpdateVolumeFramework(camera, cameraStack.AdditionalCameraDataList[i]);
					}
					using (new ProfilingScope(ProfilingSampler.Get(WaaaghProfileId.RenderCamera)))
					{
						WaaaghCameraData cameraData = CreateCameraData(cameraStack, i);
						RenderSingleCamera(context, cameraData);
					}
				}
			}
		}
	}

	private WaaaghCameraData CreateCameraData(WaaaghCameraStack cameraStack, int cameraIndex)
	{
		Camera camera = cameraStack.Cameras[cameraIndex];
		WaaaghAdditionalCameraData waaaghAdditionalCameraData = cameraStack.AdditionalCameraDataList[cameraIndex];
		ScriptableRenderer renderer = GetRenderer(camera, waaaghAdditionalCameraData);
		WaaaghCameraData waaaghCameraData = renderer.FrameData.Create<WaaaghCameraData>();
		waaaghCameraData.camera = camera;
		waaaghCameraData.renderer = renderer;
		waaaghCameraData.cameraType = camera.cameraType;
		waaaghCameraData.historyManager = (waaaghAdditionalCameraData ? waaaghAdditionalCameraData.HistoryManager : null);
		InitializeBaseCameraProperties(waaaghCameraData, cameraStack);
		InitializeCameraProperties(waaaghCameraData, cameraStack, cameraIndex);
		if (waaaghCameraData.renderType == CameraRenderType.Base)
		{
			cameraStack.EnsureStackBuffer(waaaghCameraData);
		}
		waaaghCameraData.Buffer = cameraStack.Buffer;
		return waaaghCameraData;
	}

	private void InitializeBaseCameraProperties(WaaaghCameraData cameraData, WaaaghCameraStack cameraStack)
	{
		WaaaghPipelineAsset asset = Asset;
		bool isSceneViewCamera = cameraData.isSceneViewCamera;
		Camera baseCamera = cameraStack.BaseCamera;
		WaaaghAdditionalCameraData baseAdditionalCameraData = cameraStack.BaseAdditionalCameraData;
		cameraData.targetTexture = baseCamera.targetTexture;
		cameraData.TargetDepthTexture = (baseAdditionalCameraData ? baseAdditionalCameraData.TargetDepthTexture : null);
		if (isSceneViewCamera)
		{
			cameraData.allowHDROutput = false;
		}
		else if (baseAdditionalCameraData != null)
		{
			cameraData.allowHDROutput = baseAdditionalCameraData.AllowHDROutput;
		}
		else
		{
			cameraData.allowHDROutput = true;
		}
		cameraData.isHdrEnabled = baseCamera.allowHDR && asset.SupportsHDR;
		cameraData.hdrColorBufferPrecision = (asset ? asset.HDRColorBufferPrecision : HDRColorBufferPrecision._32Bits);
		cameraData.allowHDROutput &= asset.SupportsHDR;
		cameraData.stackAnyPostProcessingEnabled = cameraStack.AnyPostProcessingEnabled;
		SortingCriteria sortingCriteria = SortingCriteria.CommonOpaque;
		SortingCriteria sortingCriteria2 = SortingCriteria.SortingLayer | SortingCriteria.RenderQueue | SortingCriteria.OptimizeStateChanges | SortingCriteria.CanvasOrder;
		bool hasHiddenSurfaceRemovalOnGPU = SystemInfo.hasHiddenSurfaceRemovalOnGPU;
		bool flag = (baseCamera.opaqueSortMode == OpaqueSortMode.Default && hasHiddenSurfaceRemovalOnGPU) || baseCamera.opaqueSortMode == OpaqueSortMode.NoDistanceSort;
		cameraData.defaultOpaqueSortFlags = (flag ? sortingCriteria2 : sortingCriteria);
		cameraData.captureActions = CameraCaptureBridge.GetCaptureActions(baseCamera);
		cameraData.aspectRatio = (float)baseCamera.pixelWidth / (float)baseCamera.pixelHeight;
	}

	private void InitializeCameraProperties(WaaaghCameraData cameraData, WaaaghCameraStack cameraStack, int cameraIndex)
	{
		WaaaghPipelineAsset asset = Asset;
		bool isSceneViewCamera = cameraData.isSceneViewCamera;
		Camera camera = cameraStack.Cameras[cameraIndex];
		WaaaghAdditionalCameraData waaaghAdditionalCameraData = cameraStack.AdditionalCameraDataList[cameraIndex];
		bool flag = asset.ShadowSettings.ShadowQuality != ShadowQuality.Disable;
		cameraData.maxShadowDistance = Mathf.Min(asset.ShadowSettings.ShadowDistance, camera.farClipPlane);
		cameraData.maxShadowDistance = ((flag && cameraData.maxShadowDistance >= camera.nearClipPlane) ? cameraData.maxShadowDistance : 0f);
		cameraData.IsFogEnabled = UnityEngine.RenderSettings.fog;
		cameraData.IsSceneViewInPrefabEditMode = false;
		Color backgroundColor = camera.backgroundColor;
		cameraData.backgroundColor = CoreUtils.ConvertSRGBToActiveColorSpace(backgroundColor);
		cameraData.IsLightingEnabled = true;
		if (isSceneViewCamera)
		{
			cameraData.renderType = CameraRenderType.Base;
			cameraData.volumeLayerMask = 1;
			cameraData.volumeTrigger = null;
			cameraData.clearDepth = true;
			cameraData.postProcessEnabled = CoreUtils.ArePostProcessesEnabled(camera);
			cameraData.antialiasing = AntialiasingMode.None;
			cameraData.antialiasingQuality = AntialiasingQuality.High;
			cameraData.isDitheringEnabled = false;
			cameraData.isStopNaNEnabled = false;
			cameraData.IsSSREnabled = IsScreenSpaceReflectionsEnabled();
			cameraData.IrsData.Enabled = true;
			cameraData.IsDepthPyramidNeed = IsDepthPyramidNeed();
		}
		else if (waaaghAdditionalCameraData != null)
		{
			cameraData.renderType = waaaghAdditionalCameraData.RenderType;
			cameraData.volumeLayerMask = waaaghAdditionalCameraData.VolumeLayerMask;
			cameraData.volumeTrigger = ((waaaghAdditionalCameraData.VolumeTrigger == null) ? camera.transform : waaaghAdditionalCameraData.VolumeTrigger);
			cameraData.clearDepth = cameraData.renderType != CameraRenderType.Overlay || waaaghAdditionalCameraData.ClearDepth;
			cameraData.postProcessEnabled = waaaghAdditionalCameraData.RenderPostProcessing;
			cameraData.maxShadowDistance = (waaaghAdditionalCameraData.RenderShadows ? cameraData.maxShadowDistance : 0f);
			cameraData.antialiasing = waaaghAdditionalCameraData.Antialiasing;
			cameraData.antialiasingQuality = waaaghAdditionalCameraData.AntialiasingQuality;
			cameraData.isDitheringEnabled = waaaghAdditionalCameraData.Dithering;
			cameraData.isStopNaNEnabled = waaaghAdditionalCameraData.StopNaN;
			cameraData.IsLightingEnabled = waaaghAdditionalCameraData.IsLightingEnabled;
			cameraData.IsSSREnabled = IsScreenSpaceReflectionsEnabled();
			cameraData.IrsData.Enabled = waaaghAdditionalCameraData.AllowIndirectRendering;
			cameraData.IsDepthPyramidNeed = IsDepthPyramidNeed();
		}
		else
		{
			cameraData.renderType = CameraRenderType.Base;
			cameraData.volumeLayerMask = 1;
			cameraData.volumeTrigger = null;
			cameraData.clearDepth = true;
			cameraData.postProcessEnabled = false;
			cameraData.antialiasing = AntialiasingMode.None;
			cameraData.antialiasingQuality = AntialiasingQuality.High;
			cameraData.isDitheringEnabled = false;
			cameraData.isStopNaNEnabled = false;
			cameraData.IrsData.Enabled = false;
		}
		cameraData.SupportsProbeVolumes |= Asset != null && Asset.SupportProbeVolumes;
		cameraData.EnablesProbeVolumes = cameraData.SupportsProbeVolumes && cameraData.cameraType != CameraType.Preview && cameraData.IsLightingEnabled;
		cameraData.IrsData.IrsHasOpaques = cameraData.IrsData.Enabled && IndirectRenderingSystem.Instance.HasOpaqueObjects();
		cameraData.IrsData.IrsHasTransparents = cameraData.IrsData.Enabled && IndirectRenderingSystem.Instance.HasTransparentObjects();
		cameraData.IrsData.IrsHasOpaqueDistortions = cameraData.IrsData.Enabled && IndirectRenderingSystem.Instance.HasOpaqueDistortion();
		Rect rect = camera.rect;
		cameraData.pixelRect = camera.pixelRect;
		cameraData.pixelWidth = camera.pixelWidth;
		cameraData.pixelHeight = camera.pixelHeight;
		cameraData.isDefaultViewport = !(Math.Abs(rect.x) > 0f) && !(Math.Abs(rect.y) > 0f) && !(Math.Abs(rect.width) < 1f) && !(Math.Abs(rect.height) < 1f);
		bool flag2 = cameraData.cameraType == CameraType.SceneView || cameraData.cameraType == CameraType.Preview || cameraData.cameraType == CameraType.Reflection;
		if (camera.cameraType == CameraType.Game && cameraIndex == cameraStack.LastScaledCameraIndex && Mathf.Abs(1f - asset.RenderScale) > 0.05f && camera.targetTexture == null)
		{
			cameraData.renderScale = asset.RenderScale;
			cameraData.upscalingFilter = ResolveUpscalingFilterSelection(new Vector2(cameraData.pixelWidth, cameraData.pixelHeight), cameraData.renderScale, asset.UpscalingFilter);
			if (cameraData.renderScale > 1f)
			{
				cameraData.imageScalingMode = ImageScalingMode.Downscaling;
			}
			else if (cameraData.renderScale < 1f || (!flag2 && (cameraData.upscalingFilter == ImageUpscalingFilter.FSR || cameraData.upscalingFilter == ImageUpscalingFilter.STP)))
			{
				cameraData.imageScalingMode = ImageScalingMode.Upscaling;
				if (cameraData.upscalingFilter == ImageUpscalingFilter.STP)
				{
					cameraData.antialiasing = AntialiasingMode.TemporalAntialiasing;
				}
			}
			else
			{
				cameraData.imageScalingMode = ImageScalingMode.None;
			}
			cameraData.fsrOverrideSharpness = asset.FsrOverrideSharpness;
			cameraData.fsrSharpness = asset.FsrSharpness;
		}
		else
		{
			cameraData.renderScale = 1f;
			cameraData.imageScalingMode = ImageScalingMode.None;
			cameraData.upscalingFilter = ImageUpscalingFilter.Linear;
			cameraData.fsrSharpness = 0f;
			cameraData.fsrOverrideSharpness = false;
		}
		bool num = cameraData.renderType == CameraRenderType.Overlay;
		if (waaaghAdditionalCameraData != null)
		{
			UpdateTemporalAAData(cameraData, waaaghAdditionalCameraData);
			UpdateCullingDepthData(cameraData, waaaghAdditionalCameraData);
		}
		if (cameraData.historyManager != null)
		{
			SetupSsrHistory(cameraData, cameraData.historyManager);
			SetupRawColorAndDepthHistory(cameraData, cameraData.historyManager);
		}
		Matrix4x4 projectionMatrix = camera.projectionMatrix;
		if (num && !camera.orthographic && !Mathf.Approximately(cameraData.aspectRatio, camera.aspect))
		{
			float m = camera.projectionMatrix.m00 * camera.aspect / cameraData.aspectRatio;
			projectionMatrix.m00 = m;
		}
		bool flag3 = !Mathf.Approximately(cameraData.renderScale, 1f);
		cameraData.CameraRenderTargetBufferType = ((!flag3 || cameraIndex > cameraStack.LastScaledCameraIndex) ? CameraRenderTargetType.NonScaled : CameraRenderTargetType.Scaled);
		if (cameraIndex == cameraStack.LastCameraIndex)
		{
			cameraData.CameraResolveTargetBufferType = CameraResolveTargetType.Backbuffer;
			cameraData.CameraResolveRequired = true;
		}
		else if (!flag3 || cameraIndex >= cameraStack.LastScaledCameraIndex)
		{
			cameraData.CameraResolveTargetBufferType = CameraResolveTargetType.NonScaled;
			cameraData.CameraResolveRequired = cameraData.CameraRenderTargetBufferType == CameraRenderTargetType.Scaled;
		}
		else
		{
			cameraData.CameraResolveTargetBufferType = CameraResolveTargetType.None;
			cameraData.CameraResolveRequired = false;
		}
		bool preserveFramebufferAlpha = Graphics.preserveFramebufferAlpha;
		Vector2 viewportSize = ((cameraData.CameraRenderTargetBufferType == CameraRenderTargetType.Scaled) ? new Vector2(cameraData.scaledWidth, cameraData.scaledHeight) : new Vector2(cameraData.pixelWidth, cameraData.pixelHeight));
		cameraData.cameraTargetDescriptor = CreateRenderTextureDescriptor(in camera, in viewportSize, cameraData.isHdrEnabled, cameraData.hdrColorBufferPrecision, preserveFramebufferAlpha);
		TemporalAA.JitterFunc jitterFunc = (cameraData.IsSTPEnabled() ? StpUtils.s_JitterFunc : TemporalAA.s_JitterFunc);
		Matrix4x4 jitterMatrix = TemporalAA.CalculateJitterMatrix(cameraData, jitterFunc);
		cameraData.SetViewProjectionAndJitterMatrix(camera.worldToCameraMatrix, projectionMatrix, jitterMatrix);
		cameraData.worldSpaceCameraPos = camera.transform.position;
		GraphicsFormatUtility.GetAlphaComponentCount(cameraData.cameraTargetDescriptor.graphicsFormat);
		cameraData.isAlphaOutputEnabled = GraphicsFormatUtility.HasAlphaChannel(cameraData.cameraTargetDescriptor.graphicsFormat);
		if (cameraData.camera.cameraType == CameraType.SceneView && CoreUtils.IsSceneFilteringEnabled())
		{
			cameraData.isAlphaOutputEnabled = true;
		}
		bool flag4 = false;
		bool flag5 = !cameraData.postProcessEnabled || (cameraData.postProcessEnabled && flag4);
		cameraData.isAlphaOutputEnabled &= flag5;
	}

	private void RenderSingleCamera(ScriptableRenderContext context, WaaaghCameraData cameraData)
	{
		Camera camera = cameraData.camera;
		ScriptableRenderer renderer = cameraData.renderer;
		if (renderer == null)
		{
			Debug.LogWarning($"Trying to render {camera.name} with an invalid renderer. Camera rendering will be skipped.");
		}
		else
		{
			if (!cameraData.camera.TryGetCullingParameters(stereoAware: false, out var cullingParameters))
			{
				return;
			}
			using ContextContainer contextContainer = renderer.FrameData;
			ScriptableRenderer.Current = renderer;
			using (new ProfilingScope(ProfilingSampler.Get(WaaaghProfileId.CameraSetupData)))
			{
				renderer.SetupCullingParameters(ref cullingParameters, cameraData);
				if (camera.cameraType == CameraType.Reflection || camera.cameraType == CameraType.Preview)
				{
					ScriptableRenderContext.EmitGeometryForCamera(camera);
				}
				ProbeVolumesOptions options = null;
				WaaaghAdditionalCameraData component = camera.GetComponent<WaaaghAdditionalCameraData>();
				if (component != null)
				{
					options = component.VolumeStack?.GetComponent<ProbeVolumesOptions>();
				}
				ProbeReferenceVolume.instance.RenderDebug(camera, options, Texture2D.whiteTexture);
				if (component != null)
				{
					component.MotionVectorsPersistentData.Update(cameraData);
				}
				if (cameraData.taaHistory != null)
				{
					UpdateTemporalAATargets(cameraData);
				}
				if (cameraData.SsrHistory != null)
				{
					UpdateSsrHistoryTextures(cameraData);
				}
				if (cameraData.CullingDepthHistory != null)
				{
					UpdateCullingDepthTarget(cameraData);
				}
			}
			using (new ProfilingScope(ProfilingSampler.Get(WaaaghProfileId.CameraCull)))
			{
				contextContainer.Create<WaaaghRenderingData>().CullResults = context.Cull(ref cullingParameters);
			}
			using (new ProfilingScope(ProfilingSampler.Get(WaaaghProfileId.CameraSetupData)))
			{
				CreateWaaaghResourceData(contextContainer);
				CreateWaaaghRendererListData(contextContainer);
				CreateShadowData(contextContainer);
				CreatePostProcessingData(contextContainer);
				CreateRenderingData(contextContainer);
			}
			using (new ProfilingScope(ProfilingSampler.Get(WaaaghProfileId.CameraSetupRenderer)))
			{
				renderer.SetupInternal(context, renderer.FrameData);
			}
			using (new ProfilingScope(ProfilingSampler.Get(WaaaghProfileId.CameraExecuteRenderer)))
			{
				renderer.Execute(context, renderer.FrameData);
			}
			using (new ProfilingScope(ProfilingSampler.Get(WaaaghProfileId.CameraSubmit)))
			{
				context.Submit();
			}
			ScriptableRenderer.Current = null;
		}
	}

	private WaaaghRenderingData CreateRenderingData(ContextContainer frameData)
	{
		WaaaghRenderingData waaaghRenderingData = frameData.Get<WaaaghRenderingData>();
		WaaaghPipelineAsset asset = Asset;
		waaaghRenderingData.RenderGraph = m_RenderGraph;
		waaaghRenderingData.VisibleLights = waaaghRenderingData.CullResults.visibleLights;
		waaaghRenderingData.SupportsDynamicBatching = asset.SupportsDynamicBatching;
		waaaghRenderingData.PerObjectData = GetPerObjectData();
		waaaghRenderingData.GPUDrivenBatchRendererGroup = GPUDrivenBatchRendererGroup;
		waaaghRenderingData.VirtualTextureManager = VirtualTextureManager;
		waaaghRenderingData.LightCookieManager = m_LightCookieManager;
		InitializeTimeData(out waaaghRenderingData.TimeData);
		return waaaghRenderingData;
	}

	private WaaaghPostProcessingData CreatePostProcessingData(ContextContainer frameData)
	{
		WaaaghPostProcessingData waaaghPostProcessingData = frameData.Create<WaaaghPostProcessingData>();
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		waaaghPostProcessingData.isEnabled = waaaghCameraData.stackAnyPostProcessingEnabled;
		WaaaghPipelineAsset asset = Asset;
		waaaghPostProcessingData.gradingMode = (asset.SupportsHDR ? asset.PostProcessSettings.ColorGradingMode : ColorGradingMode.LowDynamicRange);
		if (waaaghCameraData.stackLastCameraOutputToHDR)
		{
			waaaghPostProcessingData.gradingMode = ColorGradingMode.HighDynamicRange;
		}
		waaaghPostProcessingData.lutSize = asset.PostProcessSettings.ColorGradingLutSize;
		waaaghPostProcessingData.useFastSRGBLinearConversion = asset.PostProcessSettings.UseFastSRGBLinearConversion;
		waaaghPostProcessingData.supportScreenSpaceLensFlare = asset.PostProcessSettings.SupportScreenSpaceLensFlare;
		waaaghPostProcessingData.supportDataDrivenLensFlare = asset.PostProcessSettings.SupportDataDrivenLensFlare;
		return waaaghPostProcessingData;
	}

	private WaaaghShadowData CreateShadowData(ContextContainer frameData)
	{
		WaaaghShadowData waaaghShadowData = frameData.Create<WaaaghShadowData>();
		ShadowSettings shadowSettings = Asset.ShadowSettings;
		waaaghShadowData.StaticShadowsCacheEnabled = shadowSettings.StaticShadowsCacheEnabled;
		waaaghShadowData.ShadowManager = m_ShadowManager;
		waaaghShadowData.AtlasSize = shadowSettings.AtlasSize;
		waaaghShadowData.CacheAtlasSize = shadowSettings.CacheAtlasSize;
		waaaghShadowData.SpotLightResolution = shadowSettings.SpotLightResolution;
		waaaghShadowData.DirectionalLightCascades = shadowSettings.DirectionalLightCascades;
		waaaghShadowData.DirectionalLightCascadeResolution = shadowSettings.DirectionalLightCascadeResolution;
		waaaghShadowData.PointLightResolution = shadowSettings.PointLightResolution;
		waaaghShadowData.ShadowNearPlane = shadowSettings.ShadowNearPlane;
		waaaghShadowData.ShadowQuality = shadowSettings.ShadowQuality;
		waaaghShadowData.DepthBias = shadowSettings.DepthBias;
		waaaghShadowData.NormalBias = shadowSettings.NormalBias;
		waaaghShadowData.DirectionalSlopeBias = shadowSettings.DirectionalSlopeBias;
		waaaghShadowData.PointSlopeBias = shadowSettings.PointSlopeBias;
		waaaghShadowData.ReceiverNormalBias = shadowSettings.ReceiverNormalBias;
		waaaghShadowData.ShadowUpdateDistances = shadowSettings.ShadowUpdateDistances;
		return waaaghShadowData;
	}

	private WaaaghResourceData CreateWaaaghResourceData(ContextContainer frameData)
	{
		return frameData.Create<WaaaghResourceData>();
	}

	private WaaaghRendererListData CreateWaaaghRendererListData(ContextContainer frameData)
	{
		return frameData.Create<WaaaghRendererListData>();
	}

	private static void UpdateTemporalAATargets(WaaaghCameraData cameraData)
	{
		if (cameraData.IsTemporalAAEnabled())
		{
			bool flag = false;
			bool flag2;
			if (cameraData.IsSTPEnabled())
			{
				cameraData.taaHistory.Reset();
				flag2 = cameraData.stpHistory.Update(cameraData);
			}
			else
			{
				flag2 = cameraData.taaHistory.Update(ref cameraData.cameraTargetDescriptor, flag);
			}
			if (flag2)
			{
				cameraData.taaSettings.resetHistoryFrames += ((!flag) ? 1 : 2);
			}
		}
		else
		{
			cameraData.taaHistory.Reset();
			if (cameraData.IsSTPEnabled())
			{
				cameraData.stpHistory.Reset();
			}
		}
	}

	private static void UpdateTemporalAAData(WaaaghCameraData cameraData, WaaaghAdditionalCameraData additionalCameraData)
	{
		additionalCameraData.HistoryManager.RequestAccess<TaaHistory>();
		cameraData.taaHistory = additionalCameraData.HistoryManager.GetHistoryForWrite<TaaHistory>();
		if (cameraData.IsSTPEnabled())
		{
			additionalCameraData.HistoryManager.RequestAccess<StpHistory>();
			cameraData.stpHistory = additionalCameraData.HistoryManager.GetHistoryForWrite<StpHistory>();
		}
		ref TemporalAA.Settings taaSettings = ref additionalCameraData.TaaSettings;
		cameraData.taaSettings = taaSettings;
		taaSettings.resetHistoryFrames -= ((taaSettings.resetHistoryFrames > 0) ? 1 : 0);
	}

	private static void SetupSsrHistory(WaaaghCameraData cameraData, WaaaghCameraHistory history)
	{
		if (cameraData.IsSSREnabled)
		{
			history.RequestAccess<SsrHistory>();
			cameraData.SsrHistory = history.GetHistoryForWrite<SsrHistory>();
		}
	}

	private static void UpdateSsrHistoryTextures(WaaaghCameraData cameraData)
	{
		if (cameraData.IsSSREnabled)
		{
			RenderTextureDescriptor cameraDesc = cameraData.cameraTargetDescriptor;
			ScreenSpaceReflections component = VolumeManager.instance.stack.GetComponent<ScreenSpaceReflections>();
			if (component.Quality.value == ScreenSpaceReflectionsQuality.Half)
			{
				cameraDesc.width /= 2;
				cameraDesc.height /= 2;
			}
			if (component.ColorPrecision.value == ColorPrecision.Color64)
			{
				cameraDesc.colorFormat = RenderTextureFormat.ARGBHalf;
			}
			else
			{
				cameraDesc.colorFormat = RenderTextureFormat.ARGB32;
			}
			cameraData.SsrHistory.Update(ref cameraDesc);
		}
		else
		{
			cameraData.SsrHistory.Reset();
		}
	}

	private static void UpdateCullingDepthData(WaaaghCameraData cameraData, WaaaghAdditionalCameraData additionalCameraData)
	{
		additionalCameraData.HistoryManager.RequestAccess<CullingDepthHistory>();
		cameraData.CullingDepthHistory = additionalCameraData.HistoryManager.GetHistoryForWrite<CullingDepthHistory>();
	}

	private static void UpdateCullingDepthTarget(WaaaghCameraData cameraData)
	{
		GPUDrivenBRGSettings gPUDrivenBRGSettings = Asset.GPUDrivenBRGSettings;
		if (gPUDrivenBRGSettings != null && gPUDrivenBRGSettings.IsEnabledAndSupported && gPUDrivenBRGSettings.OcclusionCulling && gPUDrivenBRGSettings.DepthReprojection)
		{
			RenderTextureDescriptor depthDesc = new RenderTextureDescriptor(cameraData.scaledWidth, cameraData.scaledHeight, GraphicsFormat.R32_SFloat, GraphicsFormat.None);
			cameraData.CullingDepthHistory.Update(depthDesc);
		}
		else
		{
			cameraData.CullingDepthHistory.Reset();
		}
	}

	private static void SetupRawColorAndDepthHistory(WaaaghCameraData cameraData, WaaaghCameraHistory history)
	{
		if (cameraData != null && history != null && cameraData.IsSSREnabled)
		{
			history.RequestAccess<RawColorHistory>();
		}
	}

	private static ImageUpscalingFilter ResolveUpscalingFilterSelection(Vector2 imageSize, float renderScale, UpscalingFilterSelection selection)
	{
		ImageUpscalingFilter result = ImageUpscalingFilter.Linear;
		if ((selection == UpscalingFilterSelection.FSR && !FSRUtils.IsSupported()) || (selection == UpscalingFilterSelection.STP && !STP.IsSupported()))
		{
			selection = UpscalingFilterSelection.Auto;
		}
		switch (selection)
		{
		case UpscalingFilterSelection.Auto:
		{
			float num = 1f / renderScale;
			if (Mathf.Approximately(num - Mathf.Floor(num), 0f))
			{
				float num2 = imageSize.x / num;
				float num3 = imageSize.y / num;
				if (Mathf.Approximately(num2 - Mathf.Floor(num2), 0f) && Mathf.Approximately(num3 - Mathf.Floor(num3), 0f))
				{
					result = ImageUpscalingFilter.Point;
				}
			}
			break;
		}
		case UpscalingFilterSelection.Point:
			result = ImageUpscalingFilter.Point;
			break;
		case UpscalingFilterSelection.FSR:
			result = ImageUpscalingFilter.FSR;
			break;
		case UpscalingFilterSelection.STP:
			result = ImageUpscalingFilter.STP;
			break;
		}
		return result;
	}

	internal static bool HDROutputForMainDisplayIsActive()
	{
		bool num = SystemInfo.hdrDisplaySupportFlags.HasFlag(HDRDisplaySupportFlags.Supported) && Asset.SupportsHDR;
		bool flag = HDROutputSettings.main.available && HDROutputSettings.main.active;
		return num && flag;
	}

	private void UpdateRealtimeGI()
	{
		Scene activeScene = SceneManager.GetActiveScene();
		if (activeScene != m_ActiveScene)
		{
			m_ActiveScene = activeScene;
			UpdateLigths();
		}
	}

	private static void UpdateLigths()
	{
		Light[] array = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
		Dictionary<Light, bool> dictionary = new Dictionary<Light, bool>();
		Light[] array2 = array;
		foreach (Light light in array2)
		{
			dictionary[light] = light.enabled;
		}
		array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].enabled = false;
		}
		array2 = array;
		foreach (Light light2 in array2)
		{
			light2.enabled = dictionary[light2];
		}
	}

	private void UpdateFrameCount(List<Camera> cameras)
	{
		FrameId.Update();
	}

	private static void UpdateVolumeFramework(Camera camera, WaaaghAdditionalCameraData additionalCameraData)
	{
		if (!((camera.cameraType == CameraType.SceneView) | (additionalCameraData != null && additionalCameraData.RequiresVolumeFrameworkUpdate)) && (bool)additionalCameraData)
		{
			if (additionalCameraData.VolumeStack == null)
			{
				camera.UpdateVolumeStack(additionalCameraData);
			}
			VolumeManager.instance.stack = additionalCameraData.VolumeStack;
		}
		else
		{
			camera.GetVolumeLayerMaskAndTrigger(additionalCameraData, out var layerMask, out var trigger);
			VolumeManager.instance.ResetMainStack();
			VolumeManager.instance.Update(trigger, layerMask);
			OnVolumeManagerUpdate(camera);
			OnAfterVolumeManagerUpdate(camera, additionalCameraData);
		}
	}

	private static void OnVolumeManagerUpdate(Camera camera)
	{
		VolumeManagerUpdated?.Invoke(camera);
	}

	private static void OnAfterVolumeManagerUpdate(Camera camera, WaaaghAdditionalCameraData additionalCameraData)
	{
		VolumeStack volumeStack = (additionalCameraData ? additionalCameraData.VolumeStack : null);
		if (volumeStack == null)
		{
			volumeStack = VolumeManager.instance.stack;
		}
		GlobalEffect.OnAfterVolumeStackUpdated?.Invoke(camera, volumeStack);
	}

	private void SetupPerFrameShaderConstants()
	{
		WaaaghPipelineAsset asset = Asset;
		SphericalHarmonicsL2 ambientProbe = UnityEngine.RenderSettings.ambientProbe;
		Shader.SetGlobalVector(value: CoreUtils.ConvertLinearToActiveColorSpace(new Color(ambientProbe[0, 0], ambientProbe[1, 0], ambientProbe[2, 0]) * UnityEngine.RenderSettings.reflectionIntensity), nameID: ShaderPropertyId._GlossyEnvironmentColor);
		Shader.SetGlobalVector(ShaderPropertyId._GlossyEnvironmentCubeMapHDR, ReflectionProbe.defaultTextureHDRDecodeValues);
		Shader.SetGlobalTexture(ShaderPropertyId._GlossyEnvironmentCubeMap, ReflectionProbe.defaultTexture);
		Shader.SetGlobalVector(ShaderPropertyId.unity_AmbientSky, CoreUtils.ConvertSRGBToActiveColorSpace(UnityEngine.RenderSettings.ambientSkyColor));
		Shader.SetGlobalVector(ShaderPropertyId.unity_AmbientEquator, CoreUtils.ConvertSRGBToActiveColorSpace(UnityEngine.RenderSettings.ambientEquatorColor));
		Shader.SetGlobalVector(ShaderPropertyId.unity_AmbientGround, CoreUtils.ConvertSRGBToActiveColorSpace(UnityEngine.RenderSettings.ambientGroundColor));
		Shader.SetGlobalVector(ShaderPropertyId._SubtractiveShadowColor, CoreUtils.ConvertSRGBToActiveColorSpace(UnityEngine.RenderSettings.subtractiveShadowColor));
		Shader.SetGlobalVector(ShaderPropertyId._HexRatio, new Vector4(1f, 1.7320508f));
		if (asset.TerrainSettings.TriplanarEnabled)
		{
			Shader.EnableKeyword(ShaderKeywordStrings._TRIPLANAR);
		}
		else
		{
			Shader.DisableKeyword(ShaderKeywordStrings._TRIPLANAR);
		}
		Shader.SetGlobalTexture(FogOfWarConstantBuffer._FogOfWarMask, Texture2D.blackTexture);
		OcclusionClippingSettings renderPipelineSettings = GraphicsSettings.GetRenderPipelineSettings<OcclusionClippingSettings>();
		if (renderPipelineSettings != null)
		{
			if (renderPipelineSettings.ClippingType == OcclusionClippingType.ScreenSpaceDithering)
			{
				Shader.EnableKeyword("OCCLUDED_OBJECT_CLIP_TYPE_DITHERING");
			}
			else
			{
				Shader.DisableKeyword("OCCLUDED_OBJECT_CLIP_TYPE_DITHERING");
			}
		}
	}

	private static ScriptableRenderer GetRenderer(Camera camera, WaaaghAdditionalCameraData additionalCameraData)
	{
		if (!(additionalCameraData != null))
		{
			return Asset.ScriptableRenderer;
		}
		return additionalCameraData.ScriptableRenderer;
	}

	private static bool IsScreenSpaceReflectionsEnabled()
	{
		return VolumeManager.instance.stack.GetComponent<ScreenSpaceReflections>().IsActive();
	}

	private static bool IsDepthPyramidNeed()
	{
		return VolumeManager.instance.stack.GetComponent<ScreenSpaceReflections>().IsActive();
	}

	private static RenderTextureDescriptor CreateRenderTextureDescriptor(in Camera camera, in Vector2 viewportSize, bool hdrEnabled, HDRColorBufferPrecision hdrColorBufferPrecision, bool needsAlphaChannel)
	{
		RenderTextureDescriptor result;
		if (camera.targetTexture == null)
		{
			result = new RenderTextureDescriptor((int)viewportSize.x, (int)viewportSize.y);
			result.graphicsFormat = MakeRenderTextureGraphicsFormat(hdrEnabled, hdrColorBufferPrecision, needsAlphaChannel);
			result.depthBufferBits = 32;
			result.msaaSamples = 1;
		}
		else
		{
			result = camera.targetTexture.descriptor;
			result.width = (int)viewportSize.x;
			result.height = (int)viewportSize.y;
			if (camera.cameraType == CameraType.SceneView && !hdrEnabled)
			{
				result.graphicsFormat = SystemInfo.GetGraphicsFormat(DefaultFormat.LDR);
			}
		}
		result.width = Mathf.Max(1, result.width);
		result.height = Mathf.Max(1, result.height);
		result.enableRandomWrite = false;
		result.bindMS = false;
		result.useDynamicScale = camera.allowDynamicResolution;
		result.msaaSamples = 1;
		return result;
	}

	internal static GraphicsFormat MakeRenderTextureGraphicsFormat(bool isHdrEnabled, HDRColorBufferPrecision hdrColorBufferPrecision, bool needsAlpha)
	{
		if (isHdrEnabled)
		{
			if (!needsAlpha && hdrColorBufferPrecision != HDRColorBufferPrecision._64Bits && RenderingUtils.SupportsGraphicsFormat(GraphicsFormat.B10G11R11_UFloatPack32, GraphicsFormatUsage.Linear | GraphicsFormatUsage.Render))
			{
				return GraphicsFormat.B10G11R11_UFloatPack32;
			}
			if (RenderingUtils.SupportsGraphicsFormat(GraphicsFormat.R16G16B16A16_SFloat, GraphicsFormatUsage.Linear | GraphicsFormatUsage.Render))
			{
				return GraphicsFormat.R16G16B16A16_SFloat;
			}
			return SystemInfo.GetGraphicsFormat(DefaultFormat.HDR);
		}
		return SystemInfo.GetGraphicsFormat(DefaultFormat.LDR);
	}

	private void InitializeTimeData(out TimeData timeData)
	{
		timeData.Time = Time.time % 14400f;
		timeData.DeltaTime = Time.deltaTime;
		timeData.SmoothDeltaTime = Time.smoothDeltaTime;
		timeData.FrameId = FrameId.FrameCount;
		timeData.UnscaledTime = Time.unscaledTime;
	}

	private static PerObjectData GetPerObjectData()
	{
		return PerObjectData.LightProbe | PerObjectData.ReflectionProbes | PerObjectData.Lightmaps;
	}

	private void SortCameras(List<Camera> cameras)
	{
		if (cameras.Count > 1)
		{
			cameras.Sort(m_CameraComparison);
		}
	}

	public static bool IsGameCamera(Camera camera)
	{
		if (camera == null)
		{
			throw new ArgumentNullException("camera");
		}
		if (camera.cameraType != CameraType.Game)
		{
			return camera.cameraType == CameraType.VR;
		}
		return true;
	}

	private static void SetSupportedRenderingFeatures()
	{
	}

	internal static void GetHDROutputLuminanceParameters(HDROutputUtils.HDRDisplayInformation hdrDisplayInformation, ColorGamut hdrDisplayColorGamut, Tonemapping tonemapping, out Vector4 hdrOutputParameters)
	{
		float x = hdrDisplayInformation.minToneMapLuminance;
		float y = hdrDisplayInformation.maxToneMapLuminance;
		float num = hdrDisplayInformation.paperWhiteNits;
		if (!tonemapping.detectPaperWhite.value)
		{
			num = tonemapping.paperWhite.value;
		}
		if (!tonemapping.detectBrightnessLimits.value)
		{
			x = tonemapping.minNits.value;
			y = tonemapping.maxNits.value;
		}
		hdrOutputParameters = new Vector4(x, y, num, 1f / num);
	}

	internal static void GetHDROutputGradingParameters(Tonemapping tonemapping, out Vector4 hdrOutputParameters)
	{
		int num = 0;
		float y = 0f;
		switch (tonemapping.mode.value)
		{
		case TonemappingMode.Neutral:
			num = (int)tonemapping.neutralHDRRangeReductionMode.value;
			y = tonemapping.hueShiftAmount.value;
			break;
		case TonemappingMode.ACES:
			num = (int)tonemapping.acesPreset.value;
			break;
		}
		hdrOutputParameters = new Vector4(num, y, 0f, 0f);
	}

	internal static GraphicsFormat MakeUnormRenderTextureGraphicsFormat()
	{
		if (SystemInfo.IsFormatSupported(GraphicsFormat.A2B10G10R10_UNormPack32, GraphicsFormatUsage.Blend))
		{
			return GraphicsFormat.A2B10G10R10_UNormPack32;
		}
		return GraphicsFormat.R8G8B8A8_UNorm;
	}

	private static void AdjustUIOverlayOwnership(int cameraCount)
	{
		if (XRSystem.displayActive || cameraCount == 0)
		{
			SupportedRenderingFeatures.active.rendersUIOverlay = false;
		}
		else
		{
			SupportedRenderingFeatures.active.rendersUIOverlay = true;
		}
	}

	private void LightmappingDelegate(Light[] requests, NativeArray<LightDataGI> lightsOutput)
	{
		LightDataGI value = default(LightDataGI);
		if (!SupportedRenderingFeatures.active.enlighten || (SupportedRenderingFeatures.active.lightmapBakeTypes | LightmapBakeType.Realtime) == (LightmapBakeType)0)
		{
			for (int i = 0; i < requests.Length; i++)
			{
				Light light = requests[i];
				value.InitNoBake(light.GetInstanceID());
				lightsOutput[i] = value;
			}
			return;
		}
		for (int j = 0; j < requests.Length; j++)
		{
			Light light2 = requests[j];
			switch (light2.type)
			{
			case UnityEngine.LightType.Directional:
			{
				DirectionalLight dir = default(DirectionalLight);
				LightmapperUtils.Extract(light2, ref dir);
				value.Init(ref dir);
				break;
			}
			case UnityEngine.LightType.Point:
			{
				PointLight point = default(PointLight);
				LightmapperUtils.Extract(light2, ref point);
				value.Init(ref point);
				break;
			}
			case UnityEngine.LightType.Spot:
			{
				SpotLight spot = default(SpotLight);
				LightmapperUtils.Extract(light2, ref spot);
				spot.innerConeAngle = light2.innerSpotAngle * (MathF.PI / 180f);
				spot.angularFalloff = AngularFalloffType.AnalyticAndInnerAngle;
				value.Init(ref spot);
				break;
			}
			case UnityEngine.LightType.Area:
				value.InitNoBake(light2.GetInstanceID());
				break;
			case UnityEngine.LightType.Disc:
				value.InitNoBake(light2.GetInstanceID());
				break;
			default:
				value.InitNoBake(light2.GetInstanceID());
				break;
			}
			value.falloff = FalloffType.InverseSquared;
			lightsOutput[j] = value;
		}
	}
}
