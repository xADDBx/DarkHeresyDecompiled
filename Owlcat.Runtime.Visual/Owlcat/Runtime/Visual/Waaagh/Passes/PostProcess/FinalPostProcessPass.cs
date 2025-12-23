using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.PipelineResources;
using Owlcat.Runtime.Visual.Waaagh.PostProcess;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.PostProcess;

public class FinalPostProcessPass : ScriptableRenderPass
{
	private class Materials : MaterialCollection<PostProcessRuntimeShaders>
	{
		public Material FinalPass;

		public Material ScalingSetup;

		public Material Easu;

		public override void Init(PostProcessRuntimeShaders resources)
		{
			FinalPass = Load(resources.FinalPostPassPS);
			ScalingSetup = Load(resources.ScalingSetupPS);
			Easu = Load(resources.EasuPS);
		}
	}

	public struct FinalBlitSettings
	{
		public bool isFxaaEnabled;

		public bool isFsrEnabled;

		public bool isTaaSharpeningEnabled;

		public bool requireHDROutput;

		public bool resolveToDebugScreen;

		public bool isAlphaOutputEnabled;

		public HDROutputUtils.Operation hdrOperations;

		public static FinalBlitSettings Create()
		{
			FinalBlitSettings result = default(FinalBlitSettings);
			result.isFxaaEnabled = false;
			result.isFsrEnabled = false;
			result.isTaaSharpeningEnabled = false;
			result.requireHDROutput = false;
			result.resolveToDebugScreen = false;
			result.isAlphaOutputEnabled = false;
			result.hdrOperations = HDROutputUtils.Operation.None;
			return result;
		}
	}

	private class PostProcessingFinalSetupPassData
	{
		internal TextureHandle destinationTexture;

		internal TextureHandle sourceTexture;

		internal Material material;

		internal WaaaghCameraData cameraData;
	}

	private class PostProcessingFinalFSRScalePassData
	{
		internal TextureHandle destinationTexture;

		internal TextureHandle sourceTexture;

		internal Material material;

		internal bool enableAlphaOutput;
	}

	private class PostProcessingFinalBlitPassData
	{
		internal TextureHandle destinationTexture;

		internal TextureHandle sourceTexture;

		internal Material material;

		internal WaaaghCameraData cameraData;

		internal FinalBlitSettings settings;
	}

	private MaterialLibrary<Materials, PostProcessRuntimeShaders> m_MaterialLibrary;

	private Materials m_Materials;

	private PostProcessResources m_Resources;

	private Tonemapping m_Tonemapping;

	private FilmGrain m_FilmGrain;

	private bool m_EnableColorEncodingIfNeeded;

	private int m_DitheringTextureIndex;

	private Material m_BlitMaterial;

	public override string Name => "FinalPostProcessPass";

	public FinalPostProcessPass(RenderPassEvent evt, PostProcessResources resources, Material blitMaterial)
		: base(evt)
	{
		m_Resources = resources;
		m_MaterialLibrary = new MaterialLibrary<Materials, PostProcessRuntimeShaders>(resources.Shaders);
		m_BlitMaterial = blitMaterial;
	}

	public void Cleanup()
	{
		m_MaterialLibrary.Cleanup();
	}

	public override void RecordRenderGraph(ContextContainer frameData)
	{
		VolumeStack stack = VolumeManager.instance.stack;
		m_Tonemapping = stack.GetComponent<Tonemapping>();
		m_FilmGrain = stack.GetComponent<FilmGrain>();
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		RenderGraph renderGraph = frameData.Get<WaaaghRenderingData>().RenderGraph;
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		m_Materials = m_MaterialLibrary.Get(waaaghCameraData.camera);
		Material finalPass = m_Materials.FinalPass;
		finalPass.shaderKeywords = null;
		FinalBlitSettings settings = FinalBlitSettings.Create();
		ProfilingSampler sampler = ProfilingSampler.Get(WaaaghProfileId.RenderPostProcessFinal);
		renderGraph.BeginProfilingSampler(sampler, ".\\Library\\PackageCache\\com.owlcat.visual@141c9a01de77\\Runtime\\Waaagh\\Passes\\PostProcess\\FinalPostProcessPass.cs", 117);
		m_EnableColorEncodingIfNeeded = true;
		if (m_FilmGrain.IsActive())
		{
			finalPass.EnableKeyword(ShaderKeywordStrings.FilmGrain);
			PostProcessUtils.ConfigureFilmGrain(m_Resources, m_FilmGrain, waaaghCameraData.pixelWidth, waaaghCameraData.pixelHeight, finalPass);
		}
		if (waaaghCameraData.isDitheringEnabled)
		{
			finalPass.EnableKeyword(ShaderKeywordStrings.Dithering);
			m_DitheringTextureIndex = PostProcessUtils.ConfigureDithering(m_Resources, m_DitheringTextureIndex, waaaghCameraData.pixelWidth, waaaghCameraData.pixelHeight, finalPass);
		}
		if (RequireSRGBConversionBlitToBackBuffer(waaaghCameraData.requireSrgbConversion))
		{
			finalPass.EnableKeyword("_LINEAR_TO_SRGB_CONVERSION");
		}
		settings.hdrOperations = HDROutputUtils.Operation.None;
		settings.requireHDROutput = RequireHDROutput(waaaghCameraData);
		if (settings.requireHDROutput)
		{
			settings.hdrOperations = (m_EnableColorEncodingIfNeeded ? HDROutputUtils.Operation.ColorEncoding : HDROutputUtils.Operation.None);
			if (!waaaghCameraData.postProcessEnabled)
			{
				settings.hdrOperations |= HDROutputUtils.Operation.ColorConversion;
			}
			SetupHDROutput(waaaghCameraData.hdrDisplayInformation, waaaghCameraData.hdrDisplayColorGamut, finalPass, settings.hdrOperations);
		}
		settings.isAlphaOutputEnabled = waaaghCameraData.isAlphaOutputEnabled;
		settings.isFxaaEnabled = waaaghCameraData.antialiasing == AntialiasingMode.FastApproximateAntialiasing;
		settings.isFsrEnabled = waaaghCameraData.imageScalingMode == ImageScalingMode.Upscaling && waaaghCameraData.upscalingFilter == ImageUpscalingFilter.FSR;
		settings.isTaaSharpeningEnabled = waaaghCameraData.IsTemporalAAEnabled() && waaaghCameraData.taaSettings.contrastAdaptiveSharpening > 0f && !settings.isFsrEnabled && !waaaghCameraData.IsSTPEnabled();
		RenderTextureDescriptor cameraTargetDescriptor = waaaghCameraData.cameraTargetDescriptor;
		cameraTargetDescriptor.msaaSamples = 1;
		cameraTargetDescriptor.depthBufferBits = 0;
		if (!settings.requireHDROutput)
		{
			cameraTargetDescriptor.graphicsFormat = WaaaghPipeline.MakeUnormRenderTextureGraphicsFormat();
		}
		TextureHandle destination = WaaaghRenderer.CreateRenderGraphTexture(renderGraph, cameraTargetDescriptor, "scalingSetupTarget", clear: true);
		RenderTextureDescriptor desc = cameraTargetDescriptor;
		desc.width = waaaghCameraData.pixelWidth;
		desc.height = waaaghCameraData.pixelHeight;
		TextureHandle destination2 = WaaaghRenderer.CreateRenderGraphTexture(renderGraph, desc, "_UpscaledTexture", clear: true);
		TextureHandle source = waaaghResourceData.CameraColorBuffer;
		if (waaaghCameraData.imageScalingMode != 0)
		{
			if (settings.isFxaaEnabled || settings.isFsrEnabled)
			{
				RenderFinalSetup(renderGraph, waaaghCameraData, in source, in destination, ref settings);
				source = destination;
				settings.isFxaaEnabled = false;
			}
			switch (waaaghCameraData.imageScalingMode)
			{
			case ImageScalingMode.Upscaling:
				switch (waaaghCameraData.upscalingFilter)
				{
				case ImageUpscalingFilter.Point:
					if (!settings.isTaaSharpeningEnabled)
					{
						finalPass.EnableKeyword("_POINT_SAMPLING");
					}
					break;
				case ImageUpscalingFilter.FSR:
					RenderFinalFSRScale(renderGraph, in source, in destination2, settings.isAlphaOutputEnabled);
					source = destination2;
					break;
				case ImageUpscalingFilter.STP:
					source = waaaghResourceData.CameraNonScaledColorBuffer;
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
		TextureHandle postProcessingTarget = waaaghResourceData.CameraColorBuffer;
		TextureHandle overlayUITexture = waaaghResourceData.OverlayUITexture;
		RenderFinalBlit(renderGraph, waaaghCameraData, in source, in overlayUITexture, in postProcessingTarget, ref settings);
		renderGraph.EndProfilingSampler(sampler, ".\\Library\\PackageCache\\com.owlcat.visual@141c9a01de77\\Runtime\\Waaagh\\Passes\\PostProcess\\FinalPostProcessPass.cs", 262);
	}

	public void RenderFinalSetup(RenderGraph renderGraph, WaaaghCameraData cameraData, in TextureHandle source, in TextureHandle destination, ref FinalBlitSettings settings)
	{
		PostProcessingFinalSetupPassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<PostProcessingFinalSetupPassData>("Postprocessing Final Setup Pass", out passData, ProfilingSampler.Get(WaaaghProfileId.FinalSetup), ".\\Library\\PackageCache\\com.owlcat.visual@141c9a01de77\\Runtime\\Waaagh\\Passes\\PostProcess\\FinalPostProcessPass.cs", 276);
		Material scalingSetup = m_Materials.ScalingSetup;
		if (settings.isFxaaEnabled)
		{
			scalingSetup.EnableKeyword(ShaderKeywordStrings.Fxaa);
		}
		if (settings.isFsrEnabled)
		{
			scalingSetup.EnableKeyword(settings.hdrOperations.HasFlag(HDROutputUtils.Operation.ColorEncoding) ? "_GAMMA_20_AND_HDR_INPUT" : "_GAMMA_20");
		}
		if (settings.hdrOperations.HasFlag(HDROutputUtils.Operation.ColorEncoding))
		{
			SetupHDROutput(cameraData.hdrDisplayInformation, cameraData.hdrDisplayColorGamut, scalingSetup, settings.hdrOperations);
		}
		if (settings.isAlphaOutputEnabled)
		{
			CoreUtils.SetKeyword(scalingSetup, "_ENABLE_ALPHA_OUTPUT", settings.isAlphaOutputEnabled);
		}
		rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
		passData.destinationTexture = destination;
		rasterRenderGraphBuilder.SetRenderAttachment(destination, 0);
		passData.sourceTexture = source;
		rasterRenderGraphBuilder.UseTexture(in source);
		passData.cameraData = cameraData;
		passData.material = scalingSetup;
		rasterRenderGraphBuilder.SetRenderFunc(delegate(PostProcessingFinalSetupPassData data, RasterGraphContext context)
		{
			RasterCommandBuffer cmd = context.cmd;
			RTHandle rTHandle = data.sourceTexture;
			PostProcessUtils.SetSourceSize(cmd, rTHandle);
			ScaleViewportAndBlit(context.cmd, rTHandle, data.destinationTexture, data.cameraData, data.material);
		});
	}

	public void RenderFinalFSRScale(RenderGraph renderGraph, in TextureHandle source, in TextureHandle destination, bool enableAlphaOutput)
	{
		m_Materials.Easu.shaderKeywords = null;
		PostProcessingFinalFSRScalePassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<PostProcessingFinalFSRScalePassData>("Postprocessing Final FSR Scale Pass", out passData, ProfilingSampler.Get(WaaaghProfileId.FinalFSRScale), ".\\Library\\PackageCache\\com.owlcat.visual@141c9a01de77\\Runtime\\Waaagh\\Passes\\PostProcess\\FinalPostProcessPass.cs", 326);
		rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
		passData.destinationTexture = destination;
		rasterRenderGraphBuilder.SetRenderAttachment(destination, 0);
		passData.sourceTexture = source;
		rasterRenderGraphBuilder.UseTexture(in source);
		passData.material = m_Materials.Easu;
		passData.enableAlphaOutput = enableAlphaOutput;
		rasterRenderGraphBuilder.SetRenderFunc(delegate(PostProcessingFinalFSRScalePassData data, RasterGraphContext context)
		{
			RasterCommandBuffer cmd = context.cmd;
			TextureHandle sourceTexture = data.sourceTexture;
			TextureHandle destinationTexture = data.destinationTexture;
			Material material = data.material;
			bool enableAlphaOutput2 = data.enableAlphaOutput;
			RTHandle rTHandle = sourceTexture;
			RTHandle rTHandle2 = destinationTexture;
			Vector2 vector = new Vector2(rTHandle.referenceSize.x, rTHandle.referenceSize.y);
			Vector2 outputImageSizeInPixels = new Vector2(rTHandle2.referenceSize.x, rTHandle2.referenceSize.y);
			FSRUtils.SetEasuConstants(cmd, vector, vector, outputImageSizeInPixels);
			CoreUtils.SetKeyword(material, "_ENABLE_ALPHA_OUTPUT", enableAlphaOutput2);
			Vector2 vector2 = (rTHandle.useScaling ? new Vector2(rTHandle.rtHandleProperties.rtHandleScale.x, rTHandle.rtHandleProperties.rtHandleScale.y) : Vector2.one);
			Blitter.BlitTexture(cmd, rTHandle, vector2, material, 0);
		});
	}

	public void RenderFinalBlit(RenderGraph renderGraph, WaaaghCameraData cameraData, in TextureHandle source, in TextureHandle overlayUITexture, in TextureHandle postProcessingTarget, ref FinalBlitSettings settings)
	{
		bool num = PostProcessPass.EqualsValueType(source, postProcessingTarget);
		TextureHandle input = source;
		if (num)
		{
			RenderTextureDescriptor compatibleDescriptor = GetCompatibleDescriptor(cameraData.cameraTargetDescriptor, cameraData.cameraTargetDescriptor.width, cameraData.cameraTargetDescriptor.height, cameraData.cameraTargetDescriptor.graphicsFormat);
			TextureHandle textureHandle = WaaaghRenderer.CreateRenderGraphTexture(renderGraph, compatibleDescriptor, "FinalPostSwapBuffer", clear: false, FilterMode.Bilinear);
			PostProcessingFinalBlitPassData passData;
			using (IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<PostProcessingFinalBlitPassData>("Postprocessing Final Blit Swap Copy", out passData, ".\\Library\\PackageCache\\com.owlcat.visual@141c9a01de77\\Runtime\\Waaagh\\Passes\\PostProcess\\FinalPostProcessPass.cs", 382))
			{
				rasterRenderGraphBuilder.UseTexture(in source);
				passData.sourceTexture = source;
				rasterRenderGraphBuilder.SetRenderAttachment(textureHandle, 0);
				passData.destinationTexture = textureHandle;
				passData.cameraData = cameraData;
				passData.material = m_BlitMaterial;
				rasterRenderGraphBuilder.SetRenderFunc(delegate(PostProcessingFinalBlitPassData data, RasterGraphContext context)
				{
					ScaleViewportAndBlit(context.cmd, data.sourceTexture, data.destinationTexture, data.cameraData, data.material);
				});
			}
			input = textureHandle;
		}
		PostProcessingFinalBlitPassData passData2;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder2 = renderGraph.AddRasterRenderPass<PostProcessingFinalBlitPassData>("Postprocessing Final Blit Pass", out passData2, ProfilingSampler.Get(WaaaghProfileId.FinalPostBlit), ".\\Library\\PackageCache\\com.owlcat.visual@141c9a01de77\\Runtime\\Waaagh\\Passes\\PostProcess\\FinalPostProcessPass.cs", 400);
		rasterRenderGraphBuilder2.AllowGlobalStateModification(value: true);
		passData2.destinationTexture = postProcessingTarget;
		rasterRenderGraphBuilder2.SetRenderAttachment(postProcessingTarget, 0);
		passData2.sourceTexture = input;
		rasterRenderGraphBuilder2.UseTexture(in input);
		passData2.cameraData = cameraData;
		passData2.material = m_Materials.FinalPass;
		passData2.settings = settings;
		if (settings.requireHDROutput && m_EnableColorEncodingIfNeeded)
		{
			rasterRenderGraphBuilder2.UseTexture(in overlayUITexture);
		}
		rasterRenderGraphBuilder2.SetRenderFunc(delegate(PostProcessingFinalBlitPassData data, RasterGraphContext context)
		{
			RasterCommandBuffer cmd = context.cmd;
			Material material = data.material;
			bool isFxaaEnabled = data.settings.isFxaaEnabled;
			bool isFsrEnabled = data.settings.isFsrEnabled;
			bool isTaaSharpeningEnabled = data.settings.isTaaSharpeningEnabled;
			bool requireHDROutput = data.settings.requireHDROutput;
			bool resolveToDebugScreen = data.settings.resolveToDebugScreen;
			bool isAlphaOutputEnabled = data.settings.isAlphaOutputEnabled;
			RTHandle rTHandle = data.sourceTexture;
			_ = (RTHandle)data.destinationTexture;
			PostProcessUtils.SetSourceSize(cmd, data.sourceTexture);
			if (isFxaaEnabled)
			{
				material.EnableKeyword(ShaderKeywordStrings.Fxaa);
			}
			if (isFsrEnabled)
			{
				float sharpnessLinear = (data.cameraData.fsrOverrideSharpness ? data.cameraData.fsrSharpness : 0.92f);
				if (data.cameraData.fsrSharpness > 0f)
				{
					material.EnableKeyword(requireHDROutput ? "_EASU_RCAS_AND_HDR_INPUT" : "_RCAS");
					FSRUtils.SetRcasConstantsLinear(cmd, sharpnessLinear);
				}
			}
			else if (isTaaSharpeningEnabled)
			{
				material.EnableKeyword("_RCAS");
				FSRUtils.SetRcasConstantsLinear(cmd, data.cameraData.taaSettings.contrastAdaptiveSharpening);
			}
			if (isAlphaOutputEnabled)
			{
				CoreUtils.SetKeyword(material, "_ENABLE_ALPHA_OUTPUT", isAlphaOutputEnabled);
			}
			int num2 = 0 & ((!resolveToDebugScreen) ? 1 : 0);
			Vector2 vector = (rTHandle.useScaling ? new Vector2(rTHandle.rtHandleProperties.rtHandleScale.x, rTHandle.rtHandleProperties.rtHandleScale.y) : Vector2.one);
			Vector4 scaleBias = ((num2 != 0 && data.cameraData.targetTexture == null && SystemInfo.graphicsUVStartsAtTop) ? new Vector4(vector.x, 0f - vector.y, 0f, vector.y) : new Vector4(vector.x, vector.y, 0f, 0f));
			cmd.SetViewport(data.cameraData.pixelRect);
			Blitter.BlitTexture(cmd, rTHandle, scaleBias, material, 0);
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

	private void SetupHDROutput(HDROutputUtils.HDRDisplayInformation hdrDisplayInformation, ColorGamut hdrDisplayColorGamut, Material material, HDROutputUtils.Operation hdrOperations)
	{
		WaaaghPipeline.GetHDROutputLuminanceParameters(hdrDisplayInformation, hdrDisplayColorGamut, m_Tonemapping, out var hdrOutputParameters);
		material.SetVector(ShaderPropertyId._HDROutputLuminanceParams, hdrOutputParameters);
		HDROutputUtils.ConfigureHDROutput(material, hdrDisplayColorGamut, hdrOperations);
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
}
