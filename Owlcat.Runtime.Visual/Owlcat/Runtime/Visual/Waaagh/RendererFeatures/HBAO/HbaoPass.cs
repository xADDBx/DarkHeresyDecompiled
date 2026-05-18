using Owlcat.Runtime.Visual.Waaagh.FrameData;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.HBAO;

internal static class HbaoPass
{
	private sealed class PassData
	{
		public Material Material;

		public RenderTextureDescriptor TargetDescriptor;

		public RenderTextureDescriptor AOTextureDescriptor;

		public TextureHandle CameraDepthTexture;

		public TextureHandle CameraNormalsTexture;

		public TextureHandle SourceTexture;

		public TextureHandle AOTexture;

		public TextureHandle TempTexture;

		public TextureHandle DestinationTexture;

		public TextureHandle MotionVectorTexture;

		public HBAOHistoryBuffer HistoryBuffers;

		public Mesh FullscreenTriangle;

		public MaterialPropertyBlock MaterialProperties = new MaterialPropertyBlock();

		public bool UseLitAO;

		public bool UseColorBleeding;

		public bool UseBlur;

		public bool UseTemporalFilter;

		public float DirectLightingStrength;

		public bool ShowDebug;

		public bool ShowViewNormals;

		public bool RenderingInSceneView;
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

		public static int _CameraNormalsTexture;

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
			_CameraNormalsTexture = Shader.PropertyToID("_CameraNormalsTexture");
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

	private struct RenderTextureDescriptors
	{
		public RenderTextureDescriptor sourceDesc;

		public RenderTextureDescriptor aoDesc;

		public RenderTextureDescriptor ssaoDesc;

		public RenderTextureDescriptor deinterleavedDepthDesc;

		public RenderTextureDescriptor deinterleavedNormalsDesc;

		public RenderTextureDescriptor deinterleavedAoDesc;

		public RenderTextureDescriptor reinterleavedAoDesc;
	}

	public struct Formats
	{
		public RenderTextureFormat colorFormat;

		public RenderTextureFormat ssaoFormat;

		public RenderTextureFormat depthFormat;

		public RenderTextureFormat normalsFormat;

		public GraphicsFormat graphicsColorFormat;

		public GraphicsFormat graphicsDepthFormat;

		public GraphicsFormat graphicsNormalsFormat;

		public bool motionVectorsSupported;
	}

	public struct Resources
	{
		public Texture2D NoiseTexture;

		public Mesh FullscreenTriangle;
	}

	private static readonly float[] s_temporalRotations = new float[6] { 60f, 300f, 180f, 240f, 120f, 0f };

	private static readonly float[] s_temporalOffsets = new float[4] { 0f, 0.5f, 0.25f, 0.75f };

	private static readonly Vector4[] s_UVToViewPerEye = new Vector4[2];

	private static readonly float[] s_RadiusPerEye = new float[2];

	private static readonly string[] s_ShaderKeywords = new string[12];

	public static void Record(in RecordContext context, HBAO settings, in Resources resources, in Formats formats, Material material, HBAOHistoryBuffer historyBuffers)
	{
		RenderTextureDescriptors descriptors = FetchRenderParameters(settings, in formats, context.CameraData);
		CheckParameters(settings, in formats);
		UpdateMaterialPropertiesRG(settings, in resources, in descriptors, context.CameraData, material);
		UpdateShaderKeywordsRG(settings, context.CameraData, material);
		historyBuffers?.historyRTSystem.SwapAndSetReferenceSize(descriptors.aoDesc.width, descriptors.aoDesc.height);
		PassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<PassData>("HBAO", out passData, WaaaghProfileId.HBAO.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\RendererFeatures\\HBAO\\HbaoPass.cs", 33);
		unsafeRenderGraphBuilder.AllowGlobalStateModification(value: true);
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		TextureHandle input = context.FrameResources.CameraStackTargets.Depth;
		TextureHandle input2 = context.FrameResources.GBuffer.Normals;
		TextureHandle input3 = context.FrameResources.CameraAdditionalTargets.MotionVectors;
		TextureHandle color = context.FrameResources.CameraStackTargets.Color;
		RenderTextureDescriptor cameraTargetDescriptor = context.CameraData.cameraTargetDescriptor;
		cameraTargetDescriptor.depthBufferBits = 0;
		cameraTargetDescriptor.msaaSamples = 1;
		bool num = settings.perPixelNormals.value == HBAO.PerPixelNormals.Camera;
		bool value = settings.temporalFilterEnabled.value;
		bool flag = settings.mode.value == HBAO.Mode.LitAO;
		bool flag2 = settings.blurType.value != HBAO.BlurType.None;
		bool value2 = settings.temporalFilterEnabled.value;
		bool value3 = settings.colorBleedingEnabled.value;
		bool flag3 = settings.debugMode.value != HBAO.DebugMode.Disabled;
		passData.Material = material;
		passData.TargetDescriptor = cameraTargetDescriptor;
		passData.AOTextureDescriptor = descriptors.aoDesc;
		passData.CameraDepthTexture = input;
		passData.SourceTexture = color;
		passData.AOTexture = RenderGraphUtility.CreateRenderGraphTexture(context.RenderGraph, descriptors.aoDesc, "_HBAO_AOTexture0", clear: false, FilterMode.Bilinear);
		passData.TempTexture = (flag2 ? RenderGraphUtility.CreateRenderGraphTexture(context.RenderGraph, descriptors.aoDesc, "_HBAO_AOTexture1", clear: false, FilterMode.Bilinear) : TextureHandle.nullHandle);
		passData.DestinationTexture = ((flag && !flag3) ? RenderGraphUtility.CreateRenderGraphTexture(context.RenderGraph, descriptors.ssaoDesc, "ScreenSpaceOcclusion", clear: false, FilterMode.Bilinear) : RenderGraphUtility.CreateRenderGraphTexture(context.RenderGraph, cameraTargetDescriptor, "ScreenSpaceOcclusion", clear: false, FilterMode.Bilinear));
		passData.HistoryBuffers = historyBuffers;
		passData.FullscreenTriangle = resources.FullscreenTriangle;
		passData.UseLitAO = flag;
		passData.UseColorBleeding = value3;
		passData.UseBlur = flag2;
		passData.UseTemporalFilter = value2;
		passData.DirectLightingStrength = settings.directLightingStrength.value;
		passData.ShowDebug = flag3;
		passData.ShowViewNormals = settings.debugMode.value == HBAO.DebugMode.ViewNormals;
		passData.RenderingInSceneView = context.CameraData.camera.cameraType == CameraType.SceneView;
		unsafeRenderGraphBuilder.UseTexture(in input);
		if (num)
		{
			unsafeRenderGraphBuilder.UseTexture(in input2);
			passData.CameraNormalsTexture = input2;
		}
		if (value)
		{
			unsafeRenderGraphBuilder.UseTexture(in input3);
			passData.MotionVectorTexture = input3;
		}
		unsafeRenderGraphBuilder.UseTexture(in passData.SourceTexture, (flag && !flag3) ? AccessFlags.Read : AccessFlags.ReadWrite);
		unsafeRenderGraphBuilder.UseTexture(in passData.AOTexture, AccessFlags.ReadWrite);
		if (passData.TempTexture.IsValid())
		{
			unsafeRenderGraphBuilder.UseTexture(in passData.TempTexture, AccessFlags.ReadWrite);
		}
		unsafeRenderGraphBuilder.UseTexture(in passData.DestinationTexture, (flag && !flag3) ? AccessFlags.Write : AccessFlags.ReadWrite);
		if (flag && !flag3)
		{
			unsafeRenderGraphBuilder.SetGlobalTextureAfterPass(in passData.DestinationTexture, GlobalTextureShaderPropertyId._ScreenSpaceOcclusionTexture);
		}
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(PassData data, UnsafeGraphContext context)
		{
			Render(data, context);
		});
	}

	private static void Render(PassData data, UnsafeGraphContext rgContext)
	{
		CommandBuffer nativeCommandBuffer = CommandBufferHelpers.GetNativeCommandBuffer(rgContext.cmd);
		MaterialPropertyBlock materialProperties = data.MaterialProperties;
		materialProperties.SetTexture(ShaderProperties.cameraDepthTexture, data.CameraDepthTexture);
		materialProperties.SetTexture(ShaderProperties._CameraNormalsTexture, data.CameraNormalsTexture);
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
			Rect viewportRect = new Rect(Vector2.zero, (Vector2)data.HistoryBuffers.historyRTSystem.rtHandleProperties.currentViewportSize);
			if (data.UseColorBleeding)
			{
				RTHandle frameRT = data.HistoryBuffers.historyRTSystem.GetFrameRT(0, 0);
				RTHandle frameRT2 = data.HistoryBuffers.historyRTSystem.GetFrameRT(1, 0);
				RTHandle frameRT3 = data.HistoryBuffers.historyRTSystem.GetFrameRT(0, 1);
				RTHandle frameRT4 = data.HistoryBuffers.historyRTSystem.GetFrameRT(1, 1);
				RenderTargetIdentifier[] tempArray = rgContext.renderGraphPool.GetTempArray<RenderTargetIdentifier>(2);
				tempArray[0] = frameRT;
				tempArray[1] = frameRT2;
				materialProperties.SetTexture(ShaderProperties.tempTex, frameRT4);
				BlitFullscreenTriangle(nativeCommandBuffer, frameRT3, tempArray, viewportRect, data.Material, data.FullscreenTriangle, 7, materialProperties);
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

	private static RenderTextureDescriptors FetchRenderParameters(HBAO settings, in Formats formats, WaaaghCameraData cameraData)
	{
		RenderTextureDescriptors result = default(RenderTextureDescriptors);
		RenderTextureDescriptor cameraTargetDescriptor = cameraData.cameraTargetDescriptor;
		cameraTargetDescriptor.msaaSamples = 1;
		cameraTargetDescriptor.depthBufferBits = 0;
		result.sourceDesc = cameraTargetDescriptor;
		int num = cameraTargetDescriptor.width;
		int num2 = cameraTargetDescriptor.height;
		int num3 = ((settings.resolution.value == HBAO.Resolution.Full) ? 1 : ((settings.deinterleaving.value != 0) ? 1 : 2));
		if (num3 > 1)
		{
			num = (num + num % 2) / num3;
			num2 = (num2 + num2 % 2) / num3;
		}
		result.aoDesc = GetStereoCompatibleDescriptor(in result.sourceDesc, num, num2, formats.colorFormat);
		result.ssaoDesc = GetStereoCompatibleDescriptor(in result.sourceDesc, num, num2, formats.ssaoFormat);
		if (settings.deinterleaving.value != 0)
		{
			int num4 = cameraTargetDescriptor.width + ((cameraTargetDescriptor.width % 4 != 0) ? (4 - cameraTargetDescriptor.width % 4) : 0);
			int num5 = cameraTargetDescriptor.height + ((cameraTargetDescriptor.height % 4 != 0) ? (4 - cameraTargetDescriptor.height % 4) : 0);
			int width = num4 / 4;
			int height = num5 / 4;
			result.deinterleavedDepthDesc = GetStereoCompatibleDescriptor(in result.sourceDesc, width, height, formats.depthFormat);
			result.deinterleavedNormalsDesc = GetStereoCompatibleDescriptor(in result.sourceDesc, width, height, formats.normalsFormat);
			result.deinterleavedAoDesc = GetStereoCompatibleDescriptor(in result.sourceDesc, width, height, formats.colorFormat);
			result.reinterleavedAoDesc = GetStereoCompatibleDescriptor(in result.sourceDesc, num4, num5, formats.colorFormat);
		}
		return result;
	}

	private static RenderTextureDescriptor GetStereoCompatibleDescriptor(in RenderTextureDescriptor sourceDesc, int width, int height, RenderTextureFormat format)
	{
		RenderTextureDescriptor result = sourceDesc;
		result.depthBufferBits = 0;
		result.msaaSamples = 1;
		result.width = width;
		result.height = height;
		result.colorFormat = format;
		result.sRGB = false;
		return result;
	}

	public static Formats GetSupportedRenderTextureFormats()
	{
		Formats result = default(Formats);
		result.colorFormat = RenderTextureFormat.Default;
		result.ssaoFormat = (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.R8) ? RenderTextureFormat.R8 : RenderTextureFormat.ARGB32);
		result.graphicsColorFormat = GraphicsFormat.R8G8B8A8_SRGB;
		result.graphicsDepthFormat = (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RFloat) ? GraphicsFormat.R32_SFloat : GraphicsFormat.R16_SFloat);
		result.graphicsNormalsFormat = GraphicsFormat.R8G8B8A8_UNorm;
		result.depthFormat = (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RFloat) ? RenderTextureFormat.RFloat : RenderTextureFormat.RHalf);
		result.normalsFormat = RenderTextureFormat.Default;
		result.motionVectorsSupported = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RGHalf);
		return result;
	}

	private static void CheckParameters(HBAO settings, in Formats formats)
	{
		if (settings.deinterleaving.value != 0 && SystemInfo.supportedRenderTargetCount < 4)
		{
			settings.SetDeinterleaving(HBAO.Deinterleaving.Disabled);
		}
		if (settings.temporalFilterEnabled.value && !formats.motionVectorsSupported)
		{
			settings.EnableTemporalFilter(enabled: false);
		}
		if (settings.colorBleedingEnabled.value && settings.temporalFilterEnabled.value && SystemInfo.supportedRenderTargetCount < 2)
		{
			settings.EnableTemporalFilter(enabled: false);
		}
		if (settings.colorBleedingEnabled.value && settings.mode.value == HBAO.Mode.LitAO)
		{
			settings.EnableColorBleeding(enabled: false);
		}
	}

	private static void UpdateMaterialPropertiesRG(HBAO settings, in Resources resources, in RenderTextureDescriptors descriptors, WaaaghCameraData cameraData, Material material)
	{
		int width = cameraData.cameraTargetDescriptor.width;
		int height = cameraData.cameraTargetDescriptor.height;
		int num = 1;
		for (int i = 0; i < num; i++)
		{
			Matrix4x4 projectionMatrix = cameraData.GetProjectionMatrix(i);
			float m = projectionMatrix.m00;
			float m2 = projectionMatrix.m11;
			s_UVToViewPerEye[i] = new Vector4(2f / m, -2f / m2, -1f / m, 1f / m2);
			s_RadiusPerEye[i] = settings.radius.value * 0.5f * ((float)(height / ((settings.deinterleaving.value != HBAO.Deinterleaving.x4) ? 1 : 4)) / (2f / m2));
		}
		float num2 = Mathf.Max(16f, settings.maxRadiusPixels.value * Mathf.Sqrt((float)(width * height) / 2073600f));
		num2 /= (float)((settings.deinterleaving.value != HBAO.Deinterleaving.x4) ? 1 : 4);
		Vector4 value = ((settings.deinterleaving.value == HBAO.Deinterleaving.x4) ? new Vector4((float)descriptors.reinterleavedAoDesc.width / (float)width, (float)descriptors.reinterleavedAoDesc.height / (float)height, 1f / ((float)descriptors.reinterleavedAoDesc.width / (float)width), 1f / ((float)descriptors.reinterleavedAoDesc.height / (float)height)) : ((settings.resolution.value == HBAO.Resolution.Half) ? new Vector4(((float)width + 0.5f) / (float)width, ((float)height + 0.5f) / (float)height, 1f, 1f) : Vector4.one));
		bool flag = QualitySettings.activeColorSpace == ColorSpace.Linear;
		material.SetTexture(ShaderProperties.noiseTex, resources.NoiseTexture);
		material.SetVector(ShaderProperties.inputTexelSize, new Vector4(1f / (float)width, 1f / (float)height, width, height));
		if (descriptors.sourceDesc.useDynamicScale)
		{
			material.SetVector(ShaderProperties.aoTexelSize, new Vector4(1f / ((float)descriptors.aoDesc.width * ScalableBufferManager.widthScaleFactor), 1f / ((float)descriptors.aoDesc.height * ScalableBufferManager.heightScaleFactor), (float)descriptors.aoDesc.width * ScalableBufferManager.widthScaleFactor, (float)descriptors.aoDesc.height * ScalableBufferManager.heightScaleFactor));
		}
		else
		{
			material.SetVector(ShaderProperties.aoTexelSize, new Vector4(1f / (float)descriptors.aoDesc.width, 1f / (float)descriptors.aoDesc.height, descriptors.aoDesc.width, descriptors.aoDesc.height));
		}
		material.SetVector(ShaderProperties.deinterleavedAOTexelSize, new Vector4(1f / (float)descriptors.deinterleavedAoDesc.width, 1f / (float)descriptors.deinterleavedAoDesc.height, descriptors.deinterleavedAoDesc.width, descriptors.deinterleavedAoDesc.height));
		material.SetVector(ShaderProperties.reinterleavedAOTexelSize, new Vector4(1f / (float)descriptors.reinterleavedAoDesc.width, 1f / (float)descriptors.reinterleavedAoDesc.height, descriptors.reinterleavedAoDesc.width, descriptors.reinterleavedAoDesc.height));
		material.SetVector(ShaderProperties.targetScale, value);
		material.SetVectorArray(ShaderProperties.uvToView, s_UVToViewPerEye);
		material.SetFloatArray(ShaderProperties.radius, s_RadiusPerEye);
		material.SetFloat(ShaderProperties.maxRadiusPixels, num2);
		material.SetFloat(ShaderProperties.negInvRadius2, -1f / (settings.radius.value * settings.radius.value));
		material.SetFloat(ShaderProperties.angleBias, settings.bias.value);
		material.SetFloat(ShaderProperties.aoMultiplier, 2f * (1f / (1f - settings.bias.value)));
		material.SetFloat(ShaderProperties.intensity, flag ? settings.intensity.value : (settings.intensity.value * 0.45454547f));
		material.SetFloat(ShaderProperties.multiBounceInfluence, settings.multiBounceInfluence.value);
		material.SetFloat(ShaderProperties.offscreenSamplesContrib, settings.offscreenSamplesContribution.value);
		material.SetFloat(ShaderProperties.maxDistance, settings.maxDistance.value);
		material.SetFloat(ShaderProperties.distanceFalloff, settings.distanceFalloff.value);
		material.SetColor(ShaderProperties.baseColor, settings.baseColor.value);
		material.SetFloat(ShaderProperties.blurSharpness, settings.sharpness.value);
		material.SetFloat(ShaderProperties.colorBleedSaturation, settings.saturation.value);
		material.SetFloat(ShaderProperties.colorBleedBrightnessMask, settings.brightnessMask.value);
		material.SetVector(ShaderProperties.colorBleedBrightnessMaskRange, AdjustBrightnessMaskToGammaSpace(new Vector2(Mathf.Pow(settings.brightnessMaskRange.value.x, 3f), Mathf.Pow(settings.brightnessMaskRange.value.y, 3f)), flag));
	}

	private static Vector2 AdjustBrightnessMaskToGammaSpace(Vector2 v, bool isLinearColorSpace)
	{
		if (!isLinearColorSpace)
		{
			return ToGammaSpace(v);
		}
		return v;
	}

	private static Vector2 ToGammaSpace(Vector2 v)
	{
		return new Vector2(ToGammaSpace(v.x), ToGammaSpace(v.y));
	}

	private static float ToGammaSpace(float v)
	{
		return Mathf.Pow(v, 0.45454547f);
	}

	private static void UpdateShaderKeywordsRG(HBAO settings, WaaaghCameraData cameraData, Material material)
	{
		s_ShaderKeywords[0] = ShaderProperties.GetOrthographicProjectionKeyword(cameraData.camera.orthographic);
		s_ShaderKeywords[1] = ShaderProperties.GetQualityKeyword(settings.quality.value);
		s_ShaderKeywords[2] = ShaderProperties.GetNoiseKeyword(settings.noiseType.value);
		s_ShaderKeywords[3] = ShaderProperties.GetDeinterleavingKeyword(settings.deinterleaving.value);
		s_ShaderKeywords[4] = ShaderProperties.GetDebugKeyword(settings.debugMode.value);
		s_ShaderKeywords[5] = ShaderProperties.GetMultibounceKeyword(settings.useMultiBounce.value, settings.mode.value == HBAO.Mode.LitAO);
		s_ShaderKeywords[6] = ShaderProperties.GetOffscreenSamplesContributionKeyword(settings.offscreenSamplesContribution.value);
		s_ShaderKeywords[7] = ShaderProperties.GetPerPixelNormalsKeyword(settings.perPixelNormals.value);
		s_ShaderKeywords[8] = ShaderProperties.GetBlurRadiusKeyword(settings.blurType.value);
		s_ShaderKeywords[9] = ShaderProperties.GetVarianceClippingKeyword(settings.varianceClipping.value);
		s_ShaderKeywords[10] = ShaderProperties.GetColorBleedingKeyword(settings.colorBleedingEnabled.value, settings.mode.value == HBAO.Mode.LitAO);
		s_ShaderKeywords[11] = ShaderProperties.GetModeKeyword(settings.mode.value, settings.debugMode.value);
		material.shaderKeywords = s_ShaderKeywords;
	}
}
