using Owlcat.Runtime.Visual.Waaagh.Debugging;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Debug;

public class DebugMapOverlayPass : ScriptableRenderPass<DebugMapOverlayPassData>
{
	private WaaaghDebugData m_DebugData;

	public override string Name => "DebugMapOverlayPass";

	public DebugMapOverlayPass(RenderPassEvent evt, WaaaghDebugData debugData)
		: base(evt)
	{
		m_DebugData = debugData;
	}

	protected override void Setup(RenderGraphBuilder builder, DebugMapOverlayPassData data, ContextContainer frameData)
	{
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		WaaaghRenderingData waaaghRenderingData = frameData.Get<WaaaghRenderingData>();
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		switch (m_DebugData.RenderingDebug.DebugMapOverlay)
		{
		case DebugMapOverlay.MotionVectors:
			data.OverlayHandle = builder.ReadTexture(in waaaghResourceData.CameraMotionVectorsRT);
			break;
		case DebugMapOverlay.Depth:
		{
			TextureHandle input = waaaghResourceData.CameraDepthBuffer;
			data.OverlayHandle = builder.ReadTexture(in input);
			break;
		}
		default:
			data.OverlayHandle = waaaghRenderingData.RenderGraph.defaultResources.magentaTextureXR;
			break;
		}
		data.Size = m_DebugData.RenderingDebug.MapSize;
		data.FlipVertically = waaaghCameraData.resolveToScreen;
	}

	protected override void Render(DebugMapOverlayPassData data, RenderGraphContext context)
	{
		if (data.OverlayHandle.IsValid())
		{
			Vector4 scaleBiasTex = new Vector4(1f, 1f, 0f, 0f);
			Vector4 scaleBiasRT = new Vector4(data.Size, data.Size, 0f, data.FlipVertically ? (1f - data.Size) : 0f);
			Blitter.BlitQuad(context.cmd, data.OverlayHandle, scaleBiasTex, scaleBiasRT, 0, bilinear: false);
		}
	}
}
