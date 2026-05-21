using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VolumetricLighting.Passes;

internal static class VolumetricLightingApplyOpaquePass
{
	private sealed class PassData
	{
		public Material ApplyOpaqueMaterial;
	}

	public static void Record(in RecordContext context, Material applyOpaqueMaterial, VolumetricLightingData volumetricLightingData)
	{
		PassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<PassData>("VolumetricLightingApplyOpaque", out passData, WaaaghProfileId.VolumetricLightingApplyOpaque.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\RendererFeatures\\VolumetricLighting\\Passes\\VolumetricLightingApplyOpaquePass.cs", 17);
		passData.ApplyOpaqueMaterial = applyOpaqueMaterial;
		unsafeRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraDepthTexture);
		unsafeRenderGraphBuilder.UseTexture(in volumetricLightingData.ScatterTexture);
		unsafeRenderGraphBuilder.SetRenderAttachment(context.FrameResources.CameraStackTargets.Color, 0);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(PassData data, UnsafeGraphContext context)
		{
			context.cmd.DrawProcedural(Matrix4x4.identity, data.ApplyOpaqueMaterial, 0, MeshTopology.Triangles, 3);
		});
	}
}
