using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders.Debugging;

internal static class Wireframe
{
	private sealed class PassData
	{
		public bool UsingSceneViewWireframeMode;

		public Camera Camera;

		public RendererListHandle WireRendererList;
	}

	private static Material s_WireMaterial;

	public static void Record(in RendererRecordContext context, TextureHandle targetColor, TextureHandle targetDepth)
	{
		bool usingSceneViewWireframeMode = context.CameraData.cameraType == CameraType.SceneView && GL.wireframe;
		PassData passData2;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<PassData>("Draw Wireframe", out passData2, ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\Debugging\\Wireframe.cs", 28);
		passData2.UsingSceneViewWireframeMode = usingSceneViewWireframeMode;
		passData2.Camera = context.CameraData.camera;
		passData2.WireRendererList = CreateWireRendererList(in context, usingSceneViewWireframeMode);
		unsafeRenderGraphBuilder.UseRendererList(in passData2.WireRendererList);
		unsafeRenderGraphBuilder.SetRenderAttachment(targetColor, 0);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(PassData passData, UnsafeGraphContext context)
		{
			context.cmd.SetupCameraProperties(passData.Camera);
			if (passData.UsingSceneViewWireframeMode)
			{
				context.cmd.DrawRendererList(passData.WireRendererList);
			}
			else
			{
				context.cmd.SetWireframe(enable: true);
				context.cmd.DrawRendererList(passData.WireRendererList);
				context.cmd.SetWireframe(enable: false);
			}
		});
	}

	private static RendererListHandle CreateWireRendererList(in RendererRecordContext context, bool usingSceneViewWireframeMode)
	{
		SortingSettings sortingSettings = new SortingSettings(context.CameraData.camera);
		sortingSettings.criteria = SortingCriteria.None;
		SortingSettings sortingSettings2 = sortingSettings;
		DrawingSettings drawingSettings = new DrawingSettings(ShaderTagId.none, sortingSettings2);
		drawingSettings.perObjectData = PerObjectData.None;
		drawingSettings.enableDynamicBatching = false;
		drawingSettings.enableInstancing = false;
		DrawingSettings drawSettings = drawingSettings;
		for (int i = 0; i < ShaderConstants.LightModeTags.ForwardAll.Length; i++)
		{
			drawSettings.SetShaderPassName(i, ShaderConstants.LightModeTags.ForwardAll[i]);
		}
		if (!usingSceneViewWireframeMode)
		{
			drawSettings.overrideMaterial = GetOverrideWireMaterial();
		}
		FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.all);
		filteringSettings.batchLayerMask = 4294967283u;
		FilteringSettings filteringSettings2 = filteringSettings;
		RendererListParams desc = new RendererListParams(context.RenderingData.CullResults, drawSettings, filteringSettings2);
		return context.RenderGraph.CreateRendererList(in desc);
	}

	private static Material GetOverrideWireMaterial()
	{
		if (s_WireMaterial == null)
		{
			Shader shader = Shader.Find("Hidden/Owlcat/Wireframe");
			if ((bool)shader)
			{
				s_WireMaterial = new Material(shader);
			}
		}
		return s_WireMaterial;
	}
}
