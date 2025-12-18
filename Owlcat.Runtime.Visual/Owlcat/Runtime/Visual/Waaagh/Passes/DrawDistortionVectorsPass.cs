using Owlcat.Runtime.Visual.Waaagh.FrameData;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class DrawDistortionVectorsPass : DrawRendererListPass<DrawDistortionVectorsPassData>
{
	private ShaderTagId m_ShaderTagId = new ShaderTagId("DistortionVectors");

	private Material m_ApplyDistortionMaterial;

	public override string Name => "DrawDistortionVectorsPass";

	public DrawDistortionVectorsPass(RenderPassEvent evt, Material applyDistortionMaterial)
		: base(evt)
	{
		m_ApplyDistortionMaterial = applyDistortionMaterial;
	}

	protected override void GetOrCreateRendererList(ScriptableRenderContext context, ContextContainer frameData, out RendererList rendererList, out RendererListParams rendererListParams)
	{
		WaaaghRendererListData waaaghRendererListData = frameData.Get<WaaaghRendererListData>();
		rendererList = waaaghRendererListData.DistortionVectors.List;
		rendererListParams = waaaghRendererListData.DistortionVectors.ListParams;
	}

	protected override void Setup(RenderGraphBuilder builder, DrawDistortionVectorsPassData data, ContextContainer frameData)
	{
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		TextureDesc desc = RenderingUtils.CreateTextureDesc("DistortionRT", waaaghCameraData.cameraTargetDescriptor);
		desc.colorFormat = GraphicsFormat.R16G16B16A16_SFloat;
		data.DistortionRT = builder.CreateTransientTexture(in desc);
		data.CameraColorPyramidRT = builder.ReadTexture(in waaaghResourceData.CameraColorPyramidRT);
		data.CameraColorRT = waaaghResourceData.CameraColorBuffer;
		data.CameraDepthRT = waaaghResourceData.CameraDepthBuffer;
		data.ApplyDistortionMaterial = m_ApplyDistortionMaterial;
	}

	protected override void Render(DrawDistortionVectorsPassData data, RenderGraphContext context)
	{
		context.cmd.SetRenderTarget(data.DistortionRT, data.CameraDepthRT);
		context.cmd.ClearRenderTarget(clearDepth: false, clearColor: true, data.ClearColor);
		context.cmd.DrawRendererList(data.RendererList);
		context.cmd.SetGlobalTexture(ShaderPropertyId._DistortionVectorsRT, data.DistortionRT);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraColorPyramidRT, data.CameraColorPyramidRT);
		context.cmd.Blit(data.DistortionRT, data.CameraColorRT, data.ApplyDistortionMaterial, 0);
	}
}
