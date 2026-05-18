using System;
using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.Waaagh.Data;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.PostProcess;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders.PostProcess;

public sealed class PostProcessor : IDisposable
{
	public struct OverridesStack
	{
		public ChannelMixer ChannelMixer;

		public ColorAdjustments ColorAdjustments;

		public ColorCurves ColorCurves;

		public LiftGammaGain LiftGammaGain;

		public ShadowsMidtonesHighlights ShadowsMidtonesHighlights;

		public SplitToning SplitToning;

		public Tonemapping Tonemapping;

		public WhiteBalance WhiteBalance;

		public DepthOfField DepthOfField;

		public ScreenSpaceLensFlare ScreenSpaceLensFlare;

		public PaniniProjection PaniniProjection;

		public MotionBlur MotionBlur;

		public RadialBlur RadialBlur;

		public Bloom Bloom;

		public BloomEnhanced BloomEnhanced;

		public ColorLookup ColorLookup;

		public LensDistortion LensDistortion;

		public ChromaticAberration ChromaticAberration;

		public Vignette Vignette;

		public FilmGrain FilmGrain;
	}

	public struct PostProcessGpuResources
	{
		public TextureHandle ColorGradingLut;
	}

	public static class ShaderIDs
	{
		public static readonly int _Lut_Params = Shader.PropertyToID("_Lut_Params");

		public static readonly int _ColorBalance = Shader.PropertyToID("_ColorBalance");

		public static readonly int _ColorFilter = Shader.PropertyToID("_ColorFilter");

		public static readonly int _ChannelMixerRed = Shader.PropertyToID("_ChannelMixerRed");

		public static readonly int _ChannelMixerGreen = Shader.PropertyToID("_ChannelMixerGreen");

		public static readonly int _ChannelMixerBlue = Shader.PropertyToID("_ChannelMixerBlue");

		public static readonly int _HueSatCon = Shader.PropertyToID("_HueSatCon");

		public static readonly int _Lift = Shader.PropertyToID("_Lift");

		public static readonly int _Gamma = Shader.PropertyToID("_Gamma");

		public static readonly int _Gain = Shader.PropertyToID("_Gain");

		public static readonly int _Shadows = Shader.PropertyToID("_Shadows");

		public static readonly int _Midtones = Shader.PropertyToID("_Midtones");

		public static readonly int _Highlights = Shader.PropertyToID("_Highlights");

		public static readonly int _ShaHiLimits = Shader.PropertyToID("_ShaHiLimits");

		public static readonly int _SplitShadows = Shader.PropertyToID("_SplitShadows");

		public static readonly int _SplitHighlights = Shader.PropertyToID("_SplitHighlights");

		public static readonly int _CurveMaster = Shader.PropertyToID("_CurveMaster");

		public static readonly int _CurveRed = Shader.PropertyToID("_CurveRed");

		public static readonly int _CurveGreen = Shader.PropertyToID("_CurveGreen");

		public static readonly int _CurveBlue = Shader.PropertyToID("_CurveBlue");

		public static readonly int _CurveHueVsHue = Shader.PropertyToID("_CurveHueVsHue");

		public static readonly int _CurveHueVsSat = Shader.PropertyToID("_CurveHueVsSat");

		public static readonly int _CurveLumVsSat = Shader.PropertyToID("_CurveLumVsSat");

		public static readonly int _CurveSatVsSat = Shader.PropertyToID("_CurveSatVsSat");

		public static readonly int hdrColorSpace = Shader.PropertyToID("_HDRColorspace");

		public static readonly int hdrEncoding = Shader.PropertyToID("_HDREncoding");

		public static readonly int _Params = Shader.PropertyToID("_Params");

		public static readonly int _Params1 = Shader.PropertyToID("_Params1");

		public static readonly int _Curve = Shader.PropertyToID("_Curve");

		public static readonly int _BaseTex = Shader.PropertyToID("_BaseTex");

		public static readonly int _Bloom_Texture = Shader.PropertyToID("_Bloom_Texture");

		public static readonly int _Bloom_Params = Shader.PropertyToID("_Bloom_Params");

		public static readonly int _Bloom_RGBM = Shader.PropertyToID("_Bloom_RGBM");

		public static readonly int _LensDirt_Texture = Shader.PropertyToID("_LensDirt_Texture");

		public static readonly int _LensDirt_Params = Shader.PropertyToID("_LensDirt_Params");

		public static readonly int _LensDirt_Intensity = Shader.PropertyToID("_LensDirt_Intensity");

		public static readonly int _UserLut_Params = Shader.PropertyToID("_UserLut_Params");

		public static readonly int _InternalLut = Shader.PropertyToID("_InternalLut");

		public static readonly int _UserLut = Shader.PropertyToID("_UserLut");

		public static readonly int _SourceTexLowMip = Shader.PropertyToID("_SourceTexLowMip");

		public static readonly int _Metrics = Shader.PropertyToID("_Metrics");

		public static readonly int _AreaTexture = Shader.PropertyToID("_AreaTexture");

		public static readonly int _SearchTexture = Shader.PropertyToID("_SearchTexture");

		public static readonly int _EdgeTexture = Shader.PropertyToID("_EdgeTexture");

		public static readonly int _BlendTexture = Shader.PropertyToID("_BlendTexture");

		public static readonly int _StencilRef = Shader.PropertyToID("_StencilRef");

		public static readonly int _StencilMask = Shader.PropertyToID("_StencilMask");

		public static readonly int _FullCoCTexture = Shader.PropertyToID("_FullCoCTexture");

		public static readonly int _HalfCoCTexture = Shader.PropertyToID("_HalfCoCTexture");

		public static readonly int _DofTexture = Shader.PropertyToID("_DofTexture");

		public static readonly int _CoCParams = Shader.PropertyToID("_CoCParams");

		public static readonly int _BokehKernel = Shader.PropertyToID("_BokehKernel");

		public static readonly int _BokehConstants = Shader.PropertyToID("_BokehConstants");

		public static readonly int _PongTexture = Shader.PropertyToID("_PongTexture");

		public static readonly int _PingTexture = Shader.PropertyToID("_PingTexture");

		public static readonly int _DownSampleScaleFactor = Shader.PropertyToID("_DownSampleScaleFactor");

		public static readonly int _ColorTexture = Shader.PropertyToID("_ColorTexture");

		public static readonly int _Distortion_Params1 = Shader.PropertyToID("_Distortion_Params1");

		public static readonly int _Distortion_Params2 = Shader.PropertyToID("_Distortion_Params2");

		public static readonly int _Chroma_Params = Shader.PropertyToID("_Chroma_Params");

		public static readonly int _Vignette_Params1 = Shader.PropertyToID("_Vignette_Params1");

		public static readonly int _Vignette_Params2 = Shader.PropertyToID("_Vignette_Params2");

		public static readonly int _MotionVectorTexture = Shader.PropertyToID("_MotionVectorTexture");
	}

	private class UpdateCameraResolutionPassData
	{
		internal Vector2Int newCameraTargetSize;
	}

	private class FinalizePassData
	{
		internal TextureHandle source;

		internal TextureHandle destination;

		internal WaaaghCameraData cameraData;

		internal Material material;
	}

	public const int k_MaxPyramidSize = 16;

	public FrameState FrameState;

	public OverridesStack Overrides;

	public PostProcessGpuResources GpuResources;

	public int DitheringTextureIndex;

	public PostProcessResources Resources { get; }

	public PostProcessSettings Settings { get; }

	public PostProcessMaterialLibrary MatLib { get; }

	public StaticState StaticState { get; private set; }

	internal CameraStackTargets CameraStackTargets { get; private set; }

	public PostProcessor(PostProcessResources resources, PostProcessSettings settings, Material blitMaterial)
	{
		Resources = resources;
		Settings = settings;
		MatLib = new PostProcessMaterialLibrary(resources.Shaders, blitMaterial);
		SetupStaticState();
	}

	public void Dispose()
	{
		MatLib.Dispose();
	}

	private void SetupStaticState()
	{
		StaticState = new StaticState
		{
			BloomMipDownName = new string[16],
			BloomMipUpName = new string[16]
		};
		for (int i = 0; i < 16; i++)
		{
			StaticState.BloomMipUpName[i] = "_BloomMipUp" + i;
			StaticState.BloomMipDownName[i] = "_BloomMipDown" + i;
		}
		StaticState.BloomMipUp = new TextureHandle[16];
		StaticState.BloomMipDown = new TextureHandle[16];
		GraphicsFormat graphicsFormat = GraphicsFormat.B10G11R11_UFloatPack32;
		WaaaghPipelineAsset asset = WaaaghPipeline.Asset;
		if ((bool)asset)
		{
			graphicsFormat = WaaaghPipeline.MakeRenderTextureGraphicsFormat(asset.SupportsHDR, asset.HDRColorBufferPrecision, needsAlpha: false);
		}
		bool num = IsHDRFormat(graphicsFormat);
		bool defaultColorFormatUseAlpha = IsAlphaFormat(graphicsFormat);
		if (num)
		{
			StaticState.DefaultColorFormatUseAlpha = defaultColorFormatUseAlpha;
			StaticState.DefaultColorFormatUseRGBM = false;
			if (SystemInfo.IsFormatSupported(graphicsFormat, GraphicsFormatUsage.Blend))
			{
				StaticState.DefaultColorFormat = graphicsFormat;
			}
			else if (SystemInfo.IsFormatSupported(GraphicsFormat.B10G11R11_UFloatPack32, GraphicsFormatUsage.Blend))
			{
				StaticState.DefaultColorFormat = GraphicsFormat.B10G11R11_UFloatPack32;
				StaticState.DefaultColorFormatUseAlpha = false;
			}
			else
			{
				StaticState.DefaultColorFormat = ((QualitySettings.activeColorSpace == ColorSpace.Linear) ? GraphicsFormat.R8G8B8A8_SRGB : GraphicsFormat.R8G8B8A8_UNorm);
				StaticState.DefaultColorFormatUseRGBM = true;
			}
		}
		else
		{
			StaticState.DefaultColorFormat = ((QualitySettings.activeColorSpace == ColorSpace.Linear) ? GraphicsFormat.R8G8B8A8_SRGB : GraphicsFormat.R8G8B8A8_UNorm);
			StaticState.DefaultColorFormatUseAlpha = true;
			StaticState.DefaultColorFormatUseRGBM = false;
		}
		if (SystemInfo.IsFormatSupported(GraphicsFormat.R8G8_UNorm, GraphicsFormatUsage.Render) && SystemInfo.graphicsDeviceVendor.ToLowerInvariant().Contains("arm"))
		{
			StaticState.SMAAEdgeFormat = GraphicsFormat.R8G8_UNorm;
		}
		else
		{
			StaticState.SMAAEdgeFormat = GraphicsFormat.R8G8B8A8_UNorm;
		}
		if (SystemInfo.IsFormatSupported(GraphicsFormat.R16_UNorm, GraphicsFormatUsage.Blend))
		{
			StaticState.GaussianCoCFormat = GraphicsFormat.R16_UNorm;
		}
		else if (SystemInfo.IsFormatSupported(GraphicsFormat.R16_SFloat, GraphicsFormatUsage.Blend))
		{
			StaticState.GaussianCoCFormat = GraphicsFormat.R16_SFloat;
		}
		else
		{
			StaticState.GaussianCoCFormat = GraphicsFormat.R8_UNorm;
		}
	}

	private bool IsHDRFormat(GraphicsFormat format)
	{
		if (format != GraphicsFormat.B10G11R11_UFloatPack32 && !GraphicsFormatUtility.IsHalfFormat(format))
		{
			return GraphicsFormatUtility.IsFloatFormat(format);
		}
		return true;
	}

	private bool IsAlphaFormat(GraphicsFormat format)
	{
		return GraphicsFormatUtility.HasAlphaChannel(format);
	}

	internal void Setup(in SetupContext context)
	{
		SetupFrameState(in context);
		SetupOverrides();
	}

	private void SetupFrameState(in SetupContext context)
	{
		FrameState = new FrameState
		{
			EnabledForCamera = context.CameraData.postProcessEnabled
		};
		WaaaghPipelineAsset asset = WaaaghPipeline.Asset;
		FrameState.ColorGradingMode = (asset.SupportsHDR ? asset.PostProcessSettings.ColorGradingMode : ColorGradingMode.LowDynamicRange);
		if (context.CameraData.stackLastCameraOutputToHDR)
		{
			FrameState.ColorGradingMode = ColorGradingMode.HighDynamicRange;
		}
		FrameState.LutSize = asset.PostProcessSettings.ColorGradingLutSize;
		FrameState.UseFastSRGBLinearConversion = asset.PostProcessSettings.UseFastSRGBLinearConversion;
		FrameState.SupportScreenSpaceLensFlare = asset.PostProcessSettings.SupportScreenSpaceLensFlare;
		FrameState.SupportDataDrivenLensFlare = asset.PostProcessSettings.SupportDataDrivenLensFlare;
		RenderTextureDescriptor cameraTargetDescriptor = context.CameraData.cameraTargetDescriptor;
		cameraTargetDescriptor.useMipMap = false;
		cameraTargetDescriptor.autoGenerateMips = false;
		cameraTargetDescriptor.msaaSamples = 1;
		FrameState.Descriptor = cameraTargetDescriptor;
	}

	private void SetupOverrides()
	{
		VolumeStack stack = VolumeManager.instance.stack;
		Overrides = new OverridesStack
		{
			ChannelMixer = stack.GetComponent<ChannelMixer>(),
			ColorAdjustments = stack.GetComponent<ColorAdjustments>(),
			ColorCurves = stack.GetComponent<ColorCurves>(),
			LiftGammaGain = stack.GetComponent<LiftGammaGain>(),
			ShadowsMidtonesHighlights = stack.GetComponent<ShadowsMidtonesHighlights>(),
			SplitToning = stack.GetComponent<SplitToning>(),
			Tonemapping = stack.GetComponent<Tonemapping>(),
			WhiteBalance = stack.GetComponent<WhiteBalance>(),
			DepthOfField = stack.GetComponent<DepthOfField>(),
			ScreenSpaceLensFlare = stack.GetComponent<ScreenSpaceLensFlare>(),
			PaniniProjection = stack.GetComponent<PaniniProjection>(),
			MotionBlur = stack.GetComponent<MotionBlur>(),
			RadialBlur = stack.GetComponent<RadialBlur>(),
			BloomEnhanced = stack.GetComponent<BloomEnhanced>(),
			Bloom = stack.GetComponent<Bloom>(),
			ColorLookup = stack.GetComponent<ColorLookup>(),
			LensDistortion = stack.GetComponent<LensDistortion>(),
			ChromaticAberration = stack.GetComponent<ChromaticAberration>(),
			Vignette = stack.GetComponent<Vignette>(),
			FilmGrain = stack.GetComponent<FilmGrain>()
		};
	}

	private void SetupGpuResources(in RecordContext context)
	{
		CameraStackTargets = context.FrameResources.CameraStackTargets;
		GpuResources = new PostProcessGpuResources
		{
			ColorGradingLut = TextureHandle.nullHandle
		};
	}

	public void Record(in RecordContext context)
	{
		WaaaghCameraData cameraData = context.CameraData;
		bool enabledForCamera = FrameState.EnabledForCamera;
		bool hasFinalPass = enabledForCamera && (cameraData.antialiasing == AntialiasingMode.FastApproximateAntialiasing || (cameraData.imageScalingMode == ImageScalingMode.Upscaling && cameraData.upscalingFilter != 0) || (cameraData.IsTemporalAAEnabled() && cameraData.taaSettings.contrastAdaptiveSharpening > 0f));
		FrameState.HasFinalPass = hasFinalPass;
		if (enabledForCamera)
		{
			SetupGpuResources(in context);
			ColorGradingLut.Render(this, in context);
			MainPass(in context);
		}
		if (FrameState.HasFinalPass)
		{
			FinalPass(in context);
		}
	}

	private void MainPass(in RecordContext context)
	{
		WaaaghCameraData cameraData = context.CameraData;
		bool isSceneViewCamera = cameraData.isSceneViewCamera;
		bool num = cameraData.isStopNaNEnabled && MatLib.StopNaN != null;
		bool flag = cameraData.antialiasing == AntialiasingMode.SubpixelMorphologicalAntiAliasing;
		Material material = ((Overrides.DepthOfField.mode.value == DepthOfFieldMode.Gaussian) ? MatLib.GaussianDepthOfField : MatLib.BokehDepthOfField);
		bool flag2 = Overrides.DepthOfField.IsActive() && !isSceneViewCamera && material != null;
		if (!LensFlareCommonSRP.Instance.IsEmpty())
		{
			_ = FrameState.SupportDataDrivenLensFlare;
		}
		else
			_ = 0;
		bool flag3 = Overrides.ScreenSpaceLensFlare.IsActive() && FrameState.SupportScreenSpaceLensFlare;
		bool flag4 = Overrides.PaniniProjection.IsActive() && !isSceneViewCamera;
		bool flag5 = Overrides.MotionBlur.IsActive() && !isSceneViewCamera;
		bool flag6 = Overrides.RadialBlur.IsActive() && !isSceneViewCamera;
		bool flag7 = cameraData.IsTemporalAAEnabled();
		if (cameraData.antialiasing == AntialiasingMode.TemporalAntialiasing && !flag7)
		{
			TemporalAA.ValidateAndWarn(cameraData);
		}
		bool flag8 = flag7 && cameraData.IsSTPEnabled();
		ProfilingSampler sampler = WaaaghProfileId.RenderPostProcess.Sampler();
		context.RenderGraph.BeginProfilingSampler(sampler, ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\PostProcess\\PostProcessor.cs", 408);
		if (num)
		{
			StopNaN.Render(this, context.RenderGraph);
		}
		if (flag)
		{
			RenderGraph renderGraph = context.RenderGraph;
			AntialiasingQuality antialiasingQuality = cameraData.antialiasingQuality;
			TextureHandle cameraDepthBuffer = context.FrameResources.CameraStackTargets.Depth;
			SMAA.Render(this, renderGraph, antialiasingQuality, in cameraDepthBuffer);
		}
		if (flag2)
		{
			DOF.Render(this, context.RenderGraph, context.CameraData, in context.FrameResources.CameraAdditionalTargets.DepthCopy);
		}
		if (flag7)
		{
			if (flag8)
			{
				RenderGraph renderGraph2 = context.RenderGraph;
				WaaaghCameraData cameraData2 = context.CameraData;
				TextureHandle cameraDepthBuffer = context.FrameResources.CameraStackTargets.Depth;
				STPRecorder.Render(this, renderGraph2, cameraData2, in cameraDepthBuffer, in context.FrameResources.CameraAdditionalTargets.MotionVectors);
			}
			else
			{
				RenderGraph renderGraph3 = context.RenderGraph;
				WaaaghCameraData cameraData3 = context.CameraData;
				TextureHandle cameraDepthBuffer = context.FrameResources.CameraStackTargets.Depth;
				TAA.Render(this, renderGraph3, cameraData3, in cameraDepthBuffer, in context.FrameResources.CameraAdditionalTargets.MotionVectors);
			}
		}
		if (flag5)
		{
			RenderGraph renderGraph4 = context.RenderGraph;
			WaaaghCameraData cameraData4 = context.CameraData;
			ref readonly TextureHandle motionVectors = ref context.FrameResources.CameraAdditionalTargets.MotionVectors;
			TextureHandle cameraDepthBuffer = context.FrameResources.CameraStackTargets.Depth;
			MotionBlurRecorder.Render(this, renderGraph4, cameraData4, in motionVectors, in cameraDepthBuffer);
		}
		if (flag6)
		{
			RadialBlurRecorder.Render(this, context.RenderGraph);
		}
		if (flag4)
		{
			PaniniProjectionRecorder.Render(this, context.RenderGraph, cameraData.camera);
		}
		MatLib.Uber.shaderKeywords = null;
		bool flag9 = Overrides.Bloom.IsActive();
		if (Overrides.BloomEnhanced.IsActive())
		{
			BloomRecorder.RenderBloomEnhancedTexture(this, context.RenderGraph, out var destination);
			UberPost.SetupBloomEnhancedPass(this, context.RenderGraph, in destination);
		}
		else if (flag9 || flag3)
		{
			BloomRecorder.RenderBloomTexture(this, context.RenderGraph, out var destination2, cameraData.isAlphaOutputEnabled);
			UberPost.SetupBloomPass(this, context.RenderGraph, in destination2);
		}
		UberPost.SetupLensDistortion(this, isSceneViewCamera);
		UberPost.SetupChromaticAberration(this);
		UberPost.SetupVignette(this);
		UberPost.SetupGrain(this, cameraData);
		UberPost.SetupDithering(this, cameraData);
		if (Settings.UseFastSRGBLinearConversion)
		{
			MatLib.Uber.EnableKeyword("_USE_FAST_SRGB_LINEAR_CONVERSION");
		}
		if (cameraData.StackInfo.RequiredTargets == CameraRequiredTargets.Both && cameraData.upscalingFilter == ImageUpscalingFilter.STP)
		{
			context.FrameResources.CameraStackTargets.ReplaceScaledTargetsByUnscaled();
		}
		RenderTextureDescriptor compatibleDescriptor = GetCompatibleDescriptor();
		TextureHandle dest = RenderGraphUtility.CreateRenderGraphTexture(context.RenderGraph, compatibleDescriptor, "_UberPostResult", clear: false, FilterMode.Bilinear);
		UberPost.Render(this, context.RenderGraph, cameraData, in dest);
		context.RenderGraph.EndProfilingSampler(sampler, ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\PostProcess\\PostProcessor.cs", 562);
	}

	private void FinalPass(in RecordContext context)
	{
		Material finalPass = MatLib.FinalPass;
		finalPass.shaderKeywords = null;
		WaaaghCameraData cameraData = context.CameraData;
		FinalBlitSettings settings = FinalBlitSettings.Create();
		ProfilingSampler sampler = WaaaghProfileId.RenderPostProcessFinal.Sampler();
		context.RenderGraph.BeginProfilingSampler(sampler, ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\PostProcess\\PostProcessor.cs", 574);
		if (Overrides.FilmGrain.IsActive())
		{
			finalPass.EnableKeyword(ShaderKeywordStrings.FilmGrain);
			PostProcessUtils.ConfigureFilmGrain(Resources, Overrides.FilmGrain, cameraData.pixelWidth, cameraData.pixelHeight, finalPass);
		}
		if (cameraData.isDitheringEnabled)
		{
			finalPass.EnableKeyword(ShaderKeywordStrings.Dithering);
			DitheringTextureIndex = PostProcessUtils.ConfigureDithering(Resources, DitheringTextureIndex, cameraData.pixelWidth, cameraData.pixelHeight, finalPass);
		}
		settings.isAlphaOutputEnabled = cameraData.isAlphaOutputEnabled;
		settings.isFxaaEnabled = cameraData.antialiasing == AntialiasingMode.FastApproximateAntialiasing;
		settings.isFsrEnabled = cameraData.imageScalingMode == ImageScalingMode.Upscaling && cameraData.upscalingFilter == ImageUpscalingFilter.FSR;
		settings.isTaaSharpeningEnabled = cameraData.IsTemporalAAEnabled() && cameraData.taaSettings.contrastAdaptiveSharpening > 0f && !settings.isFsrEnabled && !cameraData.IsSTPEnabled();
		RenderTextureDescriptor cameraTargetDescriptor = cameraData.cameraTargetDescriptor;
		cameraTargetDescriptor.msaaSamples = 1;
		cameraTargetDescriptor.depthBufferBits = 0;
		if (!settings.requireHDROutput)
		{
			cameraTargetDescriptor.graphicsFormat = WaaaghPipeline.MakeUnormRenderTextureGraphicsFormat();
		}
		TextureHandle destination = RenderGraphUtility.CreateRenderGraphTexture(context.RenderGraph, cameraTargetDescriptor, "scalingSetupTarget", clear: true);
		RenderTextureDescriptor desc = cameraTargetDescriptor;
		desc.width = cameraData.pixelWidth;
		desc.height = cameraData.pixelHeight;
		TextureHandle dest = RenderGraphUtility.CreateRenderGraphTexture(context.RenderGraph, desc, "_UpscaledTexture", clear: true);
		if (cameraData.imageScalingMode != 0)
		{
			if (settings.isFxaaEnabled || settings.isFsrEnabled)
			{
				FinalPost.RenderFinalSetup(this, context.RenderGraph, cameraData, in destination, ref settings);
				settings.isFxaaEnabled = false;
			}
			switch (cameraData.imageScalingMode)
			{
			case ImageScalingMode.Upscaling:
				switch (cameraData.upscalingFilter)
				{
				case ImageUpscalingFilter.Point:
					if (!settings.isTaaSharpeningEnabled)
					{
						finalPass.EnableKeyword("_POINT_SAMPLING");
					}
					break;
				case ImageUpscalingFilter.FSR:
					FSR1Recorder.Render(this, context.RenderGraph, cameraData, in dest, settings.isAlphaOutputEnabled);
					break;
				}
				break;
			case ImageScalingMode.Downscaling:
				settings.isTaaSharpeningEnabled = false;
				break;
			}
		}
		else if (settings.isFxaaEnabled)
		{
			finalPass.EnableKeyword(ShaderKeywordStrings.Fxaa);
		}
		if (cameraData.StackInfo.RequiredTargets == CameraRequiredTargets.Both && cameraData.upscalingFilter == ImageUpscalingFilter.FSR)
		{
			context.FrameResources.CameraStackTargets.ReplaceScaledTargetsByUnscaled();
		}
		TextureHandle postProcessingTarget = context.FrameResources.CameraStackTargets.Color;
		FinalPost.RenderFinalBlit(this, context.RenderGraph, cameraData, in postProcessingTarget, ref settings);
		CameraStackTargets.SetCurrentPostProcessSource(postProcessingTarget);
		context.RenderGraph.EndProfilingSampler(sampler, ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\PostProcess\\PostProcessor.cs", 730);
	}

	internal RenderTextureDescriptor GetCompatibleDescriptor()
	{
		return GetCompatibleDescriptor(FrameState.Descriptor, FrameState.Descriptor.width, FrameState.Descriptor.height, FrameState.Descriptor.graphicsFormat);
	}

	internal static RenderTextureDescriptor GetCompatibleDescriptor(RenderTextureDescriptor desc)
	{
		return GetCompatibleDescriptor(desc, desc.width, desc.height, desc.graphicsFormat);
	}

	internal static RenderTextureDescriptor GetCompatibleDescriptor(RenderTextureDescriptor desc, int width, int height, GraphicsFormat format, DepthBits depthBufferBits = DepthBits.None)
	{
		desc.depthBufferBits = (int)depthBufferBits;
		desc.msaaSamples = 1;
		desc.width = width;
		desc.height = height;
		desc.graphicsFormat = format;
		return desc;
	}

	public static void ScaleViewportAndBlit(RasterCommandBuffer cmd, RTHandle sourceTextureHdl, RTHandle dest, WaaaghCameraData cameraData, Material material)
	{
		Vector4 finalBlitScaleBias = RenderingUtils.GetFinalBlitScaleBias(sourceTextureHdl, dest, cameraData);
		RenderTargetIdentifier renderTargetIdentifier = BuiltinRenderTextureType.CameraTarget;
		if (dest.nameID == renderTargetIdentifier || cameraData.targetTexture != null)
		{
			cmd.SetViewport(cameraData.pixelRect);
		}
		Blitter.BlitTexture(cmd, sourceTextureHdl, finalBlitScaleBias, material, 0);
	}

	public static void UpdateCameraResolution(PostProcessor processor, RenderGraph renderGraph, WaaaghCameraData cameraData, Vector2Int newCameraTargetSize)
	{
		processor.FrameState.Descriptor.width = newCameraTargetSize.x;
		processor.FrameState.Descriptor.height = newCameraTargetSize.y;
		cameraData.cameraTargetDescriptor.width = newCameraTargetSize.x;
		cameraData.cameraTargetDescriptor.height = newCameraTargetSize.y;
		UpdateCameraResolutionPassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = renderGraph.AddUnsafePass<UpdateCameraResolutionPassData>("Update Camera Resolution", out passData, WaaaghProfileId.UpdateCameraResolution.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\PostProcess\\PostProcessor.cs", 778);
		passData.newCameraTargetSize = newCameraTargetSize;
		unsafeRenderGraphBuilder.AllowGlobalStateModification(value: true);
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(UpdateCameraResolutionPassData data, UnsafeGraphContext ctx)
		{
			ctx.cmd.SetGlobalVector(ShaderPropertyId._ScreenSize, new Vector4(data.newCameraTargetSize.x, data.newCameraTargetSize.y, 1f / (float)data.newCameraTargetSize.x, 1f / (float)data.newCameraTargetSize.y));
		});
	}

	internal void FinalizeToTarget(in RecordContext context)
	{
		CameraStackTargets cameraStackTargets = context.FrameResources.CameraStackTargets;
		if (EqualsValueType(cameraStackTargets.CurrentPostProcessSource, cameraStackTargets.Color))
		{
			return;
		}
		FinalizePassData passData;
		using (IRasterRenderGraphBuilder rasterRenderGraphBuilder = context.RenderGraph.AddRasterRenderPass<FinalizePassData>("PostProcess Finalize To Target", out passData, ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\PostProcess\\PostProcessor.cs", 820))
		{
			TextureHandle input = cameraStackTargets.CurrentPostProcessSource;
			rasterRenderGraphBuilder.UseTexture(in input);
			passData.source = input;
			rasterRenderGraphBuilder.SetRenderAttachment(cameraStackTargets.Color, 0);
			passData.destination = cameraStackTargets.Color;
			passData.cameraData = context.CameraData;
			passData.material = MatLib.BlitMaterial;
			rasterRenderGraphBuilder.SetRenderFunc(delegate(FinalizePassData data, RasterGraphContext ctx)
			{
				ScaleViewportAndBlit(ctx.cmd, data.source, data.destination, data.cameraData, data.material);
			});
		}
		cameraStackTargets.SetCurrentPostProcessSource(cameraStackTargets.Color);
	}

	public unsafe static bool EqualsValueType<T>(T a, T b) where T : unmanaged
	{
		return UnsafeUtility.MemCmp(&a, &b, sizeof(T)) == 0;
	}
}
