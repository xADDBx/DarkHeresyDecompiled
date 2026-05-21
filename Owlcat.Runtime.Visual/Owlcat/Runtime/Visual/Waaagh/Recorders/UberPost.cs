using System;
using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.PostProcess;
using Owlcat.Runtime.Visual.Waaagh.Recorders.PostProcess;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders;

public static class UberPost
{
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

	private class UberPostPassData
	{
		internal TextureHandle destinationTexture;

		internal TextureHandle sourceTexture;

		internal TextureHandle lutTexture;

		internal Vector4 lutParams;

		internal TextureHandle userLutTexture;

		internal Vector4 userLutParams;

		internal Material material;

		internal PostProcessResources resources;

		internal Tonemapping tonemapping;

		internal TonemappingMode toneMappingMode;

		internal bool isHdrGrading;

		internal bool isBackbuffer;

		internal bool enableAlphaOutput;

		internal WaaaghCameraData CameraData;
	}

	public static void SetupBloomEnhancedPass(PostProcessor postProcessor, RenderGraph rendergraph, in TextureHandle bloomTexture)
	{
		UberSetupBloomPassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = rendergraph.AddRasterRenderPass<UberSetupBloomPassData>("UberPost - UberPostSetupBloomPass", out passData, WaaaghProfileId.UberPostSetupBloomPass.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\PostProcess\\UberPost.cs", 29);
		RenderTextureDescriptor descriptor = postProcessor.FrameState.Descriptor;
		BloomEnhanced bloomEnhanced = postProcessor.Overrides.BloomEnhanced;
		Color color = bloomEnhanced.tint.value.linear;
		float num = ColorUtils.Luminance(in color);
		color = ((num > 0f) ? (color * (1f / num)) : Color.white);
		Vector4 bloomParams = new Vector4(bloomEnhanced.intensity.value, color.r, color.g, color.b);
		Texture texture = ((bloomEnhanced.dirtTexture.value == null) ? Texture2D.blackTexture : bloomEnhanced.dirtTexture.value);
		float num2 = (float)texture.width / (float)texture.height;
		float num3 = (float)descriptor.width / (float)descriptor.height;
		Vector4 dirtScaleOffset = new Vector4(1f, 1f, 0f, 0f);
		float value = bloomEnhanced.dirtIntensity.value;
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
		passData.useRGBM = postProcessor.StaticState.DefaultColorFormatUseRGBM;
		passData.bloomTexture = bloomTexture;
		rasterRenderGraphBuilder.UseTexture(in bloomTexture);
		passData.uberMaterial = postProcessor.MatLib.Uber;
		rasterRenderGraphBuilder.AllowPassCulling(value: false);
		rasterRenderGraphBuilder.SetRenderFunc(delegate(UberSetupBloomPassData data, RasterGraphContext context)
		{
			Material uberMaterial = data.uberMaterial;
			uberMaterial.SetVector(PostProcessor.ShaderIDs._Bloom_Params, data.bloomParams);
			uberMaterial.SetFloat(PostProcessor.ShaderIDs._Bloom_RGBM, data.useRGBM ? 1f : 0f);
			uberMaterial.SetVector(PostProcessor.ShaderIDs._LensDirt_Params, data.dirtScaleOffset);
			uberMaterial.SetFloat(PostProcessor.ShaderIDs._LensDirt_Intensity, data.dirtIntensity);
			uberMaterial.SetTexture(PostProcessor.ShaderIDs._LensDirt_Texture, data.dirtTexture);
			if (data.highQualityFilteringValue)
			{
				uberMaterial.EnableKeyword((data.dirtIntensity > 0f) ? ShaderKeywordStrings.BloomHQDirt : ShaderKeywordStrings.BloomHQ);
			}
			else
			{
				uberMaterial.EnableKeyword((data.dirtIntensity > 0f) ? ShaderKeywordStrings.BloomLQDirt : ShaderKeywordStrings.BloomLQ);
			}
			uberMaterial.SetTexture(PostProcessor.ShaderIDs._Bloom_Texture, data.bloomTexture);
		});
	}

	internal static void SetupBloomPass(PostProcessor postProcessor, RenderGraph rendergraph, in TextureHandle bloomTexture)
	{
		UberSetupBloomPassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = rendergraph.AddRasterRenderPass<UberSetupBloomPassData>("UberPost - UberPostSetupBloomPass", out passData, WaaaghProfileId.UberPostSetupBloomPass.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\PostProcess\\UberPost.cs", 96);
		Bloom bloom = postProcessor.Overrides.Bloom;
		RenderTextureDescriptor descriptor = postProcessor.FrameState.Descriptor;
		Color color = bloom.tint.value.linear;
		float num = ColorUtils.Luminance(in color);
		color = ((num > 0f) ? (color * (1f / num)) : Color.white);
		Vector4 bloomParams = new Vector4(bloom.intensity.value, color.r, color.g, color.b);
		Texture texture = ((bloom.dirtTexture.value == null) ? Texture2D.blackTexture : bloom.dirtTexture.value);
		float num2 = (float)texture.width / (float)texture.height;
		float num3 = (float)descriptor.width / (float)descriptor.height;
		Vector4 dirtScaleOffset = new Vector4(1f, 1f, 0f, 0f);
		float value = bloom.dirtIntensity.value;
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
		passData.highQualityFilteringValue = bloom.highQualityFiltering.value;
		passData.useRGBM = postProcessor.StaticState.DefaultColorFormatUseRGBM;
		passData.bloomTexture = bloomTexture;
		rasterRenderGraphBuilder.UseTexture(in bloomTexture);
		passData.uberMaterial = postProcessor.MatLib.Uber;
		rasterRenderGraphBuilder.AllowPassCulling(value: false);
		rasterRenderGraphBuilder.SetRenderFunc(delegate(UberSetupBloomPassData data, RasterGraphContext context)
		{
			Material uberMaterial = data.uberMaterial;
			uberMaterial.SetVector(PostProcessor.ShaderIDs._Bloom_Params, data.bloomParams);
			uberMaterial.SetFloat(PostProcessor.ShaderIDs._Bloom_RGBM, data.useRGBM ? 1f : 0f);
			uberMaterial.SetVector(PostProcessor.ShaderIDs._LensDirt_Params, data.dirtScaleOffset);
			uberMaterial.SetFloat(PostProcessor.ShaderIDs._LensDirt_Intensity, data.dirtIntensity);
			uberMaterial.SetTexture(PostProcessor.ShaderIDs._LensDirt_Texture, data.dirtTexture);
			if (data.highQualityFilteringValue)
			{
				uberMaterial.EnableKeyword((data.dirtIntensity > 0f) ? ShaderKeywordStrings.BloomHQDirt : ShaderKeywordStrings.BloomHQ);
			}
			else
			{
				uberMaterial.EnableKeyword((data.dirtIntensity > 0f) ? ShaderKeywordStrings.BloomLQDirt : ShaderKeywordStrings.BloomLQ);
			}
			uberMaterial.SetTexture(PostProcessor.ShaderIDs._Bloom_Texture, data.bloomTexture);
		});
	}

	public static void SetupLensDistortion(PostProcessor processor, bool isSceneView)
	{
		Material uber = processor.MatLib.Uber;
		LensDistortion lensDistortion = processor.Overrides.LensDistortion;
		float b = 1.6f * Mathf.Max(Mathf.Abs(lensDistortion.intensity.value * 100f), 1f);
		float num = MathF.PI / 180f * Mathf.Min(160f, b);
		float y = 2f * Mathf.Tan(num * 0.5f);
		Vector2 vector = lensDistortion.center.value * 2f - Vector2.one;
		Vector4 value = new Vector4(vector.x, vector.y, Mathf.Max(lensDistortion.xMultiplier.value, 0.0001f), Mathf.Max(lensDistortion.yMultiplier.value, 0.0001f));
		Vector4 value2 = new Vector4((lensDistortion.intensity.value >= 0f) ? num : (1f / num), y, 1f / lensDistortion.scale.value, lensDistortion.intensity.value * 100f);
		uber.SetVector(PostProcessor.ShaderIDs._Distortion_Params1, value);
		uber.SetVector(PostProcessor.ShaderIDs._Distortion_Params2, value2);
		if (lensDistortion.IsActive() && !isSceneView)
		{
			uber.EnableKeyword(ShaderKeywordStrings.Distortion);
		}
	}

	internal static void SetupChromaticAberration(PostProcessor postProcessor)
	{
		Material uber = postProcessor.MatLib.Uber;
		ChromaticAberration chromaticAberration = postProcessor.Overrides.ChromaticAberration;
		uber.SetFloat(PostProcessor.ShaderIDs._Chroma_Params, chromaticAberration.intensity.value * 0.05f);
		if (chromaticAberration.IsActive())
		{
			uber.EnableKeyword(ShaderKeywordStrings.ChromaticAberration);
		}
	}

	internal static void SetupVignette(PostProcessor postProcessor)
	{
		Vignette vignette = postProcessor.Overrides.Vignette;
		RenderTextureDescriptor descriptor = postProcessor.FrameState.Descriptor;
		Material uber = postProcessor.MatLib.Uber;
		Color value = vignette.color.value;
		Vector2 value2 = vignette.center.value;
		float num = (float)descriptor.width / (float)descriptor.height;
		Vector4 value3 = new Vector4(value.r, value.g, value.b, vignette.rounded.value ? num : 1f);
		Vector4 value4 = new Vector4(value2.x, value2.y, vignette.intensity.value * 3f, vignette.smoothness.value * 5f);
		uber.SetVector(PostProcessor.ShaderIDs._Vignette_Params1, value3);
		uber.SetVector(PostProcessor.ShaderIDs._Vignette_Params2, value4);
	}

	internal static void SetupGrain(PostProcessor postProcessor, WaaaghCameraData cameraData)
	{
		FilmGrain filmGrain = postProcessor.Overrides.FilmGrain;
		Material uber = postProcessor.MatLib.Uber;
		if (!postProcessor.FrameState.HasFinalPass && filmGrain.IsActive())
		{
			uber.EnableKeyword(ShaderKeywordStrings.FilmGrain);
			PostProcessUtils.ConfigureFilmGrain(postProcessor.Resources, filmGrain, cameraData.pixelWidth, cameraData.pixelHeight, uber);
		}
	}

	internal static void SetupDithering(PostProcessor postProcessor, WaaaghCameraData cameraData)
	{
		if (!postProcessor.FrameState.HasFinalPass && cameraData.isDitheringEnabled)
		{
			Material uber = postProcessor.MatLib.Uber;
			uber.EnableKeyword(ShaderKeywordStrings.Dithering);
			postProcessor.DitheringTextureIndex = PostProcessUtils.ConfigureDithering(postProcessor.Resources, postProcessor.DitheringTextureIndex, cameraData.pixelWidth, cameraData.pixelHeight, uber);
		}
	}

	public static void Render(PostProcessor postProcessor, RenderGraph renderGraph, WaaaghCameraData cameraData, in TextureHandle dest)
	{
		TextureHandle input = postProcessor.CameraStackTargets.CurrentPostProcessSource;
		Material uber = postProcessor.MatLib.Uber;
		bool isHdrGrading = postProcessor.FrameState.ColorGradingMode == ColorGradingMode.HighDynamicRange;
		int lutSize = postProcessor.FrameState.LutSize;
		int num = lutSize * lutSize;
		float w = Mathf.Pow(2f, postProcessor.Overrides.ColorAdjustments.postExposure.value);
		Vector4 lutParams = new Vector4(1f / (float)num, 1f / (float)lutSize, (float)lutSize - 1f, w);
		ColorLookup colorLookup = postProcessor.Overrides.ColorLookup;
		RTHandle rTHandle = (colorLookup.texture.value ? RTHandles.Alloc(colorLookup.texture.value) : null);
		TextureHandle input2 = ((rTHandle != null) ? renderGraph.ImportTexture(rTHandle) : TextureHandle.nullHandle);
		Vector4 userLutParams = ((!colorLookup.IsActive()) ? Vector4.zero : new Vector4(1f / (float)colorLookup.texture.value.width, 1f / (float)colorLookup.texture.value.height, (float)colorLookup.texture.value.height - 1f, colorLookup.contribution.value));
		UberPostPassData passData;
		using (IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<UberPostPassData>("Postprocessing Uber Post Pass", out passData, WaaaghProfileId.UberPost.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\PostProcess\\UberPost.cs", 296))
		{
			rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
			passData.destinationTexture = dest;
			rasterRenderGraphBuilder.SetRenderAttachment(dest, 0);
			passData.sourceTexture = input;
			rasterRenderGraphBuilder.UseTexture(in input);
			passData.lutTexture = postProcessor.GpuResources.ColorGradingLut;
			rasterRenderGraphBuilder.UseTexture(in passData.lutTexture);
			passData.lutParams = lutParams;
			if (input2.IsValid())
			{
				passData.userLutTexture = input2;
				rasterRenderGraphBuilder.UseTexture(in input2);
			}
			if (postProcessor.Overrides.Bloom.IsActive())
			{
				rasterRenderGraphBuilder.UseTexture(in postProcessor.StaticState.BloomMipUp[0]);
			}
			passData.userLutParams = userLutParams;
			passData.CameraData = cameraData;
			passData.resources = postProcessor.Resources;
			passData.tonemapping = postProcessor.Overrides.Tonemapping;
			passData.material = uber;
			passData.toneMappingMode = passData.tonemapping.mode.value;
			passData.isHdrGrading = isHdrGrading;
			passData.enableAlphaOutput = cameraData.isAlphaOutputEnabled;
			rasterRenderGraphBuilder.SetRenderFunc(delegate(UberPostPassData data, RasterGraphContext context)
			{
				RasterCommandBuffer cmd = context.cmd;
				Material material = data.material;
				RTHandle sourceTextureHdl = data.sourceTexture;
				material.SetTexture(PostProcessor.ShaderIDs._InternalLut, data.lutTexture);
				material.SetVector(PostProcessor.ShaderIDs._Lut_Params, data.lutParams);
				material.SetTexture(PostProcessor.ShaderIDs._UserLut, data.userLutTexture);
				material.SetVector(PostProcessor.ShaderIDs._UserLut_Params, data.userLutParams);
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
				PostProcessor.ScaleViewportAndBlit(cmd, sourceTextureHdl, data.destinationTexture, data.CameraData, material);
			});
		}
		postProcessor.CameraStackTargets.SetCurrentPostProcessSource(dest);
	}
}
