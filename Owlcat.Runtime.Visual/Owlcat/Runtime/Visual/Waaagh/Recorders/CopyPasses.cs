using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.PostProcess;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders;

public static class CopyPasses
{
	private class CopyDepthPassData
	{
		public Material Material;

		public int ShaderPass;

		public TextureHandle SrcTexture;

		public TextureHandle DstTexture;
	}

	private class BlitColorPassData
	{
		public Material BlitMaterial;

		public int BlitMaterialPass;

		public bool RequireSrgbConversion;

		public TextureHandle Source;

		public TextureHandle Destination;

		public WaaaghCameraData CameraData;

		public Rect CameraPixelRect;

		public AccessibilityPostProcessing.Parameters AccessibilityParameters;
	}

	private static class ShaderIDs
	{
		public static readonly int _InputDepthTex = Shader.PropertyToID("_InputDepthTex");
	}

	public static void CopyDepthToDepthCopy(in RecordContext ctx)
	{
		CopyDepthPassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = ctx.RenderGraph.AddRasterRenderPass<CopyDepthPassData>("CopyDepthToDepthCopy", out passData, WaaaghProfileId.CopyDepthToDepthCopy.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\CopyPasses.cs", 38);
		TextureHandle input = ctx.FrameResources.CameraStackTargets.Depth;
		TextureHandle depthCopy = ctx.FrameResources.CameraAdditionalTargets.DepthCopy;
		passData.Material = ctx.MaterialLibrary.CopyDepth;
		passData.ShaderPass = ctx.MaterialLibrary.CopyDepthToColorPass;
		passData.SrcTexture = input;
		rasterRenderGraphBuilder.UseTexture(in input);
		rasterRenderGraphBuilder.SetRenderAttachment(depthCopy, 0);
		rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
		rasterRenderGraphBuilder.SetGlobalTextureAfterPass(in ctx.FrameResources.CameraAdditionalTargets.DepthCopy, GlobalTextureShaderPropertyId._CameraDepthTexture);
		rasterRenderGraphBuilder.SetGlobalTextureAfterPass(in ctx.FrameResources.CameraAdditionalTargets.DepthCopy, GlobalTextureShaderPropertyId._CameraDepthRT);
		rasterRenderGraphBuilder.SetRenderFunc(delegate(CopyDepthPassData data, RasterGraphContext ctx)
		{
			ctx.cmd.SetGlobalTexture(ShaderIDs._InputDepthTex, data.SrcTexture);
			ctx.cmd.DrawProcedural(Matrix4x4.identity, data.Material, data.ShaderPass, MeshTopology.Triangles, 3);
		});
	}

	public static bool ShouldCopyDepthToFinalTarget(in RecordContext context)
	{
		WaaaghCameraData cameraData = context.CameraData;
		bool flag = cameraData.TargetTextureHasDepthBuffer();
		return cameraData.StackInfo.IsLastCamera && flag;
	}

	public static void CopyDepthToFinalTarget(in RecordContext ctx)
	{
		CopyDepthPassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = ctx.RenderGraph.AddRasterRenderPass<CopyDepthPassData>("CopyDepthToFinalTarget", out passData, WaaaghProfileId.CopyDepthToFinalTarget.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\CopyPasses.cs", 78);
		TextureHandle input = ctx.FrameResources.CameraStackTargets.Depth;
		TextureHandle color = ctx.FrameResources.FinalTarget.Color;
		TextureHandle depth = ctx.FrameResources.FinalTarget.Depth;
		passData.Material = ctx.MaterialLibrary.CopyDepth;
		passData.ShaderPass = ctx.MaterialLibrary.CopyDepthScaleOffsetPass;
		passData.SrcTexture = input;
		passData.DstTexture = depth;
		rasterRenderGraphBuilder.UseTexture(in input);
		rasterRenderGraphBuilder.SetRenderAttachment(color, 0);
		rasterRenderGraphBuilder.SetRenderAttachmentDepth(depth);
		rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
		rasterRenderGraphBuilder.SetRenderFunc(delegate(CopyDepthPassData data, RasterGraphContext ctx)
		{
			Vector4 value = new Vector4(1f, 1f, 0f, 0f);
			if (ctx.GetTextureUVOrigin(in data.SrcTexture) != ctx.GetTextureUVOrigin(in data.DstTexture))
			{
				value.y = -1f;
				value.w = 1f;
			}
			ctx.cmd.SetGlobalVector(ShaderPropertyId._BlitScaleBias, value);
			ctx.cmd.SetGlobalTexture(ShaderIDs._InputDepthTex, data.SrcTexture);
			ctx.cmd.DrawProcedural(Matrix4x4.identity, data.Material, data.ShaderPass, MeshTopology.Triangles, 3);
		});
	}

	public static void CopyColorToFinalTarget(in RecordContext context)
	{
		BlitColorPassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<BlitColorPassData>("CopyColorToFinalTarget", out passData, WaaaghProfileId.CopyColorToFinalTarget.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\CopyPasses.cs", 110);
		passData.BlitMaterial = (context.CameraData.isHDROutputActive ? context.MaterialLibrary.FinalBlitHdrMaterial : context.MaterialLibrary.FinalBlitMaterial);
		passData.BlitMaterialPass = (context.CameraData.isHDROutputActive ? context.MaterialLibrary.FinalBlitHdrMaterialLinearPass : context.MaterialLibrary.FinalBlitMaterialLinearPass);
		passData.RequireSrgbConversion = context.CameraData.requireSrgbConversion;
		passData.Source = context.FrameResources.CameraStackTargets.Color;
		passData.Destination = context.FrameResources.FinalTarget.Color;
		passData.CameraData = context.CameraData;
		passData.CameraPixelRect = context.CameraData.pixelRect;
		passData.AccessibilityParameters = AccessibilityPostProcessing.GetParameters(in context.CameraData);
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		unsafeRenderGraphBuilder.AllowGlobalStateModification(value: true);
		TextureHandle input = context.FrameResources.CameraStackTargets.Color;
		unsafeRenderGraphBuilder.UseTexture(in input);
		unsafeRenderGraphBuilder.SetRenderAttachment(context.FrameResources.FinalTarget.Color, 0);
		unsafeRenderGraphBuilder.SetRenderAttachmentDepth(context.FrameResources.FinalTarget.Depth);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(BlitColorPassData data, UnsafeGraphContext context)
		{
			Vector4 finalBlitScaleBias = RenderingUtils.GetFinalBlitScaleBias(ref data.Destination, data.CameraData);
			context.cmd.SetKeyword(in ShaderGlobalKeywords._LINEAR_TO_SRGB_CONVERSION, data.RequireSrgbConversion);
			context.cmd.SetViewport(data.CameraPixelRect);
			AccessibilityPostProcessing.SetupGlobalShaderParameters(context.cmd, in data.AccessibilityParameters);
			Blitter.BlitTexture(context.cmd, data.Source, finalBlitScaleBias, data.BlitMaterial, data.BlitMaterialPass);
		});
	}
}
