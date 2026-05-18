using System;
using Owlcat.Runtime.Visual.Overrides;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders.PostProcess;

public static class PaniniProjectionRecorder
{
	private class PaniniProjectionPassData
	{
		internal TextureHandle destinationTexture;

		internal TextureHandle sourceTexture;

		internal RenderTextureDescriptor sourceTextureDesc;

		internal Material material;

		internal Vector4 paniniParams;

		internal bool isPaniniGeneric;
	}

	public static void Render(PostProcessor processor, RenderGraph renderGraph, Camera camera)
	{
		RenderTextureDescriptor descriptor = processor.FrameState.Descriptor;
		PaniniProjection paniniProjection = processor.Overrides.PaniniProjection;
		RenderTextureDescriptor compatibleDescriptor = PostProcessor.GetCompatibleDescriptor(descriptor, descriptor.width, descriptor.height, descriptor.graphicsFormat);
		TextureHandle input = processor.CameraStackTargets.CurrentPostProcessSource;
		TextureHandle textureHandle = RenderGraphUtility.CreateRenderGraphTexture(renderGraph, compatibleDescriptor, "_PaniniProjectionTarget", clear: true, FilterMode.Bilinear);
		float value = paniniProjection.distance.value;
		Vector2 vector = CalcViewExtents(camera, in descriptor);
		Vector2 vector2 = CalcCropExtents(camera, in descriptor, value);
		float a = vector2.x / vector.x;
		float b = vector2.y / vector.y;
		float value2 = Mathf.Min(a, b);
		float num = value;
		float w = Mathf.Lerp(1f, Mathf.Clamp01(value2), paniniProjection.cropToFit.value);
		PaniniProjectionPassData passData;
		using (IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<PaniniProjectionPassData>("Panini Projection", out passData, WaaaghProfileId.PaniniProjection.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\PostProcess\\PaniniProjectionRecorder.cs", 44))
		{
			rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
			passData.destinationTexture = textureHandle;
			rasterRenderGraphBuilder.SetRenderAttachment(textureHandle, 0);
			passData.sourceTexture = input;
			rasterRenderGraphBuilder.UseTexture(in input);
			passData.material = processor.MatLib.PaniniProjection;
			passData.paniniParams = new Vector4(vector.x, vector.y, num, w);
			passData.isPaniniGeneric = 1f - Mathf.Abs(num) > float.Epsilon;
			passData.sourceTextureDesc = descriptor;
			rasterRenderGraphBuilder.SetRenderFunc(delegate(PaniniProjectionPassData data, RasterGraphContext context)
			{
				RasterCommandBuffer cmd = context.cmd;
				RTHandle rTHandle = data.sourceTexture;
				cmd.SetGlobalVector(PostProcessor.ShaderIDs._Params, data.paniniParams);
				data.material.EnableKeyword(data.isPaniniGeneric ? ShaderKeywordStrings.PaniniGeneric : ShaderKeywordStrings.PaniniUnitDistance);
				Vector2 vector3 = (rTHandle.useScaling ? new Vector2(rTHandle.rtHandleProperties.rtHandleScale.x, rTHandle.rtHandleProperties.rtHandleScale.y) : Vector2.one);
				Blitter.BlitTexture(cmd, rTHandle, vector3, data.material, 0);
			});
		}
		processor.CameraStackTargets.SetCurrentPostProcessSource(textureHandle);
	}

	private static Vector2 CalcViewExtents(Camera camera, in RenderTextureDescriptor descriptor)
	{
		float num = camera.fieldOfView * (MathF.PI / 180f);
		float num2 = (float)descriptor.width / (float)descriptor.height;
		float num3 = Mathf.Tan(0.5f * num);
		return new Vector2(num2 * num3, num3);
	}

	private static Vector2 CalcCropExtents(Camera camera, in RenderTextureDescriptor descriptor, float d)
	{
		float num = 1f + d;
		Vector2 vector = CalcViewExtents(camera, in descriptor);
		float num2 = Mathf.Sqrt(vector.x * vector.x + 1f);
		float num3 = 1f / num2;
		float num4 = num3 + d;
		return vector * num3 * (num / num4);
	}
}
