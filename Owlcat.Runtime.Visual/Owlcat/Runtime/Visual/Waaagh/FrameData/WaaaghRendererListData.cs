using Owlcat.Runtime.Visual.GPUDrivenBRG;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.FrameData;

public class WaaaghRendererListData : WaaaghResourceDataBase
{
	private ShaderTagId m_GBufferShaderTagId = new ShaderTagId("GBuffer");

	private ShaderTagId m_TerrainGBufferShaderTagId = new ShaderTagId("TerrainGBuffer");

	private ShaderTagId[] m_ForwardShaderTags = new ShaderTagId[2]
	{
		new ShaderTagId("SRPDefaultUnlit"),
		new ShaderTagId("ForwardLit")
	};

	private ShaderTagId m_DistortionVectorShaderTagId = new ShaderTagId("DistortionVectors");

	public WaaaghRendererList OpaqueGBuffer;

	public WaaaghRendererList OpaqueAlphaTestGBuffer;

	public WaaaghRendererList TerrainGBuffer;

	public WaaaghRendererList OpaqueDistortionGBuffer;

	public WaaaghRendererList OpaqueDistortionForward;

	public WaaaghRendererList Transparent;

	public WaaaghRendererList DistortionVectors;

	public WaaaghRendererList Overlay;

	public void Init(ScriptableRenderContext context, WaaaghRenderingData renderingData, WaaaghCameraData cameraData)
	{
		PerObjectData rendererConfiguration = PerObjectData.LightProbe | PerObjectData.Lightmaps | PerObjectData.OcclusionProbe | PerObjectData.ShadowMask;
		SortingCriteria sortingCriteria = SortingCriteria.SortingLayer | SortingCriteria.RenderQueue | SortingCriteria.OptimizeStateChanges | SortingCriteria.CanvasOrder;
		GPUDrivenBatchRendererGroup gPUDrivenBatchRendererGroup = renderingData.GPUDrivenBatchRendererGroup;
		if (gPUDrivenBatchRendererGroup != null)
		{
			GPUDrivenBRGSettings settings = gPUDrivenBatchRendererGroup.Settings;
			if (settings != null && settings.OpaqueSortingCPU)
			{
				sortingCriteria |= SortingCriteria.QuantizedFrontToBack;
			}
		}
		OpaqueGBuffer.ListParams = RenderingUtils.CreateRendererListParams(renderingData.CullResults, cameraData.camera, m_GBufferShaderTagId, rendererConfiguration, WaaaghRenderQueue.Opaque, sortingCriteria);
		OpaqueGBuffer.ListParams.filteringSettings.batchLayerMask = 4294967283u;
		OpaqueGBuffer.List = context.CreateRendererList(ref OpaqueGBuffer.ListParams);
		OpaqueAlphaTestGBuffer.ListParams = RenderingUtils.CreateRendererListParams(renderingData.CullResults, cameraData.camera, m_GBufferShaderTagId, rendererConfiguration, WaaaghRenderQueue.OpaqueAlphaTest, sortingCriteria);
		OpaqueAlphaTestGBuffer.ListParams.filteringSettings.batchLayerMask = 4294967283u;
		OpaqueAlphaTestGBuffer.List = context.CreateRendererList(ref OpaqueAlphaTestGBuffer.ListParams);
		TerrainGBuffer.ListParams = RenderingUtils.CreateRendererListParams(renderingData.CullResults, cameraData.camera, m_TerrainGBufferShaderTagId, rendererConfiguration, WaaaghRenderQueue.Opaque, SortingCriteria.QuantizedFrontToBack);
		TerrainGBuffer.ListParams.filteringSettings.batchLayerMask = 4294967283u;
		TerrainGBuffer.List = context.CreateRendererList(ref TerrainGBuffer.ListParams);
		OpaqueDistortionGBuffer.ListParams = RenderingUtils.CreateRendererListParams(renderingData.CullResults, cameraData.camera, m_GBufferShaderTagId, rendererConfiguration, WaaaghRenderQueue.OpaqueDistortion, cameraData.defaultOpaqueSortFlags);
		OpaqueDistortionGBuffer.ListParams.filteringSettings.batchLayerMask = 4294967283u;
		OpaqueDistortionGBuffer.List = context.CreateRendererList(ref OpaqueDistortionGBuffer.ListParams);
		OpaqueDistortionForward.ListParams = RenderingUtils.CreateRendererListParams(renderingData.CullResults, cameraData.camera, m_ForwardShaderTags, renderingData.PerObjectData, WaaaghRenderQueue.OpaqueDistortion);
		OpaqueDistortionForward.ListParams.filteringSettings.batchLayerMask = 4294967283u;
		OpaqueDistortionForward.List = context.CreateRendererList(ref OpaqueDistortionForward.ListParams);
		Transparent.ListParams = RenderingUtils.CreateRendererListParams(renderingData.CullResults, cameraData.camera, m_ForwardShaderTags, -1, renderingData.PerObjectData, WaaaghRenderQueue.Transparent, SortingCriteria.CommonTransparent);
		Transparent.ListParams.filteringSettings.batchLayerMask = 4294967283u;
		Transparent.List = context.CreateRendererList(ref Transparent.ListParams);
		DistortionVectors.ListParams = RenderingUtils.CreateRendererListParams(renderingData.CullResults, cameraData.camera, m_DistortionVectorShaderTagId, PerObjectData.None, WaaaghRenderQueue.Transparent);
		DistortionVectors.ListParams.filteringSettings.batchLayerMask = 4294967283u;
		DistortionVectors.List = context.CreateRendererList(ref DistortionVectors.ListParams);
		Overlay.ListParams = RenderingUtils.CreateRendererListParams(renderingData.CullResults, cameraData.camera, m_ForwardShaderTags, -1, renderingData.PerObjectData, WaaaghRenderQueue.Overlay, SortingCriteria.CommonTransparent);
		Overlay.ListParams.filteringSettings.batchLayerMask = 4294967283u;
		Overlay.List = context.CreateRendererList(ref Overlay.ListParams);
	}

	public override void Reset()
	{
		OpaqueGBuffer.Reset();
		OpaqueGBuffer = default(WaaaghRendererList);
		OpaqueAlphaTestGBuffer.Reset();
		OpaqueAlphaTestGBuffer = default(WaaaghRendererList);
		TerrainGBuffer.Reset();
		TerrainGBuffer = default(WaaaghRendererList);
		OpaqueDistortionGBuffer.Reset();
		OpaqueDistortionGBuffer = default(WaaaghRendererList);
		OpaqueDistortionForward.Reset();
		OpaqueDistortionForward = default(WaaaghRendererList);
		Transparent.Reset();
		Transparent = default(WaaaghRendererList);
		DistortionVectors.Reset();
		DistortionVectors = default(WaaaghRendererList);
		Overlay.Reset();
		Overlay = default(WaaaghRendererList);
	}
}
