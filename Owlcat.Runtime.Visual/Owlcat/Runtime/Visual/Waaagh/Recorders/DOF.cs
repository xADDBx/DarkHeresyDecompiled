using System;
using System.Runtime.CompilerServices;
using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.PostProcess;
using Owlcat.Runtime.Visual.Waaagh.Recorders.PostProcess;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders;

public static class DOF
{
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

	private static int s_BokehHash;

	private static float s_BokehMaxRadius;

	private static float s_BokehRCPAspect;

	private static Vector4[] s_BokehKernel;

	public static void Render(PostProcessor processor, RenderGraph renderGraph, WaaaghCameraData cameraData, in TextureHandle cameraDepth)
	{
		DepthOfField depthOfField = processor.Overrides.DepthOfField;
		RenderTextureDescriptor descriptor = processor.FrameState.Descriptor;
		Material dofMaterial = ((depthOfField.mode.value == DepthOfFieldMode.Gaussian) ? processor.MatLib.GaussianDepthOfField : processor.MatLib.BokehDepthOfField);
		RenderTextureDescriptor compatibleDescriptor = PostProcessor.GetCompatibleDescriptor(descriptor, descriptor.width, descriptor.height, descriptor.graphicsFormat);
		TextureHandle source = processor.CameraStackTargets.CurrentPostProcessSource;
		TextureHandle dest = RenderGraphUtility.CreateRenderGraphTexture(renderGraph, compatibleDescriptor, "_DoFTarget", clear: true, FilterMode.Bilinear);
		CoreUtils.SetKeyword(dofMaterial, "_ENABLE_ALPHA_OUTPUT", cameraData.isAlphaOutputEnabled);
		if (depthOfField.mode.value == DepthOfFieldMode.Gaussian)
		{
			RenderDoFGaussian(processor, renderGraph, in cameraDepth, cameraData, in source, in dest, ref dofMaterial);
		}
		else if (depthOfField.mode.value == DepthOfFieldMode.Bokeh)
		{
			RenderDoFBokeh(processor, renderGraph, in cameraDepth, cameraData, in source, in dest, ref dofMaterial);
		}
		processor.CameraStackTargets.SetCurrentPostProcessSource(dest);
	}

	private static void RenderDoFGaussian(PostProcessor processor, RenderGraph renderGraph, in TextureHandle cameraDepth, WaaaghCameraData cameraData, in TextureHandle source, in TextureHandle dest, ref Material dofMaterial)
	{
		RenderTextureDescriptor descriptor = processor.FrameState.Descriptor;
		DepthOfField depthOfField = processor.Overrides.DepthOfField;
		int num = 2;
		Material material = dofMaterial;
		int num2 = descriptor.width / num;
		int height = descriptor.height / num;
		DoFGaussianSetupPassData passData;
		using (IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<DoFGaussianSetupPassData>("Setup DoF passes", out passData, WaaaghProfileId.SetupDoF.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\PostProcess\\DOF.cs", 73))
		{
			float value = depthOfField.gaussianStart.value;
			float y = Mathf.Max(value, depthOfField.gaussianEnd.value);
			float a = depthOfField.gaussianMaxRadius.value * ((float)num2 / 1080f);
			a = Mathf.Min(a, 2f);
			passData.source = source;
			passData.downSample = num;
			passData.cocParams = new Vector3(value, y, a);
			passData.highQualitySamplingValue = depthOfField.highQualitySampling.value;
			passData.material = material;
			passData.materialCoC = processor.MatLib.GaussianDepthOfFieldCoC;
			rasterRenderGraphBuilder.AllowPassCulling(value: false);
			rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
			rasterRenderGraphBuilder.SetRenderFunc(delegate(DoFGaussianSetupPassData data, RasterGraphContext context)
			{
				RasterCommandBuffer cmd6 = context.cmd;
				Material material7 = data.material;
				material7.SetVector(PostProcessor.ShaderIDs._CoCParams, data.cocParams);
				CoreUtils.SetKeyword(material7, ShaderKeywordStrings.HighQualitySampling, data.highQualitySamplingValue);
				Material materialCoC = data.materialCoC;
				materialCoC.SetVector(PostProcessor.ShaderIDs._CoCParams, data.cocParams);
				CoreUtils.SetKeyword(materialCoC, ShaderKeywordStrings.HighQualitySampling, data.highQualitySamplingValue);
				PostProcessUtils.SetSourceSize(cmd6, data.source);
				cmd6.SetGlobalVector(PostProcessor.ShaderIDs._DownSampleScaleFactor, new Vector4(1f / (float)data.downSample, 1f / (float)data.downSample, data.downSample, data.downSample));
			});
		}
		RenderTextureDescriptor compatibleDescriptor = PostProcessor.GetCompatibleDescriptor(descriptor, descriptor.width, descriptor.height, processor.StaticState.GaussianCoCFormat);
		TextureHandle input = RenderGraphUtility.CreateRenderGraphTexture(renderGraph, compatibleDescriptor, "_FullCoCTexture", clear: true, FilterMode.Bilinear);
		RenderTextureDescriptor compatibleDescriptor2 = PostProcessor.GetCompatibleDescriptor(descriptor, num2, height, processor.StaticState.GaussianCoCFormat);
		TextureHandle input2 = RenderGraphUtility.CreateRenderGraphTexture(renderGraph, compatibleDescriptor2, "_HalfCoCTexture", clear: true, FilterMode.Bilinear);
		RenderTextureDescriptor compatibleDescriptor3 = PostProcessor.GetCompatibleDescriptor(descriptor, num2, height, processor.StaticState.DefaultColorFormat);
		TextureHandle input3 = RenderGraphUtility.CreateRenderGraphTexture(renderGraph, compatibleDescriptor3, "_PingTexture", clear: true, FilterMode.Bilinear);
		RenderTextureDescriptor compatibleDescriptor4 = PostProcessor.GetCompatibleDescriptor(descriptor, num2, height, processor.StaticState.DefaultColorFormat);
		TextureHandle input4 = RenderGraphUtility.CreateRenderGraphTexture(renderGraph, compatibleDescriptor4, "_PongTexture", clear: true, FilterMode.Bilinear);
		DoFGaussianPassData passData2;
		using (IRasterRenderGraphBuilder rasterRenderGraphBuilder2 = renderGraph.AddRasterRenderPass<DoFGaussianPassData>("Depth of Field - Compute CoC", out passData2, WaaaghProfileId.DOFComputeCOC.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\PostProcess\\DOF.cs", 122))
		{
			rasterRenderGraphBuilder2.SetRenderAttachment(input, 0);
			passData2.sourceTexture = source;
			rasterRenderGraphBuilder2.UseTexture(in source);
			passData2.depthTexture = cameraDepth;
			rasterRenderGraphBuilder2.UseTexture(in cameraDepth);
			passData2.material = processor.MatLib.GaussianDepthOfFieldCoC;
			rasterRenderGraphBuilder2.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraDepthTexture);
			rasterRenderGraphBuilder2.SetRenderFunc(delegate(DoFGaussianPassData data, RasterGraphContext context)
			{
				Material material6 = data.material;
				RasterCommandBuffer cmd5 = context.cmd;
				RTHandle rTHandle5 = data.sourceTexture;
				material6.SetTexture(GlobalTextureShaderPropertyId._CameraDepthTexture, data.depthTexture);
				Vector2 vector5 = (rTHandle5.useScaling ? new Vector2(rTHandle5.rtHandleProperties.rtHandleScale.x, rTHandle5.rtHandleProperties.rtHandleScale.y) : Vector2.one);
				Blitter.BlitTexture(cmd5, rTHandle5, vector5, material6, 0);
			});
		}
		DoFGaussianPassData passData3;
		using (IRasterRenderGraphBuilder rasterRenderGraphBuilder3 = renderGraph.AddRasterRenderPass<DoFGaussianPassData>("Depth of Field - Downscale & Prefilter Color + CoC", out passData3, WaaaghProfileId.DOFDownscalePrefilter.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\PostProcess\\DOF.cs", 147))
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
				material5.SetTexture(PostProcessor.ShaderIDs._FullCoCTexture, data.cocTexture);
				Vector2 vector4 = (rTHandle4.useScaling ? new Vector2(rTHandle4.rtHandleProperties.rtHandleScale.x, rTHandle4.rtHandleProperties.rtHandleScale.y) : Vector2.one);
				Blitter.BlitTexture(cmd4, rTHandle4, vector4, material5, 1);
			});
		}
		DoFGaussianPassData passData4;
		using (IRasterRenderGraphBuilder rasterRenderGraphBuilder4 = renderGraph.AddRasterRenderPass<DoFGaussianPassData>("Depth of Field - Blur H", out passData4, WaaaghProfileId.DOFBlurH.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\PostProcess\\DOF.cs", 175))
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
				material4.SetTexture(PostProcessor.ShaderIDs._HalfCoCTexture, data.cocTexture);
				Vector2 vector3 = (rTHandle3.useScaling ? new Vector2(rTHandle3.rtHandleProperties.rtHandleScale.x, rTHandle3.rtHandleProperties.rtHandleScale.y) : Vector2.one);
				Blitter.BlitTexture(cmd3, rTHandle3, vector3, material4, 2);
			});
		}
		DoFGaussianPassData passData5;
		using (IRasterRenderGraphBuilder rasterRenderGraphBuilder5 = renderGraph.AddRasterRenderPass<DoFGaussianPassData>("Depth of Field - Blur V", out passData5, WaaaghProfileId.DOFBlurV.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\PostProcess\\DOF.cs", 198))
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
				material3.SetTexture(PostProcessor.ShaderIDs._HalfCoCTexture, data.cocTexture);
				Vector2 vector2 = (rTHandle2.useScaling ? new Vector2(rTHandle2.rtHandleProperties.rtHandleScale.x, rTHandle2.rtHandleProperties.rtHandleScale.y) : Vector2.one);
				Blitter.BlitTexture(cmd2, rTHandle2, vector2, material3, 3);
			});
		}
		DoFGaussianPassData passData6;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder6 = renderGraph.AddRasterRenderPass<DoFGaussianPassData>("Depth of Field - Composite", out passData6, WaaaghProfileId.DOFComposite.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\PostProcess\\DOF.cs", 221);
		rasterRenderGraphBuilder6.SetRenderAttachment(dest, 0);
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
			material2.SetTexture(PostProcessor.ShaderIDs._ColorTexture, data.colorTexture);
			material2.SetTexture(PostProcessor.ShaderIDs._FullCoCTexture, data.cocTexture);
			Vector2 vector = (rTHandle.useScaling ? new Vector2(rTHandle.rtHandleProperties.rtHandleScale.x, rTHandle.rtHandleProperties.rtHandleScale.y) : Vector2.one);
			Blitter.BlitTexture(cmd, rTHandle, vector, material2, 4);
		});
	}

	private static void RenderDoFBokeh(PostProcessor processor, RenderGraph renderGraph, in TextureHandle cameraDepth, WaaaghCameraData cameraData, in TextureHandle source, in TextureHandle dest, ref Material dofMaterial)
	{
		RenderTextureDescriptor descriptor = processor.FrameState.Descriptor;
		DepthOfField depthOfField = processor.Overrides.DepthOfField;
		int num = 2;
		Material material = dofMaterial;
		int num2 = descriptor.width / num;
		int num3 = descriptor.height / num;
		DoFBokehSetupPassData passData;
		using (IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<DoFBokehSetupPassData>("Setup DoF passes", out passData, WaaaghProfileId.SetupDoF.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\PostProcess\\DOF.cs", 285))
		{
			float num4 = depthOfField.focalLength.value / 1000f;
			float num5 = depthOfField.focalLength.value / depthOfField.aperture.value;
			float value = depthOfField.focusDistance.value;
			float y = num5 * num4 / (value - num4);
			float maxBokehRadiusInPixels = GetMaxBokehRadiusInPixels(descriptor.height);
			float num6 = 1f / ((float)num2 / (float)num3);
			int hashCode = depthOfField.GetHashCode();
			if (hashCode != s_BokehHash || maxBokehRadiusInPixels != s_BokehMaxRadius || num6 != s_BokehRCPAspect)
			{
				s_BokehHash = hashCode;
				s_BokehMaxRadius = maxBokehRadiusInPixels;
				s_BokehRCPAspect = num6;
				PrepareBokehKernel(maxBokehRadiusInPixels, num6, depthOfField);
			}
			float uvMargin = 1f / (float)descriptor.height * (float)num;
			passData.bokehKernel = s_BokehKernel;
			passData.source = source;
			passData.downSample = num;
			passData.uvMargin = uvMargin;
			passData.cocParams = new Vector4(value, y, maxBokehRadiusInPixels, num6);
			passData.useFastSRGBLinearConversion = processor.Settings.UseFastSRGBLinearConversion;
			passData.material = material;
			passData.materialCoC = processor.MatLib.BokehDepthOfFieldCoC;
			rasterRenderGraphBuilder.AllowPassCulling(value: false);
			rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
			rasterRenderGraphBuilder.SetRenderFunc(delegate(DoFBokehSetupPassData data, RasterGraphContext context)
			{
				RasterCommandBuffer cmd6 = context.cmd;
				CoreUtils.SetKeyword(data.material, "_USE_FAST_SRGB_LINEAR_CONVERSION", data.useFastSRGBLinearConversion);
				CoreUtils.SetKeyword(data.materialCoC, "_USE_FAST_SRGB_LINEAR_CONVERSION", data.useFastSRGBLinearConversion);
				cmd6.SetGlobalVector(PostProcessor.ShaderIDs._CoCParams, data.cocParams);
				cmd6.SetGlobalVectorArray(PostProcessor.ShaderIDs._BokehKernel, data.bokehKernel);
				cmd6.SetGlobalVector(PostProcessor.ShaderIDs._DownSampleScaleFactor, new Vector4(1f / (float)data.downSample, 1f / (float)data.downSample, data.downSample, data.downSample));
				cmd6.SetGlobalVector(PostProcessor.ShaderIDs._BokehConstants, new Vector4(data.uvMargin, data.uvMargin * 2f));
				PostProcessUtils.SetSourceSize(cmd6, data.source);
			});
		}
		RenderTextureDescriptor compatibleDescriptor = PostProcessor.GetCompatibleDescriptor(descriptor, descriptor.width, descriptor.height, GraphicsFormat.R8_UNorm);
		TextureHandle input = RenderGraphUtility.CreateRenderGraphTexture(renderGraph, compatibleDescriptor, "_FullCoCTexture", clear: true, FilterMode.Bilinear);
		RenderTextureDescriptor compatibleDescriptor2 = PostProcessor.GetCompatibleDescriptor(descriptor, num2, num3, GraphicsFormat.R16G16B16A16_SFloat);
		TextureHandle input2 = RenderGraphUtility.CreateRenderGraphTexture(renderGraph, compatibleDescriptor2, "_PingTexture", clear: true, FilterMode.Bilinear);
		RenderTextureDescriptor compatibleDescriptor3 = PostProcessor.GetCompatibleDescriptor(descriptor, num2, num3, GraphicsFormat.R16G16B16A16_SFloat);
		TextureHandle input3 = RenderGraphUtility.CreateRenderGraphTexture(renderGraph, compatibleDescriptor3, "_PongTexture", clear: true, FilterMode.Bilinear);
		DoFBokehPassData passData2;
		using (IRasterRenderGraphBuilder rasterRenderGraphBuilder2 = renderGraph.AddRasterRenderPass<DoFBokehPassData>("Depth of Field - Compute CoC", out passData2, WaaaghProfileId.DOFComputeCOC.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\PostProcess\\DOF.cs", 346))
		{
			rasterRenderGraphBuilder2.SetRenderAttachment(input, 0);
			passData2.sourceTexture = source;
			rasterRenderGraphBuilder2.UseTexture(in source);
			passData2.depthTexture = cameraDepth;
			rasterRenderGraphBuilder2.UseTexture(in cameraDepth);
			passData2.material = processor.MatLib.BokehDepthOfFieldCoC;
			rasterRenderGraphBuilder2.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraDepthTexture);
			rasterRenderGraphBuilder2.SetRenderFunc(delegate(DoFBokehPassData data, RasterGraphContext context)
			{
				Material material6 = data.material;
				RasterCommandBuffer cmd5 = context.cmd;
				RTHandle rTHandle5 = data.sourceTexture;
				material6.SetTexture(GlobalTextureShaderPropertyId._CameraDepthTexture, data.depthTexture);
				Vector2 vector5 = (rTHandle5.useScaling ? new Vector2(rTHandle5.rtHandleProperties.rtHandleScale.x, rTHandle5.rtHandleProperties.rtHandleScale.y) : Vector2.one);
				Blitter.BlitTexture(cmd5, rTHandle5, vector5, material6, 0);
			});
		}
		DoFBokehPassData passData3;
		using (IRasterRenderGraphBuilder rasterRenderGraphBuilder3 = renderGraph.AddRasterRenderPass<DoFBokehPassData>("Depth of Field - Downscale & Prefilter Color + CoC", out passData3, WaaaghProfileId.DOFDownscalePrefilter.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\PostProcess\\DOF.cs", 371))
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
				material5.SetTexture(PostProcessor.ShaderIDs._FullCoCTexture, data.cocTexture);
				Vector2 vector4 = (rTHandle4.useScaling ? new Vector2(rTHandle4.rtHandleProperties.rtHandleScale.x, rTHandle4.rtHandleProperties.rtHandleScale.y) : Vector2.one);
				Blitter.BlitTexture(cmd4, rTHandle4, vector4, material5, 1);
			});
		}
		DoFBokehPassData passData4;
		using (IRasterRenderGraphBuilder rasterRenderGraphBuilder4 = renderGraph.AddRasterRenderPass<DoFBokehPassData>("Depth of Field - Bokeh Blur", out passData4, WaaaghProfileId.DOFBlurBokeh.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\PostProcess\\DOF.cs", 394))
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
		using (IRasterRenderGraphBuilder rasterRenderGraphBuilder5 = renderGraph.AddRasterRenderPass<DoFBokehPassData>("Depth of Field - Post-filtering", out passData5, WaaaghProfileId.DOFPostFilter.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\PostProcess\\DOF.cs", 413))
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
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder6 = renderGraph.AddRasterRenderPass<DoFBokehPassData>("Depth of Field - Composite", out passData6, WaaaghProfileId.DOFComposite.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\PostProcess\\DOF.cs", 433);
		rasterRenderGraphBuilder6.SetRenderAttachment(dest, 0);
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
			material2.SetTexture(PostProcessor.ShaderIDs._DofTexture, data.dofTexture);
			Vector2 vector = (rTHandle.useScaling ? new Vector2(rTHandle.rtHandleProperties.rtHandleScale.x, rTHandle.rtHandleProperties.rtHandleScale.y) : Vector2.one);
			Blitter.BlitTexture(cmd, rTHandle, vector, material2, 4);
		});
	}

	private static void PrepareBokehKernel(float maxRadius, float rcpAspect, DepthOfField depthOfField)
	{
		if (s_BokehKernel == null)
		{
			s_BokehKernel = new Vector4[42];
		}
		int num = 0;
		float num2 = depthOfField.bladeCount.value;
		float p = 1f - depthOfField.bladeCurvature.value;
		float num3 = depthOfField.bladeRotation.value * (MathF.PI / 180f);
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
				s_BokehKernel[num] = new Vector4(num13, num14, z, w);
				num++;
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float GetMaxBokehRadiusInPixels(float viewportHeight)
	{
		return Mathf.Min(0.05f, 14f / viewportHeight);
	}
}
