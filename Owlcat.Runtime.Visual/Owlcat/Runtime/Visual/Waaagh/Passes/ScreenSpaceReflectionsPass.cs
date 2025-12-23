using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.History;
using Owlcat.Runtime.Visual.Waaagh.PipelineResources;
using Owlcat.Runtime.Visual.Waaagh.Utilities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class ScreenSpaceReflectionsPass : ScriptableRenderPass
{
	private class RayTracingPassData
	{
		public TextureHandle SsrHitPointRT;

		public TextureHandle CameraNormalsRT;

		public TextureHandle CameraTranslucencyRT;

		public TextureHandle CameraDepthCopyRT;

		public TextureHandle CameraDepthPyramidRT;

		public Texture2D RankingTileXSPP;

		public Texture2D OwenScrambledTexture;

		public Texture2D ScramblingTileXSPP;

		public LocalKeyword SsrApproxKeyword;

		public Vector4 SsrScreenSize;

		public int MaxRaySteps;

		internal float ThicknessScale;

		internal float ThicknessBias;

		internal int MinDepthLevel;

		internal float MaxRoughness;

		internal float BRDFBias;

		internal int FrameCount;

		internal Vector4 RoughnessRemap;

		internal bool IsStochastic;

		internal float FresnelPower;

		public ComputeShader SsrCS;

		public ComputeShaderKernelDescriptor RayTraceKernel;
	}

	private class ReprojectionPassData
	{
		public TextureHandle SsrRT;

		public TextureHandle SsrHitPointRT;

		public TextureHandle CameraMotionVectorsRT;

		public TextureHandle CameraColorHistoryRT;

		public TextureHandle CameraDepthCopyRT;

		internal float EdgeFadeRcpLength;

		internal float RoughnessFadeEndTimesRcpLength;

		internal float RoughnessFadeRcpLength;

		internal Vector4 SsrScreenSize;

		public ComputeShader SsrCS;

		public ComputeShaderKernelDescriptor ReprojectionKernel;
	}

	private class AccumulationPassData
	{
		public TextureHandle SsrRT;

		public TextureHandle SsrRTPrev;

		public TextureHandle CameraMotionVectorsRT;

		public TextureHandle SsrHitPointRT;

		internal float AccumulationAmount;

		internal float SpeedRejectionScalerFactor;

		internal float SpeedRejection;

		internal bool UseReprojectedHistory;

		internal Vector4 SsrScreenSize;

		public ComputeShader SsrCS;

		public ComputeShaderKernelDescriptor AccumulateKernel;
	}

	private class BlurPassData
	{
		public TextureHandle SsrRT;

		public TextureHandle SsrHitPointRT;

		public TextureHandle CameraNormalsRT;

		internal Vector4 SsrScreenSize;

		public ComputeShader SsrCS;

		public ComputeShaderKernelDescriptor BlurKernel;
	}

	private class SsrPyramidPassData
	{
		public TextureHandle SsrRT;

		public TextureHandle SsrRTPrev;

		public TextureHandle SsrPyramidMips;

		public TextureHandle SsrHitPointRT;

		public TextureHandle CameraNormalsRT;

		public TextureHandle CameraTranslucencyRT;

		internal Vector4 SsrScreenSize;

		internal Vector4 SmoothnessRemap;

		public Material BlitMaterial;

		public Material SsrBlurMaterial;

		public int SsrBlurPass;

		internal int SsrCompositeSsrPass;
	}

	private static class ShaderConstants
	{
		public static readonly int _SsrIterLimit = Shader.PropertyToID("_SsrIterLimit");

		public static readonly int _SsrThicknessScale = Shader.PropertyToID("_SsrThicknessScale");

		public static readonly int _SsrThicknessBias = Shader.PropertyToID("_SsrThicknessBias");

		public static readonly int _SsrRoughnessFadeEnd = Shader.PropertyToID("_SsrRoughnessFadeEnd");

		public static readonly int _SsrMinDepthMipLevel = Shader.PropertyToID("_SsrMinDepthMipLevel");

		public static readonly int _SsrHitPointTextureSize = Shader.PropertyToID("_SsrHitPointTextureSize");

		public static readonly int _SsrBRDFBias = Shader.PropertyToID("_SsrBRDFBias");

		public static readonly int _SsrFrameCount = Shader.PropertyToID("_SsrFrameCount");

		public static readonly int _SsrRoughnessRemap = Shader.PropertyToID("_SsrRoughnessRemap");

		public static readonly int _SsrEdgeFadeRcpLength = Shader.PropertyToID("_SsrEdgeFadeRcpLength");

		public static readonly int _SsrRoughnessFadeEndTimesRcpLength = Shader.PropertyToID("_SsrRoughnessFadeEndTimesRcpLength");

		public static readonly int _SsrRoughnessFadeRcpLength = Shader.PropertyToID("_SsrRoughnessFadeRcpLength");

		public static readonly int _SsrAccumulationAmount = Shader.PropertyToID("_SsrAccumulationAmount");

		public static readonly int _SsrPRBSpeedRejectionScalerFactor = Shader.PropertyToID("_SsrPRBSpeedRejectionScalerFactor");

		public static readonly int _SsrPBRSpeedRejection = Shader.PropertyToID("_SsrPBRSpeedRejection");

		public static readonly int _SsrUseReprojectedHistory = Shader.PropertyToID("_SsrUseReprojectedHistory");

		public static readonly int _HighlightSuppression = Shader.PropertyToID("_HighlightSuppression");

		public static readonly int _SsrPyramidLodCount = Shader.PropertyToID("_SsrPyramidLodCount");

		public static readonly int _SsrScreenSize = Shader.PropertyToID("_SsrScreenSize");

		public static readonly int _SsrBlruEnabled = Shader.PropertyToID("_SsrBlruEnabled");

		public static readonly int _SsrFresnelPower = Shader.PropertyToID("_SsrFresnelPower");

		public static readonly int _SsrHitPointTexture = Shader.PropertyToID("_SsrHitPointTexture");

		public static readonly int _RankingTileXSPP = Shader.PropertyToID("_RankingTileXSPP");

		public static readonly int _OwenScrambledTexture = Shader.PropertyToID("_OwenScrambledTexture");

		public static readonly int _ScramblingTileXSPP = Shader.PropertyToID("_ScramblingTileXSPP");

		public static readonly int _CameraMotionVectorsTexture = Shader.PropertyToID("_CameraMotionVectorsTexture");

		public static readonly int _ColorPyramidTexture = Shader.PropertyToID("_ColorPyramidTexture");

		public static readonly int _SSRAccumTexture = Shader.PropertyToID("_SSRAccumTexture");

		public static readonly int _SsrAccumPrev = Shader.PropertyToID("_SsrAccumPrev");

		public static readonly int _SsrPyramidTexture = Shader.PropertyToID("_SsrPyramidTexture");
	}

	private const int kMaxSsrPyramidLodCount = 6;

	private ComputeShader m_SssrCs;

	private ComputeShaderKernelDescriptor m_RaytraceKernel;

	private ComputeShaderKernelDescriptor m_ReprojectionKernel;

	private ComputeShaderKernelDescriptor m_AccumulateKernel;

	private ComputeShaderKernelDescriptor m_BlurKernel;

	private Material m_BlitMaterial;

	private Material m_SsrBlurMaterial;

	private int m_SsrBlurPass;

	private int m_SsrCompositeSsrPass;

	private RenderRuntimeTextures m_TextureResources;

	private ScreenSpaceReflections m_Settings;

	private LocalKeyword m_SsrApproxKeyword;

	public override string Name => "ScreenSpaceReflectionsPass";

	public ScreenSpaceReflectionsPass(RenderPassEvent evt, ComputeShader ssrrCs, Material blitMaterial, Material ssrBlurMaterial, RenderRuntimeTextures textureResources)
		: base(evt)
	{
		m_SssrCs = ssrrCs;
		m_RaytraceKernel = m_SssrCs.GetKernelDescriptor("ScreenSpaceReflectionsTracing");
		m_ReprojectionKernel = m_SssrCs.GetKernelDescriptor("ScreenSpaceReflectionsReprojection");
		m_AccumulateKernel = m_SssrCs.GetKernelDescriptor("ScreenSpaceReflectionsAccumulate");
		m_BlurKernel = m_SssrCs.GetKernelDescriptor("ScreenSpaceBilateralBlur");
		m_SsrApproxKeyword = new LocalKeyword(m_SssrCs, "SSR_APPROX");
		m_BlitMaterial = blitMaterial;
		m_SsrBlurMaterial = ssrBlurMaterial;
		m_SsrBlurPass = m_SsrBlurMaterial.FindPass("BilateralBlur");
		m_SsrCompositeSsrPass = m_SsrBlurMaterial.FindPass("CompositeSSR");
		m_TextureResources = textureResources;
	}

	public override void ConfigureRendererLists(ScriptableRenderContext context, ContextContainer frameData)
	{
		WaaaghRendererListData waaaghRendererListData = frameData.Get<WaaaghRendererListData>();
		DependsOn(in waaaghRendererListData.OpaqueGBuffer.List);
		DependsOn(in waaaghRendererListData.OpaqueAlphaTestGBuffer.List);
		DependsOn(in waaaghRendererListData.TerrainGBuffer.List);
	}

	public override void RecordRenderGraph(ContextContainer frameData)
	{
		VolumeStack stack = VolumeManager.instance.stack;
		m_Settings = stack.GetComponent<ScreenSpaceReflections>();
		if (!m_Settings.IsActive())
		{
			return;
		}
		frameData.Get<WaaaghRendererListData>();
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		WaaaghRenderingData waaaghRenderingData = frameData.Get<WaaaghRenderingData>();
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		RenderGraph renderGraph = waaaghRenderingData.RenderGraph;
		ProfilingSampler sampler = ProfilingSampler.Get(WaaaghProfileId.SSR);
		renderGraph.BeginProfilingSampler(sampler, ".\\Library\\PackageCache\\com.owlcat.visual@141c9a01de77\\Runtime\\Waaagh\\Passes\\ScreenSpaceReflectionsPass.cs", 209);
		int2 @int = new int2(waaaghCameraData.cameraTargetDescriptor.width, waaaghCameraData.cameraTargetDescriptor.height);
		int minDepthLevel = 0;
		switch (m_Settings.Quality.value)
		{
		case ScreenSpaceReflectionsQuality.Half:
			@int /= 2;
			minDepthLevel = 1;
			break;
		case ScreenSpaceReflectionsQuality.Full:
			minDepthLevel = 0;
			break;
		}
		int value = m_Settings.MaxRaySteps.value;
		Camera camera = waaaghCameraData.camera;
		float nearClipPlane = camera.nearClipPlane;
		float farClipPlane = camera.farClipPlane;
		float value2 = m_Settings.ObjectThickness.value;
		float num = 1f / (1f + value2);
		float thicknessBias = (0f - nearClipPlane) / (farClipPlane - nearClipPlane) * (value2 * num);
		float value3 = m_Settings.MaxRoughness.value;
		Vector4 ssrScreenSize = new Vector4(@int.x, @int.y, 1f / (float)@int.x, 1f / (float)@int.y);
		float bRDFBias = 0.7f;
		int frameId = waaaghRenderingData.TimeData.FrameId;
		float edgeFadeRcpLength = Mathf.Min(1f / m_Settings.ScreenFadeDistance.value, float.MaxValue);
		float value4 = m_Settings.RoughnessFadeStart.value;
		float value5 = m_Settings.MaxRoughness.value;
		float num2 = value5 - value4;
		float roughnessFadeEndTimesRcpLength = ((num2 != 0f) ? (value5 * (1f / num2)) : 1f);
		float roughnessFadeRcpLength = ((num2 != 0f) ? (1f / num2) : 0f);
		float accumulationAmount = 1f - m_Settings.HistoryInfluence.value;
		float speedRejectionScalerFactor = Mathf.Pow(m_Settings.SpeedRejectionScalerFactor.value * 0.1f, 2f);
		float speedRejection = Mathf.Clamp01(1f - m_Settings.SpeedRejectionParam.value);
		bool value6 = m_Settings.BlurEnabled.value;
		float value7 = m_Settings.FresnelPower.value;
		bool isPlaying = Application.isPlaying;
		Vector4 roughnessRemap = m_Settings.RoughnessRemap.value;
		roughnessRemap.z = roughnessRemap.y - roughnessRemap.x;
		float4 @float = (Vector4)m_Settings.RoughnessRemap.value;
		@float.xy = 1f - @float.xy;
		@float.xy = @float.yx;
		@float.z = @float.y - @float.x;
		bool value8 = m_Settings.StochasticSSR.value;
		TextureDesc desc = RenderingUtils.CreateTextureDesc("SsrHiPointRT", waaaghCameraData.cameraTargetDescriptor);
		desc.colorFormat = GraphicsFormat.R16G16_SFloat;
		desc.filterMode = FilterMode.Point;
		desc.wrapMode = TextureWrapMode.Clamp;
		desc.enableRandomWrite = true;
		desc.width = @int.x;
		desc.height = @int.y;
		TextureHandle input = renderGraph.CreateTexture(in desc);
		SsrHistory ssrHistory = waaaghCameraData.SsrHistory;
		waaaghResourceData.SsrRT = waaaghRenderingData.RenderGraph.ImportTexture(ssrHistory.GetCurrentTexture());
		TextureHandle input2 = waaaghRenderingData.RenderGraph.ImportTexture(ssrHistory.GetPreviousTexture());
		RawColorHistory rawColorHistory = waaaghCameraData.historyManager?.GetHistoryForRead<RawColorHistory>();
		if (rawColorHistory == null)
		{
			renderGraph.EndProfilingSampler(sampler, ".\\Library\\PackageCache\\com.owlcat.visual@141c9a01de77\\Runtime\\Waaagh\\Passes\\ScreenSpaceReflectionsPass.cs", 276);
			return;
		}
		TextureHandle input3 = waaaghRenderingData.RenderGraph.ImportTexture(rawColorHistory.GetCurrentTexture());
		RayTracingPassData passData2;
		using (RenderGraphBuilder renderGraphBuilder = renderGraph.AddRenderPass<RayTracingPassData>("Ray Trace", out passData2, ".\\Library\\PackageCache\\com.owlcat.visual@141c9a01de77\\Runtime\\Waaagh\\Passes\\ScreenSpaceReflectionsPass.cs", 284))
		{
			passData2.SsrHitPointRT = renderGraphBuilder.WriteTexture(in input);
			passData2.CameraNormalsRT = renderGraphBuilder.ReadTexture(in waaaghResourceData.CameraNormalsRT);
			passData2.CameraDepthCopyRT = renderGraphBuilder.ReadTexture(in waaaghResourceData.CameraDepthCopyRT);
			passData2.CameraTranslucencyRT = renderGraphBuilder.ReadTexture(in waaaghResourceData.CameraTranslucencyRT);
			passData2.CameraDepthPyramidRT = renderGraphBuilder.ReadTexture(in waaaghResourceData.CameraDepthPyramidRT);
			passData2.ScramblingTileXSPP = m_TextureResources.ScramblingTile1SPP;
			passData2.RankingTileXSPP = m_TextureResources.RankingTile1SPP;
			passData2.OwenScrambledTexture = m_TextureResources.OwenScrambled256Tex;
			passData2.BRDFBias = bRDFBias;
			passData2.FrameCount = frameId;
			passData2.MaxRaySteps = value;
			passData2.MinDepthLevel = minDepthLevel;
			passData2.RoughnessRemap = roughnessRemap;
			passData2.SsrScreenSize = ssrScreenSize;
			passData2.ThicknessBias = thicknessBias;
			passData2.ThicknessScale = num;
			passData2.MaxRoughness = value3;
			passData2.SsrApproxKeyword = m_SsrApproxKeyword;
			passData2.IsStochastic = value8;
			passData2.FresnelPower = value7;
			passData2.SsrCS = m_SssrCs;
			passData2.RayTraceKernel = m_RaytraceKernel;
			renderGraphBuilder.SetRenderFunc(delegate(RayTracingPassData passData, RenderGraphContext context)
			{
				ExecuteRayTrace(passData, context);
			});
		}
		ReprojectionPassData passData3;
		using (RenderGraphBuilder renderGraphBuilder2 = renderGraph.AddRenderPass<ReprojectionPassData>("Reprojection", out passData3, ".\\Library\\PackageCache\\com.owlcat.visual@141c9a01de77\\Runtime\\Waaagh\\Passes\\ScreenSpaceReflectionsPass.cs", 326))
		{
			passData3.SsrRT = renderGraphBuilder2.WriteTexture(in waaaghResourceData.SsrRT);
			passData3.SsrHitPointRT = renderGraphBuilder2.ReadTexture(in input);
			passData3.CameraMotionVectorsRT = renderGraphBuilder2.ReadTexture(in waaaghResourceData.CameraMotionVectorsRT);
			passData3.CameraColorHistoryRT = renderGraphBuilder2.ReadTexture(in input3);
			passData3.CameraDepthCopyRT = renderGraphBuilder2.ReadTexture(in waaaghResourceData.CameraDepthCopyRT);
			passData3.SsrScreenSize = ssrScreenSize;
			passData3.RoughnessFadeRcpLength = roughnessFadeRcpLength;
			passData3.RoughnessFadeEndTimesRcpLength = roughnessFadeEndTimesRcpLength;
			passData3.EdgeFadeRcpLength = edgeFadeRcpLength;
			passData3.SsrCS = m_SssrCs;
			passData3.ReprojectionKernel = m_ReprojectionKernel;
			renderGraphBuilder2.SetRenderFunc(delegate(ReprojectionPassData passData, RenderGraphContext context)
			{
				ExecuteReprojection(passData, context);
			});
		}
		if (value8)
		{
			AccumulationPassData passData4;
			using (RenderGraphBuilder renderGraphBuilder3 = renderGraph.AddRenderPass<AccumulationPassData>("Accumulation", out passData4, ".\\Library\\PackageCache\\com.owlcat.visual@141c9a01de77\\Runtime\\Waaagh\\Passes\\ScreenSpaceReflectionsPass.cs", 359))
			{
				passData4.CameraMotionVectorsRT = renderGraphBuilder3.ReadTexture(in waaaghResourceData.CameraMotionVectorsRT);
				passData4.SsrHitPointRT = renderGraphBuilder3.ReadTexture(in input);
				passData4.SsrRT = renderGraphBuilder3.WriteTexture(in waaaghResourceData.SsrRT);
				passData4.SsrRTPrev = renderGraphBuilder3.ReadTexture(in input2);
				passData4.AccumulationAmount = accumulationAmount;
				passData4.SpeedRejection = speedRejection;
				passData4.SpeedRejectionScalerFactor = speedRejectionScalerFactor;
				passData4.UseReprojectedHistory = isPlaying;
				passData4.SsrScreenSize = ssrScreenSize;
				passData4.SsrCS = m_SssrCs;
				passData4.AccumulateKernel = m_AccumulateKernel;
				renderGraphBuilder3.SetRenderFunc(delegate(AccumulationPassData passData, RenderGraphContext context)
				{
					ExecuteAccumulation(passData, context);
				});
			}
			if (value6)
			{
				BlurPassData passData5;
				using RenderGraphBuilder renderGraphBuilder4 = renderGraph.AddRenderPass<BlurPassData>("Blur", out passData5, ".\\Library\\PackageCache\\com.owlcat.visual@141c9a01de77\\Runtime\\Waaagh\\Passes\\ScreenSpaceReflectionsPass.cs", 392);
				passData5.SsrRT = renderGraphBuilder4.ReadWriteTexture(in waaaghResourceData.SsrRT);
				passData5.SsrHitPointRT = renderGraphBuilder4.ReadTexture(in input);
				passData5.CameraNormalsRT = renderGraphBuilder4.ReadTexture(in waaaghResourceData.CameraNormalsRT);
				passData5.SsrScreenSize = ssrScreenSize;
				passData5.SsrCS = m_SssrCs;
				passData5.BlurKernel = m_BlurKernel;
				renderGraphBuilder4.SetRenderFunc(delegate(BlurPassData passData, RenderGraphContext context)
				{
					ExecuteBlur(passData, context);
				});
			}
		}
		else
		{
			SsrPyramidPassData passData6;
			using (RenderGraphBuilder renderGraphBuilder5 = renderGraph.AddRenderPass<SsrPyramidPassData>("Ssr Pyramid", out passData6, ".\\Library\\PackageCache\\com.owlcat.visual@141c9a01de77\\Runtime\\Waaagh\\Passes\\ScreenSpaceReflectionsPass.cs", 421))
			{
				TextureDesc desc2 = RenderingUtils.CreateTextureDesc("SsrPyramidMipsRT", ssrHistory.Descriptor);
				desc2.autoGenerateMips = false;
				desc2.useMipMap = true;
				desc2.filterMode = FilterMode.Trilinear;
				passData6.SsrPyramidMips = renderGraphBuilder5.CreateTransientTexture(in desc2);
				passData6.SsrRT = renderGraphBuilder5.ReadWriteTexture(in waaaghResourceData.SsrRT);
				passData6.SsrHitPointRT = renderGraphBuilder5.ReadTexture(in input);
				passData6.CameraNormalsRT = renderGraphBuilder5.ReadTexture(in waaaghResourceData.CameraNormalsRT);
				passData6.CameraTranslucencyRT = renderGraphBuilder5.ReadTexture(in waaaghResourceData.CameraTranslucencyRT);
				passData6.SsrScreenSize = ssrScreenSize;
				passData6.SmoothnessRemap = @float;
				passData6.BlitMaterial = m_BlitMaterial;
				passData6.SsrBlurMaterial = m_SsrBlurMaterial;
				passData6.SsrBlurPass = m_SsrBlurPass;
				passData6.SsrCompositeSsrPass = m_SsrCompositeSsrPass;
				renderGraphBuilder5.SetRenderFunc(delegate(SsrPyramidPassData passData, RenderGraphContext context)
				{
					ExecuteSsrPyramid(passData, context);
				});
			}
			AccumulationPassData passData7;
			using RenderGraphBuilder renderGraphBuilder6 = renderGraph.AddRenderPass<AccumulationPassData>("Accumulation", out passData7, ".\\Library\\PackageCache\\com.owlcat.visual@141c9a01de77\\Runtime\\Waaagh\\Passes\\ScreenSpaceReflectionsPass.cs", 457);
			passData7.CameraMotionVectorsRT = renderGraphBuilder6.ReadTexture(in waaaghResourceData.CameraMotionVectorsRT);
			passData7.SsrHitPointRT = renderGraphBuilder6.ReadTexture(in input);
			passData7.SsrRT = renderGraphBuilder6.WriteTexture(in waaaghResourceData.SsrRT);
			passData7.SsrRTPrev = renderGraphBuilder6.ReadTexture(in input2);
			passData7.AccumulationAmount = accumulationAmount;
			passData7.SpeedRejection = speedRejection;
			passData7.SpeedRejectionScalerFactor = speedRejectionScalerFactor;
			passData7.UseReprojectedHistory = isPlaying;
			passData7.SsrScreenSize = ssrScreenSize;
			passData7.SsrCS = m_SssrCs;
			passData7.AccumulateKernel = m_AccumulateKernel;
			renderGraphBuilder6.SetRenderFunc(delegate(AccumulationPassData passData, RenderGraphContext context)
			{
				ExecuteAccumulation(passData, context);
			});
		}
		renderGraph.EndProfilingSampler(sampler, ".\\Library\\PackageCache\\com.owlcat.visual@141c9a01de77\\Runtime\\Waaagh\\Passes\\ScreenSpaceReflectionsPass.cs", 486);
	}

	private static void ExecuteRayTrace(RayTracingPassData data, RenderGraphContext context)
	{
		context.cmd.SetRenderTarget(data.SsrHitPointRT);
		context.cmd.ClearRenderTarget(clearDepth: false, clearColor: true, Color.clear);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraNormalsRT, data.CameraNormalsRT);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraTranslucencyRT, data.CameraTranslucencyRT);
		context.cmd.SetComputeIntParam(data.SsrCS, ShaderConstants._SsrIterLimit, data.MaxRaySteps);
		context.cmd.SetComputeFloatParam(data.SsrCS, ShaderConstants._SsrThicknessScale, data.ThicknessScale);
		context.cmd.SetComputeFloatParam(data.SsrCS, ShaderConstants._SsrThicknessBias, data.ThicknessBias);
		context.cmd.SetComputeIntParam(data.SsrCS, ShaderConstants._SsrMinDepthMipLevel, data.MinDepthLevel);
		context.cmd.SetComputeFloatParam(data.SsrCS, ShaderConstants._SsrRoughnessFadeEnd, data.MaxRoughness);
		context.cmd.SetComputeVectorParam(data.SsrCS, ShaderConstants._SsrHitPointTextureSize, data.SsrScreenSize);
		context.cmd.SetComputeFloatParam(data.SsrCS, ShaderConstants._SsrBRDFBias, data.BRDFBias);
		context.cmd.SetComputeFloatParam(data.SsrCS, ShaderConstants._SsrFrameCount, data.FrameCount);
		context.cmd.SetComputeVectorParam(data.SsrCS, ShaderConstants._SsrRoughnessRemap, data.RoughnessRemap);
		context.cmd.SetComputeTextureParam(data.SsrCS, data.RayTraceKernel.Index, ShaderPropertyId._CameraDepthRT, data.CameraDepthCopyRT);
		context.cmd.SetComputeTextureParam(data.SsrCS, data.RayTraceKernel.Index, ShaderPropertyId._CameraDepthPyramidRT, data.CameraDepthPyramidRT);
		context.cmd.SetComputeTextureParam(data.SsrCS, data.RayTraceKernel.Index, ShaderConstants._SsrHitPointTexture, data.SsrHitPointRT);
		context.cmd.SetComputeTextureParam(data.SsrCS, data.RayTraceKernel.Index, ShaderConstants._RankingTileXSPP, data.RankingTileXSPP);
		context.cmd.SetComputeTextureParam(data.SsrCS, data.RayTraceKernel.Index, ShaderConstants._OwenScrambledTexture, data.OwenScrambledTexture);
		context.cmd.SetComputeTextureParam(data.SsrCS, data.RayTraceKernel.Index, ShaderConstants._ScramblingTileXSPP, data.ScramblingTileXSPP);
		context.cmd.SetKeyword(data.SsrCS, in data.SsrApproxKeyword, !data.IsStochastic);
		int3 dispatchSize = data.RayTraceKernel.GetDispatchSize(data.SsrScreenSize.x, data.SsrScreenSize.y, 1f);
		context.cmd.DispatchCompute(data.SsrCS, data.RayTraceKernel.Index, dispatchSize.x, dispatchSize.y, dispatchSize.z);
		context.cmd.SetGlobalFloat(ShaderConstants._SsrFresnelPower, data.FresnelPower);
	}

	private static void ExecuteReprojection(ReprojectionPassData data, RenderGraphContext context)
	{
		context.cmd.SetRenderTarget(data.SsrRT);
		context.cmd.ClearRenderTarget(clearDepth: false, clearColor: true, Color.clear);
		context.cmd.SetComputeFloatParam(data.SsrCS, ShaderConstants._SsrEdgeFadeRcpLength, data.EdgeFadeRcpLength);
		context.cmd.SetComputeFloatParam(data.SsrCS, ShaderConstants._SsrRoughnessFadeEndTimesRcpLength, data.RoughnessFadeEndTimesRcpLength);
		context.cmd.SetComputeFloatParam(data.SsrCS, ShaderConstants._SsrRoughnessFadeRcpLength, data.RoughnessFadeRcpLength);
		context.cmd.SetComputeTextureParam(data.SsrCS, data.ReprojectionKernel.Index, ShaderConstants._SsrHitPointTexture, data.SsrHitPointRT);
		context.cmd.SetComputeTextureParam(data.SsrCS, data.ReprojectionKernel.Index, ShaderConstants._CameraMotionVectorsTexture, data.CameraMotionVectorsRT);
		context.cmd.SetComputeTextureParam(data.SsrCS, data.ReprojectionKernel.Index, ShaderConstants._ColorPyramidTexture, data.CameraColorHistoryRT);
		context.cmd.SetComputeTextureParam(data.SsrCS, data.ReprojectionKernel.Index, ShaderConstants._SSRAccumTexture, data.SsrRT);
		context.cmd.SetComputeTextureParam(data.SsrCS, data.ReprojectionKernel.Index, ShaderPropertyId._CameraDepthTexture, data.CameraDepthCopyRT);
		int3 dispatchSize = data.ReprojectionKernel.GetDispatchSize(data.SsrScreenSize.x, data.SsrScreenSize.y, 1f);
		context.cmd.DispatchCompute(data.SsrCS, data.ReprojectionKernel.Index, dispatchSize.x, dispatchSize.y, dispatchSize.z);
	}

	private static void ExecuteAccumulation(AccumulationPassData data, RenderGraphContext context)
	{
		context.cmd.SetComputeFloatParam(data.SsrCS, ShaderConstants._SsrAccumulationAmount, data.AccumulationAmount);
		context.cmd.SetComputeFloatParam(data.SsrCS, ShaderConstants._SsrPRBSpeedRejectionScalerFactor, data.SpeedRejectionScalerFactor);
		context.cmd.SetComputeFloatParam(data.SsrCS, ShaderConstants._SsrPBRSpeedRejection, data.SpeedRejection);
		context.cmd.SetComputeFloatParam(data.SsrCS, ShaderConstants._SsrUseReprojectedHistory, data.UseReprojectedHistory ? 1 : 0);
		context.cmd.SetComputeTextureParam(data.SsrCS, data.AccumulateKernel.Index, ShaderConstants._CameraMotionVectorsTexture, data.CameraMotionVectorsRT);
		context.cmd.SetComputeTextureParam(data.SsrCS, data.AccumulateKernel.Index, ShaderConstants._SsrHitPointTexture, data.SsrHitPointRT);
		context.cmd.SetComputeTextureParam(data.SsrCS, data.AccumulateKernel.Index, ShaderConstants._SsrAccumPrev, data.SsrRTPrev);
		context.cmd.SetComputeTextureParam(data.SsrCS, data.AccumulateKernel.Index, ShaderConstants._SSRAccumTexture, data.SsrRT);
		int3 dispatchSize = data.AccumulateKernel.GetDispatchSize(data.SsrScreenSize.x, data.SsrScreenSize.y, 1f);
		context.cmd.DispatchCompute(data.SsrCS, data.AccumulateKernel.Index, dispatchSize.x, dispatchSize.y, dispatchSize.z);
	}

	private static void ExecuteBlur(BlurPassData data, RenderGraphContext context)
	{
		context.cmd.SetComputeTextureParam(data.SsrCS, data.BlurKernel.Index, ShaderConstants._SsrHitPointTexture, data.SsrHitPointRT);
		context.cmd.SetComputeTextureParam(data.SsrCS, data.BlurKernel.Index, ShaderConstants._SSRAccumTexture, data.SsrRT);
		int3 dispatchSize = data.BlurKernel.GetDispatchSize(data.SsrScreenSize.x, data.SsrScreenSize.y, 1f);
		context.cmd.DispatchCompute(data.SsrCS, data.BlurKernel.Index, dispatchSize.x, dispatchSize.y, dispatchSize.z);
	}

	private static void ExecuteSsrPyramid(SsrPyramidPassData data, RenderGraphContext context)
	{
		int num = 0;
		int num2 = (int)data.SsrScreenSize.x;
		int num3 = (int)data.SsrScreenSize.y;
		int num4 = num2;
		int num5 = num3;
		Vector4 value = new Vector4(1f, 1f, 0f, 0f);
		context.cmd.SetGlobalTexture(ShaderPropertyId._BlitTexture, data.SsrRT);
		context.cmd.SetGlobalVector(ShaderPropertyId._BlitScaleBias, value);
		context.cmd.SetGlobalFloat(ShaderPropertyId._BlitMipLevel, 0f);
		context.cmd.SetGlobalFloat(ShaderConstants._HighlightSuppression, 1f);
		context.cmd.SetRenderTarget(data.SsrPyramidMips, 0);
		context.cmd.SetViewport(new Rect(0f, 0f, num2, num3));
		context.cmd.DrawProcedural(Matrix4x4.identity, data.BlitMaterial, 0, MeshTopology.Triangles, 3, 1);
		while (num2 >= 8 && num3 >= 8)
		{
			int num6 = math.max(1, num2 >> 1);
			int num7 = math.max(1, num3 >> 1);
			context.cmd.SetGlobalTexture(ShaderPropertyId._Source, data.SsrPyramidMips);
			context.cmd.SetGlobalVector(ShaderPropertyId._SrcScaleBias, new Vector4(1f, 1f, 0f, 0f));
			context.cmd.SetGlobalVector(ShaderPropertyId._SrcUvLimits, new Vector4(1f, 1f, 1f / (float)num2, 0f));
			context.cmd.SetGlobalFloat(ShaderPropertyId._SourceMip, num);
			context.cmd.SetRenderTarget(data.SsrRT, 0);
			context.cmd.SetViewport(new Rect(0f, 0f, num6, num7));
			context.cmd.DrawProcedural(Matrix4x4.identity, data.SsrBlurMaterial, data.SsrBlurPass, MeshTopology.Triangles, 3, 1);
			float x = (float)num6 / (float)num4;
			float y = (float)num7 / (float)num5;
			context.cmd.SetGlobalTexture(ShaderPropertyId._Source, data.SsrRT);
			context.cmd.SetGlobalVector(ShaderPropertyId._SrcScaleBias, new Vector4(x, y, 0f, 0f));
			context.cmd.SetGlobalVector(ShaderPropertyId._SrcUvLimits, new Vector4(((float)num6 - 0.5f) / (float)num4, ((float)num7 - 0.5f) / (float)num5, 0f, 0.5f / (float)num5));
			context.cmd.SetGlobalFloat(ShaderPropertyId._SourceMip, 0f);
			context.cmd.SetRenderTarget(data.SsrPyramidMips, num + 1);
			context.cmd.SetViewport(new Rect(0f, 0f, num6, num7));
			context.cmd.DrawProcedural(Matrix4x4.identity, data.SsrBlurMaterial, data.SsrBlurPass, MeshTopology.Triangles, 3, 1);
			num++;
			num2 >>= 1;
			num3 >>= 1;
			if (num > 6)
			{
				break;
			}
		}
		context.cmd.SetGlobalFloat(ShaderConstants._SsrPyramidLodCount, num);
		context.cmd.SetGlobalTexture(ShaderConstants._SsrHitPointTexture, data.SsrHitPointRT);
		context.cmd.SetGlobalTexture(ShaderConstants._SsrPyramidTexture, data.SsrPyramidMips);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraNormalsRT, data.CameraNormalsRT);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraTranslucencyRT, data.CameraTranslucencyRT);
		context.cmd.SetGlobalVector(ShaderConstants._SsrScreenSize, data.SsrScreenSize);
		context.cmd.SetRenderTarget(data.SsrRT);
		context.cmd.SetGlobalFloat(ShaderConstants._SsrBlruEnabled, 1f);
		context.cmd.SetGlobalVector(ShaderConstants._SsrRoughnessRemap, data.SmoothnessRemap);
		context.cmd.DrawProcedural(Matrix4x4.identity, data.SsrBlurMaterial, data.SsrCompositeSsrPass, MeshTopology.Triangles, 3);
	}
}
