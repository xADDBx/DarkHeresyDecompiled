using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders;

public static class MotionVectors
{
	private class PassData
	{
		internal Camera camera;

		internal TextureHandle motionVectorColor;

		internal TextureHandle motionVectorDepth;

		internal Material cameraMaterial;

		internal Material objectMaterial;

		internal RendererListHandle rendererListHdl;

		internal MotionVectorsPersistentData MotionData;
	}

	public const string k_MotionVectorsLightModeTag = "MotionVectors";

	private static readonly string[] s_ShaderTags = new string[1] { "MotionVectors" };

	public static void MotionVectorsPass(in RecordContext context)
	{
		PassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = context.RenderGraph.AddRasterRenderPass<PassData>("MotionVectorsPass", out passData, WaaaghProfileId.MotionVectorsPass.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\MotionVectors.cs", 26);
		passData.motionVectorColor = context.FrameResources.CameraAdditionalTargets.MotionVectors;
		rasterRenderGraphBuilder.SetRenderAttachment(context.FrameResources.CameraAdditionalTargets.MotionVectors, 0);
		passData.motionVectorDepth = context.FrameResources.CameraAdditionalTargets.MotionVectorsDepth;
		rasterRenderGraphBuilder.SetRenderAttachmentDepth(passData.motionVectorDepth);
		passData.camera = context.CameraData.camera;
		passData.cameraMaterial = context.MaterialLibrary.CameraMotionVectorsMaterial;
		passData.objectMaterial = context.MaterialLibrary.ObjectsMotionVectorsMaterial;
		if (context.CameraData.camera.TryGetComponent<WaaaghAdditionalCameraData>(out var component))
		{
			passData.MotionData = component.MotionVectorsPersistentData;
		}
		rasterRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraDepthTexture);
		InitRendererLists(ref passData, ref context.RenderingData.CullResults, context.RenderingData.SupportsDynamicBatching, context.RenderGraph);
		rasterRenderGraphBuilder.UseRendererList(in passData.rendererListHdl);
		rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
		rasterRenderGraphBuilder.SetRenderFunc<PassData>(ExecturePass);
	}

	private static void InitRendererLists(ref PassData passData, ref CullingResults cullResults, bool supportsDynamicBatching, RenderGraph renderGraph)
	{
		DrawingSettings drawingSettings = GetDrawingSettings(passData.camera, supportsDynamicBatching);
		RendererListParams desc = new RendererListParams(filteringSettings: new FilteringSettings(RenderQueueRange.opaque, passData.camera.cullingMask), cullingResults: cullResults, drawSettings: drawingSettings);
		passData.rendererListHdl = renderGraph.CreateRendererList(in desc);
	}

	private static DrawingSettings GetDrawingSettings(Camera camera, bool supportsDynamicBatching)
	{
		SortingSettings sortingSettings = new SortingSettings(camera);
		sortingSettings.criteria = SortingCriteria.CommonOpaque;
		SortingSettings sortingSettings2 = sortingSettings;
		DrawingSettings drawingSettings = new DrawingSettings(ShaderTagId.none, sortingSettings2);
		drawingSettings.perObjectData = PerObjectData.MotionVectors;
		drawingSettings.enableDynamicBatching = supportsDynamicBatching;
		drawingSettings.enableInstancing = true;
		DrawingSettings result = drawingSettings;
		for (int i = 0; i < s_ShaderTags.Length; i++)
		{
			result.SetShaderPassName(i, new ShaderTagId(s_ShaderTags[i]));
		}
		return result;
	}

	private static void ExecturePass(PassData passData, RasterGraphContext context)
	{
		Material cameraMaterial = passData.cameraMaterial;
		if (cameraMaterial == null)
		{
			return;
		}
		Camera camera = passData.camera;
		if (camera.cameraType == CameraType.Preview || passData.MotionData == null)
		{
			return;
		}
		using (new ProfilingScope(context.cmd, WaaaghProfileId.MotionVectors.Sampler()))
		{
			camera.depthTextureMode |= DepthTextureMode.Depth | DepthTextureMode.MotionVectors;
			passData.MotionData.SetGlobalMotionMatrices(context.cmd);
			DrawCameraMotionVectors(context.cmd, cameraMaterial);
			DrawObjectMotionVectors(context.cmd, ref passData.rendererListHdl);
		}
	}

	private static void DrawCameraMotionVectors(RasterCommandBuffer cmd, Material cameraMaterial)
	{
		cmd.DrawProcedural(Matrix4x4.identity, cameraMaterial, 0, MeshTopology.Triangles, 3, 1);
	}

	private static void DrawObjectMotionVectors(RasterCommandBuffer cmd, ref RendererListHandle rendererList)
	{
		cmd.DrawRendererList(rendererList);
	}
}
