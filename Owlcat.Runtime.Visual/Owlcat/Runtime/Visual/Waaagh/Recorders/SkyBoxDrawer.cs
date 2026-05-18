using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders;

public static class SkyBoxDrawer
{
	private class PassData
	{
		public RendererListHandle RendererList;
	}

	public static bool ShouldDrawSkybox(in RecordContext context)
	{
		if (context.CameraData.renderType == CameraRenderType.Base && context.CameraData.camera.clearFlags == CameraClearFlags.Skybox)
		{
			if (!(RenderSettings.skybox != null))
			{
				if (context.CameraData.camera.TryGetComponent<Skybox>(out var component))
				{
					return component.material != null;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public static void DrawSkybox(in RecordContext context)
	{
		PassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = context.RenderGraph.AddRasterRenderPass<PassData>("DrawSkyBox", out passData, WaaaghProfileId.DrawSkyboxPass.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\SkyBoxDrawer.cs", 22);
		passData.RendererList = context.RenderGraph.CreateSkyboxRendererList(in context.CameraData.camera);
		rasterRenderGraphBuilder.UseRendererList(in passData.RendererList);
		rasterRenderGraphBuilder.SetRenderAttachment(context.FrameResources.CameraStackTargets.Color, 0);
		rasterRenderGraphBuilder.SetRenderAttachmentDepth(context.FrameResources.CameraStackTargets.Depth, AccessFlags.Read);
		rasterRenderGraphBuilder.SetRenderFunc(delegate(PassData data, RasterGraphContext context)
		{
			context.cmd.DrawRendererList(data.RendererList);
		});
	}
}
