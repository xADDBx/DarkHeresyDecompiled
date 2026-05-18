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
using Owlcat.Runtime.Visual.Waaagh.Recorders.Debugging;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.FogOfWar;
using Owlcat.Runtime.Visual.Waaagh.Settings;
using Owlcat.Runtime.Visual.Waaagh.Shadows;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.SceneManagement;
using UnityEngine.VFX;

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

	private readonly WaaaghPipelineAsset m_PipelineAsset;

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

	internal static RTHandleResourcePool s_RTHandlePool;

	private readonly WaaaghCameraData m_ReusableCameraData = new WaaaghCameraData();

	private readonly WaaaghRenderingData m_ReusableRenderingData = new WaaaghRenderingData();

	private readonly WaaaghShadowData m_ReusableShadowData = new WaaaghShadowData();

	private readonly DebugContext m_DebugContext;

	private readonly DebugRenderer m_DebugRenderer;

	private static Dictionary<int, ProfilingSampler> s_HashSamplerCache = new Dictionary<int, ProfilingSampler>();

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
		m_PipelineAsset = asset;
		m_GlobalSettings = WaaaghPipelineGlobalSettings.Instance;
		PlatformAutoDetect.Initialize();
		SetSupportedRenderingFeatures();
		Shader.globalRenderPipeline = "OwlcatPipeline";
		WaaaghDefaultVolumeProfileSettings renderPipelineSettings = GraphicsSettings.GetRenderPipelineSettings<WaaaghDefaultVolumeProfileSettings>();
		VolumeManager.instance.Initialize(renderPipelineSettings.volumeProfile);
		Lightmapping.SetDelegate(LightmappingDelegate);
		RenderingUtils.ClearSystemInfoCache();
		m_RenderGraph = new RenderGraph("WaaaghGraph");
		s_RTHandlePool = new RTHandleResourcePool();
		m_ShadowManager = new ShadowManager(asset);
		DebugManager.instance.RefreshEditor();
		m_DebugData = asset.DebugData;
		if (m_DebugData != null)
		{
			m_DebugData.RegisterDebug(this);
			if (Debug.isDebugBuild)
			{
				m_DebugContext = new DebugContext(m_DebugData);
				m_DebugRenderer = new DebugRenderer();
			}
		}
		RTHandles.Initialize(Screen.width, Screen.height);
		DebugManager.instance.enableRuntimeUI = false;
		LightCookieManager.Settings settings = new LightCookieManager.Settings
		{
			atlasTextureResolution = asset.LightCookieSettings.Resolution,
			atlasTextureFormat = asset.LightCookieSettings.Format
		};
		m_LightCookieManager = new LightCookieManager(in settings);
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
		IndirectRenderingSystem.Instance.Initialize();
	}

	protected override void Dispose(bool disposing)
	{
		IndirectRenderingSystem.Instance.Cleanup();
		m_DebugRenderer?.Dispose();
		m_DebugContext?.Dispose();
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
		s_RTHandlePool.Cleanup();
		s_RTHandlePool = null;
		m_ShadowManager.Dispose();
		m_ShadowManager = null;
		ConstantBuffer.ReleaseAll();
		m_LightCookieManager.Dispose();
		m_LocalVolumetricFogManager.ReleaseAtlas();
		VirtualTextureManager?.Dispose();
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
		if (m_PipelineAsset.UnhandledExceptionLockActive())
		{
			return;
		}
		try
		{
			RenderUsingContextContainer(context, cameras);
		}
		catch (Exception exception)
		{
			m_PipelineAsset.IncrementUnhandledExceptionCounter();
			Debug.LogException(exception, m_PipelineAsset);
		}
	}

	private static void OnSceneUnloaded(Scene scene)
	{
		ObjectDispatcherService.ProcessUpdates();
	}

	private void RenderUsingContextContainer(ScriptableRenderContext context, List<Camera> cameras)
	{
		using (new ProfilingScope(WaaaghProfileId.Prepare.Sampler()))
		{
			bool num = FrameId.Update();
			WaaaghCameraHistoryManager.GC();
			AdjustUIOverlayOwnership(cameras.Count);
			if (num)
			{
				IndirectRenderingSystem.Instance.Submit();
			}
		}
		using (new ContextRenderingScope(context, cameras))
		{
			using (new ProfilingScope(WaaaghProfileId.Prepare.Sampler()))
			{
				GraphicsSettings.lightsUseLinearIntensity = QualitySettings.activeColorSpace == ColorSpace.Linear;
				GraphicsSettings.lightsUseColorTemperature = true;
				GraphicsSettings.useScriptableRenderPipelineBatching = Asset.UseSRPBatcher;
				SetupPerFrameShaderConstants();
				SortCameras(cameras);
			}
			using (new ProfilingScope(WaaaghProfileId.PreRender.Sampler()))
			{
				CommandBuffer commandBuffer = CommandBufferPool.Get();
				commandBuffer.BeginSample("PreRender");
				IndirectRenderingSystem.Instance.PreRender();
				VirtualTextureManager?.PreRender(commandBuffer, cameras);
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
			s_RTHandlePool.PurgeUnusedResources(Time.frameCount);
		}
		using (new ProfilingScope(WaaaghProfileId.PostRender.Sampler()))
		{
			CommandBuffer commandBuffer2 = CommandBufferPool.Get();
			commandBuffer2.BeginSample("PostRender");
			if (GPUDrivenBatchRendererGroup.IsEnabledAndInitialized)
			{
				GPUDrivenBatchRendererGroup.PostRender();
			}
			CommandQueue.PostRender();
			VirtualTextureManager?.PostRender(commandBuffer2, cameras);
			if (cameras.Count == 0)
			{
				commandBuffer2.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
			}
			commandBuffer2.EndSample("PostRender");
			using (new ProfilingScope(WaaaghProfileId.PostRenderSubmit.Sampler()))
			{
				context.ExecuteCommandBuffer(commandBuffer2);
				context.Submit();
			}
			CommandBufferPool.Release(commandBuffer2);
		}
	}

	private void RenderCameraStack(ScriptableRenderContext context, WaaaghCameraStack cameraStack)
	{
		using (new ProfilingScope(WaaaghProfileId.RenderCameraStack.Sampler()))
		{
			for (int i = 0; i < cameraStack.AdditionalCameraDataList.Count; i++)
			{
				Camera camera = cameraStack.Cameras[i];
				using (new CameraRenderingScope(context, camera))
				{
					using (new ProfilingScope(WaaaghProfileId.UpdateVolumeFramework.Sampler()))
					{
						UpdateVolumeFramework(camera, cameraStack.AdditionalCameraDataList[i]);
					}
					using (new ProfilingScope(WaaaghProfileId.RenderCamera.Sampler()))
					{
						WaaaghCameraData reusableCameraData = m_ReusableCameraData;
						WaaaghRenderingData reusableRenderingData = m_ReusableRenderingData;
						WaaaghShadowData reusableShadowData = m_ReusableShadowData;
						try
						{
							InitializeCameraData(cameraStack, i, reusableCameraData);
							RenderSingleCamera(context, reusableCameraData, reusableRenderingData, reusableShadowData);
						}
						finally
						{
							reusableCameraData.Reset();
							reusableRenderingData.Reset();
							reusableShadowData.Reset();
						}
					}
				}
			}
		}
	}

	private void InitializeCameraData(WaaaghCameraStack cameraStack, int cameraIndex, WaaaghCameraData cameraData)
	{
		Camera camera = cameraStack.Cameras[cameraIndex];
		WaaaghAdditionalCameraData waaaghAdditionalCameraData = cameraStack.AdditionalCameraDataList[cameraIndex];
		IPipelineRenderer renderer = GetRenderer(camera, waaaghAdditionalCameraData);
		cameraData.camera = camera;
		cameraData.renderer = renderer;
		cameraData.cameraType = camera.cameraType;
		cameraData.StackInfo = new StackInfo
		{
			IsSingleCamera = (cameraStack.Cameras.Count == 1),
			IsLastCamera = (cameraStack.LastCameraIndex == cameraIndex),
			StackTargetHandles = cameraStack.TargetHandles,
			RequiredTargets = CameraRequiredTargets.Unscaled
		};
		cameraData.historyManager = (waaaghAdditionalCameraData ? waaaghAdditionalCameraData.HistoryManager : null);
		InitializeBaseCameraProperties(cameraData, cameraStack);
		InitializeCameraProperties(cameraData, cameraStack, cameraIndex);
	}

	private void InitializeBaseCameraProperties(WaaaghCameraData cameraData, WaaaghCameraStack cameraStack)
	{
		WaaaghPipelineAsset asset = Asset;
		bool isSceneViewCamera = cameraData.isSceneViewCamera;
		Camera baseCamera = cameraStack.BaseCamera;
		WaaaghAdditionalCameraData baseAdditionalCameraData = cameraStack.BaseAdditionalCameraData;
		cameraData.targetTexture = baseCamera.targetTexture;
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
		if (cameraData.renderType == CameraRenderType.Overlay)
		{
			Camera baseCamera = cameraStack.BaseCamera;
			int pixelWidth = baseCamera.pixelWidth;
			int pixelHeight = baseCamera.pixelHeight;
			int num = (int)((float)pixelWidth * rect.x);
			int num2 = (int)((float)pixelWidth * (rect.x + rect.width));
			int num3 = (int)((float)pixelHeight * rect.y);
			int num4 = (int)((float)pixelHeight * (rect.y + rect.height));
			cameraData.pixelWidth = Mathf.Max(1, num2 - num);
			cameraData.pixelHeight = Mathf.Max(1, num4 - num3);
			cameraData.pixelRect = new Rect(num, num3, num2 - num, num4 - num3);
		}
		else
		{
			cameraData.pixelRect = camera.pixelRect;
			cameraData.pixelWidth = camera.pixelWidth;
			cameraData.pixelHeight = camera.pixelHeight;
		}
		cameraData.isDefaultViewport = !(Math.Abs(rect.x) > 0f) && !(Math.Abs(rect.y) > 0f) && !(Math.Abs(rect.width) < 1f) && !(Math.Abs(rect.height) < 1f);
		bool flag2 = cameraData.cameraType != CameraType.SceneView && cameraData.cameraType != CameraType.Preview && cameraData.cameraType != CameraType.Reflection;
		bool flag3 = !Mathf.Approximately(asset.RenderScale, 1f);
		cameraData.renderScale = asset.RenderScale;
		cameraData.StackInfo.StackHasScaling = camera.cameraType == CameraType.Game && cameraStack.LastScaledCameraIndex != -1 && flag3;
		if (camera.cameraType == CameraType.Game && cameraIndex == cameraStack.LastScaledCameraIndex && camera.targetTexture == null)
		{
			cameraData.upscalingFilter = ResolveUpscalingFilterSelection(new Vector2(cameraData.pixelWidth, cameraData.pixelHeight), cameraData.renderScale, asset.UpscalingFilter);
			bool flag4 = cameraData.upscalingFilter == ImageUpscalingFilter.STP;
			bool flag5 = cameraData.upscalingFilter == ImageUpscalingFilter.FSR;
			if (cameraData.renderScale > 1f)
			{
				cameraData.imageScalingMode = ImageScalingMode.Downscaling;
				cameraData.StackInfo.RequiredTargets = CameraRequiredTargets.Both;
			}
			else if (cameraData.renderScale < 1f || (flag2 && (flag4 || flag5)))
			{
				cameraData.imageScalingMode = ImageScalingMode.Upscaling;
				cameraData.StackInfo.RequiredTargets = CameraRequiredTargets.Both;
				if (cameraData.upscalingFilter == ImageUpscalingFilter.STP)
				{
					cameraData.antialiasing = AntialiasingMode.TemporalAntialiasing;
				}
			}
			else
			{
				cameraData.imageScalingMode = ImageScalingMode.None;
				cameraData.StackInfo.RequiredTargets = CameraRequiredTargets.Unscaled;
			}
			cameraData.fsrOverrideSharpness = asset.FsrOverrideSharpness;
			cameraData.fsrSharpness = asset.FsrSharpness;
		}
		else
		{
			cameraData.imageScalingMode = ImageScalingMode.None;
			cameraData.upscalingFilter = ImageUpscalingFilter.Linear;
			cameraData.fsrSharpness = 0f;
			cameraData.fsrOverrideSharpness = false;
			if (cameraData.StackInfo.StackHasScaling)
			{
				cameraData.StackInfo.RequiredTargets = ((cameraIndex < cameraStack.LastScaledCameraIndex) ? CameraRequiredTargets.Scaled : CameraRequiredTargets.Unscaled);
			}
			else
			{
				cameraData.StackInfo.RequiredTargets = CameraRequiredTargets.Unscaled;
			}
		}
		bool num5 = cameraData.renderType == CameraRenderType.Overlay;
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
		if (num5 && !camera.orthographic && !Mathf.Approximately(cameraData.aspectRatio, camera.aspect))
		{
			float m = camera.projectionMatrix.m00 * camera.aspect / cameraData.aspectRatio;
			projectionMatrix.m00 = m;
		}
		bool preserveFramebufferAlpha = Graphics.preserveFramebufferAlpha;
		Vector2 viewportSize = ((cameraData.StackInfo.RequiredTargets == CameraRequiredTargets.Unscaled) ? new Vector2(cameraData.pixelWidth, cameraData.pixelHeight) : new Vector2(cameraData.scaledWidth, cameraData.scaledHeight));
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
		bool flag6 = false;
		bool flag7 = !cameraData.postProcessEnabled || (cameraData.postProcessEnabled && flag6);
		cameraData.isAlphaOutputEnabled &= flag7;
	}

	private void RenderSingleCamera(ScriptableRenderContext context, WaaaghCameraData cameraData, WaaaghRenderingData renderingData, WaaaghShadowData shadowData)
	{
		Camera camera = cameraData.camera;
		IPipelineRenderer renderer = cameraData.renderer;
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
			GPUDrivenBatchRendererGroup.SupressCulling = !renderer.SupportsPipelineFeature(PipelineFeature.GpuDriven);
			using (new ProfilingScope(WaaaghProfileId.CameraSetupData.Sampler()))
			{
				VFXManager.PrepareCamera(camera);
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
			using (new ProfilingScope(WaaaghProfileId.CameraCull.Sampler()))
			{
				renderingData.CullResults = context.Cull(ref cullingParameters);
			}
			using (new ProfilingScope(WaaaghProfileId.CameraSetupData.Sampler()))
			{
				InitializeShadowData(shadowData);
				InitializeRenderingData(renderingData);
			}
			CommandBuffer commandBuffer = CommandBufferPool.Get();
			using (new ProfilingScope(commandBuffer, TryGetOrAddCameraSampler(camera)))
			{
				RecordAndExecuteRenderGraph(m_RenderGraph, context, renderer, commandBuffer, camera, RenderTextureUVOriginStrategy.PropagateAttachmentOrientation, renderingData, cameraData, shadowData, VirtualTextureManager, GPUDrivenBatchRendererGroup, m_DebugContext);
			}
			context.ExecuteCommandBuffer(commandBuffer);
			CommandBufferPool.Release(commandBuffer);
			using (new ProfilingScope(WaaaghProfileId.CameraSubmit.Sampler()))
			{
				context.Submit();
			}
			GPUDrivenBatchRendererGroup.SupressCulling = false;
		}
	}

	private static void RecordAndExecuteRenderGraph(RenderGraph renderGraph, ScriptableRenderContext context, IPipelineRenderer renderer, CommandBuffer cmd, Camera camera, RenderTextureUVOriginStrategy uvOriginStrategy, WaaaghRenderingData renderingData, WaaaghCameraData cameraData, WaaaghShadowData shadowData, VirtualTextureManager virtualTextureManager, GPUDrivenBatchRendererGroup gpuDrivenBatchRendererGroup, DebugContext debugContext)
	{
		RendererSetupContext context2 = new RendererSetupContext
		{
			ScriptableRenderContext = context,
			RenderingData = renderingData,
			CameraData = cameraData,
			ShadowData = shadowData
		};
		renderer.Setup(in context2);
		RenderGraphParameters parameters = new RenderGraphParameters
		{
			executionId = camera.GetEntityId(),
			generateDebugData = (camera.cameraType != CameraType.Preview && !camera.isProcessingRenderRequest),
			commandBuffer = cmd,
			scriptableRenderContext = context,
			currentFrameIndex = Time.frameCount,
			renderTextureUVOriginStrategy = uvOriginStrategy
		};
		renderGraph.BeginRecording(in parameters);
		try
		{
			RendererRecordContext context3 = new RendererRecordContext
			{
				RenderGraph = renderGraph,
				RenderingData = renderingData,
				CameraData = cameraData,
				VirtualTextureManager = virtualTextureManager,
				ShadowData = shadowData,
				GPUDrivenBatchRendererGroup = gpuDrivenBatchRendererGroup,
				DebugContext = debugContext
			};
			renderer.Record(in context3);
		}
		catch (Exception)
		{
			throw;
		}
		finally
		{
			renderGraph.EndRecordingAndExecute();
		}
		renderer.Cleanup();
	}

	private void InitializeRenderingData(WaaaghRenderingData renderingData)
	{
		WaaaghPipelineAsset asset = Asset;
		renderingData.RenderGraph = m_RenderGraph;
		renderingData.VisibleLights = renderingData.CullResults.visibleLights;
		renderingData.SupportsDynamicBatching = asset.SupportsDynamicBatching;
		renderingData.PerObjectData = GetPerObjectData();
		renderingData.GPUDrivenBatchRendererGroup = GPUDrivenBatchRendererGroup;
		renderingData.VirtualTextureManager = VirtualTextureManager;
		renderingData.LightCookieManager = m_LightCookieManager;
		InitializeTimeData(out renderingData.TimeData);
		renderingData.ShaderTimeData = new ShaderTimeData(in renderingData.TimeData);
	}

	private void InitializeShadowData(WaaaghShadowData shadowData)
	{
		ShadowSettings shadowSettings = Asset.ShadowSettings;
		shadowData.StaticShadowsCacheEnabled = shadowSettings.StaticShadowsCacheEnabled;
		shadowData.ShadowManager = m_ShadowManager;
		shadowData.AtlasSize = shadowSettings.AtlasSize;
		shadowData.CacheAtlasSize = shadowSettings.CacheAtlasSize;
		shadowData.SpotLightResolution = shadowSettings.SpotLightResolution;
		shadowData.DirectionalLightCascades = shadowSettings.DirectionalLightCascades;
		shadowData.DirectionalLightCascadeResolution = shadowSettings.DirectionalLightCascadeResolution;
		shadowData.PointLightResolution = shadowSettings.PointLightResolution;
		shadowData.ShadowNearPlane = shadowSettings.ShadowNearPlane;
		shadowData.ShadowQuality = shadowSettings.ShadowQuality;
		shadowData.DepthBias = shadowSettings.DepthBias;
		shadowData.NormalBias = shadowSettings.NormalBias;
		shadowData.DirectionalSlopeBias = shadowSettings.DirectionalSlopeBias;
		shadowData.PointSlopeBias = shadowSettings.PointSlopeBias;
		shadowData.ReceiverNormalBias = shadowSettings.ReceiverNormalBias;
		shadowData.ShadowUpdateDistances = shadowSettings.ShadowUpdateDistances;
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

	private IPipelineRenderer GetRenderer(Camera camera, WaaaghAdditionalCameraData additionalCameraData)
	{
		if (m_DebugRenderer != null && m_DebugRenderer.AnySupportedDebugActive(m_DebugContext))
		{
			return m_DebugRenderer;
		}
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
			result.msaaSamples = 1;
			result.depthBufferBits = 0;
			result.depthStencilFormat = GraphicsFormat.None;
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
		result.shadowSamplingMode = ShadowSamplingMode.None;
		result.autoGenerateMips = false;
		result.mipCount = 1;
		result.useMipMap = false;
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
}
