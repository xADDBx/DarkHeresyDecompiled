using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.HBAO.Passes;

public class HBAOPass : ScriptableRenderPass
{
	private class PassData
	{
		public TextureHandle MotionVectorTexture;

		public Material Material { get; set; }

		public RenderTextureDescriptor TargetDescriptor { get; set; }

		public RenderTextureDescriptor AOTextureDescriptor { get; set; }

		public TextureHandle CameraDepthTexture { get; set; }

		public TextureHandle SourceTexture { get; set; }

		public TextureHandle AOTexture { get; set; }

		public TextureHandle TempTexture { get; set; }

		public TextureHandle DestinationTexture { get; set; }

		public HBAOHistoryBuffer HistoryBuffers { get; set; }

		public RenderTargetIdentifier[] TemporalFilterRenderTargets { get; set; }

		public Mesh FullscreenTriangle { get; set; }

		public MaterialPropertyBlock MaterialProperties { get; set; }

		public bool UseLitAO { get; set; }

		public bool UseColorBleeding { get; set; }

		public bool UseBlur { get; set; }

		public bool UseTemporalFilter { get; set; }

		public float DirectLightingStrength { get; set; }

		public bool ShowDebug { get; set; }

		public bool ShowViewNormals { get; set; }

		public bool RenderingInSceneView { get; set; }
	}

	private static class Pass
	{
		public const int AO = 0;

		public const int AO_Deinterleaved = 1;

		public const int Deinterleave_Depth = 2;

		public const int Deinterleave_Normals = 3;

		public const int Atlas_AO_Deinterleaved = 4;

		public const int Reinterleave_AO = 5;

		public const int Blur = 6;

		public const int Temporal_Filter = 7;

		public const int Copy = 8;

		public const int Composite = 9;

		public const int Debug_ViewNormals = 10;
	}

	private static class ShaderProperties
	{
		public static int mainTex;

		public static int inputTex;

		public static int hbaoTex;

		public static int tempTex;

		public static int tempTex2;

		public static int noiseTex;

		public static int depthTex;

		public static int normalsTex;

		public static int ssaoTex;

		public static int _MotionVectorTexture;

		public static int[] depthSliceTex;

		public static int[] normalsSliceTex;

		public static int[] aoSliceTex;

		public static int[] deinterleaveOffset;

		public static int atlasOffset;

		public static int jitter;

		public static int uvTransform;

		public static int inputTexelSize;

		public static int aoTexelSize;

		public static int deinterleavedAOTexelSize;

		public static int reinterleavedAOTexelSize;

		public static int uvToView;

		public static int targetScale;

		public static int radius;

		public static int maxRadiusPixels;

		public static int negInvRadius2;

		public static int angleBias;

		public static int aoMultiplier;

		public static int intensity;

		public static int multiBounceInfluence;

		public static int offscreenSamplesContrib;

		public static int maxDistance;

		public static int distanceFalloff;

		public static int baseColor;

		public static int colorBleedSaturation;

		public static int albedoMultiplier;

		public static int colorBleedBrightnessMask;

		public static int colorBleedBrightnessMaskRange;

		public static int blurDeltaUV;

		public static int blurSharpness;

		public static int temporalParams;

		public static int historyBufferRTHandleScale;

		public static int cameraDepthTexture;

		public static int screenSpaceOcclusionTexture;

		public static int screenSpaceOcclusionParam;

		public static GlobalKeyword screenSpaceOcclusionKeyword;

		static ShaderProperties()
		{
			mainTex = Shader.PropertyToID("_MainTex");
			inputTex = Shader.PropertyToID("_InputTex");
			hbaoTex = Shader.PropertyToID("_HBAOTex");
			tempTex = Shader.PropertyToID("_TempTex");
			tempTex2 = Shader.PropertyToID("_TempTex2");
			noiseTex = Shader.PropertyToID("_NoiseTex");
			depthTex = Shader.PropertyToID("_DepthTex");
			normalsTex = Shader.PropertyToID("_NormalsTex");
			ssaoTex = Shader.PropertyToID("_SSAOTex");
			_MotionVectorTexture = Shader.PropertyToID("_MotionVectorTexture");
			depthSliceTex = new int[16];
			normalsSliceTex = new int[16];
			aoSliceTex = new int[16];
			for (int i = 0; i < 16; i++)
			{
				depthSliceTex[i] = Shader.PropertyToID("_DepthSliceTex" + i);
				normalsSliceTex[i] = Shader.PropertyToID("_NormalsSliceTex" + i);
				aoSliceTex[i] = Shader.PropertyToID("_AOSliceTex" + i);
			}
			deinterleaveOffset = new int[4]
			{
				Shader.PropertyToID("_Deinterleave_Offset00"),
				Shader.PropertyToID("_Deinterleave_Offset10"),
				Shader.PropertyToID("_Deinterleave_Offset01"),
				Shader.PropertyToID("_Deinterleave_Offset11")
			};
			atlasOffset = Shader.PropertyToID("_AtlasOffset");
			jitter = Shader.PropertyToID("_Jitter");
			uvTransform = Shader.PropertyToID("_UVTransform");
			inputTexelSize = Shader.PropertyToID("_Input_TexelSize");
			aoTexelSize = Shader.PropertyToID("_AO_TexelSize");
			deinterleavedAOTexelSize = Shader.PropertyToID("_DeinterleavedAO_TexelSize");
			reinterleavedAOTexelSize = Shader.PropertyToID("_ReinterleavedAO_TexelSize");
			uvToView = Shader.PropertyToID("_UVToView");
			targetScale = Shader.PropertyToID("_TargetScale");
			radius = Shader.PropertyToID("_Radius");
			maxRadiusPixels = Shader.PropertyToID("_MaxRadiusPixels");
			negInvRadius2 = Shader.PropertyToID("_NegInvRadius2");
			angleBias = Shader.PropertyToID("_AngleBias");
			aoMultiplier = Shader.PropertyToID("_AOmultiplier");
			intensity = Shader.PropertyToID("_Intensity");
			multiBounceInfluence = Shader.PropertyToID("_MultiBounceInfluence");
			offscreenSamplesContrib = Shader.PropertyToID("_OffscreenSamplesContrib");
			maxDistance = Shader.PropertyToID("_MaxDistance");
			distanceFalloff = Shader.PropertyToID("_DistanceFalloff");
			baseColor = Shader.PropertyToID("_BaseColor");
			colorBleedSaturation = Shader.PropertyToID("_ColorBleedSaturation");
			albedoMultiplier = Shader.PropertyToID("_AlbedoMultiplier");
			colorBleedBrightnessMask = Shader.PropertyToID("_ColorBleedBrightnessMask");
			colorBleedBrightnessMaskRange = Shader.PropertyToID("_ColorBleedBrightnessMaskRange");
			blurDeltaUV = Shader.PropertyToID("_BlurDeltaUV");
			blurSharpness = Shader.PropertyToID("_BlurSharpness");
			temporalParams = Shader.PropertyToID("_TemporalParams");
			historyBufferRTHandleScale = Shader.PropertyToID("_HistoryBuffer_RTHandleScale");
			cameraDepthTexture = Shader.PropertyToID("_CameraDepthTexture");
			screenSpaceOcclusionTexture = Shader.PropertyToID("_ScreenSpaceOcclusionTexture");
			screenSpaceOcclusionParam = Shader.PropertyToID("_AmbientOcclusionParam");
			screenSpaceOcclusionKeyword = GlobalKeyword.Create("_SCREEN_SPACE_OCCLUSION");
		}

		public static string GetOrthographicProjectionKeyword(bool orthographic)
		{
			if (!orthographic)
			{
				return "__";
			}
			return "ORTHOGRAPHIC_PROJECTION";
		}

		public static string GetQualityKeyword(HBAO.Quality quality)
		{
			return quality switch
			{
				HBAO.Quality.Lowest => "QUALITY_LOWEST", 
				HBAO.Quality.Low => "QUALITY_LOW", 
				HBAO.Quality.Medium => "QUALITY_MEDIUM", 
				HBAO.Quality.High => "QUALITY_HIGH", 
				HBAO.Quality.Highest => "QUALITY_HIGHEST", 
				_ => "QUALITY_MEDIUM", 
			};
		}

		public static string GetNoiseKeyword(HBAO.NoiseType noiseType)
		{
			return noiseType switch
			{
				HBAO.NoiseType.InterleavedGradientNoise => "INTERLEAVED_GRADIENT_NOISE", 
				_ => "__", 
			};
		}

		public static string GetDeinterleavingKeyword(HBAO.Deinterleaving deinterleaving)
		{
			if (deinterleaving != 0 && deinterleaving == HBAO.Deinterleaving.x4)
			{
				return "DEINTERLEAVED";
			}
			return "__";
		}

		public static string GetDebugKeyword(HBAO.DebugMode debugMode)
		{
			return debugMode switch
			{
				HBAO.DebugMode.AOOnly => "DEBUG_AO", 
				HBAO.DebugMode.ColorBleedingOnly => "DEBUG_COLORBLEEDING", 
				HBAO.DebugMode.SplitWithoutAOAndWithAO => "DEBUG_NOAO_AO", 
				HBAO.DebugMode.SplitWithAOAndAOOnly => "DEBUG_AO_AOONLY", 
				HBAO.DebugMode.SplitWithoutAOAndAOOnly => "DEBUG_NOAO_AOONLY", 
				_ => "__", 
			};
		}

		public static string GetMultibounceKeyword(bool useMultiBounce, bool litAoModeEnabled)
		{
			if (!useMultiBounce || litAoModeEnabled)
			{
				return "__";
			}
			return "MULTIBOUNCE";
		}

		public static string GetOffscreenSamplesContributionKeyword(float offscreenSamplesContribution)
		{
			if (!(offscreenSamplesContribution > 0f))
			{
				return "__";
			}
			return "OFFSCREEN_SAMPLES_CONTRIBUTION";
		}

		public static string GetPerPixelNormalsKeyword(HBAO.PerPixelNormals perPixelNormals)
		{
			return perPixelNormals switch
			{
				HBAO.PerPixelNormals.Reconstruct4Samples => "NORMALS_RECONSTRUCT4", 
				HBAO.PerPixelNormals.Reconstruct2Samples => "NORMALS_RECONSTRUCT2", 
				_ => "__", 
			};
		}

		public static string GetBlurRadiusKeyword(HBAO.BlurType blurType)
		{
			return blurType switch
			{
				HBAO.BlurType.Narrow => "BLUR_RADIUS_2", 
				HBAO.BlurType.Medium => "BLUR_RADIUS_3", 
				HBAO.BlurType.Wide => "BLUR_RADIUS_4", 
				HBAO.BlurType.ExtraWide => "BLUR_RADIUS_5", 
				_ => "BLUR_RADIUS_3", 
			};
		}

		public static string GetVarianceClippingKeyword(HBAO.VarianceClipping varianceClipping)
		{
			return varianceClipping switch
			{
				HBAO.VarianceClipping._4Tap => "VARIANCE_CLIPPING_4TAP", 
				HBAO.VarianceClipping._8Tap => "VARIANCE_CLIPPING_8TAP", 
				_ => "__", 
			};
		}

		public static string GetColorBleedingKeyword(bool colorBleedingEnabled, bool litAoModeEnabled)
		{
			if (!colorBleedingEnabled || litAoModeEnabled)
			{
				return "__";
			}
			return "COLOR_BLEEDING";
		}

		public static string GetModeKeyword(HBAO.Mode mode, HBAO.DebugMode debugMode)
		{
			if (mode != HBAO.Mode.LitAO || debugMode != 0)
			{
				return "__";
			}
			return "LIT_AO";
		}
	}

	private Material m_Material;

	private HBAO m_Settings;

	private HBAOFeature m_Feature;

	private Vector4[] m_UVToViewPerEye = new Vector4[2];

	private float[] m_RadiusPerEye = new float[2];

	private string[] m_ShaderKeywords;

	private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("HBAO");

	private MaterialPropertyBlock m_MaterialPropertyBlock;

	private RenderTargetIdentifier[] m_RtsTemporalFilter = new RenderTargetIdentifier[2];

	private static readonly float[] s_temporalRotations = new float[6] { 60f, 300f, 180f, 240f, 120f, 0f };

	private static readonly float[] s_temporalOffsets = new float[4] { 0f, 0.5f, 0.25f, 0.75f };

	public override string Name => "HBAOPass";

	private RenderTextureDescriptor sourceDesc { get; set; }

	private RenderTextureDescriptor aoDesc { get; set; }

	private RenderTextureDescriptor ssaoDesc { get; set; }

	private RenderTextureDescriptor deinterleavedDepthDesc { get; set; }

	private RenderTextureDescriptor deinterleavedNormalsDesc { get; set; }

	private RenderTextureDescriptor deinterleavedAoDesc { get; set; }

	private RenderTextureDescriptor reinterleavedAoDesc { get; set; }

	private RenderTextureFormat colorFormat { get; set; }

	private RenderTextureFormat ssaoFormat { get; set; }

	private RenderTextureFormat depthFormat { get; set; }

	private RenderTextureFormat normalsFormat { get; set; }

	public GraphicsFormat graphicsColorFormat { get; set; }

	private GraphicsFormat graphicsDepthFormat { get; set; }

	private GraphicsFormat graphicsNormalsFormat { get; set; }

	private static bool isLinearColorSpace => QualitySettings.activeColorSpace == ColorSpace.Linear;

	private bool motionVectorsSupported { get; set; }

	private MaterialPropertyBlock materialPropertyBlock
	{
		get
		{
			if (m_MaterialPropertyBlock != null)
			{
				return m_MaterialPropertyBlock;
			}
			m_MaterialPropertyBlock = new MaterialPropertyBlock();
			return m_MaterialPropertyBlock;
		}
	}

	public HBAOPass(RenderPassEvent evt, Material material)
		: base(evt)
	{
		m_Material = material;
		FillSupportedRenderTextureFormats();
	}

	public void FillSupportedRenderTextureFormats()
	{
		colorFormat = RenderTextureFormat.Default;
		ssaoFormat = (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.R8) ? RenderTextureFormat.R8 : RenderTextureFormat.ARGB32);
		graphicsColorFormat = GraphicsFormat.R8G8B8A8_SRGB;
		graphicsDepthFormat = (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RFloat) ? GraphicsFormat.R32_SFloat : GraphicsFormat.R16_SFloat);
		graphicsNormalsFormat = GraphicsFormat.R8G8B8A8_UNorm;
		depthFormat = (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RFloat) ? RenderTextureFormat.RFloat : RenderTextureFormat.RHalf);
		normalsFormat = RenderTextureFormat.Default;
		motionVectorsSupported = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RGHalf);
	}

	public bool Setup(HBAO settings, HBAOFeature feature)
	{
		if (!settings.IsActive())
		{
			return false;
		}
		m_Settings = settings;
		m_Feature = feature;
		return true;
	}

	public override void RecordRenderGraph(ContextContainer frameData)
	{
		RenderGraph renderGraph = frameData.Get<WaaaghRenderingData>().RenderGraph;
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		FetchRenderParameters(waaaghCameraData.cameraTargetDescriptor);
		CheckParameters();
		UpdateMaterialPropertiesRG(waaaghCameraData);
		UpdateShaderKeywordsRG(waaaghCameraData);
		HBAOHistoryBuffer currentCameraHistoryBuffersRG = m_Feature.GetCurrentCameraHistoryBuffersRG(waaaghCameraData, m_Settings);
		currentCameraHistoryBuffersRG?.historyRTSystem.SwapAndSetReferenceSize(aoDesc.width, aoDesc.height);
		PassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = renderGraph.AddUnsafePass<PassData>(Name, out passData, m_ProfilingSampler, ".\\Library\\PackageCache\\com.owlcat.visual@f3d4bf622f68\\Runtime\\Waaagh\\RendererFeatures\\HBAO\\Passes\\HBAOPass.cs", 402);
		unsafeRenderGraphBuilder.AllowGlobalStateModification(value: true);
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		TextureHandle input = waaaghResourceData.CameraDepthBuffer;
		TextureHandle input2 = waaaghResourceData.CameraNormalsRT;
		TextureHandle input3 = waaaghResourceData.CameraMotionVectorsRT;
		TextureHandle cameraColorBuffer = waaaghResourceData.CameraColorBuffer;
		RenderTextureDescriptor cameraTargetDescriptor = waaaghCameraData.cameraTargetDescriptor;
		cameraTargetDescriptor.depthBufferBits = 0;
		cameraTargetDescriptor.msaaSamples = 1;
		bool num = m_Settings.perPixelNormals.value == HBAO.PerPixelNormals.Camera;
		bool value = m_Settings.temporalFilterEnabled.value;
		bool flag = m_Settings.mode.value == HBAO.Mode.LitAO;
		bool flag2 = m_Settings.blurType.value != HBAO.BlurType.None;
		bool value2 = m_Settings.temporalFilterEnabled.value;
		bool value3 = m_Settings.colorBleedingEnabled.value;
		bool flag3 = m_Settings.debugMode.value != HBAO.DebugMode.Disabled;
		passData.Material = m_Material;
		passData.TargetDescriptor = cameraTargetDescriptor;
		passData.AOTextureDescriptor = aoDesc;
		passData.CameraDepthTexture = input;
		passData.SourceTexture = cameraColorBuffer;
		passData.AOTexture = WaaaghRenderer.CreateRenderGraphTexture(renderGraph, aoDesc, "_HBAO_AOTexture0", clear: false, FilterMode.Bilinear);
		passData.TempTexture = (flag2 ? WaaaghRenderer.CreateRenderGraphTexture(renderGraph, aoDesc, "_HBAO_AOTexture1", clear: false, FilterMode.Bilinear) : TextureHandle.nullHandle);
		passData.DestinationTexture = ((flag && !flag3) ? WaaaghRenderer.CreateRenderGraphTexture(renderGraph, ssaoDesc, "_ScreenSpaceOcclusionTexture", clear: false, FilterMode.Bilinear) : WaaaghRenderer.CreateRenderGraphTexture(renderGraph, cameraTargetDescriptor, "_ScreenSpaceOcclusionTexture", clear: false, FilterMode.Bilinear));
		passData.HistoryBuffers = currentCameraHistoryBuffersRG;
		passData.TemporalFilterRenderTargets = m_RtsTemporalFilter;
		passData.FullscreenTriangle = m_Feature.fullscreenTriangle;
		passData.MaterialProperties = materialPropertyBlock;
		passData.UseLitAO = flag;
		passData.UseColorBleeding = value3;
		passData.UseBlur = flag2;
		passData.UseTemporalFilter = value2;
		passData.DirectLightingStrength = m_Settings.directLightingStrength.value;
		passData.ShowDebug = flag3;
		passData.ShowViewNormals = m_Settings.debugMode.value == HBAO.DebugMode.ViewNormals;
		passData.RenderingInSceneView = waaaghCameraData.camera.cameraType == CameraType.SceneView;
		unsafeRenderGraphBuilder.UseTexture(in input);
		if (num)
		{
			unsafeRenderGraphBuilder.UseTexture(in input2);
		}
		if (value)
		{
			unsafeRenderGraphBuilder.UseTexture(in input3);
			passData.MotionVectorTexture = input3;
		}
		TextureHandle input4 = passData.SourceTexture;
		unsafeRenderGraphBuilder.UseTexture(in input4, (flag && !flag3) ? AccessFlags.Read : AccessFlags.ReadWrite);
		input4 = passData.AOTexture;
		unsafeRenderGraphBuilder.UseTexture(in input4, AccessFlags.ReadWrite);
		if (passData.TempTexture.IsValid())
		{
			input4 = passData.TempTexture;
			unsafeRenderGraphBuilder.UseTexture(in input4, AccessFlags.ReadWrite);
		}
		input4 = passData.DestinationTexture;
		unsafeRenderGraphBuilder.UseTexture(in input4, (flag && !flag3) ? AccessFlags.Write : AccessFlags.ReadWrite);
		if (flag && !flag3)
		{
			input4 = passData.DestinationTexture;
			unsafeRenderGraphBuilder.SetGlobalTextureAfterPass(in input4, ShaderProperties.screenSpaceOcclusionTexture);
		}
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(PassData data, UnsafeGraphContext context)
		{
			ExecutePass(data, context);
		});
	}

	private static void ExecutePass(PassData data, UnsafeGraphContext rgContext)
	{
		CommandBuffer nativeCommandBuffer = CommandBufferHelpers.GetNativeCommandBuffer(rgContext.cmd);
		MaterialPropertyBlock materialProperties = data.MaterialProperties;
		materialProperties.SetTexture(ShaderProperties.cameraDepthTexture, data.CameraDepthTexture);
		if (!data.UseLitAO || data.ShowDebug)
		{
			BlitFullscreenTriangle(nativeCommandBuffer, data.SourceTexture, data.DestinationTexture, data.Material, data.FullscreenTriangle, 8, materialProperties);
		}
		materialProperties.SetVector(ShaderProperties.temporalParams, (data.HistoryBuffers != null) ? new Vector2(s_temporalRotations[data.HistoryBuffers.frameCount % 6] / 360f, s_temporalOffsets[data.HistoryBuffers.frameCount % 4]) : Vector2.zero);
		BlitFullscreenTriangleWithClear(nativeCommandBuffer, data.SourceTexture, data.AOTexture, data.Material, new Color(0f, 0f, 0f, 1f), data.FullscreenTriangle, 0, materialProperties);
		if (data.UseBlur)
		{
			float num = data.AOTextureDescriptor.width;
			float num2 = data.AOTextureDescriptor.height;
			if (data.TargetDescriptor.useDynamicScale)
			{
				num *= ScalableBufferManager.widthScaleFactor;
				num2 *= ScalableBufferManager.heightScaleFactor;
			}
			materialProperties.SetVector(ShaderProperties.blurDeltaUV, new Vector2(1f / num, 0f));
			BlitFullscreenTriangle(nativeCommandBuffer, data.AOTexture, data.TempTexture, data.Material, data.FullscreenTriangle, 6, materialProperties);
			materialProperties.SetVector(ShaderProperties.blurDeltaUV, new Vector2(0f, 1f / num2));
			BlitFullscreenTriangle(nativeCommandBuffer, data.TempTexture, data.AOTexture, data.Material, data.FullscreenTriangle, 6, materialProperties);
		}
		materialProperties.SetTexture(ShaderProperties.hbaoTex, data.AOTexture);
		if (data.UseTemporalFilter && !data.RenderingInSceneView && data.HistoryBuffers != null)
		{
			materialProperties.SetVector(ShaderProperties.historyBufferRTHandleScale, data.HistoryBuffers.historyRTSystem.rtHandleProperties.rtHandleScale);
			materialProperties.SetTexture(ShaderProperties._MotionVectorTexture, data.MotionVectorTexture);
			if (data.HistoryBuffers.frameCount == 0)
			{
				RenderTargetIdentifier rt = new RenderTargetIdentifier(data.HistoryBuffers.historyRTSystem.GetFrameRT(0, 1), 0, CubemapFace.Unknown, -1);
				nativeCommandBuffer.SetRenderTarget(rt, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
				nativeCommandBuffer.ClearRenderTarget(clearDepth: false, clearColor: true, Color.white);
				if (data.UseColorBleeding)
				{
					RenderTargetIdentifier rt2 = new RenderTargetIdentifier(data.HistoryBuffers.historyRTSystem.GetFrameRT(1, 1), 0, CubemapFace.Unknown, -1);
					nativeCommandBuffer.SetRenderTarget(rt2, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
					nativeCommandBuffer.ClearRenderTarget(clearDepth: false, clearColor: true, new Color(0f, 0f, 0f, 1f));
				}
			}
			Rect viewportRect = new Rect(Vector2.zero, data.HistoryBuffers.historyRTSystem.rtHandleProperties.currentViewportSize);
			if (data.UseColorBleeding)
			{
				RTHandle frameRT = data.HistoryBuffers.historyRTSystem.GetFrameRT(0, 0);
				RTHandle frameRT2 = data.HistoryBuffers.historyRTSystem.GetFrameRT(1, 0);
				RTHandle frameRT3 = data.HistoryBuffers.historyRTSystem.GetFrameRT(0, 1);
				RTHandle frameRT4 = data.HistoryBuffers.historyRTSystem.GetFrameRT(1, 1);
				data.TemporalFilterRenderTargets[0] = frameRT;
				data.TemporalFilterRenderTargets[1] = frameRT2;
				materialProperties.SetTexture(ShaderProperties.tempTex, frameRT4);
				BlitFullscreenTriangle(nativeCommandBuffer, frameRT3, data.TemporalFilterRenderTargets, viewportRect, data.Material, data.FullscreenTriangle, 7, materialProperties);
				materialProperties.SetTexture(ShaderProperties.hbaoTex, frameRT2);
			}
			else
			{
				RTHandle frameRT5 = data.HistoryBuffers.historyRTSystem.GetFrameRT(0, 0);
				RTHandle frameRT6 = data.HistoryBuffers.historyRTSystem.GetFrameRT(0, 1);
				BlitFullscreenTriangle(nativeCommandBuffer, frameRT6, frameRT5, viewportRect, data.Material, data.FullscreenTriangle, 7, materialProperties);
				materialProperties.SetTexture(ShaderProperties.hbaoTex, frameRT5);
			}
			data.HistoryBuffers.frameCount++;
			data.HistoryBuffers.lastRenderedFrame = Time.frameCount;
		}
		else
		{
			materialProperties.SetVector(ShaderProperties.historyBufferRTHandleScale, Vector4.one);
		}
		if (data.UseLitAO && !data.ShowDebug)
		{
			BlitFullscreenTriangle(nativeCommandBuffer, data.SourceTexture, data.DestinationTexture, data.Material, data.FullscreenTriangle, 9, materialProperties);
			nativeCommandBuffer.SetKeyword(in ShaderProperties.screenSpaceOcclusionKeyword, value: true);
			nativeCommandBuffer.SetGlobalVector(ShaderProperties.screenSpaceOcclusionParam, new Vector4(1f, 0f, 0f, data.DirectLightingStrength));
		}
		else
		{
			BlitFullscreenTriangle(nativeCommandBuffer, data.DestinationTexture, data.SourceTexture, data.Material, data.FullscreenTriangle, data.ShowViewNormals ? 10 : 9, materialProperties);
		}
	}

	private static void BlitFullscreenTriangle(CommandBuffer cmd, RenderTexture source, RenderTargetIdentifier destination, Material material, Mesh fullscreenTriangle, int passIndex, MaterialPropertyBlock properties)
	{
		properties.SetTexture(ShaderProperties.mainTex, source);
		cmd.SetRenderTarget(new RenderTargetIdentifier(destination, 0, CubemapFace.Unknown, -1), RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
		cmd.DrawMesh(fullscreenTriangle, Matrix4x4.identity, material, 0, passIndex, properties);
	}

	private static void BlitFullscreenTriangleWithClear(CommandBuffer cmd, RenderTexture source, RenderTargetIdentifier destination, Material material, Color clearColor, Mesh fullscreenTriangle, int passIndex, MaterialPropertyBlock properties)
	{
		properties.SetTexture(ShaderProperties.mainTex, source);
		cmd.SetRenderTarget(new RenderTargetIdentifier(destination, 0, CubemapFace.Unknown, -1), RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
		cmd.ClearRenderTarget(clearDepth: false, clearColor: true, clearColor);
		cmd.DrawMesh(fullscreenTriangle, Matrix4x4.identity, material, 0, passIndex, properties);
	}

	private static void BlitFullscreenTriangle(CommandBuffer cmd, RenderTexture source, RenderTargetIdentifier[] destinations, Rect viewportRect, Material material, Mesh fullscreenTriangle, int passIndex, MaterialPropertyBlock properties)
	{
		properties.SetTexture(ShaderProperties.mainTex, source);
		cmd.SetRenderTarget(destinations, destinations[0], 0, CubemapFace.Unknown, -1);
		cmd.SetViewport(viewportRect);
		cmd.DrawMesh(fullscreenTriangle, Matrix4x4.identity, material, 0, passIndex, properties);
	}

	private static void BlitFullscreenTriangle(CommandBuffer cmd, RenderTexture source, RenderTargetIdentifier destination, Rect viewportRect, Material material, Mesh fullscreenTriangle, int passIndex, MaterialPropertyBlock properties)
	{
		properties.SetTexture(ShaderProperties.mainTex, source);
		cmd.SetRenderTarget(new RenderTargetIdentifier(destination, 0, CubemapFace.Unknown, -1), RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
		cmd.SetViewport(viewportRect);
		cmd.DrawMesh(fullscreenTriangle, Matrix4x4.identity, material, 0, passIndex, properties);
	}

	private void FetchRenderParameters(RenderTextureDescriptor cameraTextureDesc)
	{
		cameraTextureDesc.msaaSamples = 1;
		cameraTextureDesc.depthBufferBits = 0;
		sourceDesc = cameraTextureDesc;
		int num = cameraTextureDesc.width;
		int num2 = cameraTextureDesc.height;
		int num3 = ((m_Settings.resolution.value == HBAO.Resolution.Full) ? 1 : ((m_Settings.deinterleaving.value != 0) ? 1 : 2));
		if (num3 > 1)
		{
			num = (num + num % 2) / num3;
			num2 = (num2 + num2 % 2) / num3;
		}
		aoDesc = GetStereoCompatibleDescriptor(num, num2, colorFormat, 0, RenderTextureReadWrite.Linear);
		ssaoDesc = GetStereoCompatibleDescriptor(num, num2, ssaoFormat, 0, RenderTextureReadWrite.Linear);
		if (m_Settings.deinterleaving.value != 0)
		{
			int num4 = cameraTextureDesc.width + ((cameraTextureDesc.width % 4 != 0) ? (4 - cameraTextureDesc.width % 4) : 0);
			int num5 = cameraTextureDesc.height + ((cameraTextureDesc.height % 4 != 0) ? (4 - cameraTextureDesc.height % 4) : 0);
			int width = num4 / 4;
			int height = num5 / 4;
			deinterleavedDepthDesc = GetStereoCompatibleDescriptor(width, height, depthFormat, 0, RenderTextureReadWrite.Linear);
			deinterleavedNormalsDesc = GetStereoCompatibleDescriptor(width, height, normalsFormat, 0, RenderTextureReadWrite.Linear);
			deinterleavedAoDesc = GetStereoCompatibleDescriptor(width, height, colorFormat, 0, RenderTextureReadWrite.Linear);
			reinterleavedAoDesc = GetStereoCompatibleDescriptor(num4, num5, colorFormat, 0, RenderTextureReadWrite.Linear);
		}
	}

	private RenderTextureDescriptor GetStereoCompatibleDescriptor(int width, int height, RenderTextureFormat format = RenderTextureFormat.Default, int depthBufferBits = 0, RenderTextureReadWrite readWrite = RenderTextureReadWrite.Default)
	{
		RenderTextureDescriptor result = sourceDesc;
		result.depthBufferBits = depthBufferBits;
		result.msaaSamples = 1;
		result.width = width;
		result.height = height;
		result.colorFormat = format;
		switch (readWrite)
		{
		case RenderTextureReadWrite.sRGB:
			result.sRGB = true;
			break;
		case RenderTextureReadWrite.Linear:
			result.sRGB = false;
			break;
		case RenderTextureReadWrite.Default:
			result.sRGB = isLinearColorSpace;
			break;
		}
		return result;
	}

	private void CheckParameters()
	{
		if (m_Settings.deinterleaving.value != 0 && SystemInfo.supportedRenderTargetCount < 4)
		{
			m_Settings.SetDeinterleaving(HBAO.Deinterleaving.Disabled);
		}
		if (m_Settings.temporalFilterEnabled.value && !motionVectorsSupported)
		{
			m_Settings.EnableTemporalFilter(enabled: false);
		}
		if (m_Settings.colorBleedingEnabled.value && m_Settings.temporalFilterEnabled.value && SystemInfo.supportedRenderTargetCount < 2)
		{
			m_Settings.EnableTemporalFilter(enabled: false);
		}
		if (m_Settings.colorBleedingEnabled.value && m_Settings.mode.value == HBAO.Mode.LitAO)
		{
			m_Settings.EnableColorBleeding(enabled: false);
		}
	}

	private void UpdateMaterialPropertiesRG(WaaaghCameraData cameraData)
	{
		int width = cameraData.cameraTargetDescriptor.width;
		int height = cameraData.cameraTargetDescriptor.height;
		int num = 1;
		for (int i = 0; i < num; i++)
		{
			Matrix4x4 projectionMatrix = cameraData.GetProjectionMatrix(i);
			float m = projectionMatrix.m00;
			float m2 = projectionMatrix.m11;
			m_UVToViewPerEye[i] = new Vector4(2f / m, -2f / m2, -1f / m, 1f / m2);
			m_RadiusPerEye[i] = m_Settings.radius.value * 0.5f * ((float)(height / ((m_Settings.deinterleaving.value != HBAO.Deinterleaving.x4) ? 1 : 4)) / (2f / m2));
		}
		float num2 = Mathf.Max(16f, m_Settings.maxRadiusPixels.value * Mathf.Sqrt((float)(width * height) / 2073600f));
		num2 /= (float)((m_Settings.deinterleaving.value != HBAO.Deinterleaving.x4) ? 1 : 4);
		Vector4 value = ((m_Settings.deinterleaving.value == HBAO.Deinterleaving.x4) ? new Vector4((float)reinterleavedAoDesc.width / (float)width, (float)reinterleavedAoDesc.height / (float)height, 1f / ((float)reinterleavedAoDesc.width / (float)width), 1f / ((float)reinterleavedAoDesc.height / (float)height)) : ((m_Settings.resolution.value == HBAO.Resolution.Half) ? new Vector4(((float)width + 0.5f) / (float)width, ((float)height + 0.5f) / (float)height, 1f, 1f) : Vector4.one));
		m_Material.SetTexture(ShaderProperties.noiseTex, m_Feature.NoiseTexture);
		m_Material.SetVector(ShaderProperties.inputTexelSize, new Vector4(1f / (float)width, 1f / (float)height, width, height));
		if (sourceDesc.useDynamicScale)
		{
			m_Material.SetVector(ShaderProperties.aoTexelSize, new Vector4(1f / ((float)aoDesc.width * ScalableBufferManager.widthScaleFactor), 1f / ((float)aoDesc.height * ScalableBufferManager.heightScaleFactor), (float)aoDesc.width * ScalableBufferManager.widthScaleFactor, (float)aoDesc.height * ScalableBufferManager.heightScaleFactor));
		}
		else
		{
			m_Material.SetVector(ShaderProperties.aoTexelSize, new Vector4(1f / (float)aoDesc.width, 1f / (float)aoDesc.height, aoDesc.width, aoDesc.height));
		}
		m_Material.SetVector(ShaderProperties.deinterleavedAOTexelSize, new Vector4(1f / (float)deinterleavedAoDesc.width, 1f / (float)deinterleavedAoDesc.height, deinterleavedAoDesc.width, deinterleavedAoDesc.height));
		m_Material.SetVector(ShaderProperties.reinterleavedAOTexelSize, new Vector4(1f / (float)reinterleavedAoDesc.width, 1f / (float)reinterleavedAoDesc.height, reinterleavedAoDesc.width, reinterleavedAoDesc.height));
		m_Material.SetVector(ShaderProperties.targetScale, value);
		m_Material.SetVectorArray(ShaderProperties.uvToView, m_UVToViewPerEye);
		m_Material.SetFloatArray(ShaderProperties.radius, m_RadiusPerEye);
		m_Material.SetFloat(ShaderProperties.maxRadiusPixels, num2);
		m_Material.SetFloat(ShaderProperties.negInvRadius2, -1f / (m_Settings.radius.value * m_Settings.radius.value));
		m_Material.SetFloat(ShaderProperties.angleBias, m_Settings.bias.value);
		m_Material.SetFloat(ShaderProperties.aoMultiplier, 2f * (1f / (1f - m_Settings.bias.value)));
		m_Material.SetFloat(ShaderProperties.intensity, isLinearColorSpace ? m_Settings.intensity.value : (m_Settings.intensity.value * 0.45454547f));
		m_Material.SetFloat(ShaderProperties.multiBounceInfluence, m_Settings.multiBounceInfluence.value);
		m_Material.SetFloat(ShaderProperties.offscreenSamplesContrib, m_Settings.offscreenSamplesContribution.value);
		m_Material.SetFloat(ShaderProperties.maxDistance, m_Settings.maxDistance.value);
		m_Material.SetFloat(ShaderProperties.distanceFalloff, m_Settings.distanceFalloff.value);
		m_Material.SetColor(ShaderProperties.baseColor, m_Settings.baseColor.value);
		m_Material.SetFloat(ShaderProperties.blurSharpness, m_Settings.sharpness.value);
		m_Material.SetFloat(ShaderProperties.colorBleedSaturation, m_Settings.saturation.value);
		m_Material.SetFloat(ShaderProperties.colorBleedBrightnessMask, m_Settings.brightnessMask.value);
		m_Material.SetVector(ShaderProperties.colorBleedBrightnessMaskRange, AdjustBrightnessMaskToGammaSpace(new Vector2(Mathf.Pow(m_Settings.brightnessMaskRange.value.x, 3f), Mathf.Pow(m_Settings.brightnessMaskRange.value.y, 3f))));
	}

	private Vector2 AdjustBrightnessMaskToGammaSpace(Vector2 v)
	{
		if (!isLinearColorSpace)
		{
			return ToGammaSpace(v);
		}
		return v;
	}

	private float ToGammaSpace(float v)
	{
		return Mathf.Pow(v, 0.45454547f);
	}

	private Vector2 ToGammaSpace(Vector2 v)
	{
		return new Vector2(ToGammaSpace(v.x), ToGammaSpace(v.y));
	}

	private void UpdateShaderKeywordsRG(WaaaghCameraData cameraData)
	{
		if (m_ShaderKeywords == null || m_ShaderKeywords.Length != 12)
		{
			m_ShaderKeywords = new string[12];
		}
		m_ShaderKeywords[0] = ShaderProperties.GetOrthographicProjectionKeyword(cameraData.camera.orthographic);
		m_ShaderKeywords[1] = ShaderProperties.GetQualityKeyword(m_Settings.quality.value);
		m_ShaderKeywords[2] = ShaderProperties.GetNoiseKeyword(m_Settings.noiseType.value);
		m_ShaderKeywords[3] = ShaderProperties.GetDeinterleavingKeyword(m_Settings.deinterleaving.value);
		m_ShaderKeywords[4] = ShaderProperties.GetDebugKeyword(m_Settings.debugMode.value);
		m_ShaderKeywords[5] = ShaderProperties.GetMultibounceKeyword(m_Settings.useMultiBounce.value, m_Settings.mode.value == HBAO.Mode.LitAO);
		m_ShaderKeywords[6] = ShaderProperties.GetOffscreenSamplesContributionKeyword(m_Settings.offscreenSamplesContribution.value);
		m_ShaderKeywords[7] = ShaderProperties.GetPerPixelNormalsKeyword(m_Settings.perPixelNormals.value);
		m_ShaderKeywords[8] = ShaderProperties.GetBlurRadiusKeyword(m_Settings.blurType.value);
		m_ShaderKeywords[9] = ShaderProperties.GetVarianceClippingKeyword(m_Settings.varianceClipping.value);
		m_ShaderKeywords[10] = ShaderProperties.GetColorBleedingKeyword(m_Settings.colorBleedingEnabled.value, m_Settings.mode.value == HBAO.Mode.LitAO);
		m_ShaderKeywords[11] = ShaderProperties.GetModeKeyword(m_Settings.mode.value, m_Settings.debugMode.value);
		m_Material.shaderKeywords = m_ShaderKeywords;
	}
}
