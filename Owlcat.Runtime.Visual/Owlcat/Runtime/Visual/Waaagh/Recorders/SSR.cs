using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.History;
using Owlcat.Runtime.Visual.Waaagh.Utilities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders;

public static class SSR
{
	private class RayTracingPassData
	{
		public TextureHandle SsrHitPointRT;

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

	public static bool ShouldDraw(in RecordContext context)
	{
		return context.CameraData.IsSSREnabled;
	}

	public static void Draw(in RecordContext context)
	{
		RenderGraph renderGraph = context.RenderGraph;
		ScreenSpaceReflections component = VolumeManager.instance.stack.GetComponent<ScreenSpaceReflections>();
		if (component.IsActive() && context.CameraData.historyManager?.GetHistoryForRead<RawColorHistory>() != null)
		{
			ProfilingSampler sampler = WaaaghProfileId.SSR.Sampler();
			renderGraph.BeginProfilingSampler(sampler, ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\SSR.cs", 160);
			PopulatePasses(component, in context);
			renderGraph.EndProfilingSampler(sampler, ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\SSR.cs", 164);
		}
	}

	private static void PopulatePasses(ScreenSpaceReflections settings, in RecordContext context)
	{
		WaaaghCameraData cameraData = context.CameraData;
		WaaaghRenderingData renderingData = context.RenderingData;
		RenderGraph renderGraph = context.RenderGraph;
		int2 @int = new int2(cameraData.cameraTargetDescriptor.width, cameraData.cameraTargetDescriptor.height);
		int minDepthLevel = 0;
		switch (settings.Quality.value)
		{
		case ScreenSpaceReflectionsQuality.Half:
			@int /= 2;
			minDepthLevel = 1;
			break;
		case ScreenSpaceReflectionsQuality.Full:
			minDepthLevel = 0;
			break;
		}
		int value = settings.MaxRaySteps.value;
		Camera camera = cameraData.camera;
		float nearClipPlane = camera.nearClipPlane;
		float farClipPlane = camera.farClipPlane;
		float value2 = settings.ObjectThickness.value;
		float num = 1f / (1f + value2);
		float thicknessBias = (0f - nearClipPlane) / (farClipPlane - nearClipPlane) * (value2 * num);
		float value3 = settings.MaxRoughness.value;
		Vector4 ssrScreenSize = new Vector4(@int.x, @int.y, 1f / (float)@int.x, 1f / (float)@int.y);
		float bRDFBias = 0.7f;
		int frameId = renderingData.TimeData.FrameId;
		float edgeFadeRcpLength = Mathf.Min(1f / settings.ScreenFadeDistance.value, float.MaxValue);
		float value4 = settings.RoughnessFadeStart.value;
		float value5 = settings.MaxRoughness.value;
		float num2 = value5 - value4;
		float roughnessFadeEndTimesRcpLength = ((num2 != 0f) ? (value5 * (1f / num2)) : 1f);
		float roughnessFadeRcpLength = ((num2 != 0f) ? (1f / num2) : 0f);
		float accumulationAmount = 1f - settings.HistoryInfluence.value;
		float speedRejectionScalerFactor = Mathf.Pow(settings.SpeedRejectionScalerFactor.value * 0.1f, 2f);
		float speedRejection = Mathf.Clamp01(1f - settings.SpeedRejectionParam.value);
		bool value6 = settings.BlurEnabled.value;
		float value7 = settings.FresnelPower.value;
		bool isPlaying = Application.isPlaying;
		Vector4 roughnessRemap = settings.RoughnessRemap.value;
		roughnessRemap.z = roughnessRemap.y - roughnessRemap.x;
		float4 @float = (Vector4)settings.RoughnessRemap.value;
		@float.xy = 1f - @float.xy;
		@float.xy = @float.yx;
		@float.z = @float.y - @float.x;
		bool value8 = settings.StochasticSSR.value;
		TextureDesc desc = RenderingUtils.CreateTextureDesc("SsrHiPointRT", cameraData.cameraTargetDescriptor);
		desc.colorFormat = GraphicsFormat.R16G16_SFloat;
		desc.filterMode = FilterMode.Point;
		desc.wrapMode = TextureWrapMode.Clamp;
		desc.enableRandomWrite = true;
		desc.width = @int.x;
		desc.height = @int.y;
		TextureHandle input = renderGraph.CreateTexture(in desc);
		SsrHistory ssrHistory = cameraData.SsrHistory;
		TextureHandle input2 = context.FrameResources.SsrTargets.SsrCurrent;
		TextureHandle input3 = context.FrameResources.SsrTargets.SsrPrev;
		TextureHandle input4 = context.FrameResources.CameraAdditionalTargets.RawColorHistory;
		RayTracingPassData passData;
		using (IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = renderGraph.AddUnsafePass<RayTracingPassData>("SSR.RayTrace", out passData, WaaaghProfileId.SSR_RayTrace.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\SSR.cs", 243))
		{
			passData.SsrHitPointRT = input;
			unsafeRenderGraphBuilder.UseTexture(in input, AccessFlags.Write);
			unsafeRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraDepthPyramidRT);
			unsafeRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraDepthRT);
			unsafeRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraNormalsRT);
			unsafeRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraTranslucencyRT);
			passData.ScramblingTileXSPP = context.Textures.ScramblingTile1SPP;
			passData.RankingTileXSPP = context.Textures.RankingTile1SPP;
			passData.OwenScrambledTexture = context.Textures.OwenScrambled256Tex;
			passData.BRDFBias = bRDFBias;
			passData.FrameCount = frameId;
			passData.MaxRaySteps = value;
			passData.MinDepthLevel = minDepthLevel;
			passData.RoughnessRemap = roughnessRemap;
			passData.SsrScreenSize = ssrScreenSize;
			passData.ThicknessBias = thicknessBias;
			passData.ThicknessScale = num;
			passData.MaxRoughness = value3;
			passData.SsrApproxKeyword = context.MaterialLibrary.SsrCS.SsrApproxKeyword;
			passData.IsStochastic = value8;
			passData.FresnelPower = value7;
			passData.SsrCS = context.MaterialLibrary.SsrCS.CS;
			passData.RayTraceKernel = context.MaterialLibrary.SsrCS.RaytraceKernel;
			unsafeRenderGraphBuilder.SetRenderFunc<RayTracingPassData>(ExecuteRayTrace);
		}
		ReprojectionPassData passData2;
		using (IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder2 = renderGraph.AddUnsafePass<ReprojectionPassData>("SSR.Reprojection", out passData2, WaaaghProfileId.SSR_Reprojection.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\SSR.cs", 282))
		{
			passData2.SsrRT = input2;
			unsafeRenderGraphBuilder2.UseTexture(in input2, AccessFlags.Write);
			passData2.SsrHitPointRT = input;
			unsafeRenderGraphBuilder2.UseTexture(in input);
			passData2.CameraMotionVectorsRT = context.FrameResources.CameraAdditionalTargets.MotionVectors;
			unsafeRenderGraphBuilder2.UseTexture(in passData2.CameraMotionVectorsRT);
			passData2.CameraColorHistoryRT = input4;
			unsafeRenderGraphBuilder2.UseTexture(in input4);
			unsafeRenderGraphBuilder2.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraDepthTexture);
			passData2.SsrScreenSize = ssrScreenSize;
			passData2.RoughnessFadeRcpLength = roughnessFadeRcpLength;
			passData2.RoughnessFadeEndTimesRcpLength = roughnessFadeEndTimesRcpLength;
			passData2.EdgeFadeRcpLength = edgeFadeRcpLength;
			passData2.SsrCS = context.MaterialLibrary.SsrCS.CS;
			passData2.ReprojectionKernel = context.MaterialLibrary.SsrCS.ReprojectionKernel;
			unsafeRenderGraphBuilder2.SetRenderFunc<ReprojectionPassData>(ExecuteReprojection);
		}
		if (value8)
		{
			AccumulationPassData passData3;
			using (IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder3 = renderGraph.AddUnsafePass<AccumulationPassData>("SSR.Accumulation", out passData3, WaaaghProfileId.SSR_Accumulation.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\SSR.cs", 317))
			{
				passData3.CameraMotionVectorsRT = context.FrameResources.CameraAdditionalTargets.MotionVectors;
				unsafeRenderGraphBuilder3.UseTexture(in passData3.CameraMotionVectorsRT);
				passData3.SsrHitPointRT = input;
				unsafeRenderGraphBuilder3.UseTexture(in input);
				passData3.SsrRT = input2;
				unsafeRenderGraphBuilder3.UseTexture(in input2, AccessFlags.Write);
				passData3.SsrRTPrev = input3;
				unsafeRenderGraphBuilder3.UseTexture(in input3);
				passData3.AccumulationAmount = accumulationAmount;
				passData3.SpeedRejection = speedRejection;
				passData3.SpeedRejectionScalerFactor = speedRejectionScalerFactor;
				passData3.UseReprojectedHistory = isPlaying;
				passData3.SsrScreenSize = ssrScreenSize;
				passData3.SsrCS = context.MaterialLibrary.SsrCS.CS;
				passData3.AccumulateKernel = context.MaterialLibrary.SsrCS.AccumulateKernel;
				unsafeRenderGraphBuilder3.SetRenderFunc<AccumulationPassData>(ExecuteAccumulation);
			}
			BlurPassData passData4;
			if (value6)
			{
				using (IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder4 = renderGraph.AddUnsafePass<BlurPassData>("SSR.Blur", out passData4, WaaaghProfileId.SSR_Blur.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\SSR.cs", 354))
				{
					passData4.SsrRT = input2;
					unsafeRenderGraphBuilder4.UseTexture(in input2, AccessFlags.ReadWrite);
					passData4.SsrHitPointRT = input;
					unsafeRenderGraphBuilder4.UseTexture(in input);
					unsafeRenderGraphBuilder4.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraNormalsRT);
					passData4.SsrScreenSize = ssrScreenSize;
					passData4.SsrCS = context.MaterialLibrary.SsrCS.CS;
					passData4.BlurKernel = context.MaterialLibrary.SsrCS.BlurKernel;
					unsafeRenderGraphBuilder4.SetRenderFunc<BlurPassData>(ExecuteBlur);
					return;
				}
			}
			return;
		}
		SsrPyramidPassData passData5;
		using (IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder5 = renderGraph.AddUnsafePass<SsrPyramidPassData>("SSR.Pyramid", out passData5, WaaaghProfileId.SSR_Pyramid.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\SSR.cs", 381))
		{
			TextureDesc desc2 = RenderingUtils.CreateTextureDesc("SsrPyramidMipsRT", ssrHistory.Descriptor);
			desc2.autoGenerateMips = false;
			desc2.useMipMap = true;
			desc2.filterMode = FilterMode.Trilinear;
			passData5.SsrPyramidMips = unsafeRenderGraphBuilder5.CreateTransientTexture(in desc2);
			passData5.SsrRT = input2;
			unsafeRenderGraphBuilder5.UseTexture(in input2, AccessFlags.ReadWrite);
			passData5.SsrHitPointRT = input;
			unsafeRenderGraphBuilder5.UseTexture(in input);
			unsafeRenderGraphBuilder5.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraNormalsRT);
			unsafeRenderGraphBuilder5.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraTranslucencyRT);
			passData5.SsrScreenSize = ssrScreenSize;
			passData5.SmoothnessRemap = @float;
			passData5.BlitMaterial = context.MaterialLibrary.BlitMaterial;
			passData5.SsrBlurMaterial = context.MaterialLibrary.SsrMaterial;
			passData5.SsrBlurPass = context.MaterialLibrary.SsrBlurPass;
			passData5.SsrCompositeSsrPass = context.MaterialLibrary.SsrCompositeSsrPass;
			unsafeRenderGraphBuilder5.SetRenderFunc<SsrPyramidPassData>(ExecuteSsrPyramid);
		}
		AccumulationPassData passData6;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder6 = renderGraph.AddUnsafePass<AccumulationPassData>("Accumulation", out passData6, WaaaghProfileId.SSR_Accumulation.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\SSR.cs", 418);
		passData6.CameraMotionVectorsRT = context.FrameResources.CameraAdditionalTargets.MotionVectors;
		unsafeRenderGraphBuilder6.UseTexture(in passData6.CameraMotionVectorsRT);
		passData6.SsrHitPointRT = input;
		unsafeRenderGraphBuilder6.UseTexture(in input);
		passData6.SsrRT = input2;
		unsafeRenderGraphBuilder6.UseTexture(in input2, AccessFlags.Write);
		passData6.SsrRTPrev = input3;
		unsafeRenderGraphBuilder6.UseTexture(in input3);
		passData6.AccumulationAmount = accumulationAmount;
		passData6.SpeedRejection = speedRejection;
		passData6.SpeedRejectionScalerFactor = speedRejectionScalerFactor;
		passData6.UseReprojectedHistory = isPlaying;
		passData6.SsrScreenSize = ssrScreenSize;
		passData6.SsrCS = context.MaterialLibrary.SsrCS.CS;
		passData6.AccumulateKernel = context.MaterialLibrary.SsrCS.AccumulateKernel;
		unsafeRenderGraphBuilder6.SetRenderFunc<AccumulationPassData>(ExecuteAccumulation);
	}

	private static void ExecuteRayTrace(RayTracingPassData data, UnsafeGraphContext context)
	{
		context.cmd.SetRenderTarget(data.SsrHitPointRT);
		context.cmd.ClearRenderTarget(clearDepth: false, clearColor: true, Color.clear);
		context.cmd.SetComputeIntParam(data.SsrCS, ShaderConstants._SsrIterLimit, data.MaxRaySteps);
		context.cmd.SetComputeFloatParam(data.SsrCS, ShaderConstants._SsrThicknessScale, data.ThicknessScale);
		context.cmd.SetComputeFloatParam(data.SsrCS, ShaderConstants._SsrThicknessBias, data.ThicknessBias);
		context.cmd.SetComputeIntParam(data.SsrCS, ShaderConstants._SsrMinDepthMipLevel, data.MinDepthLevel);
		context.cmd.SetComputeFloatParam(data.SsrCS, ShaderConstants._SsrRoughnessFadeEnd, data.MaxRoughness);
		context.cmd.SetComputeVectorParam(data.SsrCS, ShaderConstants._SsrHitPointTextureSize, data.SsrScreenSize);
		context.cmd.SetComputeFloatParam(data.SsrCS, ShaderConstants._SsrBRDFBias, data.BRDFBias);
		context.cmd.SetComputeFloatParam(data.SsrCS, ShaderConstants._SsrFrameCount, data.FrameCount);
		context.cmd.SetComputeVectorParam(data.SsrCS, ShaderConstants._SsrRoughnessRemap, data.RoughnessRemap);
		context.cmd.SetComputeTextureParam(data.SsrCS, data.RayTraceKernel.Index, ShaderConstants._SsrHitPointTexture, data.SsrHitPointRT);
		context.cmd.SetComputeTextureParam(data.SsrCS, data.RayTraceKernel.Index, ShaderConstants._RankingTileXSPP, data.RankingTileXSPP);
		context.cmd.SetComputeTextureParam(data.SsrCS, data.RayTraceKernel.Index, ShaderConstants._OwenScrambledTexture, data.OwenScrambledTexture);
		context.cmd.SetComputeTextureParam(data.SsrCS, data.RayTraceKernel.Index, ShaderConstants._ScramblingTileXSPP, data.ScramblingTileXSPP);
		context.cmd.SetKeyword(data.SsrCS, in data.SsrApproxKeyword, !data.IsStochastic);
		int3 dispatchSize = data.RayTraceKernel.GetDispatchSize(data.SsrScreenSize.x, data.SsrScreenSize.y, 1f);
		context.cmd.DispatchCompute(data.SsrCS, data.RayTraceKernel.Index, dispatchSize.x, dispatchSize.y, dispatchSize.z);
		context.cmd.SetGlobalFloat(ShaderConstants._SsrFresnelPower, data.FresnelPower);
	}

	private static void ExecuteReprojection(ReprojectionPassData data, UnsafeGraphContext context)
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
		int3 dispatchSize = data.ReprojectionKernel.GetDispatchSize(data.SsrScreenSize.x, data.SsrScreenSize.y, 1f);
		context.cmd.DispatchCompute(data.SsrCS, data.ReprojectionKernel.Index, dispatchSize.x, dispatchSize.y, dispatchSize.z);
	}

	private static void ExecuteAccumulation(AccumulationPassData data, UnsafeGraphContext context)
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

	private static void ExecuteBlur(BlurPassData data, UnsafeGraphContext context)
	{
		context.cmd.SetComputeTextureParam(data.SsrCS, data.BlurKernel.Index, ShaderConstants._SsrHitPointTexture, data.SsrHitPointRT);
		context.cmd.SetComputeTextureParam(data.SsrCS, data.BlurKernel.Index, ShaderConstants._SSRAccumTexture, data.SsrRT);
		int3 dispatchSize = data.BlurKernel.GetDispatchSize(data.SsrScreenSize.x, data.SsrScreenSize.y, 1f);
		context.cmd.DispatchCompute(data.SsrCS, data.BlurKernel.Index, dispatchSize.x, dispatchSize.y, dispatchSize.z);
	}

	private static void ExecuteSsrPyramid(SsrPyramidPassData data, UnsafeGraphContext context)
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
		context.cmd.SetGlobalVector(ShaderConstants._SsrScreenSize, data.SsrScreenSize);
		context.cmd.SetRenderTarget(data.SsrRT);
		context.cmd.SetGlobalFloat(ShaderConstants._SsrBlruEnabled, 1f);
		context.cmd.SetGlobalVector(ShaderConstants._SsrRoughnessRemap, data.SmoothnessRemap);
		context.cmd.DrawProcedural(Matrix4x4.identity, data.SsrBlurMaterial, data.SsrCompositeSsrPass, MeshTopology.Triangles, 3);
	}
}
