using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.Waaagh.Data;
using Owlcat.Runtime.Visual.Waaagh.History;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.FrameData;

public sealed class WaaaghCameraData
{
	private Matrix4x4 m_ViewMatrix;

	private Matrix4x4 m_ProjectionMatrix;

	private Matrix4x4 m_JitterMatrix;

	public Camera camera;

	internal WaaaghCameraHistory m_HistoryManager;

	public CameraRenderType renderType;

	public RenderTexture targetTexture;

	public RenderTextureDescriptor cameraTargetDescriptor;

	internal Rect pixelRect;

	internal bool useScreenCoordOverride;

	internal Vector4 screenSizeOverride;

	internal Vector4 screenCoordScaleBias;

	internal int pixelWidth;

	internal int pixelHeight;

	internal float aspectRatio;

	public float renderScale;

	internal ImageScalingMode imageScalingMode;

	internal StackInfo StackInfo;

	internal ImageUpscalingFilter upscalingFilter;

	internal bool fsrOverrideSharpness;

	internal float fsrSharpness;

	internal HDRColorBufferPrecision hdrColorBufferPrecision;

	public bool clearDepth;

	public CameraType cameraType;

	public bool isDefaultViewport;

	public bool isHdrEnabled;

	public bool allowHDROutput;

	public bool isAlphaOutputEnabled;

	public bool requiresDepthTexture;

	public bool requiresOpaqueTexture;

	internal bool useGPUOcclusionCulling;

	public IrsData IrsData;

	internal bool stackLastCameraOutputToHDR;

	public SortingCriteria defaultOpaqueSortFlags;

	public float maxShadowDistance;

	public bool postProcessEnabled;

	public IEnumerator<Action<RenderTargetIdentifier, CommandBuffer>> captureActions;

	public LayerMask volumeLayerMask;

	public Transform volumeTrigger;

	public bool isStopNaNEnabled;

	public bool isDitheringEnabled;

	public AntialiasingMode antialiasing;

	public AntialiasingQuality antialiasingQuality;

	public IPipelineRenderer renderer;

	public Vector3 worldSpaceCameraPos;

	public Color backgroundColor;

	internal TaaHistory taaHistory;

	internal StpHistory stpHistory;

	internal SsrHistory SsrHistory;

	internal TemporalAA.Settings taaSettings;

	internal bool IsLightingEnabled;

	internal bool SupportsProbeVolumes;

	internal bool EnablesProbeVolumes;

	internal CullingDepthHistory CullingDepthHistory;

	internal bool IsSSREnabled;

	internal bool IsDepthPyramidNeed;

	internal bool IsFogEnabled;

	internal bool IsSceneViewInPrefabEditMode;

	public int scaledWidth => Mathf.Max(1, (int)((float)pixelWidth * renderScale));

	public int scaledHeight => Mathf.Max(1, (int)((float)pixelHeight * renderScale));

	public WaaaghCameraHistory historyManager
	{
		get
		{
			return m_HistoryManager;
		}
		set
		{
			m_HistoryManager = value;
		}
	}

	internal bool requireSrgbConversion
	{
		get
		{
			if (targetTexture == null)
			{
				return Display.main.requiresSrgbBlitToBackbuffer;
			}
			return false;
		}
	}

	public bool isGameCamera => cameraType == CameraType.Game;

	public bool isSceneViewCamera => cameraType == CameraType.SceneView;

	public bool isPreviewCamera => cameraType == CameraType.Preview;

	internal bool isRenderPassSupportedCamera
	{
		get
		{
			if (cameraType != CameraType.Game)
			{
				return cameraType == CameraType.Reflection;
			}
			return true;
		}
	}

	internal bool resolveToScreen
	{
		get
		{
			if (StackInfo.IsLastCamera)
			{
				if (cameraType != CameraType.Game)
				{
					return camera.cameraType == CameraType.VR;
				}
				return true;
			}
			return false;
		}
	}

	public bool isHDROutputActive
	{
		get
		{
			if (WaaaghPipeline.HDROutputForMainDisplayIsActive() && allowHDROutput)
			{
				return resolveToScreen;
			}
			return false;
		}
	}

	public HDROutputUtils.HDRDisplayInformation hdrDisplayInformation
	{
		get
		{
			HDROutputSettings main = HDROutputSettings.main;
			return new HDROutputUtils.HDRDisplayInformation(main.maxFullFrameToneMapLuminance, main.maxToneMapLuminance, main.minToneMapLuminance, main.paperWhiteNits);
		}
	}

	public ColorGamut hdrDisplayColorGamut => HDROutputSettings.main.displayColorGamut;

	public bool rendersOverlayUI
	{
		get
		{
			if (SupportedRenderingFeatures.active.rendersUIOverlay)
			{
				return resolveToScreen;
			}
			return false;
		}
	}

	internal bool resetHistory => taaSettings.resetHistoryFrames != 0;

	internal void SetViewAndProjectionMatrix(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix)
	{
		m_ViewMatrix = viewMatrix;
		m_ProjectionMatrix = projectionMatrix;
		m_JitterMatrix = Matrix4x4.identity;
	}

	internal void SetViewProjectionAndJitterMatrix(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, Matrix4x4 jitterMatrix)
	{
		m_ViewMatrix = viewMatrix;
		m_ProjectionMatrix = projectionMatrix;
		m_JitterMatrix = jitterMatrix;
	}

	public Matrix4x4 GetViewMatrix(int viewIndex = 0)
	{
		return m_ViewMatrix;
	}

	public Matrix4x4 GetProjectionMatrix(int viewIndex = 0)
	{
		return m_JitterMatrix * m_ProjectionMatrix;
	}

	internal Matrix4x4 GetProjectionMatrixNoJitter(int viewIndex = 0)
	{
		return m_ProjectionMatrix;
	}

	internal Matrix4x4 GetJitterMatrix(int viewIndex = 0)
	{
		return m_JitterMatrix;
	}

	public Matrix4x4 GetGPUProjectionMatrix(int viewIndex = 0)
	{
		return GL.GetGPUProjectionMatrix(GetProjectionMatrix(viewIndex), IsCameraProjectionMatrixFlipped());
	}

	public Matrix4x4 GetGPUProjectionMatrixNoJitter(int viewIndex = 0)
	{
		return GL.GetGPUProjectionMatrix(GetProjectionMatrixNoJitter(viewIndex), IsCameraProjectionMatrixFlipped());
	}

	internal Matrix4x4 GetGPUProjectionMatrix(bool renderIntoTexture, int viewIndex = 0)
	{
		return GL.GetGPUProjectionMatrix(GetProjectionMatrix(viewIndex), renderIntoTexture);
	}

	public bool TargetTextureHasDepthBuffer()
	{
		if (targetTexture != null)
		{
			return targetTexture.depthStencilFormat != GraphicsFormat.None;
		}
		return false;
	}

	public bool IsHandleYFlipped(RTHandle handle)
	{
		if (!SystemInfo.graphicsUVStartsAtTop)
		{
			return true;
		}
		if (cameraType == CameraType.SceneView || cameraType == CameraType.Preview)
		{
			return true;
		}
		RenderTargetIdentifier renderTargetIdentifier = new RenderTargetIdentifier(handle.nameID, 0);
		return !(renderTargetIdentifier == BuiltinRenderTextureType.CameraTarget) && !(renderTargetIdentifier == BuiltinRenderTextureType.Depth);
	}

	public bool IsCameraProjectionMatrixFlipped()
	{
		if (renderer != null)
		{
			bool flag = true;
			return SystemInfo.graphicsUVStartsAtTop && flag;
		}
		return true;
	}

	public bool IsRenderTargetProjectionMatrixFlipped(RTHandle color, RTHandle depth = null)
	{
		if (!SystemInfo.graphicsUVStartsAtTop)
		{
			return true;
		}
		if (!(targetTexture != null))
		{
			return IsHandleYFlipped(color ?? depth);
		}
		return true;
	}

	internal bool IsTemporalAAEnabled()
	{
		camera.TryGetComponent<WaaaghAdditionalCameraData>(out var _);
		if (antialiasing == AntialiasingMode.TemporalAntialiasing && postProcessEnabled && taaHistory != null && cameraTargetDescriptor.msaaSamples == 1 && !camera.allowDynamicResolution)
		{
			return renderer.SupportsPipelineFeature(PipelineFeature.MotionVectors);
		}
		return false;
	}

	internal bool IsSTPEnabled()
	{
		if (imageScalingMode == ImageScalingMode.Upscaling)
		{
			return upscalingFilter == ImageUpscalingFilter.STP;
		}
		return false;
	}

	public void Reset()
	{
		m_ViewMatrix = default(Matrix4x4);
		m_ProjectionMatrix = default(Matrix4x4);
		m_JitterMatrix = default(Matrix4x4);
		camera = null;
		renderType = CameraRenderType.Base;
		targetTexture = null;
		cameraTargetDescriptor = default(RenderTextureDescriptor);
		pixelRect = default(Rect);
		useScreenCoordOverride = false;
		screenSizeOverride = default(Vector4);
		screenCoordScaleBias = default(Vector4);
		pixelWidth = 0;
		pixelHeight = 0;
		aspectRatio = 0f;
		renderScale = 1f;
		imageScalingMode = ImageScalingMode.None;
		StackInfo = default(StackInfo);
		upscalingFilter = ImageUpscalingFilter.Point;
		fsrOverrideSharpness = false;
		fsrSharpness = 0f;
		hdrColorBufferPrecision = HDRColorBufferPrecision._32Bits;
		clearDepth = false;
		cameraType = CameraType.Game;
		isDefaultViewport = false;
		isHdrEnabled = false;
		allowHDROutput = false;
		isAlphaOutputEnabled = false;
		requiresDepthTexture = false;
		requiresOpaqueTexture = false;
		useGPUOcclusionCulling = false;
		defaultOpaqueSortFlags = SortingCriteria.None;
		maxShadowDistance = 0f;
		postProcessEnabled = false;
		captureActions = null;
		volumeLayerMask = 0;
		volumeTrigger = null;
		isStopNaNEnabled = false;
		isDitheringEnabled = false;
		antialiasing = AntialiasingMode.None;
		antialiasingQuality = AntialiasingQuality.Low;
		renderer = null;
		worldSpaceCameraPos = default(Vector3);
		backgroundColor = Color.black;
		taaHistory = null;
		stpHistory = null;
		taaSettings = default(TemporalAA.Settings);
		stackLastCameraOutputToHDR = false;
		IrsData = default(IrsData);
		SupportsProbeVolumes = false;
		CullingDepthHistory = null;
		IsSSREnabled = false;
		IsDepthPyramidNeed = false;
		IsFogEnabled = false;
		IsSceneViewInPrefabEditMode = false;
		SsrHistory = null;
	}
}
