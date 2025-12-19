using Owlcat.Runtime.Visual.Waaagh.FrameData;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class MotionVectorsPass : ScriptableRenderPass
{
	private class PassData
	{
		internal Camera camera;

		internal TextureHandle motionVectorColor;

		internal TextureHandle motionVectorDepth;

		internal TextureHandle cameraDepth;

		internal Material cameraMaterial;

		internal Material objectMaterial;

		internal RendererListHandle rendererListHdl;

		internal MotionVectorsPersistentData MotionData;
	}

	private Material m_CameraMaterial;

	private Material m_ObjectMaterial;

	public const string k_MotionVectorsLightModeTag = "MotionVectors";

	private static readonly string[] s_ShaderTags = new string[1] { "MotionVectors" };

	public override string Name => "MotionVectorsPass";

	public MotionVectorsPass(RenderPassEvent evt, Material cameraMaterial, Material objectMaterial)
		: base(evt)
	{
		m_CameraMaterial = cameraMaterial;
		m_ObjectMaterial = objectMaterial;
	}

	public override void RecordRenderGraph(ContextContainer frameData)
	{
		WaaaghRenderingData waaaghRenderingData = frameData.Get<WaaaghRenderingData>();
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		WaaaghRendererListData rendererListData = frameData.Get<WaaaghRendererListData>();
		RenderGraph renderGraph = waaaghRenderingData.RenderGraph;
		PassData passData;
		using RenderGraphBuilder renderGraphBuilder = renderGraph.AddRenderPass<PassData>("Motion Vector Pass", out passData, ProfilingSampler.Get(WaaaghProfileId.MotionVectorsPass), ".\\Library\\PackageCache\\com.owlcat.visual@2c5e70bf14b9\\Runtime\\Waaagh\\Passes\\MotionVectorsPass.cs", 46);
		TextureDesc desc = RenderingUtils.CreateTextureDesc("CameraMotionVectorsRT", waaaghCameraData.cameraTargetDescriptor);
		desc.colorFormat = GraphicsFormat.R16G16_SFloat;
		desc.filterMode = FilterMode.Bilinear;
		desc.wrapMode = TextureWrapMode.Clamp;
		desc.clearBuffer = true;
		waaaghResourceData.CameraMotionVectorsRT = renderGraph.CreateTexture(in desc);
		TextureDesc desc2 = desc;
		desc2.depthBufferBits = DepthBits.Depth24;
		desc2.filterMode = FilterMode.Point;
		desc2.name = "CameraMotionVectorsDepthRT";
		TextureHandle input = renderGraph.CreateTexture(in desc2);
		renderGraphBuilder.AllowPassCulling(value: true);
		renderGraphBuilder.AllowRendererListCulling(value: false);
		passData.motionVectorColor = renderGraphBuilder.UseColorBuffer(in waaaghResourceData.CameraMotionVectorsRT, 0);
		passData.motionVectorDepth = renderGraphBuilder.UseDepthBuffer(in input, DepthAccess.Write);
		passData.camera = waaaghCameraData.camera;
		passData.cameraMaterial = m_CameraMaterial;
		passData.objectMaterial = m_ObjectMaterial;
		passData.cameraDepth = waaaghResourceData.CameraDepthCopyRT;
		if (waaaghCameraData.camera.TryGetComponent<WaaaghAdditionalCameraData>(out var component))
		{
			passData.MotionData = component.MotionVectorsPersistentData;
		}
		renderGraphBuilder.ReadTexture(in waaaghResourceData.CameraDepthCopyRT);
		InitRendererLists(ref passData, ref waaaghRenderingData.CullResults, rendererListData, waaaghRenderingData.SupportsDynamicBatching, default(ScriptableRenderContext), renderGraph, useRenderGraph: true);
		renderGraphBuilder.UseRendererList(in passData.rendererListHdl);
		renderGraphBuilder.SetRenderFunc(delegate(PassData data, RenderGraphContext context)
		{
			ExecutePass(context.cmd, data, data.rendererListHdl);
		});
	}

	private static void ExecutePass(CommandBuffer cmd, PassData passData, RendererList rendererList)
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
		using (new ProfilingScope(cmd, ProfilingSampler.Get(WaaaghProfileId.MotionVectors)))
		{
			camera.depthTextureMode |= DepthTextureMode.Depth | DepthTextureMode.MotionVectors;
			passData.MotionData.SetGlobalMotionMatrices(cmd);
			DrawCameraMotionVectors(cmd, cameraMaterial);
			DrawObjectMotionVectors(cmd, ref rendererList);
		}
	}

	private static void DrawCameraMotionVectors(CommandBuffer cmd, Material cameraMaterial)
	{
		cmd.DrawProcedural(Matrix4x4.identity, cameraMaterial, 0, MeshTopology.Triangles, 3, 1);
	}

	private static void DrawObjectMotionVectors(CommandBuffer cmd, ref RendererList rendererList)
	{
		cmd.DrawRendererList(rendererList);
	}

	private void InitRendererLists(ref PassData passData, ref CullingResults cullResults, WaaaghRendererListData rendererListData, bool supportsDynamicBatching, ScriptableRenderContext context, RenderGraph renderGraph, bool useRenderGraph)
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
}
