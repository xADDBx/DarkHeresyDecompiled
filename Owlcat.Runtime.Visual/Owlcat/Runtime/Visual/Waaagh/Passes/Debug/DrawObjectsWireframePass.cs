using Owlcat.Runtime.Visual.Waaagh.FrameData;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Debug;

public class DrawObjectsWireframePass : DrawRendererListPass<DrawObjectsWireframePassData>
{
	private Color m_ClearColor = new Color(0.5f, 0.5f, 0.5f);

	public override string Name => "DrawObjectsWireframePassData";

	public DrawObjectsWireframePass(RenderPassEvent evt)
		: base(evt)
	{
	}

	protected override void GetOrCreateRendererList(ScriptableRenderContext context, ContextContainer frameData, out RendererList rendererList, out RendererListParams rendererListParams)
	{
		WaaaghRendererListData waaaghRendererListData = frameData.Get<WaaaghRendererListData>();
		rendererListParams = waaaghRendererListData.Transparent.ListParams;
		rendererListParams.drawSettings.perObjectData = PerObjectData.None;
		rendererListParams.filteringSettings.renderQueueRange = WaaaghRenderQueue.All;
		rendererList = context.CreateRendererList(ref rendererListParams);
	}

	protected override void Setup(RenderGraphBuilder builder, DrawObjectsWireframePassData data, ContextContainer frameData)
	{
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		builder.AllowRendererListCulling(value: false);
		TextureHandle input = waaaghResourceData.CameraResolveColorBuffer;
		data.RenderTarget = builder.UseColorBuffer(in input, 0);
		data.ClearColor = m_ClearColor;
	}

	protected override void Render(DrawObjectsWireframePassData data, RenderGraphContext context)
	{
		context.cmd.ClearRenderTarget(clearDepth: true, clearColor: true, data.ClearColor);
		context.cmd.DrawRendererList(data.RendererList);
	}
}
