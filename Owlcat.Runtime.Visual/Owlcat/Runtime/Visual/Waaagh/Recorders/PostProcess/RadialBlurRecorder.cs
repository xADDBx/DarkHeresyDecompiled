using Owlcat.Runtime.Visual.Overrides;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders.PostProcess;

public static class RadialBlurRecorder
{
	private class RadialBlurPassData
	{
		public TextureHandle Source;

		public TextureHandle Destination;

		public Material Material;
	}

	private static readonly int k_ShaderPropertyId_RadialBlurCenter = Shader.PropertyToID("_RadialBlurCenter");

	private static readonly int k_ShaderPropertyId_RadialBlurStrength = Shader.PropertyToID("_RadialBlurStrength");

	private static readonly int k_ShaderPropertyId_RadialBlurWidth = Shader.PropertyToID("_RadialBlurWidth");

	public static void Render(PostProcessor processor, RenderGraph renderGraph)
	{
		RenderTextureDescriptor descriptor = processor.FrameState.Descriptor;
		Material radialBlur = processor.MatLib.RadialBlur;
		RadialBlur radialBlur2 = processor.Overrides.RadialBlur;
		RenderTextureDescriptor compatibleDescriptor = PostProcessor.GetCompatibleDescriptor(descriptor, descriptor.width, descriptor.height, descriptor.graphicsFormat);
		TextureHandle input = processor.CameraStackTargets.CurrentPostProcessSource;
		TextureHandle input2 = RenderGraphUtility.CreateRenderGraphTexture(renderGraph, compatibleDescriptor, "_RadialBlurTarget", clear: true, FilterMode.Bilinear);
		radialBlur.SetVector(k_ShaderPropertyId_RadialBlurCenter, radialBlur2.Center.value);
		radialBlur.SetFloat(k_ShaderPropertyId_RadialBlurStrength, radialBlur2.Strength.value);
		radialBlur.SetFloat(k_ShaderPropertyId_RadialBlurWidth, radialBlur2.Width.value);
		RadialBlurPassData passData2;
		using (IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = renderGraph.AddUnsafePass<RadialBlurPassData>("Radial Blur", out passData2, WaaaghProfileId.RadialBlur.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\PostProcess\\RadialBlurRecorder.cs", 40))
		{
			passData2.Source = input;
			unsafeRenderGraphBuilder.UseTexture(in input);
			passData2.Destination = input2;
			unsafeRenderGraphBuilder.UseTexture(in input2, AccessFlags.Write);
			passData2.Material = radialBlur;
			unsafeRenderGraphBuilder.SetRenderFunc(delegate(RadialBlurPassData passData, UnsafeGraphContext context)
			{
				CommandBufferHelpers.GetNativeCommandBuffer(context.cmd).Blit(passData.Source, passData.Destination, passData.Material, 0);
			});
		}
		processor.CameraStackTargets.SetCurrentPostProcessSource(input2);
	}
}
