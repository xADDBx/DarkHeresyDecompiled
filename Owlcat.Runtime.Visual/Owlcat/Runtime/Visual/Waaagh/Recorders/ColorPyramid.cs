using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders;

internal sealed class ColorPyramid
{
	private sealed class PassData
	{
		public TextureHandle Input;

		public TextureHandle Output;

		public Material BlitMaterial;

		public Material ColorPyramidMaterial;

		public int2 TextureSize;
	}

	public static bool ShouldForceColorPyramidBuildBeforeOpaqueDistortion(in RecordContext context)
	{
		if (!context.CameraData.camera.TryGetComponent<WaaaghAdditionalCameraData>(out var component))
		{
			return WaaaghPipeline.Asset.SupportsCameraOpaqueTexture;
		}
		return component.RequiresColorTexture;
	}

	public static void BuildColorPyramid(in RecordContext context, RendererListHandle rendererListDependency = default(RendererListHandle))
	{
		PassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<PassData>("Build Color Pyramid", out passData, WaaaghProfileId.BuildColorPyramid.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\ForwardPath\\ColorPyramid.cs", 28);
		passData.Input = context.FrameResources.CameraStackTargets.Color;
		passData.Output = context.FrameResources.CameraAdditionalTargets.ColorPyramid;
		passData.TextureSize = new int2(context.CameraData.cameraTargetDescriptor.width, context.CameraData.cameraTargetDescriptor.height);
		passData.BlitMaterial = context.MaterialLibrary.ColorPyramidBlitMaterial;
		passData.ColorPyramidMaterial = context.MaterialLibrary.ColorPyramidMaterial;
		if (rendererListDependency.IsValid())
		{
			unsafeRenderGraphBuilder.UseRendererList(in rendererListDependency);
		}
		unsafeRenderGraphBuilder.UseTexture(in passData.Input, AccessFlags.ReadWrite);
		unsafeRenderGraphBuilder.UseTexture(in passData.Output, AccessFlags.WriteAll);
		unsafeRenderGraphBuilder.SetGlobalTextureAfterPass(in passData.Output, GlobalTextureShaderPropertyId._CameraColorPyramidRT);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(PassData data, UnsafeGraphContext context)
		{
			int num = 0;
			int num2 = data.TextureSize.x;
			int num3 = data.TextureSize.y;
			Vector4 value = new Vector4(1f, 1f, 0f, 0f);
			context.cmd.SetGlobalTexture(ShaderPropertyId._BlitTexture, data.Input);
			context.cmd.SetGlobalVector(ShaderPropertyId._BlitScaleBias, value);
			context.cmd.SetGlobalFloat(ShaderPropertyId._BlitMipLevel, 0f);
			context.cmd.SetRenderTarget(data.Output, 0);
			context.cmd.SetViewport(new Rect(0f, 0f, num2, num3));
			context.cmd.DrawProcedural(Matrix4x4.identity, data.BlitMaterial, 0, MeshTopology.Triangles, 3, 1);
			while (num2 >= 8 || num3 >= 8)
			{
				int num4 = math.max(1, num2 >> 1);
				int num5 = math.max(1, num3 >> 1);
				context.cmd.SetGlobalTexture(ShaderPropertyId._Source, data.Output);
				context.cmd.SetGlobalVector(ShaderPropertyId._SrcScaleBias, new Vector4(1f, 1f, 0f, 0f));
				context.cmd.SetGlobalVector(ShaderPropertyId._SrcUvLimits, new Vector4(1f, 1f, 2f / (float)num2, 0f));
				context.cmd.SetGlobalFloat(ShaderPropertyId._SourceMip, num);
				context.cmd.SetRenderTarget(data.Input, 0);
				context.cmd.SetViewport(new Rect(0f, 0f, num4, num5));
				context.cmd.DrawProcedural(Matrix4x4.identity, data.ColorPyramidMaterial, 0, MeshTopology.Triangles, 3, 1);
				float x = (float)num4 / (float)data.TextureSize.x;
				float y = (float)num5 / (float)data.TextureSize.y;
				context.cmd.SetGlobalTexture(ShaderPropertyId._Source, data.Input);
				context.cmd.SetGlobalVector(ShaderPropertyId._SrcScaleBias, new Vector4(x, y, 0f, 0f));
				context.cmd.SetGlobalVector(ShaderPropertyId._SrcUvLimits, new Vector4(((float)num4 - 0.5f) / (float)data.TextureSize.x, ((float)num5 - 0.5f) / (float)data.TextureSize.y, 0f, 1f / (float)data.TextureSize.y));
				context.cmd.SetGlobalFloat(ShaderPropertyId._SourceMip, 0f);
				context.cmd.SetRenderTarget(data.Output, num + 1);
				context.cmd.SetViewport(new Rect(0f, 0f, num4, num5));
				context.cmd.DrawProcedural(Matrix4x4.identity, data.ColorPyramidMaterial, 0, MeshTopology.Triangles, 3, 1);
				num++;
				num2 >>= 1;
				num3 >>= 1;
			}
			context.cmd.SetGlobalTexture(ShaderPropertyId._BlitTexture, data.Output);
			context.cmd.SetGlobalVector(ShaderPropertyId._BlitScaleBias, value);
			context.cmd.SetGlobalFloat(ShaderPropertyId._BlitMipLevel, 0f);
			context.cmd.SetRenderTarget(data.Input, 0);
			context.cmd.SetViewport(new Rect(0f, 0f, data.TextureSize.x, data.TextureSize.y));
			context.cmd.DrawProcedural(Matrix4x4.identity, data.BlitMaterial, 0, MeshTopology.Triangles, 3, 1);
			context.cmd.SetGlobalFloat(ShaderPropertyId._ColorPyramidLodCount, num);
		});
	}
}
