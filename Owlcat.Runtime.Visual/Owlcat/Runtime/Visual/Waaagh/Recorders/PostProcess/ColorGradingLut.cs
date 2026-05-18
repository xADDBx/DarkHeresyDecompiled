using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.PostProcess;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders.PostProcess;

public static class ColorGradingLut
{
	private class ColorGradingLutPassData
	{
		public Material Material;

		public TextureHandle LutTarget;
	}

	private static GraphicsFormat s_HdrLutFormat;

	private static GraphicsFormat s_LdrLutFormat;

	private static bool s_AllowColorGradingACESHDR;

	static ColorGradingLut()
	{
		if (SystemInfo.IsFormatSupported(GraphicsFormat.R16G16B16A16_SFloat, GraphicsFormatUsage.Blend))
		{
			s_HdrLutFormat = GraphicsFormat.R16G16B16A16_SFloat;
		}
		else if (SystemInfo.IsFormatSupported(GraphicsFormat.B10G11R11_UFloatPack32, GraphicsFormatUsage.Blend))
		{
			s_HdrLutFormat = GraphicsFormat.B10G11R11_UFloatPack32;
		}
		else
		{
			s_HdrLutFormat = GraphicsFormat.R8G8B8A8_UNorm;
		}
		s_LdrLutFormat = GraphicsFormat.R8G8B8A8_UNorm;
		s_AllowColorGradingACESHDR = true;
		if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3 && Graphics.minOpenGLESVersion <= OpenGLESVersion.OpenGLES30 && SystemInfo.graphicsDeviceName.StartsWith("Adreno (TM) 3"))
		{
			s_AllowColorGradingACESHDR = false;
		}
	}

	public static void Render(PostProcessor postProcessor, in RecordContext context)
	{
		bool num = postProcessor.Settings.ColorGradingMode == ColorGradingMode.HighDynamicRange;
		int colorGradingLutSize = postProcessor.Settings.ColorGradingLutSize;
		int num2 = colorGradingLutSize * colorGradingLutSize;
		GraphicsFormat colorFormat = (num ? s_HdrLutFormat : s_LdrLutFormat);
		PostProcessMaterialLibrary matLib = postProcessor.MatLib;
		Material material = (num ? matLib.LutBuilderHdr : matLib.LutBuilderLdr);
		TextureDesc desc = new TextureDesc
		{
			width = num2,
			height = colorGradingLutSize,
			colorFormat = colorFormat,
			dimension = TextureDimension.Tex2D,
			slices = 1,
			msaaSamples = MSAASamples.None,
			anisoLevel = 1,
			name = "_ColorGradingLUT"
		};
		VolumeStack stack = VolumeManager.instance.stack;
		ChannelMixer component = stack.GetComponent<ChannelMixer>();
		ColorAdjustments component2 = stack.GetComponent<ColorAdjustments>();
		ColorCurves component3 = stack.GetComponent<ColorCurves>();
		LiftGammaGain component4 = stack.GetComponent<LiftGammaGain>();
		ShadowsMidtonesHighlights component5 = stack.GetComponent<ShadowsMidtonesHighlights>();
		SplitToning component6 = stack.GetComponent<SplitToning>();
		Tonemapping component7 = stack.GetComponent<Tonemapping>();
		WhiteBalance component8 = stack.GetComponent<WhiteBalance>();
		Vector3 vector = ColorUtils.ColorBalanceToLMSCoeffs(component8.temperature.value, component8.tint.value);
		Vector4 value = new Vector4(component2.hueShift.value / 360f, component2.saturation.value / 100f + 1f, component2.contrast.value / 100f + 1f, 0f);
		Vector4 value2 = new Vector4(component.redOutRedIn.value / 100f, component.redOutGreenIn.value / 100f, component.redOutBlueIn.value / 100f, 0f);
		Vector4 value3 = new Vector4(component.greenOutRedIn.value / 100f, component.greenOutGreenIn.value / 100f, component.greenOutBlueIn.value / 100f, 0f);
		Vector4 value4 = new Vector4(component.blueOutRedIn.value / 100f, component.blueOutGreenIn.value / 100f, component.blueOutBlueIn.value / 100f, 0f);
		Vector4 value5 = new Vector4(component5.shadowsStart.value, component5.shadowsEnd.value, component5.highlightsStart.value, component5.highlightsEnd.value);
		Vector4 inShadows = component5.shadows.value;
		Vector4 inMidtones = component5.midtones.value;
		Vector4 inHighlights = component5.highlights.value;
		(Vector4, Vector4, Vector4) tuple = ColorUtils.PrepareShadowsMidtonesHighlights(in inShadows, in inMidtones, in inHighlights);
		Vector4 item = tuple.Item1;
		Vector4 item2 = tuple.Item2;
		Vector4 item3 = tuple.Item3;
		inShadows = component4.lift.value;
		inMidtones = component4.gamma.value;
		inHighlights = component4.gain.value;
		(Vector4, Vector4, Vector4) tuple2 = ColorUtils.PrepareLiftGammaGain(in inShadows, in inMidtones, in inHighlights);
		Vector4 item4 = tuple2.Item1;
		Vector4 item5 = tuple2.Item2;
		Vector4 item6 = tuple2.Item3;
		inShadows = component6.shadows.value;
		inMidtones = component6.highlights.value;
		var (value6, value7) = ColorUtils.PrepareSplitToning(in inShadows, in inMidtones, component6.balance.value);
		material.SetVector(value: new Vector4(colorGradingLutSize, 0.5f / (float)num2, 0.5f / (float)colorGradingLutSize, (float)colorGradingLutSize / ((float)colorGradingLutSize - 1f)), nameID: PostProcessor.ShaderIDs._Lut_Params);
		material.SetVector(PostProcessor.ShaderIDs._ColorBalance, vector);
		material.SetVector(PostProcessor.ShaderIDs._ColorFilter, component2.colorFilter.value.linear);
		material.SetVector(PostProcessor.ShaderIDs._ChannelMixerRed, value2);
		material.SetVector(PostProcessor.ShaderIDs._ChannelMixerGreen, value3);
		material.SetVector(PostProcessor.ShaderIDs._ChannelMixerBlue, value4);
		material.SetVector(PostProcessor.ShaderIDs._HueSatCon, value);
		material.SetVector(PostProcessor.ShaderIDs._Lift, item4);
		material.SetVector(PostProcessor.ShaderIDs._Gamma, item5);
		material.SetVector(PostProcessor.ShaderIDs._Gain, item6);
		material.SetVector(PostProcessor.ShaderIDs._Shadows, item);
		material.SetVector(PostProcessor.ShaderIDs._Midtones, item2);
		material.SetVector(PostProcessor.ShaderIDs._Highlights, item3);
		material.SetVector(PostProcessor.ShaderIDs._ShaHiLimits, value5);
		material.SetVector(PostProcessor.ShaderIDs._SplitShadows, value6);
		material.SetVector(PostProcessor.ShaderIDs._SplitHighlights, value7);
		material.SetTexture(PostProcessor.ShaderIDs._CurveMaster, component3.master.value.GetTexture());
		material.SetTexture(PostProcessor.ShaderIDs._CurveRed, component3.red.value.GetTexture());
		material.SetTexture(PostProcessor.ShaderIDs._CurveGreen, component3.green.value.GetTexture());
		material.SetTexture(PostProcessor.ShaderIDs._CurveBlue, component3.blue.value.GetTexture());
		material.SetTexture(PostProcessor.ShaderIDs._CurveHueVsHue, component3.hueVsHue.value.GetTexture());
		material.SetTexture(PostProcessor.ShaderIDs._CurveHueVsSat, component3.hueVsSat.value.GetTexture());
		material.SetTexture(PostProcessor.ShaderIDs._CurveLumVsSat, component3.lumVsSat.value.GetTexture());
		material.SetTexture(PostProcessor.ShaderIDs._CurveSatVsSat, component3.satVsSat.value.GetTexture());
		if (num)
		{
			material.shaderKeywords = null;
			switch (component7.mode.value)
			{
			case TonemappingMode.Neutral:
				material.EnableKeyword(ShaderKeywordStrings.TonemapNeutral);
				break;
			case TonemappingMode.ACES:
				material.EnableKeyword(s_AllowColorGradingACESHDR ? ShaderKeywordStrings.TonemapACES : ShaderKeywordStrings.TonemapNeutral);
				break;
			case TonemappingMode.Makeev:
				material.EnableKeyword(MakeevTonemapping.TrySetupParameters(material, postProcessor.Resources, component7) ? ShaderKeywordStrings.TonemapMakeev : ShaderKeywordStrings.TonemapNeutral);
				break;
			}
			WaaaghCameraData cameraData = context.CameraData;
			if (cameraData.isHDROutputActive)
			{
				WaaaghPipeline.GetHDROutputLuminanceParameters(cameraData.hdrDisplayInformation, cameraData.hdrDisplayColorGamut, component7, out var hdrOutputParameters);
				WaaaghPipeline.GetHDROutputGradingParameters(component7, out var hdrOutputParameters2);
				material.SetVector(ShaderPropertyId._HDROutputLuminanceParams, hdrOutputParameters);
				material.SetVector(ShaderPropertyId._HDROutputGradingParams, hdrOutputParameters2);
				HDROutputUtils.ConfigureHDROutput(material, cameraData.hdrDisplayColorGamut, HDROutputUtils.Operation.ColorConversion);
			}
		}
		ColorGradingLutPassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = context.RenderGraph.AddRasterRenderPass<ColorGradingLutPassData>("ColorGradingLut", out passData, ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\PostProcess\\ColorGradingLut.cs", 184);
		passData.Material = material;
		postProcessor.GpuResources.ColorGradingLut = context.RenderGraph.CreateTexture(in desc);
		passData.LutTarget = postProcessor.GpuResources.ColorGradingLut;
		rasterRenderGraphBuilder.SetRenderAttachment(passData.LutTarget, 0, AccessFlags.WriteAll);
		rasterRenderGraphBuilder.SetRenderFunc(delegate(ColorGradingLutPassData data, RasterGraphContext context)
		{
			Blitter.BlitTexture(context.cmd, data.LutTarget, Vector2.one, data.Material, 0);
		});
	}
}
