using Owlcat.Runtime.Visual.Waaagh.FrameData;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class DecalPreviewPass : DrawRendererListPass<DecalPreviewPassData>
{
	private ShaderTagId m_ShaderTagId;

	public override string Name => "DecalPreviewPass";

	public DecalPreviewPass(RenderPassEvent evt)
		: base(evt)
	{
		m_ShaderTagId = new ShaderTagId("DecalPreview");
	}

	protected override void GetOrCreateRendererList(ScriptableRenderContext context, ContextContainer frameData, out RendererList rendererList, out RendererListParams rendererListParams)
	{
		frameData.Get<WaaaghRenderingData>();
		rendererListParams = CreateRendererListParams(frameData);
		rendererList = context.CreateRendererList(ref rendererListParams);
	}

	private RendererListParams CreateRendererListParams(ContextContainer frameData)
	{
		WaaaghRenderingData waaaghRenderingData = frameData.Get<WaaaghRenderingData>();
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		return RenderingUtils.CreateRendererListParams(waaaghRenderingData.CullResults, waaaghCameraData.camera, m_ShaderTagId, waaaghRenderingData.PerObjectData, RenderQueueRange.opaque);
	}

	protected override void Setup(RenderGraphBuilder builder, DecalPreviewPassData data, ContextContainer frameData)
	{
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		TextureHandle input = waaaghResourceData.CameraDepthBuffer;
		data.CameraDepthRT = builder.UseDepthBuffer(in input, DepthAccess.Read);
		input = waaaghResourceData.CameraColorBuffer;
		data.CameraColorRT = builder.UseColorBuffer(in input, 0);
	}

	protected override void Render(DecalPreviewPassData data, RenderGraphContext context)
	{
		context.cmd.DrawRendererList(data.RendererList);
	}
}
