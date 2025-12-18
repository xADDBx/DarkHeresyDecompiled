using System;
using System.Runtime.CompilerServices;
using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.PipelineResources;
using Owlcat.Runtime.Visual.Waaagh.PostProcess;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.PostProcess;

public class PostProcessPass : ScriptableRenderPass
{
	private class Materials : MaterialCollection<PostProcessRuntimeShaders>
	{
		public Material StopNaN;

		public Material SubpixelMorphologicalAntialiasing;

		public Material GaussianDepthOfField;

		public Material GaussianDepthOfFieldCoC;

		public Material BokehDepthOfField;

		public Material BokehDepthOfFieldCoC;

		public Material CameraMotionBlur;

		public Material PaniniProjection;

		public Material Bloom;

		public Material BloomEnhanced;

		public Material[] bloomUpsample;

		public Material TemporalAntialiasing;

		public Material Uber;

		public Material LensFlareDataDriven;

		public Material LensFlareScreenSpace;

		public Material RadialBlur;

		public Material Daltonization;

		public override void Init(PostProcessRuntimeShaders resources)
		{
			StopNaN = Load(resources.StopNanPS);
			SubpixelMorphologicalAntialiasing = Load(resources.SubpixelMorphologicalAntialiasingPS);
			GaussianDepthOfField = Load(resources.GaussianDepthOfFieldPS);
			GaussianDepthOfFieldCoC = Load(resources.GaussianDepthOfFieldPS);
			BokehDepthOfField = Load(resources.BokehDepthOfFieldPS);
			BokehDepthOfFieldCoC = Load(resources.BokehDepthOfFieldPS);
			CameraMotionBlur = Load(resources.CameraMotionBlurPS);
			PaniniProjection = Load(resources.PaniniProjectionPS);
			Bloom = Load(resources.BloomPS);
			BloomEnhanced = Load(resources.BloomEnhancedPS);
			TemporalAntialiasing = Load(resources.TemporalAntialiasingPS);
			Uber = Load(resources.UberPostPS);
			LensFlareDataDriven = Load(resources.LensFlareDataDrivenPS);
			LensFlareScreenSpace = Load(resources.LensFlareScreenSpacePS);
			RadialBlur = Load(resources.RadialBlurPS);
			Daltonization = Load(resources.DaltonizationPS);
			bloomUpsample = new Material[16];
			for (uint num = 0u; num < 16; num++)
			{
				bloomUpsample[num] = Load(resources.BloomPS);
			}
		}
	}

	internal static class ShaderConstants
	{
		public static readonly int _TempTarget = Shader.PropertyToID("_TempTarget");

		public static readonly int _TempTarget2 = Shader.PropertyToID("_TempTarget2");

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

		public static readonly int _Metrics = Shader.PropertyToID("_Metrics");

		public static readonly int _AreaTexture = Shader.PropertyToID("_AreaTexture");

		public static readonly int _SearchTexture = Shader.PropertyToID("_SearchTexture");

		public static readonly int _EdgeTexture = Shader.PropertyToID("_EdgeTexture");

		public static readonly int _BlendTexture = Shader.PropertyToID("_BlendTexture");

		public static readonly int _ColorTexture = Shader.PropertyToID("_ColorTexture");

		public static readonly int _Params = Shader.PropertyToID("_Params");

		public static readonly int _Params1 = Shader.PropertyToID("_Params1");

		public static readonly int _Curve = Shader.PropertyToID("_Curve");

		public static readonly int _BaseTex = Shader.PropertyToID("_BaseTex");

		public static readonly int _SourceTexLowMip = Shader.PropertyToID("_SourceTexLowMip");

		public static readonly int _Bloom_Params = Shader.PropertyToID("_Bloom_Params");

		public static readonly int _Bloom_RGBM = Shader.PropertyToID("_Bloom_RGBM");

		public static readonly int _Bloom_Texture = Shader.PropertyToID("_Bloom_Texture");

		public static readonly int _LensDirt_Texture = Shader.PropertyToID("_LensDirt_Texture");

		public static readonly int _LensDirt_Params = Shader.PropertyToID("_LensDirt_Params");

		public static readonly int _LensDirt_Intensity = Shader.PropertyToID("_LensDirt_Intensity");

		public static readonly int _Distortion_Params1 = Shader.PropertyToID("_Distortion_Params1");

		public static readonly int _Distortion_Params2 = Shader.PropertyToID("_Distortion_Params2");

		public static readonly int _Chroma_Params = Shader.PropertyToID("_Chroma_Params");

		public static readonly int _Vignette_Params1 = Shader.PropertyToID("_Vignette_Params1");

		public static readonly int _Vignette_Params2 = Shader.PropertyToID("_Vignette_Params2");

		public static readonly int _Vignette_ParamsXR = Shader.PropertyToID("_Vignette_ParamsXR");

		public static readonly int _Lut_Params = Shader.PropertyToID("_Lut_Params");

		public static readonly int _UserLut_Params = Shader.PropertyToID("_UserLut_Params");

		public static readonly int _InternalLut = Shader.PropertyToID("_InternalLut");

		public static readonly int _UserLut = Shader.PropertyToID("_UserLut");

		public static readonly int _DownSampleScaleFactor = Shader.PropertyToID("_DownSampleScaleFactor");

		public static readonly int _FlareOcclusionRemapTex = Shader.PropertyToID("_FlareOcclusionRemapTex");

		public static readonly int _FlareOcclusionTex = Shader.PropertyToID("_FlareOcclusionTex");

		public static readonly int _FlareOcclusionIndex = Shader.PropertyToID("_FlareOcclusionIndex");

		public static readonly int _FlareTex = Shader.PropertyToID("_FlareTex");

		public static readonly int _FlareColorValue = Shader.PropertyToID("_FlareColorValue");

		public static readonly int _FlareData0 = Shader.PropertyToID("_FlareData0");

		public static readonly int _FlareData1 = Shader.PropertyToID("_FlareData1");

		public static readonly int _FlareData2 = Shader.PropertyToID("_FlareData2");

		public static readonly int _FlareData3 = Shader.PropertyToID("_FlareData3");

		public static readonly int _FlareData4 = Shader.PropertyToID("_FlareData4");

		public static readonly int _FlareData5 = Shader.PropertyToID("_FlareData5");

		public static readonly int _FullscreenProjMat = Shader.PropertyToID("_FullscreenProjMat");

		public static int[] _BloomMipUp;

		public static int[] _BloomMipDown;

		public static readonly int _MotionVectorTexture = Shader.PropertyToID("_MotionVectorTexture");
	}

	private class StopNaNsPassData
	{
		internal TextureHandle stopNaNTarget;

		internal TextureHandle sourceTexture;

		internal Material stopNaN;
	}

	private class SMAASetupPassData
	{
		internal Vector4 metrics;

		internal Texture2D areaTexture;

		internal Texture2D searchTexture;

		internal float stencilRef;

		internal float stencilMask;

		internal AntialiasingQuality antialiasingQuality;

		internal Material material;
	}

	private class SMAAPassData
	{
		internal TextureHandle destinationTexture;

		internal TextureHandle sourceTexture;

		internal TextureHandle depthStencilTexture;

		internal TextureHandle blendTexture;

		internal Material material;
	}

	private class DoFGaussianSetupPassData
	{
		internal TextureHandle source;

		internal int downSample;

		internal Vector3 cocParams;

		internal bool highQualitySamplingValue;

		internal Material material;

		internal Material materialCoC;
	}

	private class DoFGaussianPassData
	{
		internal TextureHandle cocTexture;

		internal TextureHandle colorTexture;

		internal TextureHandle sourceTexture;

		internal TextureHandle depthTexture;

		internal Material material;
	}

	private class DoFBokehSetupPassData
	{
		internal Vector4[] bokehKernel;

		internal TextureHandle source;

		internal int downSample;

		internal float uvMargin;

		internal Vector4 cocParams;

		internal bool useFastSRGBLinearConversion;

		internal Material material;

		internal Material materialCoC;
	}

	private class DoFBokehPassData
	{
		internal TextureHandle cocTexture;

		internal TextureHandle dofTexture;

		internal TextureHandle sourceTexture;

		internal TextureHandle depthTexture;

		internal Material material;
	}

	private class MotionBlurPassData
	{
		internal TextureHandle destinationTexture;

		internal TextureHandle sourceTexture;

		internal TextureHandle motionVectors;

		internal Material material;

		internal int passIndex;

		internal Camera camera;

		internal XRPass xr;

		internal float intensity;

		internal float clamp;

		internal bool enableAlphaOutput;
	}

	private class RadialBlurPassData
	{
		public TextureHandle Source;

		public TextureHandle Destination;

		public Material Material;
	}

	private class PaniniProjectionPassData
	{
		internal TextureHandle destinationTexture;

		internal TextureHandle sourceTexture;

		internal RenderTextureDescriptor sourceTextureDesc;

		internal Material material;

		internal Vector4 paniniParams;

		internal bool isPaniniGeneric;
	}

	private class UberSetupBloomPassData
	{
		internal Vector4 bloomParams;

		internal Vector4 dirtScaleOffset;

		internal float dirtIntensity;

		internal Texture dirtTexture;

		internal bool highQualityFilteringValue;

		internal bool useRGBM;

		internal TextureHandle bloomTexture;

		internal Material uberMaterial;
	}

	private class BloomPassData
	{
		internal int mipCount;

		internal Material material;

		internal Material[] upsampleMaterials;

		internal TextureHandle sourceTexture;

		internal TextureHandle[] bloomMipUp;

		internal TextureHandle[] bloomMipDown;
	}

	internal struct BloomMaterialParams
	{
		internal Vector4 parameters;

		internal bool highQualityFiltering;

		internal bool useRGBM;

		internal bool enableAlphaOutput;

		internal bool Equals(ref BloomMaterialParams other)
		{
			if (parameters == other.parameters && highQualityFiltering == other.highQualityFiltering && useRGBM == other.useRGBM)
			{
				return enableAlphaOutput == other.enableAlphaOutput;
			}
			return false;
		}
	}

	internal class BloomEnhancedPassData
	{
		public Material Material;

		public TextureHandle Source;

		public TextureHandle[] BloomMipDown;

		public TextureHandle[] BloomMipUp;

		public int MipCount;
	}

	private class UpdateCameraResolutionPassData
	{
		internal Vector2Int newCameraTargetSize;
	}

	private class LensFlarePassData
	{
		internal TextureHandle destinationTexture;

		internal RenderTextureDescriptor sourceDescriptor;

		internal WaaaghCameraData cameraData;

		internal Material material;

		internal Rect viewport;

		internal float paniniDistance;

		internal float paniniCropToFit;

		internal float width;

		internal float height;

		internal bool usePanini;

		internal XRPass DefaultXr;
	}

	private class LensFlareScreenSpacePassData
	{
		internal TextureHandle destinationTexture;

		internal TextureHandle streakTmpTexture;

		internal TextureHandle streakTmpTexture2;

		internal TextureHandle originalBloomTexture;

		internal TextureHandle screenSpaceLensFlareBloomMipTexture;

		internal TextureHandle result;

		internal RenderTextureDescriptor sourceDescriptor;

		internal Camera camera;

		internal Material material;

		internal ScreenSpaceLensFlare lensFlareScreenSpace;

		internal int downsample;
	}

	private class UberPostPassData
	{
		internal TextureHandle destinationTexture;

		internal TextureHandle sourceTexture;

		internal TextureHandle lutTexture;

		internal Vector4 lutParams;

		internal TextureHandle userLutTexture;

		internal Vector4 userLutParams;

		internal Material material;

		internal WaaaghCameraData cameraData;

		internal PostProcessResources resources;

		internal Tonemapping tonemapping;

		internal TonemappingMode toneMappingMode;

		internal bool isHdrGrading;

		internal bool isBackbuffer;

		internal bool enableAlphaOutput;
	}

	private PostProcessResources m_Resources;

	private RenderTextureDescriptor m_Descriptor;

	private MaterialLibrary<Materials, PostProcessRuntimeShaders> m_MaterialLibrary;

	private Materials m_Materials;

	private bool m_UseFastSRGBLinearConversion;

	private bool m_SupportScreenSpaceLensFlare;

	private bool m_SupportDataDrivenLensFlare;

	private readonly GraphicsFormat m_SMAAEdgeFormat;

	private readonly GraphicsFormat m_GaussianCoCFormat;

	private readonly GraphicsFormat m_DefaultColorFormat;

	private const int k_MaxPyramidSize = 16;

	private int m_DitheringTextureIndex;

	private bool m_DefaultColorFormatIsAlpha;

	private bool m_DefaultColorFormatUseRGBM;

	private int m_BokehHash;

	private float m_BokehMaxRadius;

	private float m_BokehRCPAspect;

	private Vector4[] m_BokehKernel;

	private XRPass m_DefaultXr;

	private bool m_EnableColorEncodingIfNeeded;

	private bool m_HasFinalPass;

	private BloomMaterialParams m_BloomParamsPrev;

	private RTHandle[] m_BloomMipDown;

	private RTHandle[] m_BloomMipUp;

	private TextureHandle[] _BloomMipUp;

	private TextureHandle[] _BloomMipDown;

	private Material m_BlitMaterial;

	private DepthOfField m_DepthOfField;

	private Bloom m_Bloom;

	private BloomEnhanced m_BloomEnhanced;

	private Tonemapping m_Tonemapping;

	private ColorAdjustments m_ColorAdjustments;

	private ScreenSpaceLensFlare m_LensFlareScreenSpace;

	private PaniniProjection m_PaniniProjection;

	private LensDistortion m_LensDistortion;

	private ChromaticAberration m_ChromaticAberration;

	private Vignette m_Vignette;

	private FilmGrain m_FilmGrain;

	private ColorLookup m_ColorLookup;

	private MotionBlur m_MotionBlur;

	private RadialBlur m_RadialBlur;

	internal static readonly int k_ShaderPropertyId_ViewProjM = Shader.PropertyToID("_ViewProjM");

	internal static readonly int k_ShaderPropertyId_PrevViewProjM = Shader.PropertyToID("_PrevViewProjM");

	internal static readonly int k_ShaderPropertyId_ViewProjMStereo = Shader.PropertyToID("_ViewProjMStereo");

	internal static readonly int k_ShaderPropertyId_PrevViewProjMStereo = Shader.PropertyToID("_PrevViewProjMStereo");

	private static readonly int k_ShaderPropertyId_RadialBlurCenter = Shader.PropertyToID("_RadialBlurCenter");

	private static readonly int k_ShaderPropertyId_RadialBlurStrength = Shader.PropertyToID("_RadialBlurStrength");

	private static readonly int k_ShaderPropertyId_RadialBlurWidth = Shader.PropertyToID("_RadialBlurWidth");

	private const string _TemporalAATargetName = "_TemporalAATarget";

	private const string _UpscaledColorTargetName = "_UpscaledColorTarget";

	public bool HasFinalPass
	{
		get
		{
			return m_HasFinalPass;
		}
		set
		{
			m_HasFinalPass = value;
		}
	}

	public override string Name => "PostProcessPass";

	public PostProcessPass(RenderPassEvent evt, PostProcessResources resources, ref PostProcessParams parameters)
		: base(evt)
	{
		m_Resources = resources;
		m_MaterialLibrary = new MaterialLibrary<Materials, PostProcessRuntimeShaders>(resources.Shaders);
		m_BlitMaterial = parameters.BlitMaterial;
		m_DefaultXr = XRPass.CreateDefault(default(XRPassCreateInfo));
		ShaderConstants._BloomMipUp = new int[16];
		ShaderConstants._BloomMipDown = new int[16];
		m_BloomMipUp = new RTHandle[16];
		m_BloomMipDown = new RTHandle[16];
		_BloomMipUp = new TextureHandle[16];
		_BloomMipDown = new TextureHandle[16];
		for (int i = 0; i < 16; i++)
		{
			ShaderConstants._BloomMipUp[i] = Shader.PropertyToID("_BloomMipUp" + i);
			ShaderConstants._BloomMipDown[i] = Shader.PropertyToID("_BloomMipDown" + i);
			m_BloomMipUp[i] = RTHandles.Alloc(ShaderConstants._BloomMipUp[i], "_BloomMipUp" + i);
			m_BloomMipDown[i] = RTHandles.Alloc(ShaderConstants._BloomMipDown[i], "_BloomMipDown" + i);
		}
		bool num = IsHDRFormat(parameters.RequestColorFormat);
		bool defaultColorFormatIsAlpha = IsAlphaFormat(parameters.RequestColorFormat);
		if (num)
		{
			m_DefaultColorFormatIsAlpha = defaultColorFormatIsAlpha;
			m_DefaultColorFormatUseRGBM = false;
			if (SystemInfo.IsFormatSupported(parameters.RequestColorFormat, GraphicsFormatUsage.Blend))
			{
				m_DefaultColorFormat = parameters.RequestColorFormat;
			}
			else if (SystemInfo.IsFormatSupported(GraphicsFormat.B10G11R11_UFloatPack32, GraphicsFormatUsage.Blend))
			{
				m_DefaultColorFormat = GraphicsFormat.B10G11R11_UFloatPack32;
				m_DefaultColorFormatIsAlpha = false;
			}
			else
			{
				m_DefaultColorFormat = ((QualitySettings.activeColorSpace == ColorSpace.Linear) ? GraphicsFormat.R8G8B8A8_SRGB : GraphicsFormat.R8G8B8A8_UNorm);
				m_DefaultColorFormatUseRGBM = true;
			}
		}
		else
		{
			m_DefaultColorFormat = ((QualitySettings.activeColorSpace == ColorSpace.Linear) ? GraphicsFormat.R8G8B8A8_SRGB : GraphicsFormat.R8G8B8A8_UNorm);
			m_DefaultColorFormatIsAlpha = true;
			m_DefaultColorFormatUseRGBM = false;
		}
		if (SystemInfo.IsFormatSupported(GraphicsFormat.R8G8_UNorm, GraphicsFormatUsage.Render) && SystemInfo.graphicsDeviceVendor.ToLowerInvariant().Contains("arm"))
		{
			m_SMAAEdgeFormat = GraphicsFormat.R8G8_UNorm;
		}
		else
		{
			m_SMAAEdgeFormat = GraphicsFormat.R8G8B8A8_UNorm;
		}
		if (SystemInfo.IsFormatSupported(GraphicsFormat.R16_UNorm, GraphicsFormatUsage.Blend))
		{
			m_GaussianCoCFormat = GraphicsFormat.R16_UNorm;
		}
		else if (SystemInfo.IsFormatSupported(GraphicsFormat.R16_SFloat, GraphicsFormatUsage.Blend))
		{
			m_GaussianCoCFormat = GraphicsFormat.R16_SFloat;
		}
		else
		{
			m_GaussianCoCFormat = GraphicsFormat.R8_UNorm;
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

	public override void RecordRenderGraph(ContextContainer frameData)
	{
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		WaaaghRenderingData waaaghRenderingData = frameData.Get<WaaaghRenderingData>();
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		WaaaghPostProcessingData waaaghPostProcessingData = frameData.Get<WaaaghPostProcessingData>();
		m_Materials = m_MaterialLibrary.Get(waaaghCameraData.camera);
		VolumeStack stack = VolumeManager.instance.stack;
		m_DepthOfField = stack.GetComponent<DepthOfField>();
		m_Bloom = stack.GetComponent<Bloom>();
		m_BloomEnhanced = stack.GetComponent<BloomEnhanced>();
		m_Tonemapping = stack.GetComponent<Tonemapping>();
		m_ColorAdjustments = stack.GetComponent<ColorAdjustments>();
		m_LensFlareScreenSpace = stack.GetComponent<ScreenSpaceLensFlare>();
		m_PaniniProjection = stack.GetComponent<PaniniProjection>();
		m_ChromaticAberration = stack.GetComponent<ChromaticAberration>();
		m_Vignette = stack.GetComponent<Vignette>();
		m_FilmGrain = stack.GetComponent<FilmGrain>();
		m_ColorLookup = stack.GetComponent<ColorLookup>();
		m_LensDistortion = stack.GetComponent<LensDistortion>();
		m_MotionBlur = stack.GetComponent<MotionBlur>();
		m_RadialBlur = stack.GetComponent<RadialBlur>();
		m_Descriptor = waaaghCameraData.cameraTargetDescriptor;
		m_Descriptor.useMipMap = false;
		m_Descriptor.autoGenerateMips = false;
		m_UseFastSRGBLinearConversion = waaaghPostProcessingData.useFastSRGBLinearConversion;
		m_SupportScreenSpaceLensFlare = waaaghPostProcessingData.supportScreenSpaceLensFlare;
		m_SupportDataDrivenLensFlare = waaaghPostProcessingData.supportDataDrivenLensFlare;
		m_EnableColorEncodingIfNeeded = false;
		bool isSceneViewCamera = waaaghCameraData.isSceneViewCamera;
		TextureHandle activeCameraColor = waaaghResourceData.CameraColorBuffer;
		bool num = waaaghCameraData.isStopNaNEnabled && m_Materials.StopNaN != null;
		bool flag = waaaghCameraData.antialiasing == AntialiasingMode.SubpixelMorphologicalAntiAliasing;
		Material material = ((m_DepthOfField.mode.value == DepthOfFieldMode.Gaussian) ? m_Materials.GaussianDepthOfField : m_Materials.BokehDepthOfField);
		bool flag2 = m_DepthOfField.IsActive() && !isSceneViewCamera && material != null;
		bool flag3 = !LensFlareCommonSRP.Instance.IsEmpty() && m_SupportDataDrivenLensFlare;
		bool flag4 = m_LensFlareScreenSpace.IsActive() && m_SupportScreenSpaceLensFlare;
		bool flag5 = m_PaniniProjection.IsActive() && !isSceneViewCamera;
		bool flag6 = m_MotionBlur.IsActive() && !isSceneViewCamera;
		bool flag7 = m_RadialBlur.IsActive() && !isSceneViewCamera;
		bool flag8 = waaaghCameraData.IsTemporalAAEnabled();
		if (waaaghCameraData.antialiasing == AntialiasingMode.TemporalAntialiasing && !flag8)
		{
			TemporalAA.ValidateAndWarn(waaaghCameraData);
		}
		bool flag9 = flag8 && waaaghCameraData.IsSTPEnabled();
		ProfilingSampler sampler = ProfilingSampler.Get(WaaaghProfileId.RenderPostProcess);
		waaaghRenderingData.RenderGraph.BeginProfilingSampler(sampler, ".\\Library\\PackageCache\\com.owlcat.visual@f3d4bf622f68\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 360);
		if (num)
		{
			RenderStopNaN(waaaghRenderingData.RenderGraph, waaaghCameraData.cameraTargetDescriptor, in activeCameraColor, out var stopNaNTarget);
			activeCameraColor = stopNaNTarget;
		}
		if (flag)
		{
			RenderSMAA(waaaghRenderingData.RenderGraph, waaaghResourceData, waaaghCameraData.antialiasingQuality, in activeCameraColor, out var SMAATarget);
			activeCameraColor = SMAATarget;
		}
		if (flag2)
		{
			RenderDoF(waaaghRenderingData.RenderGraph, waaaghResourceData, waaaghCameraData, in activeCameraColor, out var destination);
			activeCameraColor = destination;
		}
		if (flag8)
		{
			if (flag9)
			{
				RenderSTP(waaaghRenderingData.RenderGraph, waaaghResourceData, waaaghCameraData, ref activeCameraColor, out var destination2);
				activeCameraColor = destination2;
			}
			else
			{
				RenderTemporalAA(waaaghRenderingData.RenderGraph, waaaghResourceData, waaaghCameraData, ref activeCameraColor, out var destination3);
				activeCameraColor = destination3;
			}
		}
		if (flag6)
		{
			RenderMotionBlur(waaaghRenderingData.RenderGraph, waaaghResourceData, waaaghCameraData, in activeCameraColor, out var destination4);
			activeCameraColor = destination4;
		}
		if (flag7)
		{
			RenderRadialBlur(waaaghRenderingData.RenderGraph, waaaghResourceData, waaaghCameraData, in activeCameraColor, out var destination5);
			activeCameraColor = destination5;
		}
		if (flag5)
		{
			RenderPaniniProjection(waaaghRenderingData.RenderGraph, waaaghCameraData.camera, in activeCameraColor, out var destination6);
			activeCameraColor = destination6;
		}
		m_Materials.Uber.shaderKeywords = null;
		bool flag10 = m_Bloom.IsActive();
		if (m_BloomEnhanced.IsActive())
		{
			RenderBloomEnhancedTexture(waaaghRenderingData.RenderGraph, in activeCameraColor, out var destination7, waaaghCameraData.isAlphaOutputEnabled);
			if (flag4)
			{
				int num2 = Mathf.Clamp(m_LensFlareScreenSpace.bloomMip.value, 0, m_Bloom.maxIterations.value / 2);
				destination7 = RenderLensFlareScreenSpace(waaaghRenderingData.RenderGraph, waaaghCameraData.camera, in activeCameraColor, _BloomMipUp[0], _BloomMipUp[num2], enableXR: false);
			}
			UberPostSetupBloomEnhancedPass(waaaghRenderingData.RenderGraph, in destination7, m_Materials.Uber);
		}
		else if (flag10 || flag4)
		{
			RenderBloomTexture(waaaghRenderingData.RenderGraph, in activeCameraColor, out var destination8, waaaghCameraData.isAlphaOutputEnabled);
			if (flag4)
			{
				int num3 = Mathf.Clamp(m_LensFlareScreenSpace.bloomMip.value, 0, m_Bloom.maxIterations.value / 2);
				destination8 = RenderLensFlareScreenSpace(waaaghRenderingData.RenderGraph, waaaghCameraData.camera, in activeCameraColor, _BloomMipUp[0], _BloomMipUp[num3], enableXR: false);
			}
			UberPostSetupBloomPass(waaaghRenderingData.RenderGraph, in destination8, m_Materials.Uber);
		}
		if (flag3)
		{
			LensFlareDataDrivenComputeOcclusion(waaaghRenderingData.RenderGraph, waaaghResourceData, waaaghCameraData);
			RenderLensFlareDataDriven(waaaghRenderingData.RenderGraph, waaaghResourceData, waaaghCameraData, in activeCameraColor);
		}
		SetupLensDistortion(m_Materials.Uber, isSceneViewCamera);
		SetupChromaticAberration(m_Materials.Uber);
		SetupVignette(m_Materials.Uber, m_DefaultXr);
		SetupGrain(waaaghCameraData, m_Materials.Uber);
		SetupDithering(waaaghCameraData, m_Materials.Uber);
		if (RequireSRGBConversionBlitToBackBuffer(waaaghCameraData.requireSrgbConversion))
		{
			m_Materials.Uber.EnableKeyword("_LINEAR_TO_SRGB_CONVERSION");
		}
		if (m_UseFastSRGBLinearConversion)
		{
			m_Materials.Uber.EnableKeyword("_USE_FAST_SRGB_LINEAR_CONVERSION");
		}
		bool flag11 = RequireHDROutput(waaaghCameraData);
		if (flag11)
		{
			HDROutputUtils.Operation hdrOperations = ((!m_HasFinalPass && m_EnableColorEncodingIfNeeded) ? HDROutputUtils.Operation.ColorEncoding : HDROutputUtils.Operation.None);
			SetupHDROutput(waaaghCameraData.hdrDisplayInformation, waaaghCameraData.hdrDisplayColorGamut, m_Materials.Uber, hdrOperations);
		}
		bool isAlphaOutputEnabled = waaaghCameraData.isAlphaOutputEnabled;
		TextureHandle destTexture = waaaghResourceData.CameraColorBuffer;
		TextureHandle lutTexture = waaaghResourceData.ColorGradingLUT;
		TextureHandle overlayUITexture = waaaghResourceData.OverlayUITexture;
		RenderUberPost(waaaghRenderingData.RenderGraph, waaaghCameraData, waaaghPostProcessingData, in activeCameraColor, in destTexture, in lutTexture, in overlayUITexture, flag11, isAlphaOutputEnabled);
		waaaghRenderingData.RenderGraph.EndProfilingSampler(sampler, ".\\Library\\PackageCache\\com.owlcat.visual@f3d4bf622f68\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 507);
	}

	public void RenderStopNaN(RenderGraph renderGraph, RenderTextureDescriptor cameraTargetDescriptor, in TextureHandle activeCameraColor, out TextureHandle stopNaNTarget)
	{
		RenderTextureDescriptor compatibleDescriptor = GetCompatibleDescriptor(cameraTargetDescriptor, cameraTargetDescriptor.width, cameraTargetDescriptor.height, cameraTargetDescriptor.graphicsFormat);
		stopNaNTarget = WaaaghRenderer.CreateRenderGraphTexture(renderGraph, compatibleDescriptor, "_StopNaNsTarget", clear: true, FilterMode.Bilinear);
		StopNaNsPassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<StopNaNsPassData>("Stop NaNs", out passData, ProfilingSampler.Get(WaaaghProfileId.StopNaNs), ".\\Library\\PackageCache\\com.owlcat.visual@f3d4bf622f68\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 528);
		passData.stopNaNTarget = stopNaNTarget;
		rasterRenderGraphBuilder.SetRenderAttachment(stopNaNTarget, 0, AccessFlags.ReadWrite);
		passData.sourceTexture = activeCameraColor;
		rasterRenderGraphBuilder.UseTexture(in activeCameraColor);
		passData.stopNaN = m_Materials.StopNaN;
		rasterRenderGraphBuilder.SetRenderFunc(delegate(StopNaNsPassData data, RasterGraphContext context)
		{
			RasterCommandBuffer cmd = context.cmd;
			RTHandle rTHandle = data.sourceTexture;
			Vector2 vector = (rTHandle.useScaling ? new Vector2(rTHandle.rtHandleProperties.rtHandleScale.x, rTHandle.rtHandleProperties.rtHandleScale.y) : Vector2.one);
			Blitter.BlitTexture(cmd, rTHandle, vector, data.stopNaN, 0);
		});
	}

	public void RenderSMAA(RenderGraph renderGraph, WaaaghResourceData resourceData, AntialiasingQuality antialiasingQuality, in TextureHandle source, out TextureHandle SMAATarget)
	{
		RenderTextureDescriptor compatibleDescriptor = GetCompatibleDescriptor(m_Descriptor, m_Descriptor.width, m_Descriptor.height, m_Descriptor.graphicsFormat);
		SMAATarget = WaaaghRenderer.CreateRenderGraphTexture(renderGraph, compatibleDescriptor, "_SMAATarget", clear: true, FilterMode.Bilinear);
		RenderTextureDescriptor compatibleDescriptor2 = GetCompatibleDescriptor(m_Descriptor, m_Descriptor.width, m_Descriptor.height, m_SMAAEdgeFormat);
		TextureHandle input = WaaaghRenderer.CreateRenderGraphTexture(renderGraph, compatibleDescriptor2, "_EdgeStencilTexture", clear: true, FilterMode.Bilinear);
		RenderTextureDescriptor compatibleDescriptor3 = GetCompatibleDescriptor(m_Descriptor, m_Descriptor.width, m_Descriptor.height, GraphicsFormat.None, DepthBits.Depth24);
		TextureHandle textureHandle = WaaaghRenderer.CreateRenderGraphTexture(renderGraph, compatibleDescriptor3, "_EdgeTexture", clear: true, FilterMode.Bilinear);
		RenderTextureDescriptor compatibleDescriptor4 = GetCompatibleDescriptor(m_Descriptor, m_Descriptor.width, m_Descriptor.height, GraphicsFormat.R8G8B8A8_UNorm);
		TextureHandle input2 = WaaaghRenderer.CreateRenderGraphTexture(renderGraph, compatibleDescriptor4, "_BlendTexture", clear: true);
		Material subpixelMorphologicalAntialiasing = m_Materials.SubpixelMorphologicalAntialiasing;
		SMAASetupPassData passData;
		using (IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<SMAASetupPassData>("SMAA Material Setup", out passData, ProfilingSampler.Get(WaaaghProfileId.SMAAMaterialSetup), ".\\Library\\PackageCache\\com.owlcat.visual@f3d4bf622f68\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 602))
		{
			passData.metrics = new Vector4(1f / (float)m_Descriptor.width, 1f / (float)m_Descriptor.height, m_Descriptor.width, m_Descriptor.height);
			passData.areaTexture = m_Resources.Textures.SmaaAreaTex;
			passData.searchTexture = m_Resources.Textures.SmaaSearchTex;
			passData.stencilRef = 64f;
			passData.stencilMask = 64f;
			passData.antialiasingQuality = antialiasingQuality;
			passData.material = subpixelMorphologicalAntialiasing;
			rasterRenderGraphBuilder.AllowPassCulling(value: false);
			rasterRenderGraphBuilder.SetRenderFunc(delegate(SMAASetupPassData data, RasterGraphContext context)
			{
				data.material.SetVector(ShaderConstants._Metrics, data.metrics);
				data.material.SetTexture(ShaderConstants._AreaTexture, data.areaTexture);
				data.material.SetTexture(ShaderConstants._SearchTexture, data.searchTexture);
				data.material.SetFloat(ShaderConstants._StencilRef, data.stencilRef);
				data.material.SetFloat(ShaderConstants._StencilMask, data.stencilMask);
				data.material.shaderKeywords = null;
				switch (data.antialiasingQuality)
				{
				case AntialiasingQuality.Low:
					data.material.EnableKeyword(ShaderKeywordStrings.SmaaLow);
					break;
				case AntialiasingQuality.Medium:
					data.material.EnableKeyword(ShaderKeywordStrings.SmaaMedium);
					break;
				case AntialiasingQuality.High:
					data.material.EnableKeyword(ShaderKeywordStrings.SmaaHigh);
					break;
				}
			});
		}
		SMAAPassData passData2;
		using (IRasterRenderGraphBuilder rasterRenderGraphBuilder2 = renderGraph.AddRasterRenderPass<SMAAPassData>("SMAA Edge Detection", out passData2, ProfilingSampler.Get(WaaaghProfileId.SMAAEdgeDetection), ".\\Library\\PackageCache\\com.owlcat.visual@f3d4bf622f68\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 643))
		{
			passData2.destinationTexture = input;
			rasterRenderGraphBuilder2.SetRenderAttachment(input, 0);
			passData2.depthStencilTexture = textureHandle;
			rasterRenderGraphBuilder2.SetRenderAttachmentDepth(textureHandle);
			passData2.sourceTexture = source;
			rasterRenderGraphBuilder2.UseTexture(in source);
			TextureHandle input3 = resourceData.CameraDepthBuffer;
			rasterRenderGraphBuilder2.UseTexture(in input3);
			passData2.material = subpixelMorphologicalAntialiasing;
			rasterRenderGraphBuilder2.SetRenderFunc(delegate(SMAAPassData data, RasterGraphContext context)
			{
				Material material3 = data.material;
				RasterCommandBuffer cmd3 = context.cmd;
				RTHandle rTHandle3 = data.sourceTexture;
				Vector2 vector3 = (rTHandle3.useScaling ? new Vector2(rTHandle3.rtHandleProperties.rtHandleScale.x, rTHandle3.rtHandleProperties.rtHandleScale.y) : Vector2.one);
				Blitter.BlitTexture(cmd3, rTHandle3, vector3, material3, 0);
			});
		}
		SMAAPassData passData3;
		using (IRasterRenderGraphBuilder rasterRenderGraphBuilder3 = renderGraph.AddRasterRenderPass<SMAAPassData>("SMAA Blend weights", out passData3, ProfilingSampler.Get(WaaaghProfileId.SMAABlendWeight), ".\\Library\\PackageCache\\com.owlcat.visual@f3d4bf622f68\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 666))
		{
			passData3.destinationTexture = input2;
			rasterRenderGraphBuilder3.SetRenderAttachment(input2, 0);
			passData3.depthStencilTexture = textureHandle;
			rasterRenderGraphBuilder3.SetRenderAttachmentDepth(textureHandle, AccessFlags.Read);
			passData3.sourceTexture = input;
			rasterRenderGraphBuilder3.UseTexture(in input);
			passData3.material = subpixelMorphologicalAntialiasing;
			rasterRenderGraphBuilder3.SetRenderFunc(delegate(SMAAPassData data, RasterGraphContext context)
			{
				Material material2 = data.material;
				RasterCommandBuffer cmd2 = context.cmd;
				RTHandle rTHandle2 = data.sourceTexture;
				Vector2 vector2 = (rTHandle2.useScaling ? new Vector2(rTHandle2.rtHandleProperties.rtHandleScale.x, rTHandle2.rtHandleProperties.rtHandleScale.y) : Vector2.one);
				Blitter.BlitTexture(cmd2, rTHandle2, vector2, material2, 1);
			});
		}
		SMAAPassData passData4;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder4 = renderGraph.AddRasterRenderPass<SMAAPassData>("SMAA Neighborhood blending", out passData4, ProfilingSampler.Get(WaaaghProfileId.SMAANeighborhoodBlend), ".\\Library\\PackageCache\\com.owlcat.visual@f3d4bf622f68\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 688);
		rasterRenderGraphBuilder4.AllowGlobalStateModification(value: true);
		passData4.destinationTexture = SMAATarget;
		rasterRenderGraphBuilder4.SetRenderAttachment(SMAATarget, 0);
		passData4.sourceTexture = source;
		rasterRenderGraphBuilder4.UseTexture(in source);
		passData4.blendTexture = input2;
		rasterRenderGraphBuilder4.UseTexture(in input2);
		passData4.material = subpixelMorphologicalAntialiasing;
		rasterRenderGraphBuilder4.SetRenderFunc(delegate(SMAAPassData data, RasterGraphContext context)
		{
			Material material = data.material;
			RasterCommandBuffer cmd = context.cmd;
			RTHandle rTHandle = data.sourceTexture;
			material.SetTexture(ShaderConstants._BlendTexture, data.blendTexture);
			Vector2 vector = (rTHandle.useScaling ? new Vector2(rTHandle.rtHandleProperties.rtHandleScale.x, rTHandle.rtHandleProperties.rtHandleScale.y) : Vector2.one);
			Blitter.BlitTexture(cmd, rTHandle, vector, material, 2);
		});
	}

	public void RenderDoF(RenderGraph renderGraph, WaaaghResourceData resourceData, WaaaghCameraData cameraData, in TextureHandle source, out TextureHandle destination)
	{
		Material dofMaterial = ((m_DepthOfField.mode.value == DepthOfFieldMode.Gaussian) ? m_Materials.GaussianDepthOfField : m_Materials.BokehDepthOfField);
		RenderTextureDescriptor compatibleDescriptor = GetCompatibleDescriptor(m_Descriptor, m_Descriptor.width, m_Descriptor.height, m_Descriptor.graphicsFormat);
		destination = WaaaghRenderer.CreateRenderGraphTexture(renderGraph, compatibleDescriptor, "_DoFTarget", clear: true, FilterMode.Bilinear);
		CoreUtils.SetKeyword(dofMaterial, "_ENABLE_ALPHA_OUTPUT", cameraData.isAlphaOutputEnabled);
		if (m_DepthOfField.mode.value == DepthOfFieldMode.Gaussian)
		{
			RenderDoFGaussian(renderGraph, resourceData, cameraData, in source, in destination, ref dofMaterial);
		}
		else if (m_DepthOfField.mode.value == DepthOfFieldMode.Bokeh)
		{
			RenderDoFBokeh(renderGraph, resourceData, cameraData, in source, in destination, ref dofMaterial);
		}
	}

	public void RenderDoFGaussian(RenderGraph renderGraph, WaaaghResourceData resourceData, WaaaghCameraData cameraData, in TextureHandle source, in TextureHandle destination, ref Material dofMaterial)
	{
		int num = 2;
		Material material = dofMaterial;
		int num2 = m_Descriptor.width / num;
		int height = m_Descriptor.height / num;
		DoFGaussianSetupPassData passData;
		using (IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<DoFGaussianSetupPassData>("Setup DoF passes", out passData, ProfilingSampler.Get(WaaaghProfileId.SetupDoF), ".\\Library\\PackageCache\\com.owlcat.visual@f3d4bf622f68\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 764))
		{
			float value = m_DepthOfField.gaussianStart.value;
			float y = Mathf.Max(value, m_DepthOfField.gaussianEnd.value);
			float a = m_DepthOfField.gaussianMaxRadius.value * ((float)num2 / 1080f);
			a = Mathf.Min(a, 2f);
			passData.source = source;
			passData.downSample = num;
			passData.cocParams = new Vector3(value, y, a);
			passData.highQualitySamplingValue = m_DepthOfField.highQualitySampling.value;
			passData.material = material;
			passData.materialCoC = m_Materials.GaussianDepthOfFieldCoC;
			rasterRenderGraphBuilder.AllowPassCulling(value: false);
			rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
			rasterRenderGraphBuilder.SetRenderFunc(delegate(DoFGaussianSetupPassData data, RasterGraphContext context)
			{
				RasterCommandBuffer cmd6 = context.cmd;
				Material material7 = data.material;
				material7.SetVector(ShaderConstants._CoCParams, data.cocParams);
				CoreUtils.SetKeyword(material7, ShaderKeywordStrings.HighQualitySampling, data.highQualitySamplingValue);
				Material materialCoC = data.materialCoC;
				materialCoC.SetVector(ShaderConstants._CoCParams, data.cocParams);
				CoreUtils.SetKeyword(materialCoC, ShaderKeywordStrings.HighQualitySampling, data.highQualitySamplingValue);
				PostProcessUtils.SetSourceSize(cmd6, data.source);
				cmd6.SetGlobalVector(ShaderConstants._DownSampleScaleFactor, new Vector4(1f / (float)data.downSample, 1f / (float)data.downSample, data.downSample, data.downSample));
			});
		}
		RenderTextureDescriptor compatibleDescriptor = GetCompatibleDescriptor(m_Descriptor, m_Descriptor.width, m_Descriptor.height, m_GaussianCoCFormat);
		TextureHandle input = WaaaghRenderer.CreateRenderGraphTexture(renderGraph, compatibleDescriptor, "_FullCoCTexture", clear: true, FilterMode.Bilinear);
		RenderTextureDescriptor compatibleDescriptor2 = GetCompatibleDescriptor(m_Descriptor, num2, height, m_GaussianCoCFormat);
		TextureHandle input2 = WaaaghRenderer.CreateRenderGraphTexture(renderGraph, compatibleDescriptor2, "_HalfCoCTexture", clear: true, FilterMode.Bilinear);
		RenderTextureDescriptor compatibleDescriptor3 = GetCompatibleDescriptor(m_Descriptor, num2, height, m_DefaultColorFormat);
		TextureHandle input3 = WaaaghRenderer.CreateRenderGraphTexture(renderGraph, compatibleDescriptor3, "_PingTexture", clear: true, FilterMode.Bilinear);
		RenderTextureDescriptor compatibleDescriptor4 = GetCompatibleDescriptor(m_Descriptor, num2, height, m_DefaultColorFormat);
		TextureHandle input4 = WaaaghRenderer.CreateRenderGraphTexture(renderGraph, compatibleDescriptor4, "_PongTexture", clear: true, FilterMode.Bilinear);
		DoFGaussianPassData passData2;
		using (IRasterRenderGraphBuilder rasterRenderGraphBuilder2 = renderGraph.AddRasterRenderPass<DoFGaussianPassData>("Depth of Field - Compute CoC", out passData2, ProfilingSampler.Get(WaaaghProfileId.DOFComputeCOC), ".\\Library\\PackageCache\\com.owlcat.visual@f3d4bf622f68\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 813))
		{
			rasterRenderGraphBuilder2.SetRenderAttachment(input, 0);
			passData2.sourceTexture = source;
			rasterRenderGraphBuilder2.UseTexture(in source);
			passData2.depthTexture = resourceData.CameraDepthCopyRT;
			rasterRenderGraphBuilder2.UseTexture(in resourceData.CameraDepthCopyRT);
			passData2.material = m_Materials.GaussianDepthOfFieldCoC;
			rasterRenderGraphBuilder2.UseGlobalTexture(ShaderPropertyId._CameraDepthTexture);
			rasterRenderGraphBuilder2.SetRenderFunc(delegate(DoFGaussianPassData data, RasterGraphContext context)
			{
				Material material6 = data.material;
				RasterCommandBuffer cmd5 = context.cmd;
				RTHandle rTHandle5 = data.sourceTexture;
				material6.SetTexture(ShaderPropertyId._CameraDepthTexture, data.depthTexture);
				Vector2 vector5 = (rTHandle5.useScaling ? new Vector2(rTHandle5.rtHandleProperties.rtHandleScale.x, rTHandle5.rtHandleProperties.rtHandleScale.y) : Vector2.one);
				Blitter.BlitTexture(cmd5, rTHandle5, vector5, material6, 0);
			});
		}
		DoFGaussianPassData passData3;
		using (IRasterRenderGraphBuilder rasterRenderGraphBuilder3 = renderGraph.AddRasterRenderPass<DoFGaussianPassData>("Depth of Field - Downscale & Prefilter Color + CoC", out passData3, ProfilingSampler.Get(WaaaghProfileId.DOFDownscalePrefilter), ".\\Library\\PackageCache\\com.owlcat.visual@f3d4bf622f68\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 838))
		{
			rasterRenderGraphBuilder3.SetRenderAttachment(input2, 0);
			rasterRenderGraphBuilder3.SetRenderAttachment(input3, 1);
			if (!renderGraph.nativeRenderPassesEnabled)
			{
				rasterRenderGraphBuilder3.SetRenderAttachmentDepth(renderGraph.CreateTexture(input2), AccessFlags.ReadWrite);
			}
			rasterRenderGraphBuilder3.AllowGlobalStateModification(value: true);
			passData3.sourceTexture = source;
			rasterRenderGraphBuilder3.UseTexture(in source);
			passData3.cocTexture = input;
			rasterRenderGraphBuilder3.UseTexture(in input);
			passData3.material = material;
			rasterRenderGraphBuilder3.SetRenderFunc(delegate(DoFGaussianPassData data, RasterGraphContext context)
			{
				Material material5 = data.material;
				RasterCommandBuffer cmd4 = context.cmd;
				RTHandle rTHandle4 = data.sourceTexture;
				material5.SetTexture(ShaderConstants._FullCoCTexture, data.cocTexture);
				Vector2 vector4 = (rTHandle4.useScaling ? new Vector2(rTHandle4.rtHandleProperties.rtHandleScale.x, rTHandle4.rtHandleProperties.rtHandleScale.y) : Vector2.one);
				Blitter.BlitTexture(cmd4, rTHandle4, vector4, material5, 1);
			});
		}
		DoFGaussianPassData passData4;
		using (IRasterRenderGraphBuilder rasterRenderGraphBuilder4 = renderGraph.AddRasterRenderPass<DoFGaussianPassData>("Depth of Field - Blur H", out passData4, ProfilingSampler.Get(WaaaghProfileId.DOFBlurH), ".\\Library\\PackageCache\\com.owlcat.visual@f3d4bf622f68\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 866))
		{
			rasterRenderGraphBuilder4.SetRenderAttachment(input4, 0);
			rasterRenderGraphBuilder4.AllowGlobalStateModification(value: true);
			passData4.sourceTexture = input3;
			rasterRenderGraphBuilder4.UseTexture(in input3);
			passData4.cocTexture = input2;
			rasterRenderGraphBuilder4.UseTexture(in input2);
			passData4.material = material;
			rasterRenderGraphBuilder4.SetRenderFunc(delegate(DoFGaussianPassData data, RasterGraphContext context)
			{
				Material material4 = data.material;
				RasterCommandBuffer cmd3 = context.cmd;
				RTHandle rTHandle3 = data.sourceTexture;
				material4.SetTexture(ShaderConstants._HalfCoCTexture, data.cocTexture);
				Vector2 vector3 = (rTHandle3.useScaling ? new Vector2(rTHandle3.rtHandleProperties.rtHandleScale.x, rTHandle3.rtHandleProperties.rtHandleScale.y) : Vector2.one);
				Blitter.BlitTexture(cmd3, rTHandle3, vector3, material4, 2);
			});
		}
		DoFGaussianPassData passData5;
		using (IRasterRenderGraphBuilder rasterRenderGraphBuilder5 = renderGraph.AddRasterRenderPass<DoFGaussianPassData>("Depth of Field - Blur V", out passData5, ProfilingSampler.Get(WaaaghProfileId.DOFBlurV), ".\\Library\\PackageCache\\com.owlcat.visual@f3d4bf622f68\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 889))
		{
			rasterRenderGraphBuilder5.SetRenderAttachment(input3, 0);
			rasterRenderGraphBuilder5.AllowGlobalStateModification(value: true);
			passData5.sourceTexture = input4;
			rasterRenderGraphBuilder5.UseTexture(in input4);
			passData5.cocTexture = input2;
			rasterRenderGraphBuilder5.UseTexture(in input2);
			passData5.material = material;
			rasterRenderGraphBuilder5.SetRenderFunc(delegate(DoFGaussianPassData data, RasterGraphContext context)
			{
				Material material3 = data.material;
				RasterCommandBuffer cmd2 = context.cmd;
				RTHandle rTHandle2 = data.sourceTexture;
				material3.SetTexture(ShaderConstants._HalfCoCTexture, data.cocTexture);
				Vector2 vector2 = (rTHandle2.useScaling ? new Vector2(rTHandle2.rtHandleProperties.rtHandleScale.x, rTHandle2.rtHandleProperties.rtHandleScale.y) : Vector2.one);
				Blitter.BlitTexture(cmd2, rTHandle2, vector2, material3, 3);
			});
		}
		DoFGaussianPassData passData6;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder6 = renderGraph.AddRasterRenderPass<DoFGaussianPassData>("Depth of Field - Composite", out passData6, ProfilingSampler.Get(WaaaghProfileId.DOFComposite), ".\\Library\\PackageCache\\com.owlcat.visual@f3d4bf622f68\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 912);
		rasterRenderGraphBuilder6.SetRenderAttachment(destination, 0);
		rasterRenderGraphBuilder6.AllowGlobalStateModification(value: true);
		passData6.sourceTexture = source;
		rasterRenderGraphBuilder6.UseTexture(in source);
		passData6.cocTexture = input;
		rasterRenderGraphBuilder6.UseTexture(in input);
		passData6.colorTexture = input3;
		rasterRenderGraphBuilder6.UseTexture(in input3);
		passData6.material = material;
		rasterRenderGraphBuilder6.SetRenderFunc(delegate(DoFGaussianPassData data, RasterGraphContext context)
		{
			Material material2 = data.material;
			RasterCommandBuffer cmd = context.cmd;
			RTHandle rTHandle = data.sourceTexture;
			material2.SetTexture(ShaderConstants._ColorTexture, data.colorTexture);
			material2.SetTexture(ShaderConstants._FullCoCTexture, data.cocTexture);
			Vector2 vector = (rTHandle.useScaling ? new Vector2(rTHandle.rtHandleProperties.rtHandleScale.x, rTHandle.rtHandleProperties.rtHandleScale.y) : Vector2.one);
			Blitter.BlitTexture(cmd, rTHandle, vector, material2, 4);
		});
	}

	public void RenderDoFBokeh(RenderGraph renderGraph, WaaaghResourceData resourceData, WaaaghCameraData cameraData, in TextureHandle source, in TextureHandle destination, ref Material dofMaterial)
	{
		int num = 2;
		Material material = dofMaterial;
		int num2 = m_Descriptor.width / num;
		int num3 = m_Descriptor.height / num;
		DoFBokehSetupPassData passData;
		using (IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<DoFBokehSetupPassData>("Setup DoF passes", out passData, ProfilingSampler.Get(WaaaghProfileId.SetupDoF), ".\\Library\\PackageCache\\com.owlcat.visual@f3d4bf622f68\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 967))
		{
			float num4 = m_DepthOfField.focalLength.value / 1000f;
			float num5 = m_DepthOfField.focalLength.value / m_DepthOfField.aperture.value;
			float value = m_DepthOfField.focusDistance.value;
			float y = num5 * num4 / (value - num4);
			float maxBokehRadiusInPixels = GetMaxBokehRadiusInPixels(m_Descriptor.height);
			float num6 = 1f / ((float)num2 / (float)num3);
			int hashCode = m_DepthOfField.GetHashCode();
			if (hashCode != m_BokehHash || maxBokehRadiusInPixels != m_BokehMaxRadius || num6 != m_BokehRCPAspect)
			{
				m_BokehHash = hashCode;
				m_BokehMaxRadius = maxBokehRadiusInPixels;
				m_BokehRCPAspect = num6;
				PrepareBokehKernel(maxBokehRadiusInPixels, num6);
			}
			float uvMargin = 1f / (float)m_Descriptor.height * (float)num;
			passData.bokehKernel = m_BokehKernel;
			passData.source = source;
			passData.downSample = num;
			passData.uvMargin = uvMargin;
			passData.cocParams = new Vector4(value, y, maxBokehRadiusInPixels, num6);
			passData.useFastSRGBLinearConversion = m_UseFastSRGBLinearConversion;
			passData.material = material;
			passData.materialCoC = m_Materials.BokehDepthOfFieldCoC;
			rasterRenderGraphBuilder.AllowPassCulling(value: false);
			rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
			rasterRenderGraphBuilder.SetRenderFunc(delegate(DoFBokehSetupPassData data, RasterGraphContext context)
			{
				RasterCommandBuffer cmd6 = context.cmd;
				CoreUtils.SetKeyword(data.material, "_USE_FAST_SRGB_LINEAR_CONVERSION", data.useFastSRGBLinearConversion);
				CoreUtils.SetKeyword(data.materialCoC, "_USE_FAST_SRGB_LINEAR_CONVERSION", data.useFastSRGBLinearConversion);
				cmd6.SetGlobalVector(ShaderConstants._CoCParams, data.cocParams);
				cmd6.SetGlobalVectorArray(ShaderConstants._BokehKernel, data.bokehKernel);
				cmd6.SetGlobalVector(ShaderConstants._DownSampleScaleFactor, new Vector4(1f / (float)data.downSample, 1f / (float)data.downSample, data.downSample, data.downSample));
				cmd6.SetGlobalVector(ShaderConstants._BokehConstants, new Vector4(data.uvMargin, data.uvMargin * 2f));
				PostProcessUtils.SetSourceSize(cmd6, data.source);
			});
		}
		RenderTextureDescriptor compatibleDescriptor = GetCompatibleDescriptor(m_Descriptor, m_Descriptor.width, m_Descriptor.height, GraphicsFormat.R8_UNorm);
		TextureHandle input = WaaaghRenderer.CreateRenderGraphTexture(renderGraph, compatibleDescriptor, "_FullCoCTexture", clear: true, FilterMode.Bilinear);
		RenderTextureDescriptor compatibleDescriptor2 = GetCompatibleDescriptor(m_Descriptor, num2, num3, GraphicsFormat.R16G16B16A16_SFloat);
		TextureHandle input2 = WaaaghRenderer.CreateRenderGraphTexture(renderGraph, compatibleDescriptor2, "_PingTexture", clear: true, FilterMode.Bilinear);
		RenderTextureDescriptor compatibleDescriptor3 = GetCompatibleDescriptor(m_Descriptor, num2, num3, GraphicsFormat.R16G16B16A16_SFloat);
		TextureHandle input3 = WaaaghRenderer.CreateRenderGraphTexture(renderGraph, compatibleDescriptor3, "_PongTexture", clear: true, FilterMode.Bilinear);
		DoFBokehPassData passData2;
		using (IRasterRenderGraphBuilder rasterRenderGraphBuilder2 = renderGraph.AddRasterRenderPass<DoFBokehPassData>("Depth of Field - Compute CoC", out passData2, ProfilingSampler.Get(WaaaghProfileId.DOFComputeCOC), ".\\Library\\PackageCache\\com.owlcat.visual@f3d4bf622f68\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 1028))
		{
			rasterRenderGraphBuilder2.SetRenderAttachment(input, 0);
			passData2.sourceTexture = source;
			rasterRenderGraphBuilder2.UseTexture(in source);
			passData2.depthTexture = resourceData.CameraDepthCopyRT;
			rasterRenderGraphBuilder2.UseTexture(in resourceData.CameraDepthCopyRT);
			passData2.material = m_Materials.BokehDepthOfFieldCoC;
			rasterRenderGraphBuilder2.UseGlobalTexture(ShaderPropertyId._CameraDepthTexture);
			rasterRenderGraphBuilder2.SetRenderFunc(delegate(DoFBokehPassData data, RasterGraphContext context)
			{
				Material material6 = data.material;
				RasterCommandBuffer cmd5 = context.cmd;
				RTHandle rTHandle5 = data.sourceTexture;
				material6.SetTexture(ShaderPropertyId._CameraDepthTexture, data.depthTexture);
				Vector2 vector5 = (rTHandle5.useScaling ? new Vector2(rTHandle5.rtHandleProperties.rtHandleScale.x, rTHandle5.rtHandleProperties.rtHandleScale.y) : Vector2.one);
				Blitter.BlitTexture(cmd5, rTHandle5, vector5, material6, 0);
			});
		}
		DoFBokehPassData passData3;
		using (IRasterRenderGraphBuilder rasterRenderGraphBuilder3 = renderGraph.AddRasterRenderPass<DoFBokehPassData>("Depth of Field - Downscale & Prefilter Color + CoC", out passData3, ProfilingSampler.Get(WaaaghProfileId.DOFDownscalePrefilter), ".\\Library\\PackageCache\\com.owlcat.visual@f3d4bf622f68\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 1053))
		{
			rasterRenderGraphBuilder3.SetRenderAttachment(input2, 0);
			rasterRenderGraphBuilder3.AllowGlobalStateModification(value: true);
			passData3.sourceTexture = source;
			rasterRenderGraphBuilder3.UseTexture(in source);
			passData3.cocTexture = input;
			rasterRenderGraphBuilder3.UseTexture(in input);
			passData3.material = material;
			rasterRenderGraphBuilder3.SetRenderFunc(delegate(DoFBokehPassData data, RasterGraphContext context)
			{
				Material material5 = data.material;
				RasterCommandBuffer cmd4 = context.cmd;
				RTHandle rTHandle4 = data.sourceTexture;
				material5.SetTexture(ShaderConstants._FullCoCTexture, data.cocTexture);
				Vector2 vector4 = (rTHandle4.useScaling ? new Vector2(rTHandle4.rtHandleProperties.rtHandleScale.x, rTHandle4.rtHandleProperties.rtHandleScale.y) : Vector2.one);
				Blitter.BlitTexture(cmd4, rTHandle4, vector4, material5, 1);
			});
		}
		DoFBokehPassData passData4;
		using (IRasterRenderGraphBuilder rasterRenderGraphBuilder4 = renderGraph.AddRasterRenderPass<DoFBokehPassData>("Depth of Field - Bokeh Blur", out passData4, ProfilingSampler.Get(WaaaghProfileId.DOFBlurBokeh), ".\\Library\\PackageCache\\com.owlcat.visual@f3d4bf622f68\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 1076))
		{
			rasterRenderGraphBuilder4.SetRenderAttachment(input3, 0);
			passData4.sourceTexture = input2;
			rasterRenderGraphBuilder4.UseTexture(in input2);
			passData4.material = material;
			rasterRenderGraphBuilder4.SetRenderFunc(delegate(DoFBokehPassData data, RasterGraphContext context)
			{
				Material material4 = data.material;
				RasterCommandBuffer cmd3 = context.cmd;
				RTHandle rTHandle3 = data.sourceTexture;
				Vector2 vector3 = (rTHandle3.useScaling ? new Vector2(rTHandle3.rtHandleProperties.rtHandleScale.x, rTHandle3.rtHandleProperties.rtHandleScale.y) : Vector2.one);
				Blitter.BlitTexture(cmd3, rTHandle3, vector3, material4, 2);
			});
		}
		DoFBokehPassData passData5;
		using (IRasterRenderGraphBuilder rasterRenderGraphBuilder5 = renderGraph.AddRasterRenderPass<DoFBokehPassData>("Depth of Field - Post-filtering", out passData5, ProfilingSampler.Get(WaaaghProfileId.DOFPostFilter), ".\\Library\\PackageCache\\com.owlcat.visual@f3d4bf622f68\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 1095))
		{
			rasterRenderGraphBuilder5.SetRenderAttachment(input2, 0);
			passData5.sourceTexture = input3;
			rasterRenderGraphBuilder5.UseTexture(in input3);
			passData5.material = material;
			rasterRenderGraphBuilder5.SetRenderFunc(delegate(DoFBokehPassData data, RasterGraphContext context)
			{
				Material material3 = data.material;
				RasterCommandBuffer cmd2 = context.cmd;
				RTHandle rTHandle2 = data.sourceTexture;
				Vector2 vector2 = (rTHandle2.useScaling ? new Vector2(rTHandle2.rtHandleProperties.rtHandleScale.x, rTHandle2.rtHandleProperties.rtHandleScale.y) : Vector2.one);
				Blitter.BlitTexture(cmd2, rTHandle2, vector2, material3, 3);
			});
		}
		DoFBokehPassData passData6;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder6 = renderGraph.AddRasterRenderPass<DoFBokehPassData>("Depth of Field - Composite", out passData6, ProfilingSampler.Get(WaaaghProfileId.DOFComposite), ".\\Library\\PackageCache\\com.owlcat.visual@f3d4bf622f68\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 1115);
		rasterRenderGraphBuilder6.SetRenderAttachment(destination, 0);
		rasterRenderGraphBuilder6.AllowGlobalStateModification(value: true);
		passData6.sourceTexture = source;
		rasterRenderGraphBuilder6.UseTexture(in source);
		passData6.dofTexture = input2;
		rasterRenderGraphBuilder6.UseTexture(in input2);
		rasterRenderGraphBuilder6.UseTexture(in input);
		passData6.material = material;
		rasterRenderGraphBuilder6.SetRenderFunc(delegate(DoFBokehPassData data, RasterGraphContext context)
		{
			Material material2 = data.material;
			RasterCommandBuffer cmd = context.cmd;
			RTHandle rTHandle = data.sourceTexture;
			material2.SetTexture(ShaderConstants._DofTexture, data.dofTexture);
			Vector2 vector = (rTHandle.useScaling ? new Vector2(rTHandle.rtHandleProperties.rtHandleScale.x, rTHandle.rtHandleProperties.rtHandleScale.y) : Vector2.one);
			Blitter.BlitTexture(cmd, rTHandle, vector, material2, 4);
		});
	}

	private void PrepareBokehKernel(float maxRadius, float rcpAspect)
	{
		if (m_BokehKernel == null)
		{
			m_BokehKernel = new Vector4[42];
		}
		int num = 0;
		float num2 = m_DepthOfField.bladeCount.value;
		float p = 1f - m_DepthOfField.bladeCurvature.value;
		float num3 = m_DepthOfField.bladeRotation.value * (MathF.PI / 180f);
		for (int i = 1; i < 4; i++)
		{
			float num4 = 1f / 7f;
			float num5 = ((float)i + num4) / (3f + num4);
			int num6 = i * 7;
			for (int j = 0; j < num6; j++)
			{
				float num7 = MathF.PI * 2f * (float)j / (float)num6;
				float num8 = Mathf.Cos(MathF.PI / num2);
				float num9 = Mathf.Cos(num7 - MathF.PI * 2f / num2 * Mathf.Floor((num2 * num7 + MathF.PI) / (MathF.PI * 2f)));
				float num10 = num5 * Mathf.Pow(num8 / num9, p);
				float num11 = num10 * Mathf.Cos(num7 - num3);
				float num12 = num10 * Mathf.Sin(num7 - num3);
				float num13 = num11 * maxRadius;
				float num14 = num12 * maxRadius;
				float num15 = num13 * num13;
				float num16 = num14 * num14;
				float z = Mathf.Sqrt(num15 + num16);
				float w = num13 * rcpAspect;
				m_BokehKernel[num] = new Vector4(num13, num14, z, w);
				num++;
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float GetMaxBokehRadiusInPixels(float viewportHeight)
	{
		return Mathf.Min(0.05f, 14f / viewportHeight);
	}

	public void RenderMotionBlur(RenderGraph renderGraph, WaaaghResourceData resourceData, WaaaghCameraData cameraData, in TextureHandle source, out TextureHandle destination)
	{
		Material cameraMotionBlur = m_Materials.CameraMotionBlur;
		RenderTextureDescriptor compatibleDescriptor = GetCompatibleDescriptor(m_Descriptor, m_Descriptor.width, m_Descriptor.height, m_Descriptor.graphicsFormat);
		destination = WaaaghRenderer.CreateRenderGraphTexture(renderGraph, compatibleDescriptor, "_MotionBlurTarget", clear: true, FilterMode.Bilinear);
		TextureHandle input = resourceData.CameraMotionVectorsRT;
		MotionBlurMode value = m_MotionBlur.mode.value;
		int value2 = (int)m_MotionBlur.quality.value;
		value2 += ((value == MotionBlurMode.CameraAndObjects) ? 3 : 0);
		MotionBlurPassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<MotionBlurPassData>("Motion Blur", out passData, ProfilingSampler.Get(WaaaghProfileId.MotionBlur), ".\\Library\\PackageCache\\com.owlcat.visual@f3d4bf622f68\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 1232);
		rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
		passData.destinationTexture = destination;
		rasterRenderGraphBuilder.SetRenderAttachment(destination, 0);
		passData.sourceTexture = source;
		rasterRenderGraphBuilder.UseTexture(in source);
		passData.motionVectors = input;
		rasterRenderGraphBuilder.UseTexture(in input);
		TextureHandle input2 = resourceData.CameraDepthBuffer;
		rasterRenderGraphBuilder.UseTexture(in input2);
		passData.material = cameraMotionBlur;
		passData.passIndex = value2;
		passData.camera = cameraData.camera;
		passData.xr = m_DefaultXr;
		passData.enableAlphaOutput = cameraData.isAlphaOutputEnabled;
		passData.intensity = m_MotionBlur.intensity.value;
		passData.clamp = m_MotionBlur.clamp.value;
		rasterRenderGraphBuilder.SetRenderFunc(delegate(MotionBlurPassData data, RasterGraphContext context)
		{
			RasterCommandBuffer cmd = context.cmd;
			RTHandle rTHandle = data.sourceTexture;
			UpdateMotionBlurMatrices(ref data.material, data.camera, data.xr);
			data.material.SetFloat("_Intensity", data.intensity);
			data.material.SetFloat("_Clamp", data.clamp);
			CoreUtils.SetKeyword(data.material, "_ENABLE_ALPHA_OUTPUT", data.enableAlphaOutput);
			PostProcessUtils.SetSourceSize(cmd, data.sourceTexture);
			cmd.SetGlobalTexture(ShaderConstants._MotionVectorTexture, data.motionVectors);
			Vector2 vector = (rTHandle.useScaling ? new Vector2(rTHandle.rtHandleProperties.rtHandleScale.x, rTHandle.rtHandleProperties.rtHandleScale.y) : Vector2.one);
			Blitter.BlitTexture(cmd, rTHandle, vector, data.material, data.passIndex);
		});
	}

	internal static void UpdateMotionBlurMatrices(ref Material material, Camera camera, XRPass xr)
	{
		MotionVectorsPersistentData motionVectorsPersistentData = null;
		if (camera.TryGetComponent<WaaaghAdditionalCameraData>(out var component))
		{
			motionVectorsPersistentData = component.MotionVectorsPersistentData;
		}
		if (motionVectorsPersistentData != null)
		{
			int num = 0;
			material.SetMatrix(k_ShaderPropertyId_PrevViewProjM, motionVectorsPersistentData.previousViewProjectionStereo[num]);
			material.SetMatrix(k_ShaderPropertyId_ViewProjM, motionVectorsPersistentData.viewProjectionStereo[num]);
		}
	}

	public void RenderRadialBlur(RenderGraph renderGraph, WaaaghResourceData resourceData, WaaaghCameraData cameraData, in TextureHandle source, out TextureHandle destination)
	{
		Material radialBlur = m_Materials.RadialBlur;
		RenderTextureDescriptor compatibleDescriptor = GetCompatibleDescriptor(m_Descriptor, m_Descriptor.width, m_Descriptor.height, m_Descriptor.graphicsFormat);
		destination = WaaaghRenderer.CreateRenderGraphTexture(renderGraph, compatibleDescriptor, "_RadialBlurTarget", clear: true, FilterMode.Bilinear);
		radialBlur.SetVector(k_ShaderPropertyId_RadialBlurCenter, m_RadialBlur.Center.value);
		radialBlur.SetFloat(k_ShaderPropertyId_RadialBlurStrength, m_RadialBlur.Strength.value);
		radialBlur.SetFloat(k_ShaderPropertyId_RadialBlurWidth, m_RadialBlur.Width.value);
		RadialBlurPassData passData2;
		using RenderGraphBuilder renderGraphBuilder = renderGraph.AddRenderPass<RadialBlurPassData>("Radial Blur", out passData2, ".\\Library\\PackageCache\\com.owlcat.visual@f3d4bf622f68\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 1338);
		passData2.Source = renderGraphBuilder.ReadTexture(in source);
		passData2.Destination = renderGraphBuilder.WriteTexture(in destination);
		passData2.Material = radialBlur;
		renderGraphBuilder.SetRenderFunc(delegate(RadialBlurPassData passData, RenderGraphContext context)
		{
			context.cmd.Blit(passData.Source, passData.Destination, passData.Material, 0);
		});
	}

	public void RenderPaniniProjection(RenderGraph renderGraph, Camera camera, in TextureHandle source, out TextureHandle destination)
	{
		RenderTextureDescriptor compatibleDescriptor = GetCompatibleDescriptor(m_Descriptor, m_Descriptor.width, m_Descriptor.height, m_Descriptor.graphicsFormat);
		destination = WaaaghRenderer.CreateRenderGraphTexture(renderGraph, compatibleDescriptor, "_PaniniProjectionTarget", clear: true, FilterMode.Bilinear);
		float value = m_PaniniProjection.distance.value;
		Vector2 vector = CalcViewExtents(camera);
		Vector2 vector2 = CalcCropExtents(camera, value);
		float a = vector2.x / vector.x;
		float b = vector2.y / vector.y;
		float value2 = Mathf.Min(a, b);
		float num = value;
		float w = Mathf.Lerp(1f, Mathf.Clamp01(value2), m_PaniniProjection.cropToFit.value);
		PaniniProjectionPassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<PaniniProjectionPassData>("Panini Projection", out passData, ProfilingSampler.Get(WaaaghProfileId.PaniniProjection), ".\\Library\\PackageCache\\com.owlcat.visual@f3d4bf622f68\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 1385);
		rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
		passData.destinationTexture = destination;
		rasterRenderGraphBuilder.SetRenderAttachment(destination, 0);
		passData.sourceTexture = source;
		rasterRenderGraphBuilder.UseTexture(in source);
		passData.material = m_Materials.PaniniProjection;
		passData.paniniParams = new Vector4(vector.x, vector.y, num, w);
		passData.isPaniniGeneric = 1f - Mathf.Abs(num) > float.Epsilon;
		passData.sourceTextureDesc = m_Descriptor;
		rasterRenderGraphBuilder.SetRenderFunc(delegate(PaniniProjectionPassData data, RasterGraphContext context)
		{
			RasterCommandBuffer cmd = context.cmd;
			RTHandle rTHandle = data.sourceTexture;
			cmd.SetGlobalVector(ShaderConstants._Params, data.paniniParams);
			data.material.EnableKeyword(data.isPaniniGeneric ? ShaderKeywordStrings.PaniniGeneric : ShaderKeywordStrings.PaniniUnitDistance);
			Vector2 vector3 = (rTHandle.useScaling ? new Vector2(rTHandle.rtHandleProperties.rtHandleScale.x, rTHandle.rtHandleProperties.rtHandleScale.y) : Vector2.one);
			Blitter.BlitTexture(cmd, rTHandle, vector3, data.material, 0);
		});
	}

	private Vector2 CalcViewExtents(Camera camera)
	{
		float num = camera.fieldOfView * (MathF.PI / 180f);
		float num2 = (float)m_Descriptor.width / (float)m_Descriptor.height;
		float num3 = Mathf.Tan(0.5f * num);
		return new Vector2(num2 * num3, num3);
	}

	private Vector2 CalcCropExtents(Camera camera, float d)
	{
		float num = 1f + d;
		Vector2 vector = CalcViewExtents(camera);
		float num2 = Mathf.Sqrt(vector.x * vector.x + 1f);
		float num3 = 1f / num2;
		float num4 = num3 + d;
		return vector * num3 * (num / num4);
	}

	public void UberPostSetupBloomPass(RenderGraph rendergraph, in TextureHandle bloomTexture, Material uberMaterial)
	{
		UberSetupBloomPassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = rendergraph.AddRasterRenderPass<UberSetupBloomPassData>("UberPost - UberPostSetupBloomPass", out passData, ProfilingSampler.Get(WaaaghProfileId.UberPostSetupBloomPass), ".\\Library\\PackageCache\\com.owlcat.visual@f3d4bf622f68\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 1476);
		Color color = m_Bloom.tint.value.linear;
		float num = ColorUtils.Luminance(in color);
		color = ((num > 0f) ? (color * (1f / num)) : Color.white);
		Vector4 bloomParams = new Vector4(m_Bloom.intensity.value, color.r, color.g, color.b);
		Texture texture = ((m_Bloom.dirtTexture.value == null) ? Texture2D.blackTexture : m_Bloom.dirtTexture.value);
		float num2 = (float)texture.width / (float)texture.height;
		float num3 = (float)m_Descriptor.width / (float)m_Descriptor.height;
		Vector4 dirtScaleOffset = new Vector4(1f, 1f, 0f, 0f);
		float value = m_Bloom.dirtIntensity.value;
		if (num2 > num3)
		{
			dirtScaleOffset.x = num3 / num2;
			dirtScaleOffset.z = (1f - dirtScaleOffset.x) * 0.5f;
		}
		else if (num3 > num2)
		{
			dirtScaleOffset.y = num2 / num3;
			dirtScaleOffset.w = (1f - dirtScaleOffset.y) * 0.5f;
		}
		passData.bloomParams = bloomParams;
		passData.dirtScaleOffset = dirtScaleOffset;
		passData.dirtIntensity = value;
		passData.dirtTexture = texture;
		passData.highQualityFilteringValue = m_Bloom.highQualityFiltering.value;
		passData.useRGBM = m_DefaultColorFormatUseRGBM;
		passData.bloomTexture = bloomTexture;
		rasterRenderGraphBuilder.UseTexture(in bloomTexture);
		passData.uberMaterial = uberMaterial;
		rasterRenderGraphBuilder.AllowPassCulling(value: false);
		rasterRenderGraphBuilder.SetRenderFunc(delegate(UberSetupBloomPassData data, RasterGraphContext context)
		{
			Material uberMaterial2 = data.uberMaterial;
			uberMaterial2.SetVector(ShaderConstants._Bloom_Params, data.bloomParams);
			uberMaterial2.SetFloat(ShaderConstants._Bloom_RGBM, data.useRGBM ? 1f : 0f);
			uberMaterial2.SetVector(ShaderConstants._LensDirt_Params, data.dirtScaleOffset);
			uberMaterial2.SetFloat(ShaderConstants._LensDirt_Intensity, data.dirtIntensity);
			uberMaterial2.SetTexture(ShaderConstants._LensDirt_Texture, data.dirtTexture);
			if (data.highQualityFilteringValue)
			{
				uberMaterial2.EnableKeyword((data.dirtIntensity > 0f) ? ShaderKeywordStrings.BloomHQDirt : ShaderKeywordStrings.BloomHQ);
			}
			else
			{
				uberMaterial2.EnableKeyword((data.dirtIntensity > 0f) ? ShaderKeywordStrings.BloomLQDirt : ShaderKeywordStrings.BloomLQ);
			}
			uberMaterial2.SetTexture(ShaderConstants._Bloom_Texture, data.bloomTexture);
		});
	}

	public void RenderBloomTexture(RenderGraph renderGraph, in TextureHandle source, out TextureHandle destination, bool enableAlphaOutput)
	{
		int num = 1;
		num = m_Bloom.downscale.value switch
		{
			BloomDownscaleMode.Half => 1, 
			BloomDownscaleMode.Quarter => 2, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		int num2 = m_Descriptor.width >> num;
		int num3 = m_Descriptor.height >> num;
		int num4 = Mathf.Clamp(Mathf.FloorToInt(Mathf.Log(Mathf.Max(num2, num3), 2f) - 1f), 1, m_Bloom.maxIterations.value);
		using (new ProfilingScope(ProfilingSampler.Get(WaaaghProfileId.BloomSetup)))
		{
			float value = m_Bloom.clamp.value;
			float num5 = Mathf.GammaToLinearSpace(m_Bloom.threshold.value);
			float w = num5 * 0.5f;
			float x = Mathf.Lerp(0.05f, 0.95f, m_Bloom.scatter.value);
			BloomMaterialParams other = default(BloomMaterialParams);
			other.parameters = new Vector4(x, value, num5, w);
			other.highQualityFiltering = m_Bloom.highQualityFiltering.value;
			other.useRGBM = m_DefaultColorFormatUseRGBM;
			other.enableAlphaOutput = enableAlphaOutput;
			Material bloom = m_Materials.Bloom;
			bool num6 = !m_BloomParamsPrev.Equals(ref other);
			bool flag = bloom.HasProperty(ShaderConstants._Params);
			if (num6 || !flag)
			{
				bloom.SetVector(ShaderConstants._Params, other.parameters);
				CoreUtils.SetKeyword(bloom, ShaderKeywordStrings.BloomHQ, other.highQualityFiltering);
				CoreUtils.SetKeyword(bloom, ShaderKeywordStrings.UseRGBM, other.useRGBM);
				CoreUtils.SetKeyword(bloom, "_ENABLE_ALPHA_OUTPUT", other.enableAlphaOutput);
				for (uint num7 = 0u; num7 < 16; num7++)
				{
					Material obj = m_Materials.bloomUpsample[num7];
					obj.SetVector(ShaderConstants._Params, other.parameters);
					CoreUtils.SetKeyword(obj, ShaderKeywordStrings.BloomHQ, other.highQualityFiltering);
					CoreUtils.SetKeyword(obj, ShaderKeywordStrings.UseRGBM, other.useRGBM);
					CoreUtils.SetKeyword(obj, "_ENABLE_ALPHA_OUTPUT", other.enableAlphaOutput);
				}
				m_BloomParamsPrev = other;
			}
			RenderTextureDescriptor compatibleDescriptor = GetCompatibleDescriptor(num2, num3, m_DefaultColorFormat);
			_BloomMipDown[0] = WaaaghRenderer.CreateRenderGraphTexture(renderGraph, compatibleDescriptor, m_BloomMipDown[0].name, clear: false, FilterMode.Bilinear);
			_BloomMipUp[0] = WaaaghRenderer.CreateRenderGraphTexture(renderGraph, compatibleDescriptor, m_BloomMipUp[0].name, clear: false, FilterMode.Bilinear);
			for (int i = 1; i < num4; i++)
			{
				num2 = Mathf.Max(1, num2 >> 1);
				num3 = Mathf.Max(1, num3 >> 1);
				ref TextureHandle reference = ref _BloomMipDown[i];
				ref TextureHandle reference2 = ref _BloomMipUp[i];
				compatibleDescriptor.width = num2;
				compatibleDescriptor.height = num3;
				reference = WaaaghRenderer.CreateRenderGraphTexture(renderGraph, compatibleDescriptor, m_BloomMipDown[i].name, clear: false, FilterMode.Bilinear);
				reference2 = WaaaghRenderer.CreateRenderGraphTexture(renderGraph, compatibleDescriptor, m_BloomMipUp[i].name, clear: false, FilterMode.Bilinear);
			}
		}
		BloomPassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = renderGraph.AddUnsafePass<BloomPassData>("Bloom", out passData, ProfilingSampler.Get(WaaaghProfileId.Bloom), ".\\Library\\PackageCache\\com.owlcat.visual@f3d4bf622f68\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 1656);
		passData.mipCount = num4;
		passData.material = m_Materials.Bloom;
		passData.upsampleMaterials = m_Materials.bloomUpsample;
		passData.sourceTexture = source;
		passData.bloomMipDown = _BloomMipDown;
		passData.bloomMipUp = _BloomMipUp;
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		unsafeRenderGraphBuilder.UseTexture(in source);
		for (int j = 0; j < num4; j++)
		{
			unsafeRenderGraphBuilder.UseTexture(in _BloomMipDown[j], AccessFlags.ReadWrite);
			unsafeRenderGraphBuilder.UseTexture(in _BloomMipUp[j], AccessFlags.ReadWrite);
		}
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(BloomPassData data, UnsafeGraphContext context)
		{
			CommandBuffer nativeCommandBuffer = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);
			Material material = data.material;
			int mipCount = data.mipCount;
			RenderBufferLoadAction loadAction = RenderBufferLoadAction.DontCare;
			RenderBufferStoreAction storeAction = RenderBufferStoreAction.Store;
			using (new ProfilingScope(nativeCommandBuffer, ProfilingSampler.Get(WaaaghProfileId.BloomPrefilter)))
			{
				Blitter.BlitCameraTexture(nativeCommandBuffer, data.sourceTexture, data.bloomMipDown[0], loadAction, storeAction, material, 0);
			}
			using (new ProfilingScope(nativeCommandBuffer, ProfilingSampler.Get(WaaaghProfileId.BloomDownsample)))
			{
				TextureHandle textureHandle = data.bloomMipDown[0];
				for (int k = 1; k < mipCount; k++)
				{
					TextureHandle textureHandle2 = data.bloomMipDown[k];
					TextureHandle textureHandle3 = data.bloomMipUp[k];
					Blitter.BlitCameraTexture(nativeCommandBuffer, textureHandle, textureHandle3, loadAction, storeAction, material, 1);
					Blitter.BlitCameraTexture(nativeCommandBuffer, textureHandle3, textureHandle2, loadAction, storeAction, material, 2);
					textureHandle = textureHandle2;
				}
			}
			using (new ProfilingScope(nativeCommandBuffer, ProfilingSampler.Get(WaaaghProfileId.BloomUpsample)))
			{
				for (int num8 = mipCount - 2; num8 >= 0; num8--)
				{
					TextureHandle textureHandle4 = ((num8 == mipCount - 2) ? data.bloomMipDown[num8 + 1] : data.bloomMipUp[num8 + 1]);
					TextureHandle textureHandle5 = data.bloomMipDown[num8];
					TextureHandle textureHandle6 = data.bloomMipUp[num8];
					Material material2 = data.upsampleMaterials[num8];
					material2.SetTexture(ShaderConstants._SourceTexLowMip, textureHandle4);
					Blitter.BlitCameraTexture(nativeCommandBuffer, textureHandle5, textureHandle6, loadAction, storeAction, material2, 3);
				}
			}
		});
		destination = passData.bloomMipUp[0];
	}

	private void RenderBloomEnhancedTexture(RenderGraph renderGraph, in TextureHandle source, out TextureHandle destination, bool isAlphaOutputEnabled)
	{
		int num = m_Descriptor.width / 2;
		int num2 = m_Descriptor.height / 2;
		float num3 = Mathf.Log(num2, 2f) + m_BloomEnhanced.radius.value - 8f;
		int num4 = (int)num3;
		int num5 = Mathf.Clamp(num4, 1, 16);
		float thresholdLinear = m_BloomEnhanced.thresholdLinear;
		float y = (m_BloomEnhanced.antiFlicker.value ? (-0.5f) : 0f);
		float z = 0.5f + num3 - (float)num4;
		Material bloomEnhanced = m_Materials.BloomEnhanced;
		bloomEnhanced.SetVector(ShaderConstants._Params, new Vector4(thresholdLinear, y, z, m_BloomEnhanced.dirtIntensity.value));
		bloomEnhanced.SetVector(ShaderConstants._Params1, new Vector4(m_BloomEnhanced.clamp.value, 0f, 0f, 0f));
		float num6 = thresholdLinear * m_BloomEnhanced.softKnee.value + 1E-05f;
		bloomEnhanced.SetVector(value: new Vector3(thresholdLinear - num6, num6 * 2f, 0.25f / num6), nameID: ShaderConstants._Curve);
		CoreUtils.SetKeyword(bloomEnhanced, ShaderKeywordStrings.ANTI_FLICKER, m_BloomEnhanced.antiFlicker.value);
		CoreUtils.SetKeyword(bloomEnhanced, ShaderKeywordStrings.UseRGBM, m_DefaultColorFormatUseRGBM);
		RenderTextureDescriptor compatibleDescriptor = GetCompatibleDescriptor(num, num2, m_DefaultColorFormat);
		_BloomMipDown[0] = WaaaghRenderer.CreateRenderGraphTexture(renderGraph, compatibleDescriptor, m_BloomMipDown[0].name, clear: false, FilterMode.Bilinear);
		_BloomMipUp[0] = WaaaghRenderer.CreateRenderGraphTexture(renderGraph, compatibleDescriptor, m_BloomMipUp[0].name, clear: false, FilterMode.Bilinear);
		for (int i = 1; i < num5; i++)
		{
			num = Mathf.Max(1, num >> 1);
			num2 = Mathf.Max(1, num2 >> 1);
			ref TextureHandle reference = ref _BloomMipDown[i];
			ref TextureHandle reference2 = ref _BloomMipUp[i];
			compatibleDescriptor.width = num;
			compatibleDescriptor.height = num2;
			reference = WaaaghRenderer.CreateRenderGraphTexture(renderGraph, compatibleDescriptor, m_BloomMipDown[i].name, clear: false, FilterMode.Bilinear);
			reference2 = WaaaghRenderer.CreateRenderGraphTexture(renderGraph, compatibleDescriptor, m_BloomMipUp[i].name, clear: false, FilterMode.Bilinear);
		}
		BloomEnhancedPassData passData2;
		using RenderGraphBuilder renderGraphBuilder = renderGraph.AddRenderPass<BloomEnhancedPassData>("Bloom Enhanced", out passData2, ".\\Library\\PackageCache\\com.owlcat.visual@f3d4bf622f68\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 1795);
		passData2.Source = renderGraphBuilder.ReadTexture(in source);
		passData2.Material = m_Materials.BloomEnhanced;
		passData2.BloomMipDown = _BloomMipDown;
		passData2.BloomMipUp = _BloomMipUp;
		passData2.MipCount = num5;
		for (int j = 0; j < num5; j++)
		{
			renderGraphBuilder.ReadWriteTexture(in _BloomMipDown[j]);
			renderGraphBuilder.ReadWriteTexture(in _BloomMipUp[j]);
		}
		renderGraphBuilder.SetRenderFunc(delegate(BloomEnhancedPassData passData, RenderGraphContext context)
		{
			Blitter.BlitCameraTexture(context.cmd, passData.Source, passData.BloomMipDown[0], passData.Material, 0);
			TextureHandle textureHandle = passData.BloomMipDown[0];
			for (int k = 0; k < passData.MipCount; k++)
			{
				TextureHandle textureHandle2 = passData.BloomMipDown[k];
				TextureHandle textureHandle3 = passData.BloomMipUp[k];
				Blitter.BlitCameraTexture(context.cmd, textureHandle, textureHandle3, passData.Material, 1);
				Blitter.BlitCameraTexture(context.cmd, textureHandle3, textureHandle2, passData.Material, 2);
				textureHandle = textureHandle2;
			}
			for (int num7 = passData.MipCount - 2; num7 >= 0; num7--)
			{
				TextureHandle textureHandle4 = passData.BloomMipDown[num7];
				context.cmd.SetGlobalTexture(ShaderConstants._BaseTex, textureHandle4);
				Blitter.BlitCameraTexture(context.cmd, textureHandle, passData.BloomMipUp[num7], passData.Material, 3);
				textureHandle = passData.BloomMipUp[num7];
			}
			context.cmd.SetGlobalTexture(ShaderConstants._Bloom_Texture, passData.BloomMipUp[0]);
		});
		destination = passData2.BloomMipUp[0];
	}

	private void UberPostSetupBloomEnhancedPass(RenderGraph rendergraph, in TextureHandle bloomTexture, Material uberMaterial)
	{
		UberSetupBloomPassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = rendergraph.AddRasterRenderPass<UberSetupBloomPassData>("UberPost - UberPostSetupBloomPass", out passData, ProfilingSampler.Get(WaaaghProfileId.UberPostSetupBloomPass), ".\\Library\\PackageCache\\com.owlcat.visual@f3d4bf622f68\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 1846);
		Color color = m_BloomEnhanced.tint.value.linear;
		float num = ColorUtils.Luminance(in color);
		color = ((num > 0f) ? (color * (1f / num)) : Color.white);
		Vector4 bloomParams = new Vector4(m_BloomEnhanced.intensity.value, color.r, color.g, color.b);
		Texture texture = ((m_BloomEnhanced.dirtTexture.value == null) ? Texture2D.blackTexture : m_BloomEnhanced.dirtTexture.value);
		float num2 = (float)texture.width / (float)texture.height;
		float num3 = (float)m_Descriptor.width / (float)m_Descriptor.height;
		Vector4 dirtScaleOffset = new Vector4(1f, 1f, 0f, 0f);
		float value = m_BloomEnhanced.dirtIntensity.value;
		if (num2 > num3)
		{
			dirtScaleOffset.x = num3 / num2;
			dirtScaleOffset.z = (1f - dirtScaleOffset.x) * 0.5f;
		}
		else if (num3 > num2)
		{
			dirtScaleOffset.y = num2 / num3;
			dirtScaleOffset.w = (1f - dirtScaleOffset.y) * 0.5f;
		}
		passData.bloomParams = bloomParams;
		passData.dirtScaleOffset = dirtScaleOffset;
		passData.dirtIntensity = value;
		passData.dirtTexture = texture;
		passData.highQualityFilteringValue = true;
		passData.useRGBM = m_DefaultColorFormatUseRGBM;
		passData.bloomTexture = bloomTexture;
		rasterRenderGraphBuilder.UseTexture(in bloomTexture);
		passData.uberMaterial = uberMaterial;
		rasterRenderGraphBuilder.AllowPassCulling(value: false);
		rasterRenderGraphBuilder.SetRenderFunc(delegate(UberSetupBloomPassData data, RasterGraphContext context)
		{
			Material uberMaterial2 = data.uberMaterial;
			uberMaterial2.SetVector(ShaderConstants._Bloom_Params, data.bloomParams);
			uberMaterial2.SetFloat(ShaderConstants._Bloom_RGBM, data.useRGBM ? 1f : 0f);
			uberMaterial2.SetVector(ShaderConstants._LensDirt_Params, data.dirtScaleOffset);
			uberMaterial2.SetFloat(ShaderConstants._LensDirt_Intensity, data.dirtIntensity);
			uberMaterial2.SetTexture(ShaderConstants._LensDirt_Texture, data.dirtTexture);
			if (data.highQualityFilteringValue)
			{
				uberMaterial2.EnableKeyword((data.dirtIntensity > 0f) ? ShaderKeywordStrings.BloomHQDirt : ShaderKeywordStrings.BloomHQ);
			}
			else
			{
				uberMaterial2.EnableKeyword((data.dirtIntensity > 0f) ? ShaderKeywordStrings.BloomLQDirt : ShaderKeywordStrings.BloomLQ);
			}
			uberMaterial2.SetTexture(ShaderConstants._Bloom_Texture, data.bloomTexture);
		});
	}

	private void RenderTemporalAA(RenderGraph renderGraph, WaaaghResourceData resourceData, WaaaghCameraData cameraData, ref TextureHandle source, out TextureHandle destination)
	{
		RenderTextureDescriptor compatibleDescriptor = GetCompatibleDescriptor(m_Descriptor, m_Descriptor.width, m_Descriptor.height, m_Descriptor.graphicsFormat);
		destination = WaaaghRenderer.CreateRenderGraphTexture(renderGraph, compatibleDescriptor, "_TemporalAATarget", clear: false, FilterMode.Bilinear);
		TextureHandle srcDepth = resourceData.CameraDepthBuffer;
		TextureHandle srcMotionVectors = resourceData.CameraMotionVectorsRT;
		TemporalAA.Render(renderGraph, m_Materials.TemporalAntialiasing, cameraData, ref source, ref srcDepth, ref srcMotionVectors, ref destination);
	}

	private void RenderSTP(RenderGraph renderGraph, WaaaghResourceData resourceData, WaaaghCameraData cameraData, ref TextureHandle source, out TextureHandle destination)
	{
		TextureHandle cameraDepthBuffer = resourceData.CameraDepthBuffer;
		TextureHandle cameraMotionVectorsRT = resourceData.CameraMotionVectorsRT;
		RenderTextureDescriptor compatibleDescriptor = GetCompatibleDescriptor(cameraData.cameraTargetDescriptor, cameraData.pixelWidth, cameraData.pixelHeight, cameraData.cameraTargetDescriptor.graphicsFormat);
		compatibleDescriptor.enableRandomWrite = true;
		compatibleDescriptor.sRGB = false;
		destination = WaaaghRenderer.CreateRenderGraphTexture(renderGraph, compatibleDescriptor, "_UpscaledColorTarget", clear: false, FilterMode.Bilinear);
		int frameCount = Time.frameCount;
		Texture2D noiseTexture = m_Resources.Textures.BlueNoise16Textures[frameCount & (m_Resources.Textures.BlueNoise16Textures.Length - 1)];
		StpUtils.Execute(renderGraph, resourceData, cameraData, source, cameraDepthBuffer, cameraMotionVectorsRT, destination, noiseTexture);
		UpdateCameraResolution(renderGraph, cameraData, new Vector2Int(compatibleDescriptor.width, compatibleDescriptor.height));
	}

	private void UpdateCameraResolution(RenderGraph renderGraph, WaaaghCameraData cameraData, Vector2Int newCameraTargetSize)
	{
		m_Descriptor.width = newCameraTargetSize.x;
		m_Descriptor.height = newCameraTargetSize.y;
		cameraData.cameraTargetDescriptor.width = newCameraTargetSize.x;
		cameraData.cameraTargetDescriptor.height = newCameraTargetSize.y;
		UpdateCameraResolutionPassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = renderGraph.AddUnsafePass<UpdateCameraResolutionPassData>("Update Camera Resolution", out passData, ".\\Library\\PackageCache\\com.owlcat.visual@f3d4bf622f68\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 1980);
		passData.newCameraTargetSize = newCameraTargetSize;
		unsafeRenderGraphBuilder.AllowGlobalStateModification(value: true);
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(UpdateCameraResolutionPassData data, UnsafeGraphContext ctx)
		{
			ctx.cmd.SetGlobalVector(ShaderPropertyId._ScreenSize, new Vector4(data.newCameraTargetSize.x, data.newCameraTargetSize.y, 1f / (float)data.newCameraTargetSize.x, 1f / (float)data.newCameraTargetSize.y));
		});
	}

	private static float GetLensFlareLightAttenuation(Light light, Camera cam, Vector3 wo)
	{
		if (light != null)
		{
			return light.type switch
			{
				LightType.Directional => LensFlareCommonSRP.ShapeAttenuationDirLight(light.transform.forward, cam.transform.forward), 
				LightType.Point => LensFlareCommonSRP.ShapeAttenuationPointLight(), 
				LightType.Spot => LensFlareCommonSRP.ShapeAttenuationSpotConeLight(light.transform.forward, wo, light.spotAngle, light.innerSpotAngle / 180f), 
				_ => 1f, 
			};
		}
		return 1f;
	}

	private void LensFlareDataDrivenComputeOcclusion(RenderGraph renderGraph, WaaaghResourceData resourceData, WaaaghCameraData cameraData)
	{
		if (!LensFlareCommonSRP.IsOcclusionRTCompatible())
		{
			return;
		}
		LensFlarePassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = renderGraph.AddUnsafePass<LensFlarePassData>("Lens Flare Compute Occlusion", out passData, ProfilingSampler.Get(WaaaghProfileId.LensFlareDataDrivenComputeOcclusion), ".\\Library\\PackageCache\\com.owlcat.visual@f3d4bf622f68\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 2047);
		_ = LensFlareCommonSRP.occlusionRT;
		TextureHandle input = (passData.destinationTexture = renderGraph.ImportTexture(LensFlareCommonSRP.occlusionRT));
		unsafeRenderGraphBuilder.UseTexture(in input, AccessFlags.Write);
		passData.cameraData = cameraData;
		passData.viewport = cameraData.pixelRect;
		passData.material = m_Materials.LensFlareDataDriven;
		passData.width = m_Descriptor.width;
		passData.height = m_Descriptor.height;
		if (m_PaniniProjection.IsActive())
		{
			passData.usePanini = true;
			passData.paniniDistance = m_PaniniProjection.distance.value;
			passData.paniniCropToFit = m_PaniniProjection.cropToFit.value;
		}
		else
		{
			passData.usePanini = false;
			passData.paniniDistance = 1f;
			passData.paniniCropToFit = 1f;
		}
		passData.DefaultXr = m_DefaultXr;
		unsafeRenderGraphBuilder.UseTexture(in resourceData.CameraDepthCopyRT);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(LensFlarePassData data, UnsafeGraphContext ctx)
		{
			Camera camera = data.cameraData.camera;
			XRPass defaultXr = data.DefaultXr;
			Matrix4x4 viewProjMatrix = GL.GetGPUProjectionMatrix(camera.projectionMatrix, renderIntoTexture: true) * camera.worldToCameraMatrix;
			_ = defaultXr.multipassId;
			LensFlareCommonSRP.ComputeOcclusion(data.material, camera, defaultXr, defaultXr.multipassId, data.width, data.height, data.usePanini, data.paniniDistance, data.paniniCropToFit, isCameraRelative: true, camera.transform.position, viewProjMatrix, ctx.cmd, taaEnabled: false, hasCloudLayer: false, null, null);
		});
	}

	public void RenderLensFlareDataDriven(RenderGraph renderGraph, WaaaghResourceData resourceData, WaaaghCameraData cameraData, in TextureHandle destination)
	{
		LensFlarePassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = renderGraph.AddUnsafePass<LensFlarePassData>("Lens Flare Data Driven Pass", out passData, ProfilingSampler.Get(WaaaghProfileId.LensFlareDataDriven), ".\\Library\\PackageCache\\com.owlcat.visual@f3d4bf622f68\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 2146);
		passData.destinationTexture = destination;
		unsafeRenderGraphBuilder.UseTexture(in destination, AccessFlags.Write);
		passData.sourceDescriptor = m_Descriptor;
		passData.cameraData = cameraData;
		passData.material = m_Materials.LensFlareDataDriven;
		passData.width = m_Descriptor.width;
		passData.height = m_Descriptor.height;
		passData.viewport.x = 0f;
		passData.viewport.y = 0f;
		passData.viewport.width = m_Descriptor.width;
		passData.viewport.height = m_Descriptor.height;
		if (m_PaniniProjection.IsActive())
		{
			passData.usePanini = true;
			passData.paniniDistance = m_PaniniProjection.distance.value;
			passData.paniniCropToFit = m_PaniniProjection.cropToFit.value;
		}
		else
		{
			passData.usePanini = false;
			passData.paniniDistance = 1f;
			passData.paniniCropToFit = 1f;
		}
		if (LensFlareCommonSRP.IsOcclusionRTCompatible())
		{
			TextureHandle input = renderGraph.ImportTexture(LensFlareCommonSRP.occlusionRT);
			unsafeRenderGraphBuilder.UseTexture(in input);
		}
		else
		{
			unsafeRenderGraphBuilder.UseTexture(in resourceData.CameraDepthCopyRT);
		}
		passData.DefaultXr = m_DefaultXr;
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(LensFlarePassData data, UnsafeGraphContext ctx)
		{
			Camera camera = data.cameraData.camera;
			XRPass defaultXr = data.DefaultXr;
			Matrix4x4 viewProjMatrix = GL.GetGPUProjectionMatrix(camera.projectionMatrix, renderIntoTexture: true) * camera.worldToCameraMatrix;
			LensFlareCommonSRP.DoLensFlareDataDrivenCommon(data.material, data.cameraData.camera, data.viewport, defaultXr, defaultXr.multipassId, data.width, data.height, data.usePanini, data.paniniDistance, data.paniniCropToFit, isCameraRelative: true, camera.transform.position, viewProjMatrix, ctx.cmd, taaEnabled: false, hasCloudLayer: false, null, null, data.destinationTexture, (Light light, Camera cam, Vector3 wo) => GetLensFlareLightAttenuation(light, cam, wo), debugView: false);
		});
	}

	public TextureHandle RenderLensFlareScreenSpace(RenderGraph renderGraph, Camera camera, in TextureHandle destination, TextureHandle originalBloomTexture, TextureHandle screenSpaceLensFlareBloomMipTexture, bool enableXR)
	{
		int value = (int)m_LensFlareScreenSpace.resolution.value;
		int width = m_Descriptor.width / value;
		int height = m_Descriptor.height / value;
		RenderTextureDescriptor compatibleDescriptor = GetCompatibleDescriptor(m_Descriptor, width, height, m_DefaultColorFormat);
		TextureHandle input = WaaaghRenderer.CreateRenderGraphTexture(renderGraph, compatibleDescriptor, "_StreakTmpTexture", clear: true, FilterMode.Bilinear);
		TextureHandle input2 = WaaaghRenderer.CreateRenderGraphTexture(renderGraph, compatibleDescriptor, "_StreakTmpTexture2", clear: true, FilterMode.Bilinear);
		TextureHandle input3 = WaaaghRenderer.CreateRenderGraphTexture(renderGraph, compatibleDescriptor, "Lens Flare Screen Space Result", clear: true, FilterMode.Bilinear);
		LensFlareScreenSpacePassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = renderGraph.AddUnsafePass<LensFlareScreenSpacePassData>("Lens Flare Screen Space Pass", out passData, ProfilingSampler.Get(WaaaghProfileId.LensFlareScreenSpace), ".\\Library\\PackageCache\\com.owlcat.visual@f3d4bf622f68\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 2268);
		passData.destinationTexture = destination;
		unsafeRenderGraphBuilder.UseTexture(in destination, AccessFlags.Write);
		passData.streakTmpTexture = input;
		unsafeRenderGraphBuilder.UseTexture(in input, AccessFlags.ReadWrite);
		passData.streakTmpTexture2 = input2;
		unsafeRenderGraphBuilder.UseTexture(in input2, AccessFlags.ReadWrite);
		passData.screenSpaceLensFlareBloomMipTexture = screenSpaceLensFlareBloomMipTexture;
		unsafeRenderGraphBuilder.UseTexture(in screenSpaceLensFlareBloomMipTexture, AccessFlags.ReadWrite);
		passData.originalBloomTexture = originalBloomTexture;
		unsafeRenderGraphBuilder.UseTexture(in originalBloomTexture, AccessFlags.ReadWrite);
		passData.sourceDescriptor = m_Descriptor;
		passData.camera = camera;
		passData.material = m_Materials.LensFlareScreenSpace;
		passData.lensFlareScreenSpace = m_LensFlareScreenSpace;
		passData.downsample = value;
		passData.result = input3;
		unsafeRenderGraphBuilder.UseTexture(in input3, AccessFlags.Write);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(LensFlareScreenSpacePassData data, UnsafeGraphContext context)
		{
			UnsafeCommandBuffer cmd = context.cmd;
			Camera camera2 = data.camera;
			ScreenSpaceLensFlare lensFlareScreenSpace = data.lensFlareScreenSpace;
			LensFlareCommonSRP.DoLensFlareScreenSpaceCommon(data.material, camera2, data.sourceDescriptor.width, data.sourceDescriptor.height, data.lensFlareScreenSpace.tintColor.value, data.originalBloomTexture, data.screenSpaceLensFlareBloomMipTexture, null, data.streakTmpTexture, data.streakTmpTexture2, new Vector4(lensFlareScreenSpace.intensity.value, lensFlareScreenSpace.firstFlareIntensity.value, lensFlareScreenSpace.secondaryFlareIntensity.value, lensFlareScreenSpace.warpedFlareIntensity.value), new Vector4(lensFlareScreenSpace.vignetteEffect.value, lensFlareScreenSpace.startingPosition.value, lensFlareScreenSpace.scale.value, 0f), new Vector4(lensFlareScreenSpace.samples.value, lensFlareScreenSpace.sampleDimmer.value, lensFlareScreenSpace.chromaticAbberationIntensity.value, 0f), new Vector4(lensFlareScreenSpace.streaksIntensity.value, lensFlareScreenSpace.streaksLength.value, lensFlareScreenSpace.streaksOrientation.value, lensFlareScreenSpace.streaksThreshold.value), new Vector4(data.downsample, lensFlareScreenSpace.warpedFlareScale.value.x, lensFlareScreenSpace.warpedFlareScale.value.y, 0f), cmd, data.result, debugView: false);
		});
		return passData.originalBloomTexture;
	}

	private void SetupLensDistortion(Material material, bool isSceneView)
	{
		float b = 1.6f * Mathf.Max(Mathf.Abs(m_LensDistortion.intensity.value * 100f), 1f);
		float num = MathF.PI / 180f * Mathf.Min(160f, b);
		float y = 2f * Mathf.Tan(num * 0.5f);
		Vector2 vector = m_LensDistortion.center.value * 2f - Vector2.one;
		Vector4 value = new Vector4(vector.x, vector.y, Mathf.Max(m_LensDistortion.xMultiplier.value, 0.0001f), Mathf.Max(m_LensDistortion.yMultiplier.value, 0.0001f));
		Vector4 value2 = new Vector4((m_LensDistortion.intensity.value >= 0f) ? num : (1f / num), y, 1f / m_LensDistortion.scale.value, m_LensDistortion.intensity.value * 100f);
		material.SetVector(ShaderConstants._Distortion_Params1, value);
		material.SetVector(ShaderConstants._Distortion_Params2, value2);
		if (m_LensDistortion.IsActive() && !isSceneView)
		{
			material.EnableKeyword(ShaderKeywordStrings.Distortion);
		}
	}

	private void SetupChromaticAberration(Material material)
	{
		material.SetFloat(ShaderConstants._Chroma_Params, m_ChromaticAberration.intensity.value * 0.05f);
		if (m_ChromaticAberration.IsActive())
		{
			material.EnableKeyword(ShaderKeywordStrings.ChromaticAberration);
		}
	}

	private void SetupVignette(Material material, XRPass xrPass)
	{
		Color value = m_Vignette.color.value;
		Vector2 value2 = m_Vignette.center.value;
		float num = (float)m_Descriptor.width / (float)m_Descriptor.height;
		Vector4 value3 = new Vector4(value.r, value.g, value.b, m_Vignette.rounded.value ? num : 1f);
		Vector4 value4 = new Vector4(value2.x, value2.y, m_Vignette.intensity.value * 3f, m_Vignette.smoothness.value * 5f);
		material.SetVector(ShaderConstants._Vignette_Params1, value3);
		material.SetVector(ShaderConstants._Vignette_Params2, value4);
	}

	private void SetupGrain(WaaaghCameraData cameraData, Material material)
	{
		if (!m_HasFinalPass && m_FilmGrain.IsActive())
		{
			material.EnableKeyword(ShaderKeywordStrings.FilmGrain);
			PostProcessUtils.ConfigureFilmGrain(m_Resources, m_FilmGrain, cameraData.pixelWidth, cameraData.pixelHeight, material);
		}
	}

	private void SetupDithering(WaaaghCameraData cameraData, Material material)
	{
		if (!m_HasFinalPass && cameraData.isDitheringEnabled)
		{
			material.EnableKeyword(ShaderKeywordStrings.Dithering);
			m_DitheringTextureIndex = PostProcessUtils.ConfigureDithering(m_Resources, m_DitheringTextureIndex, cameraData.pixelWidth, cameraData.pixelHeight, material);
		}
	}

	private void SetupHDROutput(HDROutputUtils.HDRDisplayInformation hdrDisplayInformation, ColorGamut hdrDisplayColorGamut, Material material, HDROutputUtils.Operation hdrOperations)
	{
		WaaaghPipeline.GetHDROutputLuminanceParameters(hdrDisplayInformation, hdrDisplayColorGamut, m_Tonemapping, out var hdrOutputParameters);
		material.SetVector(ShaderPropertyId._HDROutputLuminanceParams, hdrOutputParameters);
		HDROutputUtils.ConfigureHDROutput(material, hdrDisplayColorGamut, hdrOperations);
	}

	public unsafe static bool EqualsValueType<T>(T a, T b) where T : unmanaged
	{
		return UnsafeUtility.MemCmp(&a, &b, sizeof(T)) == 0;
	}

	public void RenderUberPost(RenderGraph renderGraph, WaaaghCameraData cameraData, WaaaghPostProcessingData postProcessingData, in TextureHandle sourceTexture, in TextureHandle destTexture, in TextureHandle lutTexture, in TextureHandle overlayUITexture, bool requireHDROutput, bool enableAlphaOutput)
	{
		bool num = EqualsValueType(sourceTexture, destTexture);
		Material uber = m_Materials.Uber;
		bool isHdrGrading = postProcessingData.gradingMode == ColorGradingMode.HighDynamicRange;
		int lutSize = postProcessingData.lutSize;
		int num2 = lutSize * lutSize;
		Vector4 lutParams = new Vector4(w: Mathf.Pow(2f, m_ColorAdjustments.postExposure.value), x: 1f / (float)num2, y: 1f / (float)lutSize, z: (float)lutSize - 1f);
		RTHandle rTHandle = (m_ColorLookup.texture.value ? RTHandles.Alloc(m_ColorLookup.texture.value) : null);
		TextureHandle input = ((rTHandle != null) ? renderGraph.ImportTexture(rTHandle) : TextureHandle.nullHandle);
		Vector4 userLutParams = ((!m_ColorLookup.IsActive()) ? Vector4.zero : new Vector4(1f / (float)m_ColorLookup.texture.value.width, 1f / (float)m_ColorLookup.texture.value.height, (float)m_ColorLookup.texture.value.height - 1f, m_ColorLookup.contribution.value));
		TextureHandle input2 = sourceTexture;
		if (num)
		{
			RenderTextureDescriptor compatibleDescriptor = GetCompatibleDescriptor();
			TextureHandle textureHandle = WaaaghRenderer.CreateRenderGraphTexture(renderGraph, compatibleDescriptor, "UberPostSwapBuffer", clear: false, FilterMode.Bilinear);
			UberPostPassData passData;
			using (IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<UberPostPassData>("Postprocessing Uber Swap Copy", out passData, ".\\Library\\PackageCache\\com.owlcat.visual@f3d4bf622f68\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 2523))
			{
				rasterRenderGraphBuilder.UseTexture(in sourceTexture);
				passData.sourceTexture = sourceTexture;
				rasterRenderGraphBuilder.SetRenderAttachment(textureHandle, 0);
				passData.destinationTexture = textureHandle;
				passData.cameraData = cameraData;
				passData.material = m_BlitMaterial;
				rasterRenderGraphBuilder.SetRenderFunc(delegate(UberPostPassData data, RasterGraphContext context)
				{
					ScaleViewportAndBlit(context.cmd, data.sourceTexture, data.destinationTexture, data.cameraData, data.material);
				});
			}
			input2 = textureHandle;
		}
		UberPostPassData passData2;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder2 = renderGraph.AddRasterRenderPass<UberPostPassData>("Postprocessing Uber Post Pass", out passData2, ProfilingSampler.Get(WaaaghProfileId.UberPost), ".\\Library\\PackageCache\\com.owlcat.visual@f3d4bf622f68\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 2541);
		rasterRenderGraphBuilder2.AllowGlobalStateModification(value: true);
		passData2.destinationTexture = destTexture;
		rasterRenderGraphBuilder2.SetRenderAttachment(destTexture, 0);
		passData2.sourceTexture = input2;
		rasterRenderGraphBuilder2.UseTexture(in input2);
		passData2.lutTexture = lutTexture;
		rasterRenderGraphBuilder2.UseTexture(in lutTexture);
		passData2.lutParams = lutParams;
		if (input.IsValid())
		{
			passData2.userLutTexture = input;
			rasterRenderGraphBuilder2.UseTexture(in input);
		}
		if (m_Bloom.IsActive())
		{
			rasterRenderGraphBuilder2.UseTexture(in _BloomMipUp[0]);
		}
		if (requireHDROutput && m_EnableColorEncodingIfNeeded)
		{
			rasterRenderGraphBuilder2.UseTexture(in overlayUITexture);
		}
		passData2.userLutParams = userLutParams;
		passData2.cameraData = cameraData;
		passData2.resources = m_Resources;
		passData2.tonemapping = m_Tonemapping;
		passData2.material = uber;
		passData2.toneMappingMode = m_Tonemapping.mode.value;
		passData2.isHdrGrading = isHdrGrading;
		passData2.enableAlphaOutput = enableAlphaOutput;
		rasterRenderGraphBuilder2.SetRenderFunc(delegate(UberPostPassData data, RasterGraphContext context)
		{
			RasterCommandBuffer cmd = context.cmd;
			_ = data.cameraData.camera;
			Material material = data.material;
			RTHandle sourceTextureHdl = data.sourceTexture;
			material.SetTexture(ShaderConstants._InternalLut, data.lutTexture);
			material.SetVector(ShaderConstants._Lut_Params, data.lutParams);
			material.SetTexture(ShaderConstants._UserLut, data.userLutTexture);
			material.SetVector(ShaderConstants._UserLut_Params, data.userLutParams);
			if (data.isHdrGrading)
			{
				material.EnableKeyword(ShaderKeywordStrings.HDRGrading);
			}
			else
			{
				switch (data.toneMappingMode)
				{
				case TonemappingMode.Neutral:
					material.EnableKeyword(ShaderKeywordStrings.TonemapNeutral);
					break;
				case TonemappingMode.ACES:
					material.EnableKeyword(ShaderKeywordStrings.TonemapACES);
					break;
				case TonemappingMode.Makeev:
					material.EnableKeyword(MakeevTonemapping.TrySetupParameters(material, data.resources, data.tonemapping) ? ShaderKeywordStrings.TonemapMakeev : ShaderKeywordStrings.TonemapNeutral);
					break;
				}
			}
			CoreUtils.SetKeyword(material, "_ENABLE_ALPHA_OUTPUT", data.enableAlphaOutput);
			ScaleViewportAndBlit(cmd, sourceTextureHdl, data.destinationTexture, data.cameraData, material);
		});
	}

	private static void ScaleViewportAndBlit(RasterCommandBuffer cmd, RTHandle sourceTextureHdl, RTHandle dest, WaaaghCameraData cameraData, Material material)
	{
		Vector4 finalBlitScaleBias = RenderingUtils.GetFinalBlitScaleBias(sourceTextureHdl, dest, cameraData);
		RenderTargetIdentifier renderTargetIdentifier = BuiltinRenderTextureType.CameraTarget;
		if (dest.nameID == renderTargetIdentifier || cameraData.targetTexture != null)
		{
			cmd.SetViewport(cameraData.pixelRect);
		}
		Blitter.BlitTexture(cmd, sourceTextureHdl, finalBlitScaleBias, material, 0);
	}

	private bool RequireSRGBConversionBlitToBackBuffer(bool requireSrgbConversion)
	{
		if (requireSrgbConversion)
		{
			return m_EnableColorEncodingIfNeeded;
		}
		return false;
	}

	private bool RequireHDROutput(WaaaghCameraData cameraData)
	{
		if (cameraData.isHDROutputActive)
		{
			return cameraData.captureActions == null;
		}
		return false;
	}

	private RenderTextureDescriptor GetCompatibleDescriptor()
	{
		return GetCompatibleDescriptor(m_Descriptor.width, m_Descriptor.height, m_Descriptor.graphicsFormat);
	}

	private RenderTextureDescriptor GetCompatibleDescriptor(int width, int height, GraphicsFormat format, DepthBits depthBufferBits = DepthBits.None)
	{
		return GetCompatibleDescriptor(m_Descriptor, width, height, format, depthBufferBits);
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

	public void Cleanup()
	{
		m_MaterialLibrary.Cleanup();
	}
}
